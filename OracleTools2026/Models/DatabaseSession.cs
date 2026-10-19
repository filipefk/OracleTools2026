namespace OracleTools2026.Models;

public sealed record DatabaseSession(
    int InstanceId,
    long Sid,
    long SerialNumber,
    string UserName,
    string OsUser,
    string Machine,
    string Status,
    string Program,
    string Type,
    string Module,
    DateTime? LogonTime,
    string State);
