using B2BStore.Settings.Api.Catalog;
using B2BStore.Settings.Api.Dtos;
using B2BStore.Settings.Api.Repositories;
using System.Globalization;

namespace B2BStore.Settings.Api.Services
{
    // Controller ile Repository arasındaki katman:
    // kontrolleri yapar, Repository'yi çağırır, cevabı (DTO) hazırlar
    public class SettingsService
    {
        private readonly SettingsRepository _repository;   // veritabanı işleri
        private readonly SettingsCatalog _catalog;         // beyaz liste ve kurallar
        public SettingsService(SettingsRepository repository, SettingsCatalog catalog)
        {
            _repository = repository;
            _catalog = catalog;
        }



        public SettingsCatalog GetCatalog()
        {
            return _catalog;
        }

        // Sağ üstteki satır seçici için: Repository'den geleni olduğu gibi iletir
        public List<RowSummaryDto> GetRowSummaries()
        {
            return _repository.GetRowSummaries();
        }

        // Seçilen satırın ayarlarını frontend'in beklediği pakete koyar
        public SettingsResponseDto GetSettings(int row)
        {
            var values = _repository.GetSettings(row);

            // Repository null döndüyse böyle bir satır yok (ör. row = 10)
            if (values == null)
                throw new KeyNotFoundException($"{row}. satır bulunamadı");


            return new SettingsResponseDto
            {
                Row = row,
                Values = values
            };
        }

        // Kaydet'e basılınca: kontrol et → kaydet → değişenleri bul → cevap dön
        public SaveSettingsResponseDto SaveSettings(int row, SaveSettingsRequestDto request)
        {
            // 1) Kurallara uyuyor mu? Uymuyorsa burada hata fırlatılır, hiçbir şey kaydedilmez
            KontrolEt(request.Changes);

            // 2) Kayıttan ÖNCEKİ hali oku (satır yoksa hata)
            Dictionary<string, string?>? onceki = _repository.GetSettings(row);
            if (onceki == null)
            {
                throw new KeyNotFoundException(row + ". satır bulunamadı");
            }

            // 3) Kaydet
            int guncellenenSatirSayisi = _repository.UpdateSettings(row, request.Changes);
            if (guncellenenSatirSayisi != 1)
            {
                throw new InvalidOperationException("Beklenmeyen sonuç: " + guncellenenSatirSayisi + " satır güncellendi");
            }

            // 4) Kayıttan SONRAKİ hali oku
            Dictionary<string, string?> sonraki = _repository.GetSettings(row)!;

            // 5) Hangi ayarlar gerçekten değişti?
            List<string> degisenler = new List<string>();
            foreach (SettingChangeDto degisiklik in request.Changes)
            {
                string? eskiDeger = onceki[degisiklik.Key];
                string? yeniDeger = sonraki[degisiklik.Key];

                if (eskiDeger != yeniDeger)
                {
                    degisenler.Add(degisiklik.Key);
                }
            }

            // 6) Cevabı hazırla
            return new SaveSettingsResponseDto
            {
                ChangedKeys = degisenler,
                Settings = new SettingsResponseDto
                {
                    Row = row,
                    Values = sonraki
                }
            };
        }


