using System;
using System.IO;
using UnityEngine;

namespace T16000M_FCS
{
    public class Config
    {
        private static readonly string ConfigPath = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/T16000M_FCS/settings.cfg");

        // Hotkeys
        public KeyCode ToggleKey = KeyCode.F8;
        public KeyCode GUIKey = KeyCode.F9;

        // Axis mappings (Unity Input axis names)
        public string PitchAxis = "Joy1 Axis 2";
        public string YawAxis = "Joy1 Axis 3";   // Twist
        public string RollAxis = "Joy1 Axis 1";

        // Sensitivity and deadzone
        public float PitchSensitivity = 1.0f;
        public float YawSensitivity = 1.0f;
        public float RollSensitivity = 1.0f;
        public float Deadzone = 0.05f;

        // Inversions
        public bool InvertPitch = false;
        public bool InvertYaw = false;
        public bool InvertRoll = false;

        // ============================================
        // Joystick 0 (T.16000M Stick) Button Mappings
        // ============================================
        // Uses KeyCode.Joystick1Button0 + buttonNum

        public int Joy0_StageButton = 0;       // Trigger = Stage
        public int Joy0_BrakesButton = 3;      // Grip button = Brakes toggle

        // Swap: 10↔4, 11↔5, 12↔6, 13↔7, 14↔8, 15↔9
        public int Joy0_RCSButton = 4;         // Swapped: was 10, now 4
        public int Joy0_SASButton = 10;        // Swapped: was 4, now 10

        public int Joy0_AbortButton = -1;      // DISABLED - no conflict with AG0

        // Action Groups on Joy0 (after swap)
        public int Joy0_AG0Button = 11;        // Swapped: was 5, now 11 (AG0 = Custom10)
        public int Joy0_AG1Button = 9;         // Swapped: was 15, now 9
        public int Joy0_AG2Button = 8;         // Swapped: was 14, now 8
        public int Joy0_AG3Button = 7;         // Swapped: was 13, now 7
        public int Joy0_AG4Button = 13;        // Swapped: was 7, now 13
        public int Joy0_AG5Button = 14;        // Swapped: was 8, now 14
        public int Joy0_AG6Button = 15;        // Swapped: was 9, now 15
        public int Joy0_AG7Button = 5;         // Swapped: was 11, now 5
        public int Joy0_AG8Button = 6;         // Swapped: was 12, now 6
        public int Joy0_AG9Button = 12;        // Swapped: was 6, now 12

        // ============================================
        // Joystick 1 (TWCS Throttle) Button Mappings
        // ============================================
        // Uses KeyCode.Joystick2Button0 + buttonNum

        public int Joy1_LightsButton = 0;      // Button 0 = Lights toggle
        public int Joy1_AG0Button = 1;         // Button 1 = AG0
        public int Joy1_GearButton = 2;        // Button 2 = Gear toggle ONLY (no brakes)
        public int Joy1_AG2DownButton = 4;     // Button 4 = AG2 (swapped from 3)
        public int Joy1_AG1UpButton = 3;       // Button 3 = AG1 (swapped from 4)
        public int Joy1_AbortButton = 5;       // Button 5 = ABORT!

        // Disabled Joy1 buttons to avoid conflicts
        public int Joy1_BrakesButton = -1;     // DISABLED - was conflicting with Gear
        public int Joy1_SASButton = -1;        // DISABLED
        public int Joy1_RCSButton = -1;        // DISABLED
        public int Joy1_StageButton = -1;      // DISABLED

        public static Config Load()
        {
            Config config = new Config();

            if (!File.Exists(ConfigPath))
            {
                Debug.Log("[T16000M_FCS] No config file found, using defaults");
                config.Save();
                return config;
            }

            try
            {
                ConfigNode node = ConfigNode.Load(ConfigPath);
                if (node == null || !node.HasNode("T16000M_FCS"))
                {
                    Debug.LogWarning("[T16000M_FCS] Invalid config file, using defaults");
                    return config;
                }

                ConfigNode settings = node.GetNode("T16000M_FCS");

                // Hotkeys
                if (settings.HasValue("ToggleKey"))
                    config.ToggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("ToggleKey"));
                if (settings.HasValue("GUIKey"))
                    config.GUIKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("GUIKey"));

