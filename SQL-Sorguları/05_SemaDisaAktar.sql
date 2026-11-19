-- DealerSettings ve DealerSystemSettings tablolarının şemasını tek sonuçta verir.
-- Sonucu: ızgaraya sağ tık > Save Results As... > analiz\sema.csv
USE store5;

SELECT  OBJECT_NAME(c.object_id)                    AS Tablo,
        c.column_id                                 AS Sira,
        c.name                                      AS Kolon,
        t.name                                      AS Tip,
        CASE WHEN t.name IN ('nvarchar','nchar') AND c.max_length > 0
             THEN c.max_length / 2 ELSE c.max_length END AS Uzunluk,
        c.is_nullable                               AS NullOlabilir,
        c.is_identity                               AS OtomatikArtan,
        dc.definition                               AS Varsayilan,
        CAST(ep.value AS nvarchar(4000))            AS Aciklama
FROM sys.columns c
JOIN sys.types t              ON c.user_type_id = t.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
LEFT JOIN sys.extended_properties ep ON ep.major_id = c.object_id
                                     AND ep.minor_id = c.column_id
                                     AND ep.name = 'MS_Description'
WHERE c.object_id IN (OBJECT_ID('dbo.DealerSettings'), OBJECT_ID('dbo.DealerSystemSettings'))
ORDER BY Tablo, Sira;
