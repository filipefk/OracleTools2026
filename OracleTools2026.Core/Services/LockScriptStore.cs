namespace OracleTools2026.Services;

public sealed class LockScriptStore
{
    public static readonly IReadOnlyList<string> FileNames = ["ScriptLock01.sql", "ScriptLock02.sql"];

    public bool Exists(int index) => AppFiles.Find(FileNames[index]) is not null;

    public async Task<string?> LoadAsync(int index, CancellationToken cancellationToken = default) =>
        AppFiles.Find(FileNames[index]) is { } path ? await AppFiles.ReadAllTextAsync(path, cancellationToken) : null;
}
