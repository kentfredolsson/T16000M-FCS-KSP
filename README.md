# T16000M FCS KSP Mod

A Kerbal Space Program mod providing full support for the **Thrustmaster T.16000M FCS HOTAS** (Hands On Throttle And Stick) system.

## Features

- Full axis support for pitch, yaw, and roll controls
- Configurable button mappings for all KSP actions
- **In-game sensitivity adjustment** with real-time preview
- **Per-axis deadzone settings** to eliminate stick drift
- **Sensitivity curves** (linear to cubic) for precise control
- Axis inversion options
- Persistent settings saved between sessions
- Support for both T.16000M joystick and TWCS throttle

## Supported Hardware

- **T.16000M Joystick** (Joy0 / Joystick1) - Right hand flight stick
- **TWCS Throttle** (Joy1 / Joystick2) - Left hand throttle quadrant

## Installation

1. Download the latest release from [Releases](https://github.com/kentfredolsson/T16000M-FCS-KSP/releases)
2. Extract the `T16000M_FCS` folder to your KSP `GameData` directory:
   ```
   Kerbal Space Program/
   └── GameData/
       └── T16000M_FCS/
           ├── T16000M_FCS.dll
           └── settings.cfg (created on first run)
   ```
3. Launch KSP and enter a flight scene

## Usage

### Hotkeys
| Key | Action |
|-----|--------|
| **F8** | Toggle mod enabled/disabled |
| **F9** | Open/close settings GUI |

### Default Button Mappings

#### T.16000M Joystick (Joy0)
| Button | Location | Action |
|--------|----------|--------|
| 0 | Trigger | Stage |
| 3 | Grip | Brakes toggle |
| 4 | Base top-left | RCS toggle |
| 10 | Base top-right | SAS toggle |
| 5-9, 11-15 | Base buttons | Action Groups 0-9 |

#### TWCS Throttle (Joy1)
| Button | Location | Action |
|--------|----------|--------|
| 0 | Paddle | Lights toggle |
| 1 | Button 1 | Action Group 0 |
| 2 | Button 2 | Gear toggle |
| 3 | Down lever | Action Group 2 |
| 4 | Up lever | Action Group 1 |
| 5 | Red button | ABORT |

### Axis Controls
- **Pitch**: Joystick Y-axis (forward/back)
- **Yaw**: Joystick twist axis
- **Roll**: Joystick X-axis (left/right)
- **Throttle**: Controlled via keyboard (Z/X/Shift/Ctrl) - throttle axis disabled to prevent conflicts

## Configuration

### In-Game Settings (F9)

The settings GUI allows real-time adjustment of:

- **Sensitivity** (0.1 - 2.0): Controls response magnitude
- **Deadzone** (0.0 - 0.3): Eliminates center drift
- **Curve** (0.0 - 1.0): 0 = linear, 1 = full cubic for fine control near center
- **Invert**: Reverse axis direction

### Settings File

Settings are saved to `GameData/T16000M_FCS/settings.cfg` in KSP's ConfigNode format:

```
T16000M_FCS_CONFIG
{
    PitchSensitivity = 1.0
    PitchDeadzone = 0.05
    PitchCurve = 0.5
    PitchInvert = false
    // ... additional axes
}
```

## Building from Source

### Requirements
- .NET SDK (targeting .NET Framework 4.x)
- KSP installation for assembly references

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/kentfredolsson/T16000M-FCS-KSP.git
   cd T16000M-FCS-KSP
   ```

2. Update assembly reference paths in `T16000M_FCS.csproj` to match your KSP installation

3. Build:
   ```bash
   dotnet build -c Release
   ```

4. Copy `bin/Release/T16000M_FCS.dll` to `GameData/T16000M_FCS/`

## Troubleshooting

### Mod not loading
- Verify DLL is in `GameData/T16000M_FCS/`
- Check `KSP.log` for error messages
- Ensure KSP version compatibility

### Axes not responding
- Confirm joystick is detected by your OS
- Check KSP's Input settings for conflicts
- Try toggling the mod with F8

### Throttle issues
- Throttle control is intentionally disabled to prevent conflicts
- Use keyboard controls: Z (full), X (cut), Shift (increase), Ctrl (decrease)

### Button triggers wrong action
- Check for overlapping button numbers in settings
- Use F9 GUI to verify current mappings
- Set unwanted mappings to -1 to disable

## KSP Version Compatibility

- **KSP 1.12.x**: Fully tested and supported
- **KSP 1.11.x**: Should work (untested)
- **KSP 2**: Not compatible

## License

MIT License - see [LICENSE](LICENSE) file

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Credits

- Thrustmaster for the excellent T.16000M FCS hardware
- KSP modding community for documentation and examples
- [Advanced Fly-By-Wire](https://github.com/linuxgurugamer/ksp-advanced-flybywire) for inspiration

## Changelog

### v1.0.0
- Initial release
- Full T.16000M joystick support
- TWCS throttle button support
- In-game settings GUI
- Sensitivity curves and deadzone support
