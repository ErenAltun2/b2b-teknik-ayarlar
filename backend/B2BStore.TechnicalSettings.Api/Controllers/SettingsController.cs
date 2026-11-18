using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Models;
using B2BStore.TechnicalSettings.Api.Repositories;
using B2BStore.TechnicalSettings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace B2BStore.TechnicalSettings.Api.Controllers;

/// <summary>
/// Teknik Ayarlar endpoint'leri. (Spring Boot karşılığı: @RestController + @RequestMapping("/api/settings"))
/// Controller sadece HTTP katmanıdır: iş kuralları SettingsService'te, SQL SettingsRepository'de.
/// </summary>
[ApiController]
[Route("api/settings")]
[Tags("Teknik Ayarlar")]
public sealed class SettingsController(
    ISettingsService service,
    SettingsCatalog catalog,
    IConfiguration configuration) : ControllerBase
{
    // Tabloda anahtar kolon olmadığı için hangi satırın düzenleneceği satır numarasıyla seçilir.
    private int DefaultRow => configuration.GetValue("DealerSettings:DefaultRow", 1);

    /// <summary>Ekrandaki tüm ayarların tanımı (grup, etiket, kontrol tipi, seçenekler, bit değerleri).</summary>
    [HttpGet("catalog")]
    public ActionResult<SettingsCatalog> GetCatalog() => Ok(catalog);

    /// <summary>DealerSettings satırları.</summary>
    [HttpGet("rows")]
    public async Task<ActionResult<IReadOnlyList<DealerSettingsRowSummary>>> GetRows()
        => Ok(await service.GetRowsAsync());

    /// <summary>Seçili satırın mevcut ayar değerleri.</summary>
    [HttpGet]
    public async Task<ActionResult<SettingsResponse>> Get([FromQuery] int? row)
        => Ok(await service.GetAsync(row ?? DefaultRow));

    /// <summary>
    /// Yalnızca değişen ayarları kaydeder.
    /// Bitwise ayarlarda sadece değişen bitler gönderilir (setBits / clearBits), diğer bitler korunur.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<SaveSettingsResponse>> Save([FromQuery] int? row, [FromBody] SaveSettingsRequest request)
    {
        try
        {
            return Ok(await service.SaveAsync(row ?? DefaultRow, request));
        }
        catch (SettingsValidationException e)
        {
            return ValidationProblem(new ValidationProblemDetails(e.Errors)
            {
                Title = "Bazı ayarlar kaydedilemedi",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}

/// <summary>Uygulamanın ve veritabanı bağlantısının ayakta olup olmadığını kontrol eder.</summary>
[ApiController]
[Route("api/health")]
[Tags("Sistem")]
public sealed class HealthController(ISettingsRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var rows = await repository.CountRowsAsync();
        return Ok(new { status = "ok", dealerSettingsRows = rows });
    }
}
