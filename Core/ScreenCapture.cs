using System.Drawing;
using System.Drawing.Imaging;

namespace MohammedLab.ColorVision.Core;

public sealed class ScreenCapture : IDisposable
{
    private Bitmap? _buffer;

    public Bitmap CaptureCenter(int width, int height)
    {
        var screenW = GetSystemMetrics(0);
        var screenH = GetSystemMetrics(1);
        width = Math.Clamp(width, 64, screenW);
        height = Math.Clamp(height, 64, screenH);

        if (_buffer is null || _buffer.Width != width || _buffer.Height != height)
        {
            _buffer?.Dispose();
            _buffer = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        var x = (screenW - width) / 2;
        var y = (screenH - height) / 2;
        using var g = Graphics.FromImage(_buffer);
        g.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
        return _buffer;
    }

    public void Dispose() => _buffer?.Dispose();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
