USE store5;

SELECT 'DealerSettings' AS Tablo,
       (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DealerSettings')) AS KolonSayisi,
       (SELECT COUNT(*) FROM dbo.DealerSettings) AS SatirSayisi
UNION ALL
SELECT 'DealerSystemSettings',
       (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DealerSystemSettings')),
       (SELECT COUNT(*) FROM dbo.DealerSystemSettings);