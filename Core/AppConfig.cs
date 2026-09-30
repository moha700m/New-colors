using System.Text.Json.Serialization;

namespace MohammedLab.ColorVision.Core;

public enum GameMode { Bo7, Bo6, Mw, Mw4, Overwatch }
public enum AimDevice { Mouse, Controller }
public enum PadButton { None, Cross, Circle, Square, Triangle, L1, R1, L2, R2, L3, R3, Create, Options, DpadUp, DpadDown, DpadLeft, DpadRight }

public sealed class AppConfig
{
    public GameMode Game { get; set; } = GameMode.Bo7;
    public GameMode GuideGame { get; set; } = GameMode.Bo7;
    public int GuideStep { get; set; }
    public int ScreenIndex { get; set; }
    public int ZoneWidth { get; set; } = 400;
    public int ZoneHeight { get; set; } = 560;
    public int CaptureFps { get; set; } = 90;
    public bool NoPreview { get; set; } = true;
    public bool ShowHud { get; set; } = true;
    public AimDevice Device { get; set; } = AimDevice.Mouse;
    public float Strength { get; set; } = 2.8f;
    public int AimPointOffsetPx { get; set; } = 31;
    public bool HoldToAim { get; set; } = true;
    public bool AlwaysTrack { get; set; } = true;
    public int AimKey { get; set; } = 0x02;
    public PadButton AimButton { get; set; } = PadButton.L2;
    public PadButton FireButton { get; set; } = PadButton.R2;
    public bool AntiRecoilOn { get; set; }
    public float AntiRecoil { get; set; } = 6.0f;
    public bool AutoFire { get; set; }

    [JsonIgnore] public static int[] ZoneWidths { get; } = [160, 240, 320, 400, 480, 560, 640, 800];
    [JsonIgnore] public static int[] ZoneHeights { get; } = [160, 240, 320, 400, 480, 560, 640, 800];
    [JsonIgnore] public static int[] CaptureFpsOptions { get; } = [30, 60, 90, 120, 144, 165, 240];
}
