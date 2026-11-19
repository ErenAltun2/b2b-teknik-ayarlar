namespace B2BStore.Settings.Api.Dtos
{
    // Kaydet'e basılınca frontend'in gönderdiği isteğin tamamı
    public class SaveSettingsRequestDto
    {
        // Değişen ayarların listesi
        public List<SettingChangeDto> Changes { get; set; } = new();
    }
}
