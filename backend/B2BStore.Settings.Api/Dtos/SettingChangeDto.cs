namespace B2BStore.Settings.Api.Dtos
{
    public class SettingChangeDto
    {
        // Değişen ayarın veritabanındaki kolon adı (ör. "MinCartCount")
        public string Key { get; set; } = "";

        // Normal ayarlarda yeni değer (ör. "5")
        public string? Value { get; set; }

        // Bitwise ayarlarda AÇILAN seçeneklerin toplamı (ör. "8")
        public string? SetBits { get; set; }

        // Bitwise ayarlarda KAPATILAN seçeneklerin toplamı
        public string? ClearBits { get; set; }
    }
}

/*
 SettingChangeDto bütün ayarları değil, sadece DEĞİŞEN TEK BİR ayarı tutar
Frontend'de Kaydet'e basıldığında sadece değişen ayarlar gönderilir.
Örnek (3 ayar değişmiş):
{
  "changes": [
    { "key": "MinCartCount",   "value": "5" },
    { "key": "SiteStatus",     "value": "0" },
    { "key": "ParameterStock", "setBits": "8", "clearBits": "1" }
  ]
}
bu degerlerı tutan sınıftır.
*/
