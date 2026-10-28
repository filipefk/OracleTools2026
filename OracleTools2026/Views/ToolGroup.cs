using OracleTools2026.Models;

namespace OracleTools2026.Views;

public sealed record ToolGroup(string Name, IReadOnlyList<ToolDefinition> Tools);
