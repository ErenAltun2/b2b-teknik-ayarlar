USE store5;
SELECT COUNT(*) AS TabloSayisi FROM sys.tables;
SELECT name FROM sys.tables WHERE name LIKE '%Dealer%' ORDER BY name;