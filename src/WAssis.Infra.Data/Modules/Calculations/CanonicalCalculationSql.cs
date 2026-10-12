using System.Reflection;
using System.Data;
using System.Text.Json.Serialization;
using Dapper;

namespace WAssis.Infra.Data.Modules.Calculations;

// Identifiers come only from compiled DTO annotations, never from request values.
internal static class CanonicalCalculationSql
{
    private static readonly HashSet<string> Dates = ["vigencia_inicio", "vigencia_fim", "vigencia_fim_anterior", "condutor_data_nascimento", "validade", "primeiro_vencimento"];
    private static (PropertyInfo Property, string Column)[] Fields<T>() => typeof(T).GetProperties()
        .Select(p => (Property: p, Column: p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name))
        .Where(x => x.Column is not null).Select(x => (x.Property, x.Column!)).ToArray();

    public static string Select<T>(string alias) => string.Join(",", Fields<T>().Select(x =>
        $"{alias}.\"{x.Column}\"{(Dates.Contains(x.Column) ? "::text" : "")} AS \"{x.Property.Name}\""));

    public static (string Sql, DynamicParameters Parameters) Insert<T>(string table, T value)
    {
        var fields = Fields<T>();
        var parameters = new DynamicParameters();
        foreach (var field in fields)
        {
            var type = Nullable.GetUnderlyingType(field.Property.PropertyType) ?? field.Property.PropertyType;
            DbType? dbType = type == typeof(Guid) ? DbType.Guid : type == typeof(int) ? DbType.Int32
                : type == typeof(bool) ? DbType.Boolean : type == typeof(decimal) ? DbType.Decimal
                : type == typeof(string) ? DbType.String : type == typeof(DateTime) ? DbType.DateTime : null;
            parameters.Add(field.Property.Name, field.Property.GetValue(value), dbType);
        }
        var columns = string.Join(",", fields.Select(x => $"\"{x.Column}\""));
        var values = string.Join(",", fields.Select(x => Dates.Contains(x.Column)
            ? $"CAST(@{x.Property.Name} AS date)" : $"@{x.Property.Name}"));
        return ($"INSERT INTO erp.{table} ({columns}) VALUES ({values})", parameters);
    }

    public static (string Sql, DynamicParameters Parameters) Update<T>(string table, T value, IReadOnlySet<string> preservedColumns)
    {
        var parameters = Insert(table, value).Parameters;
        var assignments = Fields<T>().Where(x => !preservedColumns.Contains(x.Column)).Select(x =>
            $"\"{x.Column}\"=" + (Dates.Contains(x.Column) ? $"CAST(@{x.Property.Name} AS date)" : $"@{x.Property.Name}"));
        return ($"UPDATE erp.{table} SET {string.Join(",", assignments)} WHERE id=@Id", parameters);
    }
}
