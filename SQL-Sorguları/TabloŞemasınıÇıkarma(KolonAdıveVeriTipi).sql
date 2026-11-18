SELECT 'DealerSettings' AS Tablo, c.column_id, c.name AS KolonAdi, t.name AS Tip, c.max_length, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.DealerSettings')
ORDER BY c.column_id;

SELECT 'DealerSystemSettings' AS Tablo, c.column_id, c.name AS KolonAdi, t.name AS Tip, c.max_length, c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.DealerSystemSettings')
ORDER BY c.column_id;