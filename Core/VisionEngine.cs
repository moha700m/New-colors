using System.Diagnostics;
using MohammedLab.ColorVision.Models;
namespace MohammedLab.ColorVision.Core;
public sealed class VisionEngine : IDisposable
{
    private readonly ScreenCapture _capture = new();
    private readonly ColorDetector _detector = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private long _frames, _lastFpsFrames, _lastFpsTicks;
    private double _fps;
    public AppConfig Config { get; private set; }
    public bool Running => _loop is { IsCompleted: false };
    public double CaptureFps => _fps;
    public DetectionResult LastDetection { get; private set; }
    public XInput.State LastControllerState { get; private set; }
    public event Action? TelemetryUpdated;
    public event Action<string>? Faulted;
    public VisionEngine(AppConfig config) => Config = config;
    public void ApplyConfig(AppConfig config) => Config = config;
    public bool Start()
    {
        if (Running) return true;
        _cts = new CancellationTokenSource();
        _lastFpsTicks = Stopwatch.GetTimestamp();
        _lastFpsFrames = _frames;
        _loop = Task.Run(() => RunLoop(_cts.Token));
        return true;
    }
    public async Task StopAsync()
    {
        if (_cts is null) return;
        _cts.Cancel();
        try { if (_loop is not null) await _loop.ConfigureAwait(false); } catch (OperationCanceledException) { }
        _cts.Dispose(); _cts = null; _loop = null;
    }
    private async Task RunLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var frameStart = Stopwatch.GetTimestamp();
                var cfg = Config;
                var bitmap = _capture.CaptureCenter(cfg.CaptureWidth, cfg.CaptureHeight);
                LastDetection = _detector.Detect(bitmap, cfg);
                XInput.TryGetState(cfg.ControllerIndex, out var state);
                LastControllerState = state;
                _frames++;
                UpdateFps();
                if ((_frames & 3) == 0) TelemetryUpdated?.Invoke();
                var targetMs = 1000.0 / Math.Clamp(cfg.CaptureFps, 30, 240);
                var elapsedMs = (Stopwatch.GetTimestamp() - frameStart) * 1000.0 / Stopwatch.Frequency;
                var delay = targetMs - elapsedMs;
                if (delay > 1) await Task.Delay(TimeSpan.FromMilliseconds(delay), token).ConfigureAwait(false);
                else await Task.Yield();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Faulted?.Invoke(ex.ToString()); }
    }
    private void UpdateFps()
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = (now - _lastFpsTicks) / (double)Stopwatch.Frequency;
        if (elapsed < .75) return;
        _fps = (_frames - _lastFpsFrames) / elapsed;
        _lastFpsFrames = _frames; _lastFpsTicks = now;
    }
    public void Dispose()
    {
        try { StopAsync().GetAwaiter().GetResult(); } catch { }
        _capture.Dispose();
    }
}
