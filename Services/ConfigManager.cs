using System.IO;
using System.Text.Json;

namespace DeepSeekUsageTracker.Services;

public class AppConfig
{
    public string ApiKey { get; set; } = "";
    public int RefreshIntervalMinutes { get; set; } = 10;
    public string DisplayMode { get; set; } = "balance"; // "balance" or "percentage"
    public decimal ToppedUpAmount { get; set; } // USD, used in percentage mode
}

public static class ConfigManager
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "DeepSeekUsageTracker");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOpts) ?? new AppConfig();
            }
        }
        catch { }

        return new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(config, JsonOpts);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }
}
