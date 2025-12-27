using System;
using System.IO;
using UnityEngine;

namespace FlightHUD
{
    public enum ColorScheme
    {
        Green,
        Amber,
        Cyan,
        White,
        Custom
    }

    public class HUDConfig
    {
        private static readonly string ConfigPath = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/FlightHUD/settings.cfg");

        // Hotkey
        public KeyCode HUDToggleKey = KeyCode.F10;

        // HUD Settings
        public bool HUDEnabled = true;
        public float HUDScale = 1.0f;
        public float HUDOpacity = 0.9f;

        // Color scheme
        public ColorScheme ActiveScheme = ColorScheme.Green;
        public float CustomColorR = 0f;
        public float CustomColorG = 1f;
        public float CustomColorB = 0.5f;

        // Element toggles
        public bool ShowPitchLadder = true;
        public bool ShowFlightPathVector = true;
        public bool ShowAircraftSymbol = true;
        public bool ShowAirspeedTape = true;
        public bool ShowAltitudeTape = true;
        public bool ShowHeadingTape = true;
        public bool ShowBankIndicator = true;
        public bool ShowVSI = true;
        public bool ShowGForceAOA = true;
        public bool ShowCompassRose = true;

        public Color GetHUDColor()
        {
            Color baseColor;
            switch (ActiveScheme)
            {
                case ColorScheme.Green:
                    baseColor = new Color(0f, 1f, 0.5f);
                    break;
                case ColorScheme.Amber:
                    baseColor = new Color(1f, 0.75f, 0f);
                    break;
                case ColorScheme.Cyan:
                    baseColor = new Color(0f, 1f, 1f);
                    break;
                case ColorScheme.White:
                    baseColor = new Color(1f, 1f, 1f);
                    break;
                case ColorScheme.Custom:
                default:
                    baseColor = new Color(CustomColorR, CustomColorG, CustomColorB);
                    break;
            }
            baseColor.a = HUDOpacity;
            return baseColor;
        }

        public Color GetHUDColorDim()
        {
            Color c = GetHUDColor();
            c.a *= 0.6f;
            return c;
        }

        public static HUDConfig Load()
        {
            HUDConfig config = new HUDConfig();

            if (!File.Exists(ConfigPath))
            {
                Debug.Log("[FlightHUD] No config file found, using defaults");
                config.Save();
                return config;
            }

            try
            {
                ConfigNode node = ConfigNode.Load(ConfigPath);
                if (node == null || !node.HasNode("FlightHUD"))
                {
                    Debug.LogWarning("[FlightHUD] Invalid config file, using defaults");
                    return config;
                }

                ConfigNode settings = node.GetNode("FlightHUD");

                if (settings.HasValue("HUDToggleKey"))
                    config.HUDToggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("HUDToggleKey"));

                if (settings.HasValue("HUDEnabled")) config.HUDEnabled = bool.Parse(settings.GetValue("HUDEnabled"));
                if (settings.HasValue("HUDScale")) config.HUDScale = float.Parse(settings.GetValue("HUDScale"));
                if (settings.HasValue("HUDOpacity")) config.HUDOpacity = float.Parse(settings.GetValue("HUDOpacity"));

                if (settings.HasValue("ActiveScheme"))
                    config.ActiveScheme = (ColorScheme)Enum.Parse(typeof(ColorScheme), settings.GetValue("ActiveScheme"));

                if (settings.HasValue("CustomColorR")) config.CustomColorR = float.Parse(settings.GetValue("CustomColorR"));
                if (settings.HasValue("CustomColorG")) config.CustomColorG = float.Parse(settings.GetValue("CustomColorG"));
                if (settings.HasValue("CustomColorB")) config.CustomColorB = float.Parse(settings.GetValue("CustomColorB"));

                if (settings.HasValue("ShowPitchLadder")) config.ShowPitchLadder = bool.Parse(settings.GetValue("ShowPitchLadder"));
                if (settings.HasValue("ShowFlightPathVector")) config.ShowFlightPathVector = bool.Parse(settings.GetValue("ShowFlightPathVector"));
                if (settings.HasValue("ShowAircraftSymbol")) config.ShowAircraftSymbol = bool.Parse(settings.GetValue("ShowAircraftSymbol"));
                if (settings.HasValue("ShowAirspeedTape")) config.ShowAirspeedTape = bool.Parse(settings.GetValue("ShowAirspeedTape"));
                if (settings.HasValue("ShowAltitudeTape")) config.ShowAltitudeTape = bool.Parse(settings.GetValue("ShowAltitudeTape"));
                if (settings.HasValue("ShowHeadingTape")) config.ShowHeadingTape = bool.Parse(settings.GetValue("ShowHeadingTape"));
                if (settings.HasValue("ShowBankIndicator")) config.ShowBankIndicator = bool.Parse(settings.GetValue("ShowBankIndicator"));
                if (settings.HasValue("ShowVSI")) config.ShowVSI = bool.Parse(settings.GetValue("ShowVSI"));
                if (settings.HasValue("ShowGForceAOA")) config.ShowGForceAOA = bool.Parse(settings.GetValue("ShowGForceAOA"));
                if (settings.HasValue("ShowCompassRose")) config.ShowCompassRose = bool.Parse(settings.GetValue("ShowCompassRose"));

                Debug.Log("[FlightHUD] Config loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError("[FlightHUD] Error loading config: " + e.Message);
            }

            return config;
        }

        public void Save()
        {
            try
            {
                ConfigNode root = new ConfigNode();
                ConfigNode settings = root.AddNode("FlightHUD");

                settings.AddValue("HUDToggleKey", HUDToggleKey.ToString());
                settings.AddValue("HUDEnabled", HUDEnabled.ToString());
                settings.AddValue("HUDScale", HUDScale.ToString());
                settings.AddValue("HUDOpacity", HUDOpacity.ToString());

                settings.AddValue("ActiveScheme", ActiveScheme.ToString());
                settings.AddValue("CustomColorR", CustomColorR.ToString());
                settings.AddValue("CustomColorG", CustomColorG.ToString());
                settings.AddValue("CustomColorB", CustomColorB.ToString());

                settings.AddValue("ShowPitchLadder", ShowPitchLadder.ToString());
                settings.AddValue("ShowFlightPathVector", ShowFlightPathVector.ToString());
                settings.AddValue("ShowAircraftSymbol", ShowAircraftSymbol.ToString());
                settings.AddValue("ShowAirspeedTape", ShowAirspeedTape.ToString());
                settings.AddValue("ShowAltitudeTape", ShowAltitudeTape.ToString());
                settings.AddValue("ShowHeadingTape", ShowHeadingTape.ToString());
                settings.AddValue("ShowBankIndicator", ShowBankIndicator.ToString());
                settings.AddValue("ShowVSI", ShowVSI.ToString());
                settings.AddValue("ShowGForceAOA", ShowGForceAOA.ToString());
                settings.AddValue("ShowCompassRose", ShowCompassRose.ToString());

                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                root.Save(ConfigPath);
                Debug.Log("[FlightHUD] Config saved to " + ConfigPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[FlightHUD] Error saving config: " + e.Message);
            }
        }
    }
}