                // Axes
                if (settings.HasValue("PitchAxis")) config.PitchAxis = settings.GetValue("PitchAxis");
                if (settings.HasValue("YawAxis")) config.YawAxis = settings.GetValue("YawAxis");
                if (settings.HasValue("RollAxis")) config.RollAxis = settings.GetValue("RollAxis");

                // Sensitivity
                if (settings.HasValue("PitchSensitivity")) config.PitchSensitivity = float.Parse(settings.GetValue("PitchSensitivity"));
                if (settings.HasValue("YawSensitivity")) config.YawSensitivity = float.Parse(settings.GetValue("YawSensitivity"));
                if (settings.HasValue("RollSensitivity")) config.RollSensitivity = float.Parse(settings.GetValue("RollSensitivity"));
                if (settings.HasValue("Deadzone")) config.Deadzone = float.Parse(settings.GetValue("Deadzone"));

                // Inversions
                if (settings.HasValue("InvertPitch")) config.InvertPitch = bool.Parse(settings.GetValue("InvertPitch"));
                if (settings.HasValue("InvertYaw")) config.InvertYaw = bool.Parse(settings.GetValue("InvertYaw"));
                if (settings.HasValue("InvertRoll")) config.InvertRoll = bool.Parse(settings.GetValue("InvertRoll"));

                // Joy0 Buttons
                if (settings.HasValue("Joy0_StageButton")) config.Joy0_StageButton = int.Parse(settings.GetValue("Joy0_StageButton"));
                if (settings.HasValue("Joy0_BrakesButton")) config.Joy0_BrakesButton = int.Parse(settings.GetValue("Joy0_BrakesButton"));
                if (settings.HasValue("Joy0_RCSButton")) config.Joy0_RCSButton = int.Parse(settings.GetValue("Joy0_RCSButton"));
                if (settings.HasValue("Joy0_SASButton")) config.Joy0_SASButton = int.Parse(settings.GetValue("Joy0_SASButton"));
                if (settings.HasValue("Joy0_AbortButton")) config.Joy0_AbortButton = int.Parse(settings.GetValue("Joy0_AbortButton"));
                if (settings.HasValue("Joy0_AG0Button")) config.Joy0_AG0Button = int.Parse(settings.GetValue("Joy0_AG0Button"));
                if (settings.HasValue("Joy0_AG1Button")) config.Joy0_AG1Button = int.Parse(settings.GetValue("Joy0_AG1Button"));
                if (settings.HasValue("Joy0_AG2Button")) config.Joy0_AG2Button = int.Parse(settings.GetValue("Joy0_AG2Button"));
                if (settings.HasValue("Joy0_AG3Button")) config.Joy0_AG3Button = int.Parse(settings.GetValue("Joy0_AG3Button"));
                if (settings.HasValue("Joy0_AG4Button")) config.Joy0_AG4Button = int.Parse(settings.GetValue("Joy0_AG4Button"));
                if (settings.HasValue("Joy0_AG5Button")) config.Joy0_AG5Button = int.Parse(settings.GetValue("Joy0_AG5Button"));
                if (settings.HasValue("Joy0_AG6Button")) config.Joy0_AG6Button = int.Parse(settings.GetValue("Joy0_AG6Button"));
                if (settings.HasValue("Joy0_AG7Button")) config.Joy0_AG7Button = int.Parse(settings.GetValue("Joy0_AG7Button"));
                if (settings.HasValue("Joy0_AG8Button")) config.Joy0_AG8Button = int.Parse(settings.GetValue("Joy0_AG8Button"));
                if (settings.HasValue("Joy0_AG9Button")) config.Joy0_AG9Button = int.Parse(settings.GetValue("Joy0_AG9Button"));

