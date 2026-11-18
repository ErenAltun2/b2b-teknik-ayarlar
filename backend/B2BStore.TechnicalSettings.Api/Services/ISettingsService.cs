using B2BStore.TechnicalSettings.Api.Models;

namespace B2BStore.TechnicalSettings.Api.Services;

/// <summary>Teknik Ayarlar iş kuralları: okuma, doğrulama, tip dönüşümü ve bitwise birleştirme.</summary>
public interface ISettingsService
{
    /// <summary>DealerSettings satırlarının özeti (tabloda anahtar kolon olmadığı için satır numarasıyla seçilir).</summary>
    Task<IReadOnlyList<DealerSettingsRowSummary>> GetRowsAsync();

    Task<SettingsResponse> GetAsync(int row);

    /// <summary>Sadece gönderilen (değişen) ayarları doğrular ve kaydeder.</summary>
    /// <exception cref="SettingsValidationException">Bir veya daha fazla ayar geçersizse.</exception>
    Task<SaveSettingsResponse> SaveAsync(int row, SaveSettingsRequest request);
}
