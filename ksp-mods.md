# T16000M FCS KSP Mod - Complete Project Documentation

## Overview
A KSP (Kerbal Space Program) mod to support the Thrustmaster T.16000M FCS HOTAS (Hands On Throttle And Stick) system:
- **T.16000M Joystick** (Joy0 / Joystick1Button*) - Right hand stick
- **TWCS Throttle** (Joy1 / Joystick2Button*) - Left hand throttle

## Project Location
`/Users/aarynolsson/Projects/T16000M_FCS/`

## Files

### Config.cs
Configuration class containing:
- Button mappings for both joysticks
- Axis configuration (pitch, yaw, roll, throttle)
- Sensitivity and deadzone settings
- Inversion toggles
- Load/Save methods using KSP's ConfigNode system
- Settings saved to: `GameData/T16000M_FCS/settings.cfg`

### T16000M_FCS.cs
Main plugin logic:
- `[KSPAddon(KSPAddon.Startup.Flight, false)]` - Runs during flight scene
- `OnFlyByWire` callback for continuous axis input
- `HandleJoystick0Buttons()` and `HandleJoystick1Buttons()` for button presses
- In-game GUI (F9) for settings adjustment
- Uses `Input.GetKeyDown(KeyCode.Joystick1Button0 + buttonNum)` for Joy0
- Uses `Input.GetKeyDown(KeyCode.Joystick2Button0 + buttonNum)` for Joy1

### T16000M_FCS.csproj
.NET project file referencing:
- Assembly-CSharp.dll (KSP API)
- UnityEngine.dll
- UnityEngine.CoreModule.dll
- UnityEngine.InputLegacyModule.dll
- UnityEngine.IMGUIModule.dll

---

## Current Button Mappings

### Joystick 0 (T.16000M Stick) - Final Configuration

```
Joy0_BrakesButton = 3      // Button 3 = Brakes toggle
Joy0_StageButton = 0       // Button 0 = Stage
Joy0_SASButton = 10        // Button 10 = SAS toggle
Joy0_RCSButton = 4         // Button 4 = RCS toggle
Joy0_AbortButton = -1      // Disabled

Joy0_AG0Button = 11        // Button 11
Joy0_AG1Button = 9         // Button 9
Joy0_AG2Button = 8         // Button 8
Joy0_AG3Button = 7         // Button 7
Joy0_AG4Button = 13        // Button 13
Joy0_AG5Button = 14        // Button 14
Joy0_AG6Button = 15        // Button 15
Joy0_AG7Button = 5         // Button 5
Joy0_AG8Button = 6         // Button 6
Joy0_AG9Button = 12        // Button 12
```

**Physical Layout (Base):**
```
Top Row (L-R):    [4:RCS] [5:AG7] [6:AG8] [12:AG9] [11:AG0] [10:SAS]
Bottom Row (L-R): [9:AG1] [8:AG2] [7:AG3] [13:AG4] [14:AG5] [15:AG6]
```

**Grip:**
- Button 0 = Stage (trigger)
- Button 3 = Brakes

### Joystick 1 (TWCS Throttle) - Final Configuration

```
Joy1_LightsButton = 0      // Button 0 = Lights toggle
Joy1_AG0Button = 1         // Button 1 = AG0
Joy1_GearButton = 2        // Button 2 = Gear toggle
Joy1_AG2DownButton = 3     // Button 3 = AG2 (down lever)
Joy1_AG1UpButton = 4       // Button 4 = AG1 (up lever)
Joy1_AbortButton = 5       // Button 5 = ABORT

// All other Joy1 buttons disabled (-1) to avoid overlaps:
Joy1_SASButton = -1
Joy1_RCSButton = -1
Joy1_AG1Button = -1
Joy1_AG2Button = -1
Joy1_AG3Button = -1
Joy1_AG4Button = -1
Joy1_AG5Button = -1
Joy1_AG6Button = -1
Joy1_AG7Button = -1
Joy1_AG8Button = -1
Joy1_AG9Button = -1
Joy1_BrakesButton = -1
```

---

## Axis Configuration

Axes are configured through **KSP's Settings > Input** menu, NOT in the mod:
- Pitch axis
- Yaw axis
- Roll axis

**Throttle is DISABLED** in the mod - use keyboard Z/X/Shift/Ctrl keys instead.

The mod reads axes via:
```csharp
float pitch = GetAxis(config.PitchAxis) * config.PitchSensitivity;
float yaw = GetAxis(config.YawAxis) * config.YawSensitivity;
float roll = GetAxis(config.RollAxis) * config.RollSensitivity;
```

---

## Hotkeys
- **F8** - Toggle mod enabled/disabled
- **F9** - Open/close settings GUI

---

## Build Instructions

```bash
cd /Users/aarynolsson/Projects/T16000M_FCS
dotnet build -c Release
```

Output automatically copies to:
```
~/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/T16000M_FCS/T16000M_FCS.dll
```

---

## Development History

