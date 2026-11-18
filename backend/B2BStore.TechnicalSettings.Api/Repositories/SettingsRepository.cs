using System.Data;
using B2BStore.TechnicalSettings.Api.Catalog;
using B2BStore.TechnicalSettings.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace B2BStore.TechnicalSettings.Api.Repositories;

/// <summary>
/// dbo.DealerSettings için Dapper tabanlı repository.
///
/// Tabloda PRIMARY KEY yok ve yedekte 4 satır var. Şemayı değiştirmeden tek bir satırı
/// hedeflemek için satırlar fiziksel konumlarına (%%physloc%%) göre numaralandırılır;
/// güncellemede, aynı transaction içinde az önce okunan konumdaki satır güncellenir.
/// UPDATE tam olarak 1 satırı etkilemezse transaction geri alınır.
/// Kalıcı çözüm önerisi (uygulanmadı, case gereksiz şema değişikliğine izin vermiyor):
/// tabloya identity primary key / DealerId kolonu eklenmesi.
/// </summary>
public sealed class SettingsRepository(IConfiguration configuration, SettingsCatalog catalog) : ISettingsRepository
{
    private string ConnectionString =>
        configuration.GetConnectionString("Store")
        ?? throw new InvalidOperationException("ConnectionStrings:Store tanımlı değil.");

    // Kolon adları kullanıcıdan değil, sadece katalogdan gelir (^[A-Za-z0-9_]+$ ile doğrulanmış),
    // bu yüzden kolon listesini string olarak oluşturmak SQL injection'a açık değildir.
    // Değerler ise her zaman parametre (@p0, @p1 ...) olarak gönderilir.
    private string ColumnList => string.Join(", ", catalog.Settings.Select(s => $"[{s.Key}]"));

    private SqlConnection Open()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public async Task<int> CountRowsAsync()
    {
        await using var connection = Open();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.DealerSettings;");
    }

    public async Task<IReadOnlyList<DealerSettingsRowSummary>> GetRowSummariesAsync()
    {
        await using var connection = Open();
        var rows = await connection.QueryAsync<DealerSettingsRowSummary>("""
            SELECT CAST(ROW_NUMBER() OVER (ORDER BY %%physloc%%) AS int) AS Row,
                   Logo, LayoutColor, DealerName
            FROM dbo.DealerSettings
            ORDER BY Row;
            """);
        return rows.ToList();
    }

    public async Task<StoredRow?> ReadAsync(int row)
    {
        await using var connection = Open();
        var result = await ReadRowAsync(connection, null, row, lockRow: false);
        return result?.Row;
    }

    public async Task<IReadOnlyList<string>> UpdateAsync(
        int row,
        Func<StoredRow, IReadOnlyDictionary<SettingDefinition, object?>> computeChanges)
    {
        await using var connection = Open();
        // Serializable + UPDLOCK: okuma ile yazma arasında başka bir kullanıcı aynı satırı değiştiremez.
        // Böylece bitwise birleştirme her zaman en güncel değer üzerinden yapılır.
        await using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

        var current = await ReadRowAsync(connection, transaction, row, lockRow: true)
                      ?? throw new SettingsNotFoundException(row);

        var changes = computeChanges(current.Row); // hata fırlatırsa rollback olur
        if (changes.Count > 0)
            await ExecuteUpdateAsync(connection, transaction, current.Location, changes);

        await transaction.CommitAsync();
        return changes.Keys.Select(k => k.Key).ToList();
    }

    // ------------------------------------------------------------------ private

    private async Task<(StoredRow Row, byte[] Location)?> ReadRowAsync(
        SqlConnection connection, IDbTransaction? transaction, int row, bool lockRow)
    {
        var hint = lockRow ? "WITH (UPDLOCK, HOLDLOCK)" : "";
        var sql = $"""
            SELECT __loc, {ColumnList}
            FROM (SELECT ROW_NUMBER() OVER (ORDER BY %%physloc%%) AS __rn,
                         %%physloc%% AS __loc, *
                  FROM dbo.DealerSettings {hint}) t
            WHERE __rn = @row;
            """;
        var result = await connection.QuerySingleOrDefaultAsync(sql, new { row }, transaction);
        if (result is null) return null;

        var dict = (IDictionary<string, object?>)result;
        var location = (byte[])dict["__loc"]!;
        var values = dict.Where(kv => kv.Key != "__loc")
                         .ToDictionary(kv => kv.Key, kv => kv.Value is DBNull ? null : kv.Value,
                                       StringComparer.OrdinalIgnoreCase);
        return (new StoredRow(values), location);
    }

    private static async Task ExecuteUpdateAsync(SqlConnection connection, IDbTransaction transaction,
        byte[] location, IReadOnlyDictionary<SettingDefinition, object?> changes)
    {
        var parameters = new DynamicParameters();
        parameters.Add("loc", location, DbType.Binary, size: 8);

        var assignments = new List<string>();
        var i = 0;
        foreach (var (definition, value) in changes)
        {
            var name = $"p{i++}";
            assignments.Add($"[{definition.Key}] = @{name}");
            parameters.Add(name, value, DbTypeFor(definition.SqlType),
                size: definition.SqlType == "nvarchar" ? (definition.MaxLength < 0 ? -1 : definition.MaxLength) : null);
        }

        var sql = $"UPDATE dbo.DealerSettings SET {string.Join(", ", assignments)} WHERE %%physloc%% = @loc;";
        var affected = await connection.ExecuteAsync(sql, parameters, transaction);
        if (affected != 1)
            throw new InvalidOperationException($"Güncelleme 1 satır yerine {affected} satırı etkiledi; işlem geri alındı.");
    }

    private static DbType DbTypeFor(string sqlType) => sqlType switch
    {
        "bit" => DbType.Boolean,
        "tinyint" => DbType.Byte,
        "smallint" => DbType.Int16,
        "int" => DbType.Int32,
        "bigint" => DbType.Int64,
        "decimal" => DbType.Decimal,
        "datetime" => DbType.DateTime,
        _ => DbType.String
    };
}
