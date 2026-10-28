using Oracle.ManagedDataAccess.Client;
using OracleTools2026.Models;
using System.Text;

namespace OracleTools2026.Data;

// Gera o script de criação do usuário no mesmo formato do original: CREATE USER com senha a definir,
// cotas, privilégios de sistema, roles (com as roles default) e grants de objetos concedidos diretamente.
public sealed class UserScriptGenerator(IOracleSession session)
{
    private const string PasswordPlaceholder = "<SENHA_USUARIO>";
    private const string Separator = "--=======================================================================================================================";

    public async Task<IReadOnlyList<UserScriptResult>> GenerateManyAsync(
        IReadOnlyList<string> userNames,
        IProgress<ProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);

        var results = new List<UserScriptResult>(userNames.Count);
        for (var i = 0; i < userNames.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userName = userNames[i].Trim().ToUpperInvariant();
            progress?.Report(new ProgressInfo(i + 1, userNames.Count, $"Extraindo script do usuário {userName}"));
            results.Add(await GenerateAsync(connection, userName, cancellationToken));
        }

        return results;
    }

    public async Task<UserScriptResult> GenerateAsync(string userName, CancellationToken cancellationToken = default)
    {
        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        return await GenerateAsync(connection, userName.Trim().ToUpperInvariant(), cancellationToken);
    }

    private static async Task<UserScriptResult> GenerateAsync(OracleConnection connection, string userName, CancellationToken cancellationToken)
    {
        try
        {
            var script = await BuildScriptAsync(connection, userName, cancellationToken);
            return script is null
                ? UserScriptResult.Failure(userName, $"Usuário {userName} não encontrado")
                : new UserScriptResult(userName, script, null);
        }
        catch (OracleException ex)
        {
            return UserScriptResult.Failure(userName, $"Usuário {userName}: {ex.Message}");
        }
    }

    private static async Task<string?> BuildScriptAsync(OracleConnection connection, string userName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT ACCOUNT_STATUS, DEFAULT_TABLESPACE, TEMPORARY_TABLESPACE, PROFILE
            FROM DBA_USERS
            WHERE USERNAME = :userName
            """;

        string status, defaultTablespace, temporaryTablespace, profile;
        await using (var command = connection.CreateTextCommand(sql, ("userName", userName)))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            status = reader.GetStringOrEmpty(0);
            defaultTablespace = reader.GetStringOrEmpty(1);
            temporaryTablespace = reader.GetStringOrEmpty(2);
            profile = reader.GetStringOrEmpty(3);
        }

        var user = OracleExtensions.QuoteIfNeeded(userName);
        var script = new StringBuilder();
        script.Append("CREATE USER ").AppendLine(user);
        script.Append("IDENTIFIED BY ").AppendLine(PasswordPlaceholder);
        AppendClause(script, "DEFAULT TABLESPACE", defaultTablespace);
        AppendClause(script, "TEMPORARY TABLESPACE", temporaryTablespace);
        AppendClause(script, "PROFILE", profile);

        foreach (var quota in await DatabaseUserRepository.GetQuotasAsync(connection, userName, cancellationToken))
        {
            var size = quota.IsUnlimited ? "UNLIMITED" : quota.MaxBytes.ToString();
            script.AppendLine($"QUOTA {size} ON {OracleExtensions.QuoteIfNeeded(quota.Tablespace)}");
        }

        // LOCKED(TIMED) é o bloqueio temporário por tentativas de login e não é reproduzido.
        var statusParts = status.Split('&', StringSplitOptions.TrimEntries);
        if (statusParts.Contains("LOCKED"))
        {
            script.AppendLine("ACCOUNT LOCK");
        }

        if (statusParts.Contains("EXPIRED"))
        {
            script.AppendLine("PASSWORD EXPIRE");
        }

        script.AppendLine("/");

        foreach (var privilege in await DatabaseUserRepository.GetSystemPrivilegesAsync(connection, userName, cancellationToken))
        {
            AppendStatement(script, $"GRANT {privilege.Privilege} TO {user}{(privilege.AdminOption ? " WITH ADMIN OPTION" : "")}");
        }

        var roles = await DatabaseUserRepository.GetRolesAsync(connection, userName, cancellationToken);
        foreach (var role in roles)
        {
            AppendStatement(script, $"GRANT {OracleExtensions.QuoteIfNeeded(role.Role)} TO {user}{(role.AdminOption ? " WITH ADMIN OPTION" : "")}");
        }

        // Com pelo menos uma role não default, tira o default de todas e define só as que são default.
        if (roles.Any(r => !r.IsDefault))
        {
            AppendStatement(script, $"ALTER USER {user} DEFAULT ROLE NONE");

            var defaultRoles = roles.Where(r => r.IsDefault).Select(r => OracleExtensions.QuoteIfNeeded(r.Role)).ToList();
            if (defaultRoles.Count > 0)
            {
                AppendStatement(script, $"ALTER USER {user} DEFAULT ROLE {string.Join(", ", defaultRoles)}");
            }
        }

        var objectGrants = await DatabaseUserRepository.GetObjectPrivilegesAsync(connection, userName, cancellationToken);
        if (objectGrants.Count > 0)
        {
            script.AppendLine();
            script.AppendLine(Separator);
            script.AppendLine();
            script.AppendLine("-- DAQUI PARA BAIXO SÃO GRANTS DE TABELAS OU SIMILARES DEFINIDOS DIRETO AO USUÁRIO AO INVÉS DE SEREM DEFINIDOS PARA ROLES");
            script.AppendLine();
            script.AppendLine(Separator);

            foreach (var grant in objectGrants.Where(g => g.Column is null))
            {
                AppendStatement(script, $"GRANT {grant.Privilege} ON {FormatObject(grant)} TO {user}{GrantOption(grant.Grantable)}");
            }

            // Privilégios em colunas: um GRANT por privilégio, com todas as colunas.
            var columnGrants = objectGrants
                .Where(g => g.Column is not null)
                .GroupBy(g => (g.Owner, g.ObjectName, g.ObjectType, g.Privilege, g.Grantable));
            foreach (var group in columnGrants)
            {
                var columns = string.Join(", ", group.Select(g => OracleExtensions.QuoteIfNeeded(g.Column!)));
                AppendStatement(script, $"GRANT {group.Key.Privilege} ({columns}) ON {FormatObject(group.First())} TO {user}{GrantOption(group.Key.Grantable)}");
            }
        }

        return script.ToString();
    }

    // Diretórios não têm owner na sintaxe do GRANT (o original gerava "DIRECTORY SYS.NOME", que é inválido).
    private static string FormatObject(ObjectPrivilegeGrant grant) =>
        grant.IsDirectory
            ? $"DIRECTORY {OracleExtensions.QuoteIfNeeded(grant.ObjectName)}"
            : OracleExtensions.QuoteIfNeeded(grant.Owner, grant.ObjectName);

    // O original gerava WITH ADMIN OPTION para grants de objetos, que só aceitam WITH GRANT OPTION.
    private static string GrantOption(bool grantable) => grantable ? " WITH GRANT OPTION" : "";

    private static void AppendClause(StringBuilder script, string clause, string value)
    {
        if (value.Length > 0)
        {
            script.AppendLine($"{clause} {OracleExtensions.QuoteIfNeeded(value)}");
        }
    }

    private static void AppendStatement(StringBuilder script, string statement) =>
        script.AppendLine(statement).AppendLine("/");
}
