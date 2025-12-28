using System.IO;
using UnityEngine;

namespace FlightHUD
{
    public enum ColorScheme { Green, Amber, Cyan, White, Custom }

    public static class HUDSettings
    {
        private static readonly string ConfigPath = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/FlightHUD/settings.cfg"
        );

        // General settings
        public static bool HUDEnabled = true;
        public static KeyCode ToggleKey = KeyCode.F10;
        public static float HUDScale = 1.0f;
        public static float HUDOpacity = 1.0f;

        // Color settings
        public static ColorScheme ActiveScheme = ColorScheme.Green;
        public static float CustomR = 0f;
        public static float CustomG = 1f;
        public static float CustomB = 0.5f;

        // Element visibility toggles
        public static bool ShowPitchLadder = true;
        public static bool ShowFPV = true;
        public static bool ShowBankIndicator = true;
        public static bool ShowHeadingTape = true;
        public static bool ShowAirspeedTape = true;
        public static bool ShowAltitudeTape = true;
        public static bool ShowVSI = true;
        public static bool ShowCompassRose = true;
        public static bool ShowGForce = true;
        public static bool ShowAOA = true;
        public static bool ShowMach = true;
        public static bool ShowRadarAlt = true;
        public static bool ShowStatusIcons = true;

        // Pitch Ladder transforms
        public static int PitchLadderRotation = 0;      // 0, 90, 180, 270
        public static bool PitchLadderNegateX = false;
        public static bool PitchLadderNegateY = false;
        public static bool PitchLadderMirrorX = false;
        public static bool PitchLadderMirrorY = false;

        // Pitch Ladder Numbers transforms (separate from ladder lines)
        public static int PitchNumbersRotation = 0;     // 0, 90, 180, 270
        public static bool PitchNumbersNegateX = false;
        public static bool PitchNumbersNegateY = false;
        public static bool PitchNumbersNegate = false;  // Negate the number values themselves

        // FPV transforms
        public static bool FPVNegateX = false;
        public static bool FPVNegateY = false;

        // Bank Indicator transforms
        public static bool BankNegate = false;
        public static bool BankMirror = false;

        // Settings window state
        public static bool SettingsWindowVisible = false;

        public static Color GetHUDColor()
        {
            switch (ActiveScheme)
            {
                case ColorScheme.Green:
                    return new Color(0f, 1f, 0.5f, HUDOpacity);
                case ColorScheme.Amber:
                    return new Color(1f, 0.75f, 0f, HUDOpacity);
                case ColorScheme.Cyan:
                    return new Color(0f, 1f, 1f, HUDOpacity);
                case ColorScheme.White:
                    return new Color(1f, 1f, 1f, HUDOpacity);
                case ColorScheme.Custom:
                    return new Color(CustomR, CustomG, CustomB, HUDOpacity);
                default:
                    return new Color(0f, 1f, 0.5f, HUDOpacity);
            }
        }

