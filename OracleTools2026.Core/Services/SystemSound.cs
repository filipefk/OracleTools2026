using System.Runtime.InteropServices;

namespace OracleTools2026.Services;

public static class SystemSound
{
    private const uint SimpleBeep = 0xFFFFFFFF;

    public static void Beep() => MessageBeep(SimpleBeep);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MessageBeep(uint type);
}
