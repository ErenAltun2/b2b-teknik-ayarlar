-- DealerSettings'te 4 satır var ama hiçbir anahtar (ID / DealerId) kolonu yok.
-- Bu sorgu her satırı fiziksel konumuyla numaralandırır ve satırları ayırt eden
-- kolonları yan yana gösterir. Sonucu: analiz\satirlar.csv
USE store5;

SELECT ROW_NUMBER() OVER (ORDER BY %%physloc%%) AS SatirNo,
       Logo, Logo2, LayoutColor, Layout, SiteStatus, FrontLanguageCode,
       ParameterDesign, ParameterMenu, ParameterProductDetail, ParameterProductDetail2,
       ParameterProductSearch, ParameterCart, ParameterOrder, ParameterPayment,
       ParameterStock, ParameterAdmin, ParameterLogin, ParameterLoginMenu,
       ProductTableStockType, CartType, MenuType, PagingType, HomeStyle
FROM dbo.DealerSettings
ORDER BY SatirNo;

-- Satırlar birebir aynı mı? (aynıysa 4'ten küçük bir sayı döner)
SELECT COUNT(*) AS FarkliSatirSayisi
FROM (SELECT DISTINCT * FROM dbo.DealerSettings) x;