        public static void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                Debug.Log("[FlightHUD] No config file found, using defaults");
                return;
            }

            try
            {
                ConfigNode root = ConfigNode.Load(ConfigPath);
                if (root == null) return;

                ConfigNode cfg = root.GetNode("FlightHUD");
                if (cfg == null) return;

                // General
                if (cfg.HasValue("HUDEnabled"))
                    bool.TryParse(cfg.GetValue("HUDEnabled"), out HUDEnabled);
                if (cfg.HasValue("ToggleKey"))
                    System.Enum.TryParse(cfg.GetValue("ToggleKey"), out ToggleKey);
                if (cfg.HasValue("HUDScale"))
                    float.TryParse(cfg.GetValue("HUDScale"), out HUDScale);
                if (cfg.HasValue("HUDOpacity"))
                    float.TryParse(cfg.GetValue("HUDOpacity"), out HUDOpacity);

                // Color
                if (cfg.HasValue("ColorScheme"))
                    System.Enum.TryParse(cfg.GetValue("ColorScheme"), out ActiveScheme);
                if (cfg.HasValue("CustomR"))
                    float.TryParse(cfg.GetValue("CustomR"), out CustomR);
                if (cfg.HasValue("CustomG"))
                    float.TryParse(cfg.GetValue("CustomG"), out CustomG);
                if (cfg.HasValue("CustomB"))
                    float.TryParse(cfg.GetValue("CustomB"), out CustomB);

                // Elements
                if (cfg.HasValue("ShowPitchLadder"))
                    bool.TryParse(cfg.GetValue("ShowPitchLadder"), out ShowPitchLadder);
                if (cfg.HasValue("ShowFPV"))
                    bool.TryParse(cfg.GetValue("ShowFPV"), out ShowFPV);
                if (cfg.HasValue("ShowBankIndicator"))
                    bool.TryParse(cfg.GetValue("ShowBankIndicator"), out ShowBankIndicator);
                if (cfg.HasValue("ShowHeadingTape"))
                    bool.TryParse(cfg.GetValue("ShowHeadingTape"), out ShowHeadingTape);
                if (cfg.HasValue("ShowAirspeedTape"))
                    bool.TryParse(cfg.GetValue("ShowAirspeedTape"), out ShowAirspeedTape);
                if (cfg.HasValue("ShowAltitudeTape"))
                    bool.TryParse(cfg.GetValue("ShowAltitudeTape"), out ShowAltitudeTape);
                if (cfg.HasValue("ShowVSI"))
                    bool.TryParse(cfg.GetValue("ShowVSI"), out ShowVSI);
                if (cfg.HasValue("ShowCompassRose"))
                    bool.TryParse(cfg.GetValue("ShowCompassRose"), out ShowCompassRose);
                if (cfg.HasValue("ShowGForce"))
                    bool.TryParse(cfg.GetValue("ShowGForce"), out ShowGForce);
                if (cfg.HasValue("ShowAOA"))
                    bool.TryParse(cfg.GetValue("ShowAOA"), out ShowAOA);
                if (cfg.HasValue("ShowMach"))
                    bool.TryParse(cfg.GetValue("ShowMach"), out ShowMach);
                if (cfg.HasValue("ShowRadarAlt"))
                    bool.TryParse(cfg.GetValue("ShowRadarAlt"), out ShowRadarAlt);
                if (cfg.HasValue("ShowStatusIcons"))
                    bool.TryParse(cfg.GetValue("ShowStatusIcons"), out ShowStatusIcons);

                // Transforms
                if (cfg.HasValue("PitchLadderRotation"))
                    int.TryParse(cfg.GetValue("PitchLadderRotation"), out PitchLadderRotation);
                if (cfg.HasValue("PitchLadderNegateX"))
                    bool.TryParse(cfg.GetValue("PitchLadderNegateX"), out PitchLadderNegateX);
                if (cfg.HasValue("PitchLadderNegateY"))
                    bool.TryParse(cfg.GetValue("PitchLadderNegateY"), out PitchLadderNegateY);
                if (cfg.HasValue("PitchLadderMirrorX"))
                    bool.TryParse(cfg.GetValue("PitchLadderMirrorX"), out PitchLadderMirrorX);
                if (cfg.HasValue("PitchLadderMirrorY"))
                    bool.TryParse(cfg.GetValue("PitchLadderMirrorY"), out PitchLadderMirrorY);
                if (cfg.HasValue("PitchNumbersRotation"))
                    int.TryParse(cfg.GetValue("PitchNumbersRotation"), out PitchNumbersRotation);
                if (cfg.HasValue("PitchNumbersNegateX"))
                    bool.TryParse(cfg.GetValue("PitchNumbersNegateX"), out PitchNumbersNegateX);
                if (cfg.HasValue("PitchNumbersNegateY"))
                    bool.TryParse(cfg.GetValue("PitchNumbersNegateY"), out PitchNumbersNegateY);
                if (cfg.HasValue("PitchNumbersNegate"))
                    bool.TryParse(cfg.GetValue("PitchNumbersNegate"), out PitchNumbersNegate);
                if (cfg.HasValue("FPVNegateX"))
                    bool.TryParse(cfg.GetValue("FPVNegateX"), out FPVNegateX);
                if (cfg.HasValue("FPVNegateY"))
                    bool.TryParse(cfg.GetValue("FPVNegateY"), out FPVNegateY);
                if (cfg.HasValue("BankNegate"))
                    bool.TryParse(cfg.GetValue("BankNegate"), out BankNegate);
                if (cfg.HasValue("BankMirror"))
                    bool.TryParse(cfg.GetValue("BankMirror"), out BankMirror);

                Debug.Log("[FlightHUD] Config loaded");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FlightHUD] Error loading config: {e.Message}");
            }
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                ConfigNode root = new ConfigNode();
                ConfigNode cfg = root.AddNode("FlightHUD");

                // General
                cfg.AddValue("HUDEnabled", HUDEnabled.ToString());
                cfg.AddValue("ToggleKey", ToggleKey.ToString());
                cfg.AddValue("HUDScale", HUDScale.ToString("F2"));
                cfg.AddValue("HUDOpacity", HUDOpacity.ToString("F2"));

                // Color
                cfg.AddValue("ColorScheme", ActiveScheme.ToString());
                cfg.AddValue("CustomR", CustomR.ToString("F2"));
                cfg.AddValue("CustomG", CustomG.ToString("F2"));
                cfg.AddValue("CustomB", CustomB.ToString("F2"));

                // Elements
                cfg.AddValue("ShowPitchLadder", ShowPitchLadder.ToString());
                cfg.AddValue("ShowFPV", ShowFPV.ToString());
                cfg.AddValue("ShowBankIndicator", ShowBankIndicator.ToString());
                cfg.AddValue("ShowHeadingTape", ShowHeadingTape.ToString());
                cfg.AddValue("ShowAirspeedTape", ShowAirspeedTape.ToString());
                cfg.AddValue("ShowAltitudeTape", ShowAltitudeTape.ToString());
                cfg.AddValue("ShowVSI", ShowVSI.ToString());
                cfg.AddValue("ShowCompassRose", ShowCompassRose.ToString());
                cfg.AddValue("ShowGForce", ShowGForce.ToString());
                cfg.AddValue("ShowAOA", ShowAOA.ToString());
                cfg.AddValue("ShowMach", ShowMach.ToString());
                cfg.AddValue("ShowRadarAlt", ShowRadarAlt.ToString());
                cfg.AddValue("ShowStatusIcons", ShowStatusIcons.ToString());

                // Transforms
                cfg.AddValue("PitchLadderRotation", PitchLadderRotation.ToString());
                cfg.AddValue("PitchLadderNegateX", PitchLadderNegateX.ToString());
                cfg.AddValue("PitchLadderNegateY", PitchLadderNegateY.ToString());
                cfg.AddValue("PitchLadderMirrorX", PitchLadderMirrorX.ToString());
                cfg.AddValue("PitchLadderMirrorY", PitchLadderMirrorY.ToString());
                cfg.AddValue("PitchNumbersRotation", PitchNumbersRotation.ToString());
                cfg.AddValue("PitchNumbersNegateX", PitchNumbersNegateX.ToString());
                cfg.AddValue("PitchNumbersNegateY", PitchNumbersNegateY.ToString());
                cfg.AddValue("PitchNumbersNegate", PitchNumbersNegate.ToString());
                cfg.AddValue("FPVNegateX", FPVNegateX.ToString());
                cfg.AddValue("FPVNegateY", FPVNegateY.ToString());
                cfg.AddValue("BankNegate", BankNegate.ToString());
                cfg.AddValue("BankMirror", BankMirror.ToString());

                root.Save(ConfigPath);
                Debug.Log("[FlightHUD] Config saved");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FlightHUD] Error saving config: {e.Message}");
            }
        }
    }
}
