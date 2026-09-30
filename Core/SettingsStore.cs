using System.Text.Json;

namespace MohammedLab.ColorVision.Core;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public string Folder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MohammedLab", "ColorVision");
    public string FilePath => Path.Combine(Folder, "settings.json");

    public AppConfig Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new AppConfig();
            var value = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(FilePath), JsonOptions) ?? new AppConfig();
            Clamp(value);
            return value;
        }
        catch { return new AppConfig(); }
    }

    public void Save(AppConfig value)
    {
        Clamp(value);
        Directory.CreateDirectory(Folder);
        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(value, JsonOptions));
        File.Move(tmp, FilePath, true);
    }

    public static void Clamp(AppConfig s)
    {
        s.ZoneWidth = Nearest(s.ZoneWidth, AppConfig.ZoneWidths);
        s.ZoneHeight = Nearest(s.ZoneHeight, AppConfig.ZoneHeights);
        s.CaptureFps = Nearest(s.CaptureFps, AppConfig.CaptureFpsOptions);
        s.ScreenIndex = Math.Max(0, s.ScreenIndex);
        s.Strength = Math.Clamp(s.Strength, 1f, 10f);
        s.AimPointOffsetPx = Math.Clamp(s.AimPointOffsetPx, -100, 100);
        s.AntiRecoil = Math.Clamp(s.AntiRecoil, 0f, 20f);
    }

    private static int Nearest(int value, IReadOnlyList<int> options) => options.OrderBy(x => Math.Abs(x - value)).First();
}
