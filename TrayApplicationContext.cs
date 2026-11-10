using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;

namespace XboxHeadsetTray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _statusMenuItem;
    private readonly System.Windows.Forms.Timer _pollTimer;
    private readonly AppSettings _settings;
    private Icon? _currentIcon;
    private bool? _lastConnectedState;
    private bool _isDisposed;

    public TrayApplicationContext()
    {
        _settings = AppSettings.LoadOrCreate();
        _contextMenu = new ContextMenuStrip();

        _statusMenuItem = new ToolStripMenuItem("Checking Xbox Wireless Headset...")
        {
            Enabled = false
        };

        var checkNowMenuItem = new ToolStripMenuItem("Check now", null, (_, _) => CheckHeadset());
        var listDevicesMenuItem = new ToolStripMenuItem("Show playback devices", null, (_, _) => ShowPlaybackDevices());
        var openSettingsMenuItem = new ToolStripMenuItem("Open settings file", null, (_, _) => OpenSettingsFile());
        var exitMenuItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitThread());

        _contextMenu.Items.Add(_statusMenuItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(checkNowMenuItem);
        _contextMenu.Items.Add(listDevicesMenuItem);
        _contextMenu.Items.Add(openSettingsMenuItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(exitMenuItem);

        _currentIcon = CreateStatusIcon(Color.Gray);
        _notifyIcon = new NotifyIcon
        {
            Icon = _currentIcon,
            Text = "Xbox Headset: Checking...",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => CheckHeadset();

        _pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _pollTimer.Tick += (_, _) => CheckHeadset();
        _pollTimer.Start();

        CheckHeadset();
    }

    private void CheckHeadset()
    {
        if (_isDisposed)
            return;

        try
        {
            bool connected = HeadsetMonitor.IsHeadsetConnected(_settings.DeviceNameContains);
            SetStatus(connected);
        }
        catch (Exception ex)
        {
            SetUnavailable(ex.Message);
        }
    }

    private void SetStatus(bool connected)
    {
        if (_lastConnectedState == connected)
            return;

        _lastConnectedState = connected;
        string status = connected ? "Connected" : "Disconnected";
        string message = $"Xbox Wireless Headset: {status}";
        _statusMenuItem.Text = message;
        _notifyIcon.Text = connected ? "Xbox Headset: Connected" : "Xbox Headset: Disconnected";
        ReplaceIcon(connected ? Color.LimeGreen : Color.Firebrick);
    }

    private void SetUnavailable(string reason)
    {
        _lastConnectedState = null;
        _statusMenuItem.Text = $"Status unavailable: {reason}";
        _notifyIcon.Text = "Xbox Headset: Status unavailable";
        ReplaceIcon(Color.Firebrick);
    }

    private void ShowPlaybackDevices()
    {
        try
        {
            List<PlaybackDeviceInfo> devices = HeadsetMonitor.GetPlaybackDevices();
            var text = new StringBuilder();
            text.AppendLine($"Name filter: {_settings.DeviceNameContains}");
            text.AppendLine($"Settings: {AppSettings.SettingsPath}");
            text.AppendLine();

            if (devices.Count == 0)
            {
                text.AppendLine("Windows returned no playback endpoints.");
            }
            else
            {
                foreach (PlaybackDeviceInfo device in devices)
                    text.AppendLine($"[{device.State}] {device.Name}");
            }

            text.AppendLine();
            text.Append("If headset name differs, edit DeviceNameContains in settings.json, then restart this app.");
            MessageBox.Show(text.ToString(), "Windows playback devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not enumerate playback devices.\n\n{ex.Message}",
                "Xbox Headset Tray", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenSettingsFile()
    {
        try
        {
            if (!File.Exists(AppSettings.SettingsPath))
                AppSettings.Save(new AppSettings());

            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{AppSettings.SettingsPath}\"")
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open settings file.\n\n{ex.Message}",
                "Xbox Headset Tray", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReplaceIcon(Color color)
    {
        Icon nextIcon = CreateStatusIcon(color);
        Icon? previousIcon = _currentIcon;
        _currentIcon = nextIcon;
        _notifyIcon.Icon = nextIcon;
        previousIcon?.Dispose();
    }

    private static Icon CreateStatusIcon(Color color)
    {
        using var bitmap = new Bitmap(32, 32);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var shadowBrush = new SolidBrush(Color.FromArgb(45, Color.Black));
            graphics.FillEllipse(shadowBrush, 3, 4, 27, 27);

            using var fillBrush = new SolidBrush(color);
            graphics.FillEllipse(fillBrush, 2, 2, 27, 27);

            using var outlinePen = new Pen(Color.White, 2.2f);
            graphics.DrawEllipse(outlinePen, 3, 3, 25, 25);
        }

        IntPtr iconHandle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(iconHandle).Clone();
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    protected override void ExitThreadCore()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _pollTimer.Stop();
            _pollTimer.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
            _currentIcon?.Dispose();
            _currentIcon = null;
        }

        base.ExitThreadCore();
    }

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