        // Kaydetmeden önce her değişikliği kontrol eder.
        // Bir tanesi bile hatalıysa hata fırlatır; hiçbir şey kaydedilmez.
        private void KontrolEt(List<SettingChangeDto> changes)
        {
            if (changes.Count == 0)
            {
                throw new ArgumentException("Kaydedilecek değişiklik yok");
            }

            foreach (SettingChangeDto degisiklik in changes)
            {
                // ---------- 1. KURAL: Beyaz liste (ayar katalogda var mı?) ----------

                SettingDefinition? tanim;   // bulunamazsa null kalır, bu yüzden "?"
                bool katalogdaVar = _catalog.ByKey.TryGetValue(degisiklik.Key, out tanim);

                if (katalogdaVar == false || tanim == null)
                {
                    throw new ArgumentException("Bilinmeyen ayar: " + degisiklik.Key);
                }

                // Kolon adını katalogdaki yazılışıyla değiştir (ör. "logo" -> "Logo")
                degisiklik.Key = tanim.Key;

                // ---------- 2. KURAL: Bitwise ayar ----------

                if (tanim.Bitwise == true)
                {
                    // Bitwise ayara değer gönderilmemeli, sadece SetBits/ClearBits
                    if (degisiklik.Value != null)
                    {
                        throw new ArgumentException(tanim.Key + " bitwise bir ayar; Value yerine SetBits/ClearBits gönderin");
                    }

                    // Boş geldiyse "0" kabul et
                    string acilacakYazi = degisiklik.SetBits ?? "0";
                    string kapatilacakYazi = degisiklik.ClearBits ?? "0";

                    // Yazıyı sayıya çevirmeyi dene
                    long ac;
                    long kapat;
                    bool acSayiMi = long.TryParse(acilacakYazi, out ac);
                    bool kapatSayiMi = long.TryParse(kapatilacakYazi, out kapat);

                    if (acSayiMi == false || kapatSayiMi == false)
                    {
                        throw new ArgumentException(tanim.Key + ": bitler sayı olmalı");
                    }

                    // Katalogda olmayan bir bit var mı? Açılan ve kapatılan kutucukları birleştiriyoruz (|). Bu kutucukların içinden katalogda bilinenleri çıkarıyoruz
                    // (& ~). Geriye hiçbir şey kalmazsa (0) hepsi tanıdık kutucuklar → uyuyor. Bir şey kalırsa katalogda olmayan bir kutucuğa dokunulmuş → uyuşmuyor.
                    long degisenBitler = ac | kapat;
                    long bilinmeyenBitler = degisenBitler & ~tanim.KnownMaskValue;

                    if (bilinmeyenBitler != 0)
                    {
                        throw new ArgumentException(tanim.Key + ": bilinmeyen bit");
                    }

                    continue;   // bu değişiklik tamam, sıradakine geç
                }

                // ---------- 3. KURAL: Seçenekli ayar (select) ----------

                if (tanim.Control == "select")
                {
                    // Kullanıcının gönderdiği değer, katalogdaki seçeneklerden biri mi?
                    bool secenekBulundu = false;

                    // Options katalogda boş olabilir; boşsa hiçbir değer kabul edilmez
                    List<SettingOption> secenekler = tanim.Options ?? new List<SettingOption>();

                    foreach (SettingOption secenek in secenekler)
                    {
                        if (secenek.Value == degisiklik.Value)
                        {
                            secenekBulundu = true;
                        }
                    }

                    if (secenekBulundu == false)
                    {
                        throw new ArgumentException(tanim.Key + ": geçersiz seçenek '" + degisiklik.Value + "'");
                    }

                    continue;   // bu değişiklik tamam, sıradakine geç
                }

                // ---------- 4. KURAL: Açık/kapalı ayar (switch) ----------

                if (tanim.Control == "switch")
                {
                    if (degisiklik.Value != "0" && degisiklik.Value != "1")
                    {
                        throw new ArgumentException(tanim.Key + ": sadece 0 ya da 1 olabilir");
                    }

                    continue;   // bu değişiklik tamam, sıradakine geç
                }

                // ---------- 5. KURAL: Yazı alanları (text, image, color, number...) ----------

                // Boş bırakılabilir (null); doluysa uzunluğuna bak
                if (degisiklik.Value != null)
                {
                    if (degisiklik.Value.Length > tanim.MaxLength)
                    {
                        throw new ArgumentException(tanim.Key + ": en fazla " + tanim.MaxLength + " karakter olabilir");
                    }

                    // Sayı alanıysa: gerçekten sayı mı, en küçük değerin altında mı?
                    if (tanim.Control == "number")
                    {
                        decimal sayi;
                        bool sayiMi = decimal.TryParse(degisiklik.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out sayi);

                        if (sayiMi == false)
                        {
                            throw new ArgumentException(tanim.Key + ": sayı olmalı");
                        }

                        if (tanim.Min != null && sayi < tanim.Min)
                        {
                            throw new ArgumentException(tanim.Key + ": en az " + tanim.Min + " olabilir");
                        }
                    }
                }

            }   // ← döngü sonu
        }       // ← metot sonu
    }
}
