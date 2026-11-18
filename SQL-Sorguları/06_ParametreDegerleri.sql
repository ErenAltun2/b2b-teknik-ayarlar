-- Teknik Ayarlar analizinde kullanılacak mevcut değerler.
-- Her sonuç tablosunu ayrı CSV olarak kaydet:
--   1) analiz\satir_sayisi.csv   2) analiz\bitwise_degerler.csv   3) analiz\system_settings.csv
USE store5;

-- 1) Kaç satır var? (tek bayi ayarı mı, birden fazla mı?)
SELECT (SELECT COUNT(*) FROM dbo.DealerSettings)       AS DealerSettingsSatir,
       (SELECT COUNT(*) FROM dbo.DealerSystemSettings) AS DealerSystemSettingsSatir;

-- 2) Bitwise olduğu düşünülen kolonların değerleri ve hangi bitlerin açık olduğu
;WITH Bitler AS (
    SELECT TOP (63) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
    FROM sys.all_objects
),
Degerler AS (
    SELECT v.Kolon, v.Deger
    FROM dbo.DealerSettings ds
    CROSS APPLY (VALUES
        ('ParameterDesign',                CAST(ds.ParameterDesign AS bigint)),
        ('ParameterMenu',                  CAST(ds.ParameterMenu AS bigint)),
        ('ParameterProductDetail',         CAST(ds.ParameterProductDetail AS bigint)),
        ('ParameterProductDetail2',        CAST(ds.ParameterProductDetail2 AS bigint)),
        ('ParameterProductDetailNotLogin', CAST(ds.ParameterProductDetailNotLogin AS bigint)),
        ('ParameterProductSearch',         CAST(ds.ParameterProductSearch AS bigint)),
        ('ParameterDynamicSearch',         CAST(ds.ParameterDynamicSearch AS bigint)),
        ('ParameterCart',                  CAST(ds.ParameterCart AS bigint)),
        ('ParameterOrder',                 CAST(ds.ParameterOrder AS bigint)),
        ('ParameterPayment',               CAST(ds.ParameterPayment AS bigint)),
        ('ParameterStock',                 CAST(ds.ParameterStock AS bigint)),
        ('ParameterAdmin',                 CAST(ds.ParameterAdmin AS bigint)),
        ('ParameterLogin',                 CAST(ds.ParameterLogin AS bigint)),
        ('ParameterLoginMenu',             CAST(ds.ParameterLoginMenu AS bigint)),
        ('ParameterLoginSubDealer',        CAST(ds.ParameterLoginSubDealer AS bigint)),
        ('ParameterModules',               CAST(ds.ParameterModules AS bigint)),
        ('ParameterIntegration',           CAST(ds.ParameterIntegration AS bigint)),
        ('ParameterTheme',                 CAST(ds.ParameterTheme AS bigint)),
        ('ParameterAccount',               CAST(ds.ParameterAccount AS bigint)),
        ('ExportProductList',              CAST(ds.ExportProductList AS bigint)),
        ('ShowCompanyNameAsLogo',          CAST(ds.ShowCompanyNameAsLogo AS bigint))
    ) v(Kolon, Deger)
)
SELECT d.Kolon,
       d.Deger,
       (SELECT STRING_AGG(CAST(b.n AS varchar(3)), ',') WITHIN GROUP (ORDER BY b.n)
        FROM Bitler b
        WHERE d.Deger IS NOT NULL AND d.Deger > 0
          AND (d.Deger & POWER(CAST(2 AS bigint), b.n)) <> 0) AS AcikBitler
FROM Degerler d
ORDER BY d.Kolon;

-- 3) DealerSystemSettings (anahtar-değer tablosu)
--    Uzun değerler (script, anahtar vb. olabilir) gizlenir.
SELECT SettingKey,
       CASE WHEN LEN(SettingValue) <= 30 THEN SettingValue
            ELSE '<uzun: ' + CAST(LEN(SettingValue) AS varchar(10)) + ' karakter>' END AS SettingValue,
       UsePayPackets,
       PosManagmentRatioRulePeriod
FROM dbo.DealerSystemSettings
ORDER BY SettingKey;
