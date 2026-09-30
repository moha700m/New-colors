using System.Text.Json;

namespace MohammedLab.ColorVision.Core;

public sealed class ProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string Root { get; }

    public ProfileStore()
    {
        Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MohammedLab", "ColorVision", "Profiles");
        Directory.CreateDirectory(Root);
    }

    public IReadOnlyList<string> ListProfiles() =>
        Directory.GetFiles(Root, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public AppConfig Load(string name)
    {
        var path = Path.Combine(Root, Sanitize(name) + ".json");
        if (!File.Exists(path))
        {
            var cfg = new AppConfig { ProfileName = name };
            Save(cfg);
            return cfg;
        }

        var json = File.ReadAllText(path);
        var loaded = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        loaded.ProfileName = name;
        return loaded;
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(Root);
        var path = Path.Combine(Root, Sanitize(config.ProfileName) + ".json");
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(config, JsonOptions));
        File.Move(tmp, path, true);
    }

    public string Export(AppConfig config, string destination)
    {
        File.WriteAllText(destination, JsonSerializer.Serialize(config, JsonOptions));
        return destination;
    }

    public AppConfig Import(string source)
    {
        var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(source), JsonOptions)
                  ?? throw new InvalidDataException("Invalid profile file.");
        if (string.IsNullOrWhiteSpace(cfg.ProfileName))
            cfg.ProfileName = Path.GetFileNameWithoutExtension(source);
        Save(cfg);
        return cfg;
    }

    private static string Sanitize(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        return string.IsNullOrWhiteSpace(value) ? "Default" : value.Trim();
    }
}
