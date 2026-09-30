# Mohammed Lab Color Vision

Commercial-ready Windows desktop toolkit by **Mohammed Lab** for real-time color detection, visual target inspection, controller diagnostics, profiles, and performance telemetry.

## Features
- Center-screen high-FPS capture
- OpenCV HSV color detection
- Pink, Yellow, Red and Purple presets
- Adjustable capture region and target FPS
- Nearest-color-region inspection with confidence, bounds and coordinates
- Optional sticky target inspection for short detection gaps
- XInput controller tester
- JSON profiles with import/export
- Runtime diagnostics
- Mohammed Lab branding
- Windows x64 self-contained build artifact through GitHub Actions

## Build
`./build.ps1`

## Publish
`./publish.ps1`

Output: `dist/MohammedLab-ColorVision`

Builds are validated on the Windows GitHub Actions runner before release packaging.
