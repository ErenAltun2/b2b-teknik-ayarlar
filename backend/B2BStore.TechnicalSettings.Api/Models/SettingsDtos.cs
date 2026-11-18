namespace B2BStore.TechnicalSettings.Api.Models;

// API'nin istek / cevap modelleri (Spring karşılığı: DTO sınıfları).
//
// Tüm değerler string olarak taşınır: bigint bitwise değerler (2^62'ye kadar)
// JavaScript'in güvenli tam sayı sınırını (2^53) aşar, JSON number olarak gönderilemez.
// Açık/kapalı ayarlar "1"/"0", veritabanında NULL olan değerler null olarak gelir.

public sealed record SettingsResponse(int Row, int RowCount, IReadOnlyDictionary<string, string?> Values);

public sealed record DealerSettingsRowSummary(int Row, string? Logo, string? LayoutColor, string? DealerName);

public sealed class SaveSettingsRequest
{
    public List<SettingChange> Changes { get; init; } = [];
}

public sealed class SettingChange
{
    public string Key { get; init; } = "";

    /// <summary>Normal (bitwise olmayan) ayarların yeni değeri.</summary>
    public string? Value { get; init; }

    /// <summary>Sadece bitwise: AÇILAN seçeneklerin bit toplamı (string).</summary>
    public string? SetBits { get; init; }

    /// <summary>Sadece bitwise: KAPATILAN seçeneklerin bit toplamı (string).</summary>
    public string? ClearBits { get; init; }
}

public sealed record SaveSettingsResponse(IReadOnlyList<string> ChangedKeys, SettingsResponse Settings);

/// <summary>Veritabanından okunan bir DealerSettings satırı (kolon adı → ham değer).</summary>
public sealed record StoredRow(IReadOnlyDictionary<string, object?> Values);
