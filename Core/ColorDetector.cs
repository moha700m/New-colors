using System.Diagnostics;
using System.Drawing;
using MohammedLab.ColorVision.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MohammedLab.ColorVision.Core;

public sealed class ColorDetector
{
    private DetectionResult _last;
    private long _lastFoundTicks;

    public DetectionResult Detect(Bitmap bitmap, AppConfig cfg)
    {
        var sw = Stopwatch.StartNew();
        using var mat = BitmapConverter.ToMat(bitmap);
        using var hsv = new Mat();
        Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);

        using var mask = BuildHueMask(hsv, cfg);
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
        Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);
        Cv2.Dilate(mask, mask, kernel, iterations: 1);

        Cv2.FindContours(mask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        var cx = bitmap.Width / 2.0;
        var cy = bitmap.Height / 2.0;
        var bestScore = double.MaxValue;
        DetectionResult best = DetectionResult.None();
        var candidates = 0;

        foreach (var contour in contours)
        {
            var area = Cv2.ContourArea(contour);
            if (area < cfg.MinBlobArea || area > cfg.MaxBlobArea) continue;

            var r = Cv2.BoundingRect(contour);
            if (r.Width < 2 || r.Height < 2) continue;
            candidates++;

            var tx = r.X + r.Width / 2;
            var ty = r.Y + r.Height / 2 + cfg.TargetYOffsetPx;
            var dx = tx - cx;
            var dy = ty - cy;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            var sizeBonus = Math.Min(70.0, Math.Sqrt(area)) * 0.18;
            var score = dist - sizeBonus;

            if (score < bestScore)
            {
                bestScore = score;
                var confidence = Math.Clamp(1.0 - dist / Math.Sqrt(cx * cx + cy * cy), 0.0, 1.0);
                best = new DetectionResult(true, new System.Drawing.Point(tx, ty), new Rectangle(r.X, r.Y, r.Width, r.Height), confidence, candidates, 0);
            }
        }

        if (!best.Found && cfg.StickyTarget && _last.Found)
        {
            var elapsedMs = (Stopwatch.GetTimestamp() - _lastFoundTicks) * 1000.0 / Stopwatch.Frequency;
            if (elapsedMs <= cfg.StickyMs) best = _last with { CandidateCount = candidates };
        }

        if (best.Found)
        {
            _last = best;
            _lastFoundTicks = Stopwatch.GetTimestamp();
        }

        sw.Stop();
        return best with { CandidateCount = candidates, ProcessingMs = sw.Elapsed.TotalMilliseconds };
    }

    private static Mat BuildHueMask(Mat hsv, AppConfig cfg)
    {
        var h = Math.Clamp(cfg.Hue, 0, 179);
        var tol = Math.Clamp(cfg.HueTolerance, 0, 50);
        var s = Math.Clamp(cfg.SaturationMin, 0, 255);
        var v = Math.Clamp(cfg.ValueMin, 0, 255);
        var low = h - tol;
        var high = h + tol;
        if (low >= 0 && high <= 179)
        {
            var mask = new Mat();
            Cv2.InRange(hsv, new Scalar(low, s, v), new Scalar(high, 255, 255), mask);
            return mask;
        }
        var m1 = new Mat();
        var m2 = new Mat();
        if (low < 0)
        {
            Cv2.InRange(hsv, new Scalar(0, s, v), new Scalar(high, 255, 255), m1);
            Cv2.InRange(hsv, new Scalar(180 + low, s, v), new Scalar(179, 255, 255), m2);
        }
        else
        {
            Cv2.InRange(hsv, new Scalar(low, s, v), new Scalar(179, 255, 255), m1);
            Cv2.InRange(hsv, new Scalar(0, s, v), new Scalar(high - 180, 255, 255), m2);
        }
        var combined = new Mat();
        Cv2.BitwiseOr(m1, m2, combined);
        m1.Dispose(); m2.Dispose();
        return combined;
    }
}
