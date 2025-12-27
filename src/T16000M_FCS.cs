using System;
using UnityEngine;

namespace T16000M_FCS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class T16000M_FCS : MonoBehaviour
    {
        // Configuration
        private Config config;

        // State
        private Vessel activeVessel;
        private bool isEnabled = true;
        private bool showGUI = false;
        private Rect windowRect = new Rect(100, 100, 350, 500);

        // Joystick state for GUI display
        private float lastPitch, lastYaw, lastRoll;

        public void Start()
        {
            config = Config.Load();

            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselSwitching.Add(OnVesselSwitching);

            if (FlightGlobals.ActiveVessel != null)
            {
                RegisterVessel(FlightGlobals.ActiveVessel);
            }

            Debug.Log("[T16000M_FCS] Started - Press " + config.ToggleKey + " to toggle, " + config.GUIKey + " for settings");
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselSwitching.Remove(OnVesselSwitching);

            if (activeVessel != null)
            {
                activeVessel.OnFlyByWire -= OnFlyByWire;
            }
        }

        private void OnVesselChange(Vessel vessel)
        {
            RegisterVessel(vessel);
        }

        private void OnVesselSwitching(Vessel from, Vessel to)
        {
            if (from != null)
            {
                from.OnFlyByWire -= OnFlyByWire;
            }
        }

        private void RegisterVessel(Vessel vessel)
        {
            if (activeVessel != null)
            {
                activeVessel.OnFlyByWire -= OnFlyByWire;
            }

            activeVessel = vessel;

            if (activeVessel != null)
            {
                activeVessel.OnFlyByWire += OnFlyByWire;
                Debug.Log("[T16000M_FCS] Registered with vessel: " + vessel.vesselName);
            }
        }

        public void Update()
        {
            // Toggle mod on/off
            if (Input.GetKeyDown(config.ToggleKey))
            {
                isEnabled = !isEnabled;
                ScreenMessages.PostScreenMessage(
                    "T16000M FCS: " + (isEnabled ? "ENABLED" : "DISABLED"),
                    2f, ScreenMessageStyle.UPPER_CENTER);
            }

            // Toggle GUI
            if (Input.GetKeyDown(config.GUIKey))
            {
                showGUI = !showGUI;
            }

            if (!isEnabled) return;

            // Handle button inputs for both joysticks
            HandleJoystick0Buttons();
            HandleJoystick1Buttons();
        }

        // ============================================
        // Joystick 0 (T.16000M Stick) Button Handling
        // ============================================
        private void HandleJoystick0Buttons()
        {
            // Stage (Trigger)
            if (GetJoy0ButtonDown(config.Joy0_StageButton))
            {
                KSP.UI.Screens.StageManager.ActivateNextStage();
                ScreenMessages.PostScreenMessage("STAGE", 1f, ScreenMessageStyle.UPPER_CENTER);
            }

            // Brakes toggle
            if (GetJoy0ButtonDown(config.Joy0_BrakesButton))
            {
                ToggleActionGroup(KSPActionGroup.Brakes, "Brakes");
            }

            // SAS toggle
            if (GetJoy0ButtonDown(config.Joy0_SASButton))
            {
                ToggleActionGroup(KSPActionGroup.SAS, "SAS");
            }

            // RCS toggle
            if (GetJoy0ButtonDown(config.Joy0_RCSButton))
            {
                ToggleActionGroup(KSPActionGroup.RCS, "RCS");
            }

            // Abort (if enabled)
            if (GetJoy0ButtonDown(config.Joy0_AbortButton))
            {
                ToggleActionGroup(KSPActionGroup.Abort, "ABORT!");
            }

            // Action Groups 0-9
            if (GetJoy0ButtonDown(config.Joy0_AG0Button))
                ToggleActionGroup(KSPActionGroup.Custom10, "AG0");
            if (GetJoy0ButtonDown(config.Joy0_AG1Button))
                ToggleActionGroup(KSPActionGroup.Custom01, "AG1");
            if (GetJoy0ButtonDown(config.Joy0_AG2Button))
                ToggleActionGroup(KSPActionGroup.Custom02, "AG2");
            if (GetJoy0ButtonDown(config.Joy0_AG3Button))
                ToggleActionGroup(KSPActionGroup.Custom03, "AG3");
            if (GetJoy0ButtonDown(config.Joy0_AG4Button))
                ToggleActionGroup(KSPActionGroup.Custom04, "AG4");
            if (GetJoy0ButtonDown(config.Joy0_AG5Button))
                ToggleActionGroup(KSPActionGroup.Custom05, "AG5");
            if (GetJoy0ButtonDown(config.Joy0_AG6Button))
                ToggleActionGroup(KSPActionGroup.Custom06, "AG6");
            if (GetJoy0ButtonDown(config.Joy0_AG7Button))
                ToggleActionGroup(KSPActionGroup.Custom07, "AG7");
            if (GetJoy0ButtonDown(config.Joy0_AG8Button))
                ToggleActionGroup(KSPActionGroup.Custom08, "AG8");
            if (GetJoy0ButtonDown(config.Joy0_AG9Button))
                ToggleActionGroup(KSPActionGroup.Custom09, "AG9");
        }

        // ============================================
        // Joystick 1 (TWCS Throttle) Button Handling
        // ============================================
        private void HandleJoystick1Buttons()
        {
            // Lights toggle
            if (GetJoy1ButtonDown(config.Joy1_LightsButton))
            {
                ToggleActionGroup(KSPActionGroup.Light, "Lights");
            }

            // Gear toggle (Button 2 - NO brakes on this button)
            if (GetJoy1ButtonDown(config.Joy1_GearButton))
            {
                ToggleActionGroup(KSPActionGroup.Gear, "Gear");
            }

            // Brakes toggle (disabled by default to avoid conflict)
            if (GetJoy1ButtonDown(config.Joy1_BrakesButton))
            {
                ToggleActionGroup(KSPActionGroup.Brakes, "Brakes");
            }

            // AG0
            if (GetJoy1ButtonDown(config.Joy1_AG0Button))
            {
                ToggleActionGroup(KSPActionGroup.Custom10, "AG0");
            }

            // AG1 (up lever)
            if (GetJoy1ButtonDown(config.Joy1_AG1UpButton))
            {
                ToggleActionGroup(KSPActionGroup.Custom01, "AG1");
            }

            // AG2 (down lever)
            if (GetJoy1ButtonDown(config.Joy1_AG2DownButton))
            {
                ToggleActionGroup(KSPActionGroup.Custom02, "AG2");
            }

            // ABORT! (Button 5)
            if (GetJoy1ButtonDown(config.Joy1_AbortButton))
            {
                ToggleActionGroup(KSPActionGroup.Abort, "ABORT!");
            }
        }

        // ============================================
        // Button Detection Helpers
        // ============================================

        // Joystick 0 (T.16000M) uses Joystick1Button*
        private bool GetJoy0ButtonDown(int button)
        {
            if (button < 0) return false;
            KeyCode key = KeyCode.Joystick1Button0 + button;
            return Input.GetKeyDown(key);
        }

        // Joystick 1 (TWCS Throttle) uses Joystick2Button*
        private bool GetJoy1ButtonDown(int button)
        {
            if (button < 0) return false;
            KeyCode key = KeyCode.Joystick2Button0 + button;
            return Input.GetKeyDown(key);
        }

        private void ToggleActionGroup(KSPActionGroup group, string name)
        {
            if (activeVessel != null)
            {
                activeVessel.ActionGroups.ToggleGroup(group);
                ScreenMessages.PostScreenMessage(name, 1f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        // ============================================
        // Flight Control (Axes)
        // ============================================
        private void OnFlyByWire(FlightCtrlState state)
        {
            if (!isEnabled) return;

            float pitch = GetAxis(config.PitchAxis) * config.PitchSensitivity;
            float yaw = GetAxis(config.YawAxis) * config.YawSensitivity;
            float roll = GetAxis(config.RollAxis) * config.RollSensitivity;

            // Apply inversions
            if (config.InvertPitch) pitch = -pitch;
            if (config.InvertYaw) yaw = -yaw;
            if (config.InvertRoll) roll = -roll;

            // Apply deadzone
            pitch = ApplyDeadzone(pitch, config.Deadzone);
            yaw = ApplyDeadzone(yaw, config.Deadzone);
            roll = ApplyDeadzone(roll, config.Deadzone);

            // Store for GUI
            lastPitch = pitch;
            lastYaw = yaw;
            lastRoll = roll;

            // Apply to flight controls (ADD to existing so keyboard still works)
            state.pitch = Mathf.Clamp(state.pitch + pitch, -1f, 1f);
            state.yaw = Mathf.Clamp(state.yaw + yaw, -1f, 1f);
            state.roll = Mathf.Clamp(state.roll + roll, -1f, 1f);

            // Throttle controlled via keyboard (Z/X/Shift/Ctrl) - not via joystick
        }

        private float GetAxis(string axisName)
        {
            try
            {
                return Input.GetAxis(axisName);
            }
            catch
            {
                return 0f;
            }
        }

        private float ApplyDeadzone(float value, float deadzone)
        {
            if (Mathf.Abs(value) < deadzone)
                return 0f;

            float sign = Mathf.Sign(value);
            float magnitude = Mathf.Abs(value);
            return sign * ((magnitude - deadzone) / (1f - deadzone));
        }

        // ============================================
        // GUI
        // ============================================
        public void OnGUI()
        {
            if (!showGUI) return;

            windowRect = GUILayout.Window(
                GetHashCode(),
                windowRect,
                DrawWindow,
                "T16000M FCS Controller",
                GUILayout.Width(350));
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Status
            GUILayout.Label("Status: " + (isEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>"));
            GUILayout.Label("Toggle: " + config.ToggleKey + " | GUI: " + config.GUIKey);

            GUILayout.Space(10);

            // Current input values
            GUILayout.Label("=== Current Input ===");
            GUILayout.Label(string.Format("Pitch: {0:F2}", lastPitch));
            GUILayout.Label(string.Format("Yaw: {0:F2}", lastYaw));
            GUILayout.Label(string.Format("Roll: {0:F2}", lastRoll));

            GUILayout.Space(10);

            // Connected joysticks
            GUILayout.Label("=== Connected Joysticks ===");
            string[] joysticks = Input.GetJoystickNames();
            for (int i = 0; i < joysticks.Length; i++)
            {
                if (!string.IsNullOrEmpty(joysticks[i]))
                {
                    GUILayout.Label("Joy" + i + ": " + joysticks[i]);
                }
            }

            GUILayout.Space(10);

            // Settings
            GUILayout.Label("=== Settings ===");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Deadzone:", GUILayout.Width(80));
            config.Deadzone = GUILayout.HorizontalSlider(config.Deadzone, 0f, 0.3f, GUILayout.Width(150));
            GUILayout.Label(config.Deadzone.ToString("F2"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Sensitivity:", GUILayout.Width(80));
            float sens = GUILayout.HorizontalSlider(config.PitchSensitivity, 0.1f, 2f, GUILayout.Width(150));
            config.PitchSensitivity = config.YawSensitivity = config.RollSensitivity = sens;
            GUILayout.Label(sens.ToString("F2"));
            GUILayout.EndHorizontal();

            config.InvertPitch = GUILayout.Toggle(config.InvertPitch, "Invert Pitch");
            config.InvertYaw = GUILayout.Toggle(config.InvertYaw, "Invert Yaw");
            config.InvertRoll = GUILayout.Toggle(config.InvertRoll, "Invert Roll");

            GUILayout.Space(10);

            // Button mapping info
            GUILayout.Label("=== Joy0 (Stick) Mappings ===");
            GUILayout.Label("Trigger(0)=Stage, Grip(3)=Brakes");
            GUILayout.Label("Btn4=SAS, Btn10=RCS");
            GUILayout.Label("AG0-9 on base buttons");

            GUILayout.Space(5);

            GUILayout.Label("=== Joy1 (Throttle) Mappings ===");
            GUILayout.Label("Btn0=Lights, Btn1=AG0, Btn2=Gear");
            GUILayout.Label("Btn3=AG2, Btn4=AG1, Btn5=ABORT");

            GUILayout.Space(10);

            if (GUILayout.Button("Save Settings"))
            {
                config.Save();
                ScreenMessages.PostScreenMessage("Settings saved!", 2f, ScreenMessageStyle.UPPER_CENTER);
            }

            if (GUILayout.Button("Reload Settings"))
            {
                config = Config.Load();
                ScreenMessages.PostScreenMessage("Settings reloaded!", 2f, ScreenMessageStyle.UPPER_CENTER);
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
