using System.Text.Json;

namespace XboxHeadsetTray;

internal sealed class AppSettings
{
    public string DeviceNameContains { get; set; } = "Xbox Headphones";

    public static string SettingsPath => Path.Combine(
        AppContext.BaseDirectory,
        "settings.json");

    public static AppSettings LoadOrCreate()
    {
        if (!File.Exists(SettingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);
            AppSettings? settings =
                JsonSerializer.Deserialize<AppSettings>(json);

            if (settings is not null &&
                !string.IsNullOrWhiteSpace(settings.DeviceNameContains))
            {
                return settings;
            }
        }
        catch (JsonException)
        {
            // Use defaults if JSON is invalid.
        }
        catch (IOException)
        {
            // Use defaults if file cannot be read.
        }

        var fallback = new AppSettings();
        Save(fallback);
        return fallback;
    }

    public static void Save(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SettingsPath, json);
    }
}