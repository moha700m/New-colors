using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MohammedLab.ColorVision.Core;

namespace MohammedLab.ColorVision;

public partial class MainWindow : Window
{
    private readonly ProfileStore _profiles = new();
    private AppConfig _config;
    private readonly VisionEngine _engine;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _config = _profiles.Load("Default");
        _engine = new VisionEngine(_config);
        _engine.TelemetryUpdated += Engine_TelemetryUpdated;
        _engine.Faulted += Engine_Faulted;
        LoadConfigToUi();
        TxtProfilePath.Text = _profiles.Root;
        Closed += async (_, _) => { await _engine.StopAsync(); _engine.Dispose(); };
    }

    private void LoadConfigToUi()
    {
        _loading = true;
        try
        {
            TxtProfileName.Text = _config.ProfileName;
            SldCaptureWidth.Value = _config.CaptureWidth;
            SldCaptureHeight.Value = _config.CaptureHeight;
            SldCaptureFps.Value = _config.CaptureFps;
            SldHue.Value = _config.Hue;
            SldHueTolerance.Value = _config.HueTolerance;
            SldSat.Value = _config.SaturationMin;
            SldValue.Value = _config.ValueMin;
            SldMinArea.Value = _config.MinBlobArea;
            SldTargetOffset.Value = _config.TargetYOffsetPx;
            ChkSticky.IsChecked = _config.StickyTarget;
            CmbControllerIndex.SelectedIndex = Math.Clamp(_config.ControllerIndex, 0, 3);
            RefreshLabels();
        }
        finally { _loading = false; }
    }

    private void ReadUiToConfig()
    {
        if (_loading) return;
        _config.ProfileName = string.IsNullOrWhiteSpace(TxtProfileName.Text) ? "Default" : TxtProfileName.Text.Trim();
        _config.CaptureWidth = (int)SldCaptureWidth.Value;
        _config.CaptureHeight = (int)SldCaptureHeight.Value;
        _config.CaptureFps = (int)SldCaptureFps.Value;
        _config.Hue = (int)SldHue.Value;
        _config.HueTolerance = (int)SldHueTolerance.Value;
        _config.SaturationMin = (int)SldSat.Value;
        _config.ValueMin = (int)SldValue.Value;
        _config.MinBlobArea = (int)SldMinArea.Value;
        _config.TargetYOffsetPx = (int)SldTargetOffset.Value;
        _config.StickyTarget = ChkSticky.IsChecked == true;
        _config.ControllerIndex = Math.Max(0, CmbControllerIndex.SelectedIndex);
        _engine.ApplyConfig(_config);
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (!IsInitialized) return;
        LblCaptureWidth.Text = $"{(int)SldCaptureWidth.Value}px";
        LblCaptureHeight.Text = $"{(int)SldCaptureHeight.Value}px";
        LblCaptureFps.Text = $"{(int)SldCaptureFps.Value} FPS target";
        LblHue.Text = ((int)SldHue.Value).ToString(CultureInfo.InvariantCulture);
        LblHueTolerance.Text = $"±{(int)SldHueTolerance.Value}";
        LblSat.Text = ((int)SldSat.Value).ToString(CultureInfo.InvariantCulture);
        LblValue.Text = ((int)SldValue.Value).ToString(CultureInfo.InvariantCulture);
        LblMinArea.Text = ((int)SldMinArea.Value).ToString(CultureInfo.InvariantCulture);
        LblTargetOffset.Text = $"{(int)SldTargetOffset.Value}px";
    }

    private void ConfigControl_Changed(object sender, RoutedEventArgs e) { if (!_loading) ReadUiToConfig(); }
    private void ConfigControl_Click(object sender, RoutedEventArgs e) { if (!_loading) ReadUiToConfig(); }

    private async void StartStopButton_Click(object sender, RoutedEventArgs e)
    {
        ReadUiToConfig();
        if (!_engine.Running)
        {
            if (_engine.Start())
            {
                StatusText.Text = "Running";
                StatusDot.Fill = (Brush)FindResource("SuccessBrush");
                StartStopButton.Content = "Stop Vision";
                FooterStatus.Text = "Vision engine started";
            }
        }
        else
        {
            await _engine.StopAsync();
            StatusText.Text = "Stopped";
            StatusDot.Fill = (Brush)FindResource("DangerBrush");
            StartStopButton.Content = "Start Vision";
            FooterStatus.Text = "Vision engine stopped";
        }
    }

    private void Engine_TelemetryUpdated()
    {
        Dispatcher.BeginInvoke(() =>
        {
            var d = _engine.LastDetection;
            var g = _engine.LastControllerState.Gamepad;
            TxtFps.Text = $"Capture FPS: {_engine.CaptureFps:0.0}";
            TxtDetector.Text = $"Detector: {d.ProcessingMs:0.00} ms • candidates {d.CandidateCount}";
            TxtTarget.Text = d.Found ? $"Target: X {d.Target.X}, Y {d.Target.Y} • confidence {d.Confidence:P0}" : "Target: none";
            TxtInspector.Text = d.Found ? $"Bounds {d.Bounds.X},{d.Bounds.Y}  {d.Bounds.Width}×{d.Bounds.Height} | Center {d.Target.X},{d.Target.Y} | Confidence {d.Confidence:P1}" : "No matching region detected.";
            var connected = XInput.TryGetState(_config.ControllerIndex, out _);
            TxtController.Text = connected ? $"Controller #{_config.ControllerIndex}: connected" : $"Controller #{_config.ControllerIndex}: not detected";
            TxtControllerLive.Text = $"LX {g.sThumbLX,6}   LY {g.sThumbLY,6}   RX {g.sThumbRX,6}   RY {g.sThumbRY,6}\nLT {g.bLeftTrigger,3}   RT {g.bRightTrigger,3}   Buttons 0x{(ushort)g.wButtons:X4}";
            TxtDiagnostics.Text = BuildDiagnostics();
        });
    }

    private void Engine_Faulted(string error)
    {
        Dispatcher.BeginInvoke(() =>
        {
            FooterStatus.Text = "Engine error";
            MessageBox.Show(this, error, "Mohammed Lab Color Vision", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    private string BuildDiagnostics()
    {
        var d = _engine.LastDetection;
        return $"Product: Mohammed Lab Color Vision 2.0.0\n" +
               $"OS: {Environment.OSVersion}\nRuntime: {Environment.Version}\n" +
               $"Capture: {_config.CaptureWidth}x{_config.CaptureHeight} @ {_config.CaptureFps} target FPS\n" +
               $"Actual FPS: {_engine.CaptureFps:0.0}\nDetector: {d.ProcessingMs:0.00} ms\n" +
               $"Color: H={_config.Hue} ±{_config.HueTolerance}, S>={_config.SaturationMin}, V>={_config.ValueMin}\n" +
               $"Controller index: {_config.ControllerIndex}\nProfile: {_config.ProfileName}";
    }

    private void PresetPink_Click(object sender, RoutedEventArgs e) => ApplyColorPreset("Pink", 155, 13, 130, 130);
    private void PresetYellow_Click(object sender, RoutedEventArgs e) => ApplyColorPreset("Yellow", 30, 11, 150, 150);
    private void PresetRed_Click(object sender, RoutedEventArgs e) => ApplyColorPreset("Red", 0, 9, 150, 130);
    private void PresetPurple_Click(object sender, RoutedEventArgs e) => ApplyColorPreset("Purple", 140, 12, 120, 120);
    private void ApplyColorPreset(string name, int h, int tol, int s, int v)
    {
        _config.Hue = h; _config.HueTolerance = tol; _config.SaturationMin = s; _config.ValueMin = v;
        LoadConfigToUi(); _engine.ApplyConfig(_config); FooterStatus.Text = $"Preset loaded: {name}";
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e) { ReadUiToConfig(); _profiles.Save(_config); FooterStatus.Text = $"Saved profile: {_config.ProfileName}"; }
    private void ReloadProfile_Click(object sender, RoutedEventArgs e) { var name = string.IsNullOrWhiteSpace(TxtProfileName.Text) ? "Default" : TxtProfileName.Text.Trim(); _config = _profiles.Load(name); LoadConfigToUi(); _engine.ApplyConfig(_config); FooterStatus.Text = $"Reloaded profile: {name}"; }
    private void NewProfile_Click(object sender, RoutedEventArgs e) { _config = new AppConfig { ProfileName = "New Profile" }; LoadConfigToUi(); _engine.ApplyConfig(_config); FooterStatus.Text = "New profile created"; }
    private void ExportProfile_Click(object sender, RoutedEventArgs e) { ReadUiToConfig(); var dlg = new SaveFileDialog { Filter = "Mohammed Lab profile (*.json)|*.json", FileName = _config.ProfileName + ".json" }; if (dlg.ShowDialog(this) == true) { _profiles.Export(_config, dlg.FileName); FooterStatus.Text = "Profile exported"; } }
    private void ImportProfile_Click(object sender, RoutedEventArgs e) { var dlg = new OpenFileDialog { Filter = "Mohammed Lab profile (*.json)|*.json" }; if (dlg.ShowDialog(this) == true) { _config = _profiles.Import(dlg.FileName); LoadConfigToUi(); _engine.ApplyConfig(_config); FooterStatus.Text = "Profile imported"; } }
    private void CopyDiagnostics_Click(object sender, RoutedEventArgs e) { Clipboard.SetText(BuildDiagnostics()); FooterStatus.Text = "Diagnostics copied"; }
}
