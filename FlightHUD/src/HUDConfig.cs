using System;
using System.IO;
using UnityEngine;

namespace FlightHUD
{
    public class HUDConfig
    {
        private static readonly string ConfigPath = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/FlightHUD/settings.cfg");

        // Hotkey
        public KeyCode HUDKey = KeyCode.F10;

        // HUD Settings
        public bool HUDEnabled = true;
        public float HUDScale = 1.0f;
        public float HUDOpacity = 0.9f;
        public float HUDColorR = 0f;
        public float HUDColorG = 1f;
        public float HUDColorB = 0.5f;

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

                if (settings.HasValue("HUDKey"))
                    config.HUDKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("HUDKey"));

                if (settings.HasValue("HUDEnabled")) config.HUDEnabled = bool.Parse(settings.GetValue("HUDEnabled"));
                if (settings.HasValue("HUDScale")) config.HUDScale = float.Parse(settings.GetValue("HUDScale"));
                if (settings.HasValue("HUDOpacity")) config.HUDOpacity = float.Parse(settings.GetValue("HUDOpacity"));
                if (settings.HasValue("HUDColorR")) config.HUDColorR = float.Parse(settings.GetValue("HUDColorR"));
                if (settings.HasValue("HUDColorG")) config.HUDColorG = float.Parse(settings.GetValue("HUDColorG"));
                if (settings.HasValue("HUDColorB")) config.HUDColorB = float.Parse(settings.GetValue("HUDColorB"));

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

                settings.AddValue("HUDKey", HUDKey.ToString());
                settings.AddValue("HUDEnabled", HUDEnabled.ToString());
                settings.AddValue("HUDScale", HUDScale.ToString());
                settings.AddValue("HUDOpacity", HUDOpacity.ToString());
                settings.AddValue("HUDColorR", HUDColorR.ToString());
                settings.AddValue("HUDColorG", HUDColorG.ToString());
                settings.AddValue("HUDColorB", HUDColorB.ToString());

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