                // Joy1 Buttons
                if (settings.HasValue("Joy1_LightsButton")) config.Joy1_LightsButton = int.Parse(settings.GetValue("Joy1_LightsButton"));
                if (settings.HasValue("Joy1_AG0Button")) config.Joy1_AG0Button = int.Parse(settings.GetValue("Joy1_AG0Button"));
                if (settings.HasValue("Joy1_GearButton")) config.Joy1_GearButton = int.Parse(settings.GetValue("Joy1_GearButton"));
                if (settings.HasValue("Joy1_AG2DownButton")) config.Joy1_AG2DownButton = int.Parse(settings.GetValue("Joy1_AG2DownButton"));
                if (settings.HasValue("Joy1_AG1UpButton")) config.Joy1_AG1UpButton = int.Parse(settings.GetValue("Joy1_AG1UpButton"));
                if (settings.HasValue("Joy1_AbortButton")) config.Joy1_AbortButton = int.Parse(settings.GetValue("Joy1_AbortButton"));
                if (settings.HasValue("Joy1_BrakesButton")) config.Joy1_BrakesButton = int.Parse(settings.GetValue("Joy1_BrakesButton"));

                Debug.Log("[T16000M_FCS] Config loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError("[T16000M_FCS] Error loading config: " + e.Message);
            }

            return config;
        }

        public void Save()
        {
            try
            {
                ConfigNode root = new ConfigNode();
                ConfigNode settings = root.AddNode("T16000M_FCS");

                // Hotkeys
                settings.AddValue("ToggleKey", ToggleKey.ToString());
                settings.AddValue("GUIKey", GUIKey.ToString());

                // Axes
                settings.AddValue("PitchAxis", PitchAxis);
                settings.AddValue("YawAxis", YawAxis);
                settings.AddValue("RollAxis", RollAxis);

                // Sensitivity
                settings.AddValue("PitchSensitivity", PitchSensitivity.ToString());
                settings.AddValue("YawSensitivity", YawSensitivity.ToString());
                settings.AddValue("RollSensitivity", RollSensitivity.ToString());
                settings.AddValue("Deadzone", Deadzone.ToString());

                // Inversions
                settings.AddValue("InvertPitch", InvertPitch.ToString());
                settings.AddValue("InvertYaw", InvertYaw.ToString());
                settings.AddValue("InvertRoll", InvertRoll.ToString());

                // Joy0 Buttons
                settings.AddValue("Joy0_StageButton", Joy0_StageButton.ToString());
                settings.AddValue("Joy0_BrakesButton", Joy0_BrakesButton.ToString());
                settings.AddValue("Joy0_RCSButton", Joy0_RCSButton.ToString());
                settings.AddValue("Joy0_SASButton", Joy0_SASButton.ToString());
                settings.AddValue("Joy0_AbortButton", Joy0_AbortButton.ToString());
                settings.AddValue("Joy0_AG0Button", Joy0_AG0Button.ToString());
                settings.AddValue("Joy0_AG1Button", Joy0_AG1Button.ToString());
                settings.AddValue("Joy0_AG2Button", Joy0_AG2Button.ToString());
                settings.AddValue("Joy0_AG3Button", Joy0_AG3Button.ToString());
                settings.AddValue("Joy0_AG4Button", Joy0_AG4Button.ToString());
                settings.AddValue("Joy0_AG5Button", Joy0_AG5Button.ToString());
                settings.AddValue("Joy0_AG6Button", Joy0_AG6Button.ToString());
                settings.AddValue("Joy0_AG7Button", Joy0_AG7Button.ToString());
                settings.AddValue("Joy0_AG8Button", Joy0_AG8Button.ToString());
                settings.AddValue("Joy0_AG9Button", Joy0_AG9Button.ToString());

                // Joy1 Buttons
                settings.AddValue("Joy1_LightsButton", Joy1_LightsButton.ToString());
                settings.AddValue("Joy1_AG0Button", Joy1_AG0Button.ToString());
                settings.AddValue("Joy1_GearButton", Joy1_GearButton.ToString());
                settings.AddValue("Joy1_AG2DownButton", Joy1_AG2DownButton.ToString());
                settings.AddValue("Joy1_AG1UpButton", Joy1_AG1UpButton.ToString());
                settings.AddValue("Joy1_AbortButton", Joy1_AbortButton.ToString());
                settings.AddValue("Joy1_BrakesButton", Joy1_BrakesButton.ToString());

                // Ensure directory exists
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                root.Save(ConfigPath);
                Debug.Log("[T16000M_FCS] Config saved to " + ConfigPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[T16000M_FCS] Error saving config: " + e.Message);
            }
        }
    }
}
