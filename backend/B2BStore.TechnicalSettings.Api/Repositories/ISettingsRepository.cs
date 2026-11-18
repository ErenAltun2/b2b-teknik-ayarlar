using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Models;

namespace B2BStore.TechnicalSettings.Api.Repositories;

/// <summary>
/// dbo.DealerSettings tablosuna erişim. Tüm SQL bu katmandadır; service katmanı SQL bilmez.
/// </summary>
public interface ISettingsRepository
{
    Task<int> CountRowsAsync();

    Task<IReadOnlyList<DealerSettingsRowSummary>> GetRowSummariesAsync();

    /// <summary>Seçili satırı okur; satır yoksa null döner.</summary>
    Task<StoredRow?> ReadAsync(int row);

    /// <summary>
    /// Satırı kilitleyerek okur, <paramref name="computeChanges"/> ile hangi kolonların hangi
    /// değerlere güncelleneceğini hesaplatır ve aynı transaction içinde kaydeder.
    /// computeChanges hata fırlatırsa hiçbir şey kaydedilmez (rollback).
    /// </summary>
    /// <returns>Gerçekten güncellenen kolonlar.</returns>
    Task<IReadOnlyList<string>> UpdateAsync(
        int row,
        Func<StoredRow, IReadOnlyDictionary<SettingDefinition, object?>> computeChanges);
}
