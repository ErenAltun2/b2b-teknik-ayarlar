using B2BStore.Settings.Api.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Globalization;
namespace B2BStore.Settings.Api.Repositories
{
    // Veritabanı sorgularını burada yazıyoruz.
    // DealerSettings'te primary key yok; EF Core anahtarsız tabloyu güncelleyemez.
    // Bu yüzden Dapper ile SQL'i kendimiz yazıyoruz.
    public class SettingsRepository
    {
        // appsettings'ten okunan bağlantı adresi

        private readonly string _connectionString;

        // Uygulama bu sınıfı oluştururken ayarları (IConfiguration) bize verir
        //constructor tanımladık sureklı connection baglantısını gırmemek ıcın bunu program.cs de kaydedecegız 
        public SettingsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Store")
                ?? throw new InvalidOperationException("ConnectionStrings:Store bulunamadı");
        }

        //SQL SORGULARI 

        public List<RowSummaryDto> GetRowSummaries()
        {
            //SSMS'de denediğim sorgu 
            const string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY %%physloc%%)  AS [Row],
                                       Logo, LayoutColor, DealerName
                                FROM DealerSettings";

            //sql baglantısını acıyorum
            using var connection = new SqlConnection(_connectionString);
            //sorguyu calıstıracagım 
            return connection.Query<RowSummaryDto>(sql).ToList();
        }


        // Seçilen satırın tüm ayarlarını getirir (kolon adı -> değer)
        public Dictionary<string, string?>? GetSettings(int row)
        {
            // SSMS'te denediğimiz alt sorgu; 1 yerine @row parametresi
            const string sql = @"
            SELECT *
            FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY %%physloc%%)  AS [Row], *
                FROM DealerSettings
            ) AS Numarali
            WHERE [Row] = @row";

            using var connection = new SqlConnection(_connectionString);

            // Tek satır bekliyoruz; yoksa null döner
            var satir = connection.QuerySingleOrDefault(sql, new { row });
            if (satir == null)
                return null;   // böyle bir satır yok (ör. row = 10)

            // Dapper'ın getirdiği satırı "kolon adı -> değer" olarak dolaşıyoruz
            var values = new Dictionary<string, string?>();
            foreach (var kolon in (IDictionary<string, object>)satir)
            {
                if (kolon.Key == "Row")
                    continue;   // kendi eklediğimiz numara kolonu, ayar değil

                // Her değeri yazıya çevir (NULL ise null kalsın)
                if (kolon.Value == null)
                {
                    // NULL ise null kalsın
                    values[kolon.Key] = null;
                }
                else if (kolon.Value is bool)
                {
                    // bit kolonları C#'a True/False olarak gelir;
                    // frontend ve kurallarımız 1/0 bekliyor
                    bool deger = (bool)kolon.Value;
                    values[kolon.Key] = deger ? "1" : "0";
                }
                else
                {
                    // Diğer her şeyi standart biçimde yazıya çevir
                    values[kolon.Key] = Convert.ToString(kolon.Value, CultureInfo.InvariantCulture);
                }
            }

            return values;
        }

        // Değişiklikleri kaydeder.
        // DİKKAT: Kolon adları Service'te katalogdan kontrol edilmiş olmalı (beyaz liste).
        public int UpdateSettings(int row, List<SettingChangeDto> changes)
        {
            // ---------- PARÇA 1: cümle parçaları + kâğıt ----------
            var setParcalari = new List<string>();        // "[Logo] = @deger0" gibi parçalar
            var parametreler = new DynamicParameters();   // @deger0, @ac1... değerleri
            parametreler.Add("row", row);

            for (int i = 0; i < changes.Count; i++)
            {
                var degisiklik = changes[i];
                var kolon = "[" + degisiklik.Key + "]";   // ör. [ParameterStock]

                if (degisiklik.SetBits != null || degisiklik.ClearBits != null)
                {
                    // Bitwise ayar: sadece istenen bitleri aç/kapat, diğerlerine dokunma
                    setParcalari.Add($"{kolon} = ({kolon} | @ac{i}) & ~@kapat{i}");
                    parametreler.Add($"ac{i}", long.Parse(degisiklik.SetBits ?? "0"));
                    parametreler.Add($"kapat{i}", long.Parse(degisiklik.ClearBits ?? "0"));
                }
                else
                {
                    // Normal ayar: yeni değeri direkt yaz
                    setParcalari.Add($"{kolon} = @deger{i}");
                    parametreler.Add($"deger{i}", degisiklik.Value);
                }
            }

            // ---------- PARÇA 2: cümleyi tamamla ve gönder ----------
            var sql = $@"
        WITH Numarali AS (
            SELECT ROW_NUMBER() OVER (ORDER BY %%physloc%%) AS [Row], *
            FROM DealerSettings
        )
        UPDATE Numarali
        SET {string.Join(", ", setParcalari)}
        WHERE [Row] = @row";

            using var connection = new SqlConnection(_connectionString);

            // Kaç satırın güncellendiğini döner (doğru çalışırsa 1)
            return connection.Execute(sql, parametreler);
        }

    }
}