### Initial Setup
- Created basic mod structure
- Set up axis reading via KSP's input system
- Added button mapping framework

### Throttle Issues
- **Problem**: Throttle axis was overriding KSP's keyboard controls, causing throttle to be stuck
- **Solution**: Completely removed throttle control from OnFlyByWire(), user uses Z/X keys

### Button Mapping Evolution

**Original mappings from user's blueprint (Joy0):**
```
Base top row (L-R): RCS(10), AG7(11), AG8(12), AG9(6), AG0(5), SAS(4)
Base bottom row (L-R): AG1(15), AG2(14/16), AG3(13), AG4(7), AG5(8), AG6(9)
Grip: Brakes(2), Stage(0)
```

**Issues encountered and fixes:**

1. **Button 2 overlap on Joy0**: Had two physical buttons registering as button 2
   - Fix: Moved Brakes to button 3

2. **Button 5 overlap on Joy0**: AG0 and Abort both on button 5
   - Fix: Disabled Abort on Joy0 (set to -1)

3. **Button 2 overlap on Joy1**: Brakes and Gear both on button 2
   - Fix: Disabled Brakes on Joy1, kept Gear only

4. **Button 4 overlap on Joy1**: AG2 up lever, SAS, and AG1 all on button 4
   - Fix: Made button 4 = AG1 only, disabled SAS on Joy1

5. **AG2 not working on Joy0**: Was mapped to button 14, user requested swap
   - Fix: Swapped per user request (see below)

### Final Swap Request (Joy0)
User requested swapping these button pairs:
- 10 ↔ 4 (SAS became RCS, RCS became SAS)
- 11 ↔ 5 (AG0 became AG7, AG7 became AG0)
- 12 ↔ 6 (AG9 became AG8, AG8 became AG9)
- 13 ↔ 7 (AG3 became AG4, AG4 became AG3)
- 14 ↔ 8 (AG5 became AG2, AG2 became AG5)
- 15 ↔ 9 (AG6 became AG1, AG1 became AG6)

### Joy1 Final Cleanup
User requested:
- Button 4 = AG1 only (removed all overlaps)
- Button 5 = ABORT
- Disabled all mirrored/unnecessary button mappings

---

## KSP Action Groups Reference

| KSP Action Group | Mod Name | KSPActionGroup Enum |
|------------------|----------|---------------------|
| Action Group 0 | AG0 | Custom10 |
| Action Group 1 | AG1 | Custom01 |
| Action Group 2 | AG2 | Custom02 |
| Action Group 3 | AG3 | Custom03 |
| Action Group 4 | AG4 | Custom04 |
| Action Group 5 | AG5 | Custom05 |
| Action Group 6 | AG6 | Custom06 |
| Action Group 7 | AG7 | Custom07 |
| Action Group 8 | AG8 | Custom08 |
| Action Group 9 | AG9 | Custom09 |
| SAS | SAS | SAS |
| RCS | RCS | RCS |
| Brakes | Brakes | Brakes |
| Gear | Gear | Gear |
| Lights | Lights | Light |
| Abort | ABORT | Abort |
| Stage | Stage | (uses StageManager.ActivateNextStage()) |

---

## Code Snippets

### Button Detection
```csharp
// Joystick 0 (T.16000M)
private bool GetJoy0ButtonDown(int button)
{
    if (button < 0) return false;
    KeyCode key = KeyCode.Joystick1Button0 + button;
    return Input.GetKeyDown(key);
}

// Joystick 1 (TWCS Throttle)
private bool GetJoy1ButtonDown(int button)
{
    if (button < 0) return false;
    KeyCode key = KeyCode.Joystick2Button0 + button;
    return Input.GetKeyDown(key);
}
```

### Toggle Action Group
```csharp
private void ToggleActionGroup(KSPActionGroup group, string name)
{
    if (activeVessel != null)
    {
        activeVessel.ActionGroups.ToggleGroup(group);
        ScreenMessages.PostScreenMessage(name, 1f, ScreenMessageStyle.UPPER_CENTER);
    }
}
```

### Deadzone Application
```csharp
private float ApplyDeadzone(float value, float deadzone)
{
    if (Mathf.Abs(value) < deadzone)
        return 0f;
    float sign = Mathf.Sign(value);
    float magnitude = Mathf.Abs(value);
    return sign * ((magnitude - deadzone) / (1f - deadzone));
}
```

---

## Troubleshooting

### Throttle stuck/not working
- Throttle control is intentionally disabled
- Use keyboard: Z (full), X (cut), Shift (up), Ctrl (down)

### Button triggers multiple actions
- Check for overlapping button numbers in Config.cs
- Set unwanted mapping to -1 to disable

### Action group not firing
- Verify button number matches physical button
- Check KSP's built-in input settings aren't conflicting
- Use F9 GUI to see current mappings

### Mod not loading
- Check KSP.log for errors
- Verify DLL is in GameData/T16000M_FCS/
- Ensure correct Unity/KSP version references
