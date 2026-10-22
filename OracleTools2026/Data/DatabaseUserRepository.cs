using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Text;

namespace OracleTools2026.Data;

// Usuários, profiles, cotas, privilégios e roles. As consultas usam as views DBA_* (como o original já fazia
// para cotas e privilégios), pois ALL_USERS não tem profile/status e ALL_TAB_PRIVS só mostra parte dos grants.
public sealed class DatabaseUserRepository(IOracleSession session)
{
    // Tipos de outros namespaces que podem ter o mesmo nome do objeto do grant.
    private const string ObjectTypeSql = """
        (SELECT MIN(O.OBJECT_TYPE)
         FROM DBA_OBJECTS O
         WHERE O.OWNER = G.OWNER
           AND O.OBJECT_NAME = G.TABLE_NAME
           AND O.SUBOBJECT_NAME IS NULL
           AND O.OBJECT_TYPE NOT IN ('PACKAGE BODY', 'TYPE BODY', 'INDEX', 'TRIGGER', 'LOB', 'TABLE PARTITION',
                                     'TABLE SUBPARTITION', 'INDEX PARTITION', 'INDEX SUBPARTITION', 'LOB PARTITION'))
        """;

    public async Task<IReadOnlyList<string>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand("SELECT DISTINCT PROFILE FROM DBA_PROFILES ORDER BY PROFILE");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var profiles = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            profiles.Add(reader.GetString(0));
        }

        return profiles;
    }

    public async Task<IReadOnlyList<DatabaseUser>> SearchAsync(UserSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        var lastLogin = await GetLastLoginExpressionAsync(connection, cancellationToken);

        // O alias ALL_USERS mantém compatíveis os filtros livres salvos no original (ex.: ALL_USERS.PROFILE = 'X').
        var sql = new StringBuilder($"""
            SELECT ALL_USERS.USERNAME,
                   ALL_USERS.CREATED,
                   ALL_USERS.PROFILE,
                   ALL_USERS.ACCOUNT_STATUS,
                   ALL_USERS.LOCK_DATE,
                   ALL_USERS.EXPIRY_DATE,
                   {lastLogin} AS ULTIMO_LOGIN
            FROM DBA_USERS ALL_USERS
            WHERE 1 = 1
            """);

        var parameters = new List<(string, object?)>();

        switch (criteria.OwnerFilter)
        {
            case UserOwnerFilter.Owners:
                sql.AppendLine().Append("  AND EXISTS (SELECT 1 FROM DBA_OBJECTS O WHERE O.OWNER = ALL_USERS.USERNAME)");
                break;
            case UserOwnerFilter.NonOwners:
                sql.AppendLine().Append("  AND NOT EXISTS (SELECT 1 FROM DBA_OBJECTS O WHERE O.OWNER = ALL_USERS.USERNAME)");
                break;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Profile))
        {
            sql.AppendLine().Append("  AND ALL_USERS.PROFILE = :profile");
            parameters.Add(("profile", criteria.Profile.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            sql.AppendLine().Append("  AND ALL_USERS.USERNAME LIKE '%' || :name || '%'");
            parameters.Add(("name", criteria.Name.Trim().ToUpperInvariant()));
        }

        // Filtro SQL livre digitado pelo próprio usuário da ferramenta (funcionalidade do original).
        if (!string.IsNullOrWhiteSpace(criteria.CustomFilter))
        {
            sql.AppendLine().Append($"  AND ({criteria.CustomFilter.Trim()})");
        }

        sql.AppendLine().Append("ORDER BY ALL_USERS.USERNAME");

        await using var command = connection.CreateTextCommand(sql.ToString(), [.. parameters]);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var users = new List<DatabaseUser>();
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new DatabaseUser(
                reader.GetString(0),
                reader.GetDateTimeOrNull(1),
                reader.GetStringOrEmpty(2),
                reader.GetStringOrEmpty(3),
                reader.GetDateTimeOrNull(4),
                reader.GetDateTimeOrNull(5),
                reader.GetDateTimeOrNull(6)));
        }

        return users;
    }

    // Pesquisa por padrão (aceita % e *), usada para localizar um usuário no banco.
    public async Task<IReadOnlyList<DatabaseUser>> FindAsync(string pattern, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT USERNAME, CREATED, PROFILE, ACCOUNT_STATUS, LOCK_DATE, EXPIRY_DATE
            FROM DBA_USERS
            WHERE UPPER(USERNAME) LIKE UPPER(:pattern)
            ORDER BY USERNAME
            """;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(sql, ("pattern", ToLikePattern(pattern)));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var users = new List<DatabaseUser>();
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new DatabaseUser(
                reader.GetString(0),
                reader.GetDateTimeOrNull(1),
                reader.GetStringOrEmpty(2),
                reader.GetStringOrEmpty(3),
                reader.GetDateTimeOrNull(4),
                reader.GetDateTimeOrNull(5),
                null));
        }

        return users;
    }

    public async Task<UserDetails?> GetDetailsAsync(string userName, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        var lastLogin = await GetLastLoginExpressionAsync(connection, cancellationToken);

        var sql = $"""
            SELECT ALL_USERS.ACCOUNT_STATUS,
                   ALL_USERS.DEFAULT_TABLESPACE,
                   ALL_USERS.TEMPORARY_TABLESPACE,
                   ALL_USERS.PROFILE,
                   ALL_USERS.CREATED,
                   ALL_USERS.LOCK_DATE,
                   ALL_USERS.EXPIRY_DATE,
                   {lastLogin} AS ULTIMO_LOGIN
            FROM DBA_USERS ALL_USERS
            WHERE ALL_USERS.USERNAME = :userName
            """;

        await using var command = connection.CreateTextCommand(sql, ("userName", userName));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserDetails(
            userName,
            reader.GetStringOrEmpty(0),
            reader.GetStringOrEmpty(1),
            reader.GetStringOrEmpty(2),
            reader.GetStringOrEmpty(3),
            reader.GetDateTimeOrNull(4),
            reader.GetDateTimeOrNull(5),
            reader.GetDateTimeOrNull(6),
            reader.GetDateTimeOrNull(7));
    }

    public async Task<IReadOnlyList<TablespaceQuota>> GetQuotasAsync(string userName, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetQuotasAsync(connection, userName, cancellationToken);
    }

    internal static async Task<IReadOnlyList<TablespaceQuota>> GetQuotasAsync(OracleConnection connection, string userName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TABLESPACE_NAME, MAX_BYTES, BYTES
            FROM DBA_TS_QUOTAS
            WHERE USERNAME = :userName
              AND DROPPED = 'NO'
            ORDER BY TABLESPACE_NAME
            """;

        await using var command = connection.CreateTextCommand(sql, ("userName", userName));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var quotas = new List<TablespaceQuota>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var maxBytes = reader.IsDBNull(1) ? -1 : (long)reader.GetDecimal(1);
            quotas.Add(new TablespaceQuota(reader.GetString(0), maxBytes < 0 ? null : maxBytes, reader.GetInt64OrZero(2)));
        }

        return quotas;
    }

    public async Task<IReadOnlyList<SystemPrivilegeGrant>> GetSystemPrivilegesAsync(string grantee, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetSystemPrivilegesAsync(connection, grantee, cancellationToken);
    }

    internal static async Task<IReadOnlyList<SystemPrivilegeGrant>> GetSystemPrivilegesAsync(OracleConnection connection, string grantee, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT PRIVILEGE, ADMIN_OPTION
            FROM DBA_SYS_PRIVS
            WHERE GRANTEE = :grantee
            ORDER BY PRIVILEGE
            """;

        await using var command = connection.CreateTextCommand(sql, ("grantee", grantee));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var privileges = new List<SystemPrivilegeGrant>();
        while (await reader.ReadAsync(cancellationToken))
        {
            privileges.Add(new SystemPrivilegeGrant(reader.GetString(0), reader.GetStringOrEmpty(1) == "YES"));
        }

        return privileges;
    }

    public async Task<IReadOnlyList<RoleGrant>> GetRolesAsync(string grantee, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetRolesAsync(connection, grantee, cancellationToken);
    }

    internal static async Task<IReadOnlyList<RoleGrant>> GetRolesAsync(OracleConnection connection, string grantee, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT GRANTED_ROLE, ADMIN_OPTION, DEFAULT_ROLE
            FROM DBA_ROLE_PRIVS
            WHERE GRANTEE = :grantee
            ORDER BY GRANTED_ROLE
            """;

        await using var command = connection.CreateTextCommand(sql, ("grantee", grantee));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var roles = new List<RoleGrant>();
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(new RoleGrant(reader.GetString(0), reader.GetStringOrEmpty(1) == "YES", reader.GetStringOrEmpty(2) == "YES"));
        }

        return roles;
    }

    // Grants de objetos (inclusive em colunas) concedidos diretamente ao usuário ou à role.
    public async Task<IReadOnlyList<ObjectPrivilegeGrant>> GetObjectPrivilegesAsync(string grantee, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GetObjectPrivilegesAsync(connection, grantee, cancellationToken);
    }

    internal static async Task<IReadOnlyList<ObjectPrivilegeGrant>> GetObjectPrivilegesAsync(OracleConnection connection, string grantee, CancellationToken cancellationToken)
    {
        // Agrupa para não repetir o mesmo privilégio concedido por grantors diferentes.
        const string sql = $"""
            SELECT G.OWNER,
                   G.TABLE_NAME,
                   G.PRIVILEGE,
                   G.GRANTABLE,
                   G.COLUMN_NAME,
                   {ObjectTypeSql} AS OBJECT_TYPE
            FROM (
                SELECT P.OWNER, P.TABLE_NAME, P.PRIVILEGE, MAX(P.GRANTABLE) AS GRANTABLE, CAST(NULL AS VARCHAR2(128)) AS COLUMN_NAME
                FROM DBA_TAB_PRIVS P
                WHERE P.GRANTEE = :grantee
                GROUP BY P.OWNER, P.TABLE_NAME, P.PRIVILEGE
                UNION ALL
                SELECT C.OWNER, C.TABLE_NAME, C.PRIVILEGE, MAX(C.GRANTABLE), C.COLUMN_NAME
                FROM DBA_COL_PRIVS C
                WHERE C.GRANTEE = :grantee
                GROUP BY C.OWNER, C.TABLE_NAME, C.PRIVILEGE, C.COLUMN_NAME
            ) G
            ORDER BY OBJECT_TYPE, G.OWNER, G.TABLE_NAME, G.COLUMN_NAME NULLS FIRST, G.PRIVILEGE
            """;

        await using var command = connection.CreateTextCommand(sql, ("grantee", grantee));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var grants = new List<ObjectPrivilegeGrant>();
        while (await reader.ReadAsync(cancellationToken))
        {
            grants.Add(new ObjectPrivilegeGrant(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetStringOrEmpty(5),
                reader.GetString(2),
                reader.GetStringOrEmpty(3) == "YES",
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return grants;
    }

    public async Task<IReadOnlyList<ProfileLimit>> GetProfileLimitsAsync(string profile, CancellationToken cancellationToken = default)
    {
        // Limites DEFAULT herdam o valor do profile DEFAULT; mostra o valor efetivo.
        const string sql = """
            SELECT P.RESOURCE_TYPE,
                   P.RESOURCE_NAME,
                   CASE WHEN P.LIMIT = 'DEFAULT' AND P.PROFILE <> 'DEFAULT' THEN D.LIMIT ELSE P.LIMIT END,
                   CASE WHEN P.LIMIT = 'DEFAULT' AND P.PROFILE <> 'DEFAULT' THEN 1 ELSE 0 END
            FROM DBA_PROFILES P
                LEFT JOIN DBA_PROFILES D
                    ON D.PROFILE = 'DEFAULT'
                   AND D.RESOURCE_NAME = P.RESOURCE_NAME
            WHERE P.PROFILE = :profile
            ORDER BY P.RESOURCE_TYPE, P.RESOURCE_NAME
            """;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(sql, ("profile", profile));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var limits = new List<ProfileLimit>();
        while (await reader.ReadAsync(cancellationToken))
        {
            limits.Add(new ProfileLimit(
                reader.GetStringOrEmpty(0),
                reader.GetStringOrEmpty(1),
                reader.GetStringOrEmpty(2),
                reader.GetInt64OrZero(3) == 1));
        }

        return limits;
    }

    // Último login: DBA_USERS.LAST_LOGIN só existe a partir do Oracle 12c; em versões anteriores fica vazio.
    private static async Task<string> GetLastLoginExpressionAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM ALL_TAB_COLUMNS
            WHERE OWNER = 'SYS'
              AND TABLE_NAME = 'DBA_USERS'
              AND COLUMN_NAME = 'LAST_LOGIN'
            """;

        await using var command = connection.CreateTextCommand(sql);
        var hasLastLogin = Convert.ToDecimal(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        return hasLastLogin ? "CAST(ALL_USERS.LAST_LOGIN AS DATE)" : "CAST(NULL AS DATE)";
    }

    private static string ToLikePattern(string pattern) => pattern.Trim().Replace('*', '%');
}
