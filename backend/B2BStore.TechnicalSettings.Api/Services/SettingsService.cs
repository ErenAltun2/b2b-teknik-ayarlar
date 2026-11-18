using System.Globalization;
using System.Text.RegularExpressions;
using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Models;
using B2BStore.TechnicalSettings.Api.Repositories;

namespace B2BStore.TechnicalSettings.Api.Services;

/// <summary>
/// İş kuralları katmanı. SQL bilmez; veriye sadece <see cref="ISettingsRepository"/> üzerinden erişir.
/// Görevleri: gelen değişiklikleri katalog kurallarına göre doğrulamak, string değerleri
/// kolon tiplerine çevirmek ve bitwise ayarlarda sadece değişen bitleri uygulamak.
/// </summary>
public sealed partial class SettingsService(
    ISettingsRepository repository,
    SettingsCatalog catalog,
    ILogger<SettingsService> logger) : ISettingsService
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public Task<IReadOnlyList<DealerSettingsRowSummary>> GetRowsAsync() => repository.GetRowSummariesAsync();

    public async Task<SettingsResponse> GetAsync(int row)
    {
        var stored = await repository.ReadAsync(row) ?? throw new SettingsNotFoundException(row);
        var count = await repository.CountRowsAsync();
        var values = catalog.Settings.ToDictionary(
            s => s.Key,
            s => stored.Values.TryGetValue(s.Key, out var v) ? ToWire(v) : null);
        return new SettingsResponse(row, count, values);
    }

    public async Task<SaveSettingsResponse> SaveAsync(int row, SaveSettingsRequest request)
    {
        // 1) Veritabanına gitmeden yapılabilen kontroller
        var errors = new ErrorBag();
        if (request.Changes.Count == 0) errors.Add("changes", "Kaydedilecek değişiklik yok.");
        foreach (var dup in request.Changes.GroupBy(c => c.Key, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add(dup.Key, "Aynı ayar birden fazla kez gönderildi.");
        foreach (var change in request.Changes.Where(c => !catalog.ByKey.ContainsKey(c.Key)))
            errors.Add(change.Key, "Bilinmeyen ayar.");
        errors.ThrowIfAny();

        // 2) Repository satırı kilitleyip okur, bu fonksiyonla yeni değerleri hesaplatır ve kaydeder.
        var changedKeys = await repository.UpdateAsync(row, stored => BuildUpdates(stored, request.Changes));

        logger.LogInformation("DealerSettings satır {Row} güncellendi: {Keys}", row, string.Join(", ", changedKeys));
        return new SaveSettingsResponse(changedKeys, await GetAsync(row));
    }

    /// <summary>Her değişikliği doğrular ve kolona yazılacak değeri üretir. Hata varsa hiçbiri kaydedilmez.</summary>
    private IReadOnlyDictionary<SettingDefinition, object?> BuildUpdates(StoredRow stored, IEnumerable<SettingChange> changes)
    {
        var errors = new ErrorBag();
        var updates = new Dictionary<SettingDefinition, object?>();

        foreach (var change in changes)
        {
            var def = catalog.ByKey[change.Key];
            stored.Values.TryGetValue(def.Key, out var currentRaw);
            var current = ToWire(currentRaw);

            object? dbValue;
            string? message;
            var ok = def.Bitwise
                ? TryBuildBitwise(def, current, change, out dbValue, out message)
                : TryBuildScalar(def, current, change.Value, out dbValue, out message);

            if (!ok) { errors.Add(def.Key, message!); continue; }
            if (ToWire(dbValue) == current) continue; // değer zaten aynı → güncellemeye gerek yok
            updates[def] = dbValue;
        }

        errors.ThrowIfAny();
        return updates;
    }

    // ------------------------------------------------------------------ bitwise

    private static bool TryBuildBitwise(SettingDefinition def, string? current, SettingChange change,
        out object? dbValue, out string? error)
    {
        dbValue = null;
        if (!TryParseBits(change.SetBits, out var set) || !TryParseBits(change.ClearBits, out var clear))
        { error = "Bit değerleri geçersiz."; return false; }
        if ((set & clear) != 0)
        { error = "Aynı seçenek hem açılıp hem kapatılamaz."; return false; }
        if (!BitMask.IsSubsetOf(set | clear, def.KnownMaskValue))
        { error = "Bu ayar için tanımlı olmayan ya da veritabanı kolonuna sığmayan bir seçenek gönderildi."; return false; }

        // NULL veya boş değer "hiçbir seçenek seçili değil" (0) kabul edilir.
        long existing = 0;
        if (!string.IsNullOrWhiteSpace(current) && !long.TryParse(current, NumberStyles.Integer, Inv, out existing))
        { error = $"Veritabanındaki mevcut değer sayı değil: \"{current}\"."; return false; }

        // Sadece istenen bitler değişir: (mevcut | açılanlar) & ~kapatılanlar
        var result = BitMask.Apply(existing, set, clear);
        if (result < 0 || result > BitMask.MaxFor(def.SqlType))
        { error = $"Sonuç değer ({result}) {def.SqlType} kolonuna sığmıyor."; return false; }

        dbValue = def.SqlType == "nvarchar" ? result.ToString(Inv) : ConvertInteger(def.SqlType, result);
        error = null;
        return true;
    }

    private static bool TryParseBits(string? s, out long value)
    {
        value = 0;
        return string.IsNullOrWhiteSpace(s) || (long.TryParse(s, NumberStyles.None, Inv, out value) && value >= 0);
    }

    // ------------------------------------------------------------------ tekil değerler

    private static bool TryBuildScalar(SettingDefinition def, string? current, string? value,
        out object? dbValue, out string? error)
    {
        dbValue = null;
        error = null;

        switch (def.Control)
        {
            case "switch":
            {
                var on = value switch { "1" or "true" or "True" => true, "0" or "false" or "False" => false, _ => (bool?)null };
                if (on is null) { error = "Açık/kapalı değeri \"1\" veya \"0\" olmalı."; return false; }
                dbValue = def.SqlType == "bit" ? on.Value : ConvertInteger(def.SqlType, on.Value ? 1 : 0);
                return true;
            }

            case "select":
            {
                if (value is null) { error = "Bir seçenek seçilmeli."; return false; }
                // Kayıtlı ama katalogda olmayan bir değer varsa, onun korunmasına izin verilir.
                var allowed = def.Options?.Any(o => o.Value == value && !o.Unsupported) == true || value == current;
                if (!allowed) { error = "Geçersiz seçenek."; return false; }
                return TryConvertForColumn(def, value, out dbValue, out error);
            }

            case "number":
            {
                if (string.IsNullOrWhiteSpace(value)) { error = "Bir sayı girilmeli."; return false; }
                if (!decimal.TryParse(value, NumberStyles.Number, Inv, out var number))
                { error = "Geçerli bir sayı girilmeli."; return false; }
                if (def.Min is { } min && number < min) { error = $"En az {min} olmalı."; return false; }
                return TryConvertForColumn(def, number.ToString(Inv), out dbValue, out error);
            }

            case "color":
                if (!string.IsNullOrEmpty(value) && !HexColor().IsMatch(value))
                { error = "Renk #RRGGBB biçiminde olmalı."; return false; }
                return TryConvertForColumn(def, value, out dbValue, out error);

            default: // text, textarea, image (dosya adı)
                return TryConvertForColumn(def, value, out dbValue, out error);
        }
    }

    private static bool TryConvertForColumn(SettingDefinition def, string? value, out object? dbValue, out string? error)
    {
        dbValue = null;
        error = null;
        if (value is null) return true; // tüm kolonlar NULL kabul ediyor

        switch (def.SqlType)
        {
            case "nvarchar":
                if (def.MaxLength > 0 && value.Length > def.MaxLength)
                { error = $"En fazla {def.MaxLength} karakter girilebilir."; return false; }
                dbValue = value;
                return true;

            case "decimal":
                if (!decimal.TryParse(value, NumberStyles.Number, Inv, out var d)) { error = "Geçerli bir sayı girilmeli."; return false; }
                dbValue = d;
                return true;

            case "bit":
                dbValue = value is "1" or "true" or "True";
                return true;

            case "tinyint" or "smallint" or "int" or "bigint":
                if (!long.TryParse(value, NumberStyles.Integer, Inv, out var n))
                { error = "Tam sayı girilmeli."; return false; }
                var (lo, hi) = def.SqlType switch
                {
                    "tinyint" => (0L, (long)byte.MaxValue),
                    "smallint" => (short.MinValue, short.MaxValue),
                    "int" => (int.MinValue, int.MaxValue),
                    _ => (long.MinValue, long.MaxValue)
                };
                if (n < lo || n > hi) { error = $"Değer {lo} ile {hi} arasında olmalı."; return false; }
                dbValue = ConvertInteger(def.SqlType, n);
                return true;

            default:
                error = $"Desteklenmeyen kolon tipi: {def.SqlType}";
                return false;
        }
    }

    private static object ConvertInteger(string sqlType, long n) => sqlType switch
    {
        "tinyint" => (byte)n,
        "smallint" => (short)n,
        "int" => (int)n,
        _ => n
    };

    /// <summary>Veritabanı değerini API'nin string formatına çevirir (bit → "1"/"0").</summary>
    public static string? ToWire(object? value) => value switch
    {
        null or DBNull => null,
        bool b => b ? "1" : "0",
        decimal d => d.ToString("0.############", Inv),
        IFormattable f => f.ToString(null, Inv),
        _ => value.ToString()
    };

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();

    /// <summary>Ayar bazında hata mesajlarını toplar.</summary>
    private sealed class ErrorBag
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public void Add(string key, string message)
        {
            if (!_errors.TryGetValue(key, out var list)) _errors[key] = list = [];
            list.Add(message);
        }

        public void ThrowIfAny()
        {
            if (_errors.Count > 0)
                throw new SettingsValidationException(_errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()));
        }
    }
}
