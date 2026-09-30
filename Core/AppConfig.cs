using System.Text.Json.Serialization;
namespace MohammedLab.ColorVision.Core;
public sealed class AppConfig
{
    public string ProfileName { get; set; } = "Default";
    public int CaptureWidth { get; set; } = 420;
    public int CaptureHeight { get; set; } = 320;
    public int CaptureFps { get; set; } = 120;
    public int Hue { get; set; } = 155;
    public int HueTolerance { get; set; } = 12;
    public int SaturationMin { get; set; } = 130;
    public int ValueMin { get; set; } = 130;
    public int MinBlobArea { get; set; } = 12;
    public int MaxBlobArea { get; set; } = 12000;
    public bool StickyTarget { get; set; } = true;
    public int StickyMs { get; set; } = 160;
    public int TargetYOffsetPx { get; set; } = 0;
    public int ControllerIndex { get; set; } = 0;
    [JsonIgnore] public string DisplayColor => $"HSV H={Hue} ±{HueTolerance}";
}
