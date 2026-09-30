# Mohammed Lab Color Vision

Windows desktop application built as an independent clean-room product for **Mohammed Lab**.

## Core features

- Center-screen high-FPS capture
- OpenCV HSV color detection
- Configurable capture area and target FPS
- Nearest-target selection with short sticky-target window
- Horizontal/vertical smoothing controls
- Aim point vertical offset
- Activation via LT / RT / LT+RT / LB / RB
- Anti-recoil vertical + horizontal compensation
- Physical XInput controller passthrough
- ViGEm virtual Xbox 360 controller output
- Live controller tester
- Profiles with import/export
- Runtime diagnostics
- Mohammed Lab branding

## Architecture

`Screen Capture -> HSV Detector -> Target Selection -> Assist Controller -> ViGEm Output`

The application does **not** read or modify another process's memory and does not inject code into games or other applications.

## Build requirements

- Windows 10/11 x64
- .NET 8 SDK
- ViGEmBus installed on the target machine for virtual-controller output

## Build

```powershell
./build.ps1
```

## Publish one-click folder

```powershell
./publish.ps1
```

Output is written to `dist/MohammedLab-ColorVision`.

## Commercial packaging

The source has Mohammed Lab product metadata and an all-rights-reserved commercial license template. Before selling, add your support URL, final EULA, privacy policy if telemetry/licensing is added, and code-sign the EXE/installer.

## Notes

This project is a new implementation. It does not contain the original application's binaries, screenshots, branding, or source code.
