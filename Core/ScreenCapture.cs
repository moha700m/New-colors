using System.Drawing;
using System.Drawing.Imaging;
using Forms = System.Windows.Forms;

namespace MohammedLab.ColorVision.Core;

public sealed class ScreenCapture : IDisposable
{
    private Bitmap? _buffer;
    public static string[] MonitorNames => Forms.Screen.AllScreens.Select((s, i) => $"{i + 1} - {s.DeviceName} ({s.Bounds.Width}x{s.Bounds.Height})").ToArray();

    public Bitmap CaptureCenter(int width, int height, int screenIndex = 0)
    {
        var screens = Forms.Screen.AllScreens;
        if (screens.Length == 0) throw new InvalidOperationException("No display was detected.");
        var screen = screens[Math.Clamp(screenIndex, 0, screens.Length - 1)];
        var bounds = screen.Bounds;
        width = Math.Clamp(width, 64, bounds.Width);
        height = Math.Clamp(height, 64, bounds.Height);
        if (_buffer is null || _buffer.Width != width || _buffer.Height != height)
        {
            _buffer?.Dispose();
            _buffer = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }
        var x = bounds.Left + (bounds.Width - width) / 2;
        var y = bounds.Top + (bounds.Height - height) / 2;
        using var g = Graphics.FromImage(_buffer);
        g.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
        return _buffer;
    }

    public void Dispose() => _buffer?.Dispose();
}
