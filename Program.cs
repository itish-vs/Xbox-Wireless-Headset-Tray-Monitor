using Microsoft.Win32;
namespace XboxHeadsetTray;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
	EnableStartup();
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
    private static void EnableStartup()
	{
    const string runKey =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    const string appName = "XboxHeadsetTray";

    string? exePath = Environment.ProcessPath;

    if (string.IsNullOrWhiteSpace(exePath))
        return;

    using RegistryKey? key =
        Registry.CurrentUser.CreateSubKey(runKey);

    key?.SetValue(appName, $"\"{exePath}\"");
}
}
