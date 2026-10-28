using OracleTools2026.Data;
using OracleTools2026.Presentation;

namespace OracleTools2026.ViewModels;

public sealed record ToolServices(
    IOracleSession Session,
    DatabaseObjectRepository Repository,
    IClipboardService Clipboard,
    IToolNavigator Navigator);
