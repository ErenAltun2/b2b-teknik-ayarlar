namespace B2BStore.Settings.Api.Dtos
{
    // Sayfa açılınca sistemın yuklenmesı ıcın dırekt gonderdıgımız verıler
    public class SettingsResponseDto
    {
        // Tabloda 4 satır var ve Id kolonu yok; satırları sırasıyla ayırt ediyoruz.
        // (Logolar farklı olduğu için her satır farklı bir firma olabilir, kesin değil.)
        // Row = hangi satırı gösterdiğimiz (0 = 1. satır)
        public int Row { get; set; }

        // O satırdaki ayarlar: kolon adı -> değer
        // ör. "ParameterStock" -> "3", "Logo" -> "fcs-logo.jpg"
        public Dictionary<string, string?> Values { get; set; } = new();
    }
}
