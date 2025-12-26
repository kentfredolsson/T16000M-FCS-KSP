# Claude Code Session Rules - T16000M FCS KSP Mod

## Project Overview
This is a KSP (Kerbal Space Program) mod for integrating the Thrustmaster T.16000M FCS HOTAS system:
- **T.16000M Joystick** (Joy0) - Right hand stick for pitch, yaw, roll
- **TWCS Throttle** (Joy1) - Left hand throttle with additional controls

## Session Startup Checklist
**ALWAYS perform these checks at the start of every new chat session:**

1. **Verify Git Status**
   ```bash
   git status
   git log -1 --oneline
   git fetch origin && git status
   ```
   - Ensure working directory is clean
   - Verify on latest commit
   - Confirm pushed to remote (no "ahead of origin")

2. **If behind remote**: `git pull origin main`
3. **If ahead of remote**: `git push origin main`

## Commit Rules
**MANDATORY: Create a commit and push after EVERY:**
- Bug fix
- New feature implementation
- Configuration change
- Documentation update

**Commit message format:**
```
<type>: <short description>

<optional detailed description>

Generated with Claude Code
```

Types: `feat`, `fix`, `docs`, `refactor`, `config`, `test`, `chore`

## Security Rules
**NEVER commit:**
- `.env` files
- API keys or tokens
- Personal credentials
- `*credentials*`, `*secret*`, `*token*` files
- User-specific paths or PII

**ALWAYS:**
- Scan code for hardcoded secrets before committing
- Keep `.gitignore` updated
- Review `git diff` before committing

## File Maintenance
**Keep updated:**
- `README.md` - User documentation, installation, usage
- `.gitignore` - Exclude build artifacts, sensitive files
- `CHANGELOG.md` - Track version history (when applicable)

## KSP Mod Development Standards

### Project Structure
```
ksp-mods/
├── src/
│   ├── T16000M_FCS.cs      # Main plugin logic
│   ├── Config.cs           # Configuration management
│   └── AxisProcessor.cs    # Input processing
├── GameData/
│   └── T16000M_FCS/
│       ├── T16000M_FCS.dll
│       └── settings.cfg
├── T16000M_FCS.csproj
├── README.md
├── LICENSE
├── .gitignore
└── claude.md
```

### Required References
- `Assembly-CSharp.dll` - KSP API
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.InputLegacyModule.dll`
- `UnityEngine.IMGUIModule.dll`

### Build Command
```bash
dotnet build -c Release
```

Output copies to: `~/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/T16000M_FCS/`

## Input Processing Best Practices

### Sensitivity Curves
Use cubic curves for smooth control:
```csharp
float ApplyCubicCurve(float input, float curvature)
{
    float x = Mathf.Clamp(input, -1f, 1f);
    return curvature * Mathf.Pow(x, 3) + (1f - curvature) * x;
}
```

### Deadzone Implementation
```csharp
float ApplyDeadzone(float value, float deadzone)
{
    if (Mathf.Abs(value) < deadzone) return 0f;
    float sign = Mathf.Sign(value);
    float magnitude = Mathf.Abs(value);
    return sign * ((magnitude - deadzone) / (1f - deadzone));
}
```

### Processing Pipeline Order
1. Inversion (if enabled)
2. Deadzone application
3. Sensitivity curve
4. Sensitivity multiplier
5. Clamp to valid range (-1 to 1)

## Settings System Requirements
The mod MUST support in-game adjustment of:
- **Per-axis sensitivity** (0.1 - 2.0 range)
- **Per-axis deadzone** (0.0 - 0.3 range)
- **Per-axis inversion** (toggle)
- **Curve intensity** (0.0 linear - 1.0 full cubic)
- **Button mappings** (rebindable)

Settings GUI accessible via **F9** hotkey during flight.

## Controller Mapping Reference

### T.16000M Joystick (Joy0)
| Button | Function |
|--------|----------|
| 0 | Stage (trigger) |
| 3 | Brakes toggle |
| 4 | RCS toggle |
| 10 | SAS toggle |
| 5-9, 11-15 | Action Groups |

### TWCS Throttle (Joy1)
| Button | Function |
|--------|----------|
| 0 | Lights toggle |
| 1 | AG0 |
| 2 | Gear toggle |
| 3 | AG2 (down lever) |
| 4 | AG1 (up lever) |
| 5 | ABORT |

## Testing Checklist
Before committing changes:
- [ ] Mod loads without errors (check KSP.log)
- [ ] All axes respond correctly
- [ ] Sensitivity adjustments work in GUI
- [ ] Deadzone settings apply correctly
- [ ] Button mappings fire correct actions
- [ ] Settings persist after game restart
- [ ] No NullReferenceExceptions in flight

## Git Repository
- **Remote**: https://github.com/kentfredolsson/T16000M-FCS-KSP
- **Branch**: main
- **Visibility**: Public

## Quick Reference Commands
```bash
# Check status
git status && git log -1 --oneline

# Standard commit flow
git add -A
git commit -m "type: description"
git push origin main

# Build and test
dotnet build -c Release
# Then launch KSP and test

# Security scan (before commit)
grep -r "ghp_\|password\|secret\|token" --include="*.cs" --include="*.cfg" .
```
