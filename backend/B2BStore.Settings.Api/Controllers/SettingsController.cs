using B2BStore.Settings.Api.Catalog;
using B2BStore.Settings.Api.Dtos;
using B2BStore.Settings.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace B2BStore.Settings.Api.Controllers
{
    // Frontend'in istekleri ilk buraya gelir.
    // Service'i çağırır, sonucu HTTP cevabına çevirir (200 / 400 / 404).
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _service;

        public SettingsController(SettingsService service)
        {
            _service = service;
        }

        // GET /api/settings/catalog → ekranın tarifi (gruplar, ayarlar, seçenekler)
        [HttpGet("catalog")]
        public ActionResult<SettingsCatalog> GetCatalog()
        {
            return Ok(_service.GetCatalog());
        }

        // GET /api/settings/rows → sağ üstteki satır seçici
        [HttpGet("rows")]
        public ActionResult<List<RowSummaryDto>> GetRows()
        {
            return Ok(_service.GetRowSummaries());
        }

        // GET /api/settings?row=0 → seçilen satırın ayarları
        [HttpGet]
        public ActionResult<SettingsResponseDto> GetSettings([FromQuery] int row = 1)
        {
            try
            {
                return Ok(_service.GetSettings(row));
            }
            catch (KeyNotFoundException hata)
            {
                return Problem(title: "Bulunamadı", detail: hata.Message, statusCode: 404);
            }
        }

        // PUT /api/settings?row=0 → Kaydet butonu
        [HttpPut]
        public ActionResult<SaveSettingsResponseDto> SaveSettings([FromQuery] int row, [FromBody] SaveSettingsRequestDto request)
        {
            try
            {
                return Ok(_service.SaveSettings(row, request));
            }
            catch (ArgumentException hata)       // KontrolEt'teki kurallardan biri geçemedi
            {
                return Problem(title: "Geçersiz istek", detail: hata.Message, statusCode: 400);
            }
            catch (KeyNotFoundException hata)    // böyle bir satır yok
            {
                return Problem(title: "Bulunamadı", detail: hata.Message, statusCode: 404);
            }
        }
    }
}