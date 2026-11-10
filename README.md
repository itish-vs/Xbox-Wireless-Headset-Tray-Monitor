# Xbox Wireless Headset Tray Monitor

Windows system-tray utility to show whether Windows reports the Xbox Wireless Headset playback endpoint as active.

- **Green icon:** a playback endpoint whose name contains the configured device-name text is active.
- **Red icon:** no matching active playback endpoint is detected, or the app cannot query Windows audio state.
- Checks once per second.
- Runs in the notification area. Right-click the icon for `Check now`, `Show playback devices`, `Open settings file`, or `Exit`.
- Does not use Bluetooth; it checks the audio endpoint exposed by Windows for the wireless headset/adapter.

## Requirements

- Windows 10 or Windows 11, 64-bit.
- .NET 10 SDK to build from source.
- Xbox Wireless Adapter and headset paired and working in Windows.
- Internet access during first build so NuGet can restore NAudio.

## Build a standalone EXE

1. Extract this folder to a local directory.
2. Install .NET 10 SDK if needed.
3. Double-click `build.bat`, or run in PowerShell from this folder:

   ```powershell
   dotnet publish .\XboxHeadsetTray.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o .\publish
   ```

4. Run `publish\XboxHeadsetTray.exe`.

The published build includes the .NET runtime. NuGet package NAudio is restored at build time.

## Device-name matching

On first launch, the app creates:

`%LOCALAPPDATA%\XboxHeadsetTray\settings.json`

Default contents:

```json
{
  "DeviceNameContains": "Xbox Wireless Headset"
}
```

Right-click the tray icon and choose `Show playback devices` to inspect Windows playback endpoint names and states. If your device uses a different name, change `DeviceNameContains` in `settings.json` to a unique part of that name and restart the app. For example, if Windows lists `Headphones (Xbox Wireless Headset)`, the default match should work.

## Start with Windows

Press `Win + R`, enter `shell:startup`, then place a shortcut to `XboxHeadsetTray.exe` in that folder.

## Detection limits

The app checks Windows Core Audio playback endpoint state, not the headset's internal battery or radio protocol directly. It depends on the adapter/driver exposing an endpoint state that changes when the headset disconnects. If Windows continues reporting the matching endpoint as `Active` after powering off the headset, this method cannot reliably distinguish that condition by itself. Use `Show playback devices` to inspect the reported state.
