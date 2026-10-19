using OracleTools2026.Data;

namespace OracleTools2026.UI;

public sealed record DatabaseToolDependencies(
    IOracleSession Session,
    DatabaseObjectRepository Repository,
    Func<LoginForm> LoginFormFactory,
    Func<DatabaseInfoForm> DatabaseInfoFormFactory);
