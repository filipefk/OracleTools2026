using Oracle.ManagedDataAccess.Client;

namespace OracleTools2026.Data;

internal static class OracleExtensions
{
    public static OracleCommand CreateTextCommand(this OracleConnection connection, string sql, params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.BindByName = true;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.Add(new OracleParameter(name, value ?? DBNull.Value));
        }

        return command;
    }

    public static string GetStringOrEmpty(this OracleDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);

    public static DateTime? GetDateTimeOrNull(this OracleDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);

    public static long GetInt64OrZero(this OracleDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? 0 : (long)reader.GetDecimal(ordinal);

    public static string GetNumberText(this OracleDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? "" : reader.GetOracleDecimal(ordinal).ToString();

    public static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

    public static string Quote(string owner, string name) => $"{Quote(owner)}.{Quote(name)}";
}
