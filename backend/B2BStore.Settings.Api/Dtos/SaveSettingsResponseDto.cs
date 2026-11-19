namespace B2BStore.Settings.Api.Dtos
{
    // Kaydet'ten sonra API'nin frontend'e döndüğü cevap, SaveSettingsRequestDto ıle bılgılerı servis katmanına ilettik kayıt başarılı ise dönecek olan cevap
    public class SaveSettingsResponseDto
    {
        // Gerçekten değişen ayarların kolon adları
        public List<string> ChangedKeys { get; set; } = new();

        // Kayıttan sonraki güncel ayarlar
        public SettingsResponseDto Settings { get; set; } = new();
    }
}
