using NAudio.CoreAudioApi;

namespace XboxHeadsetTray;

internal sealed record PlaybackDeviceInfo(string Name, string State);

internal static class HeadsetMonitor
{
    public static bool IsHeadsetConnected(string nameContains)
    {
        if (string.IsNullOrWhiteSpace(nameContains))
            nameContains = "Xbox Headphones";

        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);

        bool found = false;
        foreach (MMDevice device in devices)
        {
            try
            {
                string friendlyName;
                DeviceState state;
                try
                {
                    friendlyName = device.FriendlyName;
                    state = device.State;
                }
                catch
                {
                    continue;
                }

                if (friendlyName.Contains(nameContains, StringComparison.OrdinalIgnoreCase)
                    && (state & DeviceState.Active) == DeviceState.Active)
                {
                    found = true;
                }
            }
            finally
            {
                device.Dispose();
            }
        }

        return found;
    }

    public static List<PlaybackDeviceInfo> GetPlaybackDevices()
    {
        var result = new List<PlaybackDeviceInfo>();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);

        foreach (MMDevice device in devices)
        {
            try
            {
                string name;
                string state;

                try
                {
                    name = device.FriendlyName;
                }
                catch (Exception ex)
                {
                    name = $"(Name unavailable: {ex.Message})";
                }

                try
                {
                    state = device.State.ToString();
                }
                catch (Exception ex)
                {
                    state = $"Unknown ({ex.Message})";
                }

                result.Add(new PlaybackDeviceInfo(name, state));
            }
            finally
            {
                device.Dispose();
            }
        }

        return result;
    }
}
