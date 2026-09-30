# Mohammed Lab PC

Independent clean-room Windows implementation by Mohammed Lab, based on the user-visible workflow and settings of the reference application.

## Pages
- Capture
- Game
- Guide
- Check

## Reference-compatible settings
Zone width/height, capture FPS, monitor, no-preview, HUD, device (Mouse/Controller), strength, aim-point offset, hold-to-aim, always-track, aim/fire bindings, anti-recoil and auto-fire.

The detector uses the reference marker color `#FF00FA` (HSV H 140-158, S >= 90, V >= 110).

## Notes
This repository contains original Mohammed Lab source code and branding. It does not include source code, logos, screenshots, installers, or other proprietary assets from the reference product. Controller output uses ViGEm when installed.

## Build
```powershell
dotnet restore
dotnet build -c Release
```

## Publish
```powershell
./publish.ps1
```
