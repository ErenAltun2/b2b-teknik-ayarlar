namespace B2BStore.TechnicalSettings.Api.Models;

/// <summary>Bir veya daha fazla ayar doğrulamadan geçemedi (→ 400 Bad Request).</summary>
public sealed class SettingsValidationException(IDictionary<string, string[]> errors)
    : Exception("Ayarlar doğrulanamadı.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

/// <summary>İstenen satır numarası tabloda yok (→ 404 Not Found).</summary>
public sealed class SettingsNotFoundException(int row)
    : Exception($"DealerSettings tablosunda {row}. satır bulunamadı.");
