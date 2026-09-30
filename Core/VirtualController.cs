using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MohammedLab.ColorVision.Core;

public sealed class VirtualController : IDisposable
{
    private ViGEmClient? _client;
    private IXbox360Controller? _controller;
    public bool Connected => _controller is not null;
    public string Status { get; private set; } = "Disconnected";
    public bool Connect()
    {
        if (Connected) return true;
        try { _client = new ViGEmClient(); _controller = _client.CreateXbox360Controller(); _controller.Connect(); Status = "Connected"; return true; }
        catch (Exception ex) { Status = "ViGEm unavailable: " + ex.Message; Dispose(); return false; }
    }
    public void Disconnect() { try { _controller?.Disconnect(); } catch { } _controller = null; _client?.Dispose(); _client = null; if (!Status.StartsWith("ViGEm")) Status = "Disconnected"; }
    public void Submit(XInput.Gamepad s, double ax, double ay, bool autoFire)
    {
        if (_controller is null) return;
        SetAxis(Xbox360Axis.LeftThumbX, s.sThumbLX); SetAxis(Xbox360Axis.LeftThumbY, s.sThumbLY);
        var rx = Math.Clamp(s.sThumbRX / 32767.0 + ax, -1, 1); var ry = Math.Clamp(s.sThumbRY / 32767.0 + ay, -1, 1);
        SetAxis(Xbox360Axis.RightThumbX, (short)(rx * 32767)); SetAxis(Xbox360Axis.RightThumbY, (short)(ry * 32767));
        SetSlider(Xbox360Slider.LeftTrigger, s.bLeftTrigger); SetSlider(Xbox360Slider.RightTrigger, autoFire ? (byte)255 : s.bRightTrigger);
        SetBtn(Xbox360Button.Up, s.wButtons.HasFlag(XInput.Buttons.DPadUp)); SetBtn(Xbox360Button.Down, s.wButtons.HasFlag(XInput.Buttons.DPadDown));
        SetBtn(Xbox360Button.Left, s.wButtons.HasFlag(XInput.Buttons.DPadLeft)); SetBtn(Xbox360Button.Right, s.wButtons.HasFlag(XInput.Buttons.DPadRight));
        SetBtn(Xbox360Button.Start, s.wButtons.HasFlag(XInput.Buttons.Start)); SetBtn(Xbox360Button.Back, s.wButtons.HasFlag(XInput.Buttons.Back));
        SetBtn(Xbox360Button.LeftThumb, s.wButtons.HasFlag(XInput.Buttons.LeftThumb)); SetBtn(Xbox360Button.RightThumb, s.wButtons.HasFlag(XInput.Buttons.RightThumb));
        SetBtn(Xbox360Button.LeftShoulder, s.wButtons.HasFlag(XInput.Buttons.LeftShoulder)); SetBtn(Xbox360Button.RightShoulder, s.wButtons.HasFlag(XInput.Buttons.RightShoulder));
        SetBtn(Xbox360Button.A, s.wButtons.HasFlag(XInput.Buttons.A)); SetBtn(Xbox360Button.B, s.wButtons.HasFlag(XInput.Buttons.B));
        SetBtn(Xbox360Button.X, s.wButtons.HasFlag(XInput.Buttons.X)); SetBtn(Xbox360Button.Y, s.wButtons.HasFlag(XInput.Buttons.Y));
    }
    public void Replug() { Disconnect(); Thread.Sleep(300); Connect(); }
    private void SetAxis(Xbox360Axis a, short v) => _controller?.SetAxisValue(a, v);
    private void SetSlider(Xbox360Slider s, byte v) => _controller?.SetSliderValue(s, v);
    private void SetBtn(Xbox360Button b, bool v) => _controller?.SetButtonState(b, v);
    public void Dispose() => Disconnect();
}
