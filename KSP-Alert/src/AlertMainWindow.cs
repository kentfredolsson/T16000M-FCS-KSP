using System.Collections.Generic;
using UnityEngine;

namespace KSPAlert
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AlertMainWindow : MonoBehaviour
    {
        public static AlertMainWindow Instance { get; private set; }

        private bool showWindow = false;
        private Rect windowRect;
        private int windowId;
        private Vector2 scrollPosition = Vector2.zero;

        // Tabs
        private int selectedTab = 0;
        private string[] tabNames = { "ALERTS", "SETTINGS", "TEST" };

        // Silenced alerts
        private HashSet<AlertType> silencedAlerts = new HashSet<AlertType>();
        private Dictionary<AlertType, float> silenceTimers = new Dictionary<AlertType, float>();
        private const float SILENCE_DURATION = 60f;

        // Styles
        private GUIStyle windowStyle;
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle tabStyle;
        private GUIStyle selectedTabStyle;
        private GUIStyle alertLabelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleStyle;
        private GUIStyle testButtonStyle;
        private bool stylesInitialized = false;

        // Textures
        private Texture2D windowBg;
        private Texture2D warningBg;
        private Texture2D cautionBg;
        private Texture2D silencedBg;
        private Texture2D tabBg;
        private Texture2D selectedTabBg;

        private const float WINDOW_WIDTH = 320f;
        private const float WINDOW_HEIGHT = 450f;

        void Awake()
        {
            Instance = this;
            windowId = GetInstanceID();
            windowRect = new Rect(Screen.width - WINDOW_WIDTH - 20, 100, WINDOW_WIDTH, WINDOW_HEIGHT);
            CreateTextures();
        }

        void Start()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            Instance = null;
        }

        void Update()
        {
            // F10 toggles window
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ToggleWindow();
            }

            UpdateSilenceTimers();
        }

        private void UpdateSilenceTimers()
        {
            var expired = new List<AlertType>();
            foreach (var kvp in silenceTimers)
            {
                if (Time.time > kvp.Value)
                    expired.Add(kvp.Key);
            }
            foreach (var type in expired)
            {
                silencedAlerts.Remove(type);
                silenceTimers.Remove(type);
            }
        }

        private void CreateTextures()
        {
            windowBg = MakeTexture(new Color(0.1f, 0.1f, 0.15f, 0.95f));
            warningBg = MakeTexture(new Color(0.7f, 0.1f, 0.1f, 0.9f));
            cautionBg = MakeTexture(new Color(0.7f, 0.5f, 0.1f, 0.9f));
            silencedBg = MakeTexture(new Color(0.3f, 0.3f, 0.3f, 0.7f));
            tabBg = MakeTexture(new Color(0.2f, 0.2f, 0.25f, 0.9f));
            selectedTabBg = MakeTexture(new Color(0.3f, 0.5f, 0.7f, 0.9f));
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = windowBg },
                padding = new RectOffset(10, 10, 30, 10)
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.8f, 0.8f, 0.9f) }
            };

            tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { background = tabBg, textColor = Color.white },
                hover = { background = selectedTabBg, textColor = Color.white }
            };

            selectedTabStyle = new GUIStyle(tabStyle)
            {
                normal = { background = selectedTabBg, textColor = Color.white }
            };

            alertLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = 12
            };

            testButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(4, 4, 4, 4)
            };

            stylesInitialized = true;
        }

        void OnGUI()
        {
            if (!showWindow) return;
            if (!HighLogic.LoadedSceneIsFlight) return;

            InitializeStyles();

            windowRect = GUILayout.Window(
                windowId,
                windowRect,
                DrawWindow,
                "",
                windowStyle,
                GUILayout.Width(WINDOW_WIDTH),
                GUILayout.Height(WINDOW_HEIGHT)
            );
        }

        private void DrawWindow(int id)
        {
            var config = AlertManager.Instance?.Config;
            if (config == null)
            {
                GUILayout.Label("Alert system not initialized");
                GUI.DragWindow();
                return;
            }

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("KSP-ALERT", headerStyle);

            // Master ON/OFF
            string masterLabel = config.Enabled ? "ON" : "OFF";
            GUI.contentColor = config.Enabled ? Color.green : Color.red;
            if (GUILayout.Button(masterLabel, GUILayout.Width(40), GUILayout.Height(22)))
            {
                config.Enabled = !config.Enabled;
            }
            GUI.contentColor = Color.white;

            // Close button
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(22)))
            {
                showWindow = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Tab bar
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                GUIStyle style = (i == selectedTab) ? selectedTabStyle : tabStyle;
                if (GUILayout.Button(tabNames[i], style, GUILayout.Height(28)))
                {
                    selectedTab = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Tab content
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0:
                    DrawAlertsTab(config);
                    break;
                case 1:
                    DrawSettingsTab(config);
                    break;
                case 2:
                    DrawTestTab();
                    break;
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayout.Label("F10: Toggle Window", GUI.skin.label);

            GUI.DragWindow();
        }

        private void DrawAlertsTab(AlertConfig config)
        {
            // Volume control
            GUILayout.Label("Volume", sectionStyle);
            GUILayout.BeginHorizontal();

            string muteLabel = config.AudioEnabled ? "AUDIO" : "MUTED";
            GUI.contentColor = config.AudioEnabled ? Color.white : Color.red;
            if (GUILayout.Button(muteLabel, GUILayout.Width(60), GUILayout.Height(24)))
            {
                config.AudioEnabled = !config.AudioEnabled;
            }
            GUI.contentColor = Color.white;

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(24)))
            {
                config.MasterVolume = Mathf.Max(0f, config.MasterVolume - 0.1f);
            }

            GUILayout.Label($"{config.MasterVolume * 100:F0}%", headerStyle, GUILayout.Width(50));

            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(24)))
            {
                config.MasterVolume = Mathf.Min(1f, config.MasterVolume + 0.1f);
            }
            GUILayout.EndHorizontal();

            config.MasterVolume = GUILayout.HorizontalSlider(config.MasterVolume, 0f, 1f);

            GUILayout.Space(15);

            // Alert rows
            GUILayout.Label("Active Alerts", sectionStyle);
            DrawAlertRow("TERRAIN", AlertType.Terrain, config.TerrainEnabled, warningBg);
            DrawAlertRow("GEAR", AlertType.GearUp, config.GearEnabled, cautionBg);
            DrawAlertRow("FUEL", AlertType.LowFuel, config.FuelEnabled, cautionBg);
            DrawAlertRow("POWER", AlertType.LowPower, config.PowerEnabled, cautionBg);
            DrawAlertRow("HEAT", AlertType.Overheat, config.OverheatEnabled, warningBg);
            DrawAlertRow("G-FORCE", AlertType.HighG, config.HighGEnabled, cautionBg);
            DrawAlertRow("COMMS", AlertType.CommsLost, config.CommsEnabled, cautionBg);

            GUILayout.Space(10);

            // Silence controls
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SILENCE ALL", GUILayout.Height(28)))
            {
                SilenceAll();
            }
            if (GUILayout.Button("RESET ALL", GUILayout.Height(28)))
            {
                UnsilenceAll();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAlertRow(string label, AlertType type, bool enabled, Texture2D bgTex)
        {
            bool isSilenced = silencedAlerts.Contains(type);
            Texture2D rowBg = isSilenced ? silencedBg : (enabled ? bgTex : silencedBg);

            GUIStyle rowStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = rowBg },
                margin = new RectOffset(0, 0, 2, 2)
            };

            GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(32));

            // Status indicator
            Color statusColor = enabled && !isSilenced ? Color.green : Color.gray;
            GUI.contentColor = statusColor;
            GUILayout.Label(enabled && !isSilenced ? ">" : "-", GUILayout.Width(15));
            GUI.contentColor = Color.white;

            // Alert name
            string displayLabel = isSilenced ? $"{label} (SIL)" : label;
            GUILayout.Label(displayLabel, alertLabelStyle, GUILayout.Width(120));

            // Silence button
            if (isSilenced)
            {
                if (GUILayout.Button("RST", buttonStyle, GUILayout.Width(40), GUILayout.Height(24)))
                {
                    Unsilence(type);
                }
            }
            else
            {
                if (GUILayout.Button("SIL", buttonStyle, GUILayout.Width(40), GUILayout.Height(24)))
                {
                    Silence(type);
                }
            }

            // Test button
            if (GUILayout.Button("TEST", buttonStyle, GUILayout.Width(45), GUILayout.Height(24)))
            {
                AlertManager.Instance?.TriggerTestAlert(type);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSettingsTab(AlertConfig config)
        {
            // Alert toggles
            GUILayout.Label("Enable/Disable Alerts", sectionStyle);
            config.TerrainEnabled = GUILayout.Toggle(config.TerrainEnabled, "Terrain Proximity (GPWS)", toggleStyle);
            config.GearEnabled = GUILayout.Toggle(config.GearEnabled, "Landing Gear Warning", toggleStyle);
            config.FuelEnabled = GUILayout.Toggle(config.FuelEnabled, "Low Fuel Warning", toggleStyle);
            config.PowerEnabled = GUILayout.Toggle(config.PowerEnabled, "Low Power Warning", toggleStyle);
            config.OverheatEnabled = GUILayout.Toggle(config.OverheatEnabled, "Overheat Warning", toggleStyle);
            config.HighGEnabled = GUILayout.Toggle(config.HighGEnabled, "High G-Force Warning", toggleStyle);
            config.CommsEnabled = GUILayout.Toggle(config.CommsEnabled, "Communications Lost", toggleStyle);

            GUILayout.Space(15);

            // Terrain settings
            GUILayout.Label("Terrain Warning (GPWS)", sectionStyle);
            DrawSlider("Time to Impact (s):", ref config.TerrainWarningTime, 3f, 15f);
            DrawSlider("Fallback Alt (m):", ref config.TerrainWarningAltitude, 100f, 2000f);
            DrawSlider("Descent Rate (m/s):", ref config.TerrainMinDescentRate, -200f, -10f);

            GUILayout.Space(10);

            // Gear settings
            GUILayout.Label("Gear Warning", sectionStyle);
            DrawSlider("Trigger Alt (m):", ref config.GearWarningAltitude, 50f, 1000f);
            DrawSlider("Max Speed (m/s):", ref config.GearWarningSpeed, 50f, 300f);

            GUILayout.Space(10);

            // Resource settings
            GUILayout.Label("Resource Warnings", sectionStyle);
            DrawSlider("Fuel Warning (%):", ref config.FuelWarningPercent, 1f, 50f);
            DrawSlider("Power Warning (%):", ref config.PowerWarningPercent, 1f, 50f);

            GUILayout.Space(10);

            // Other thresholds
            GUILayout.Label("Other Thresholds", sectionStyle);
            DrawSlider("Overheat (%):", ref config.OverheatWarningPercent, 50f, 99f);
            DrawSlider("G-Force:", ref config.HighGWarning, 3f, 15f);

            GUILayout.Space(10);

            // Visual settings
            GUILayout.Label("Visual Settings", sectionStyle);
            config.ScreenFlashEnabled = GUILayout.Toggle(config.ScreenFlashEnabled, "Screen Flash Effect", toggleStyle);
            DrawSlider("Flash Intensity:", ref config.FlashIntensity, 0.1f, 0.5f);

            GUILayout.Space(15);

            // Save/Reset buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAVE", GUILayout.Height(30)))
            {
                config.Save();
                ScreenMessages.PostScreenMessage("Settings saved", 2f);
            }
            if (GUILayout.Button("RESET DEFAULTS", GUILayout.Height(30)))
            {
                ResetToDefaults(config);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTestTab()
        {
            GUILayout.Label("Test Alert Sounds", sectionStyle);
            GUILayout.Space(5);

            // Warning tests (red)
            GUILayout.Label("WARNINGS (Red - Fire Bell)", alertLabelStyle);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("TERRAIN", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.Terrain);
            }
            if (GUILayout.Button("HEAT", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.Overheat);
            }
            if (GUILayout.Button("STALL", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.Stall);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Caution tests (amber)
            GUILayout.Label("CAUTIONS (Amber - Chime)", alertLabelStyle);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.1f);
            if (GUILayout.Button("GEAR", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.GearUp);
            }
            if (GUILayout.Button("FUEL", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.LowFuel);
            }
            if (GUILayout.Button("POWER", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.LowPower);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.1f);
            if (GUILayout.Button("G-FORCE", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.HighG);
            }
            if (GUILayout.Button("COMMS", testButtonStyle, GUILayout.Height(35)))
            {
                AlertManager.Instance?.TriggerTestAlert(AlertType.CommsLost);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Test all
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);
            if (GUILayout.Button("TEST ALL SOUNDS (Sequential)", GUILayout.Height(40)))
            {
                StartCoroutine(TestAllAlertsSequence());
            }
            GUI.backgroundColor = Color.white;
        }

        private System.Collections.IEnumerator TestAllAlertsSequence()
        {
            AlertType[] testOrder = {
                AlertType.Terrain,
                AlertType.Overheat,
                AlertType.Stall,
                AlertType.GearUp,
                AlertType.LowFuel,
                AlertType.LowPower,
                AlertType.HighG,
                AlertType.CommsLost
            };

            foreach (var alertType in testOrder)
            {
                AlertManager.Instance?.TriggerTestAlert(alertType);
                yield return new WaitForSeconds(2.5f);
            }

            ScreenMessages.PostScreenMessage("Alert test complete", 2f);
        }

        private void DrawSlider(string label, ref float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120));
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(120));
            GUILayout.Label($"{value:F1}", GUILayout.Width(50));
            GUILayout.EndHorizontal();
        }

        private void ResetToDefaults(AlertConfig config)
        {
            var defaults = new AlertConfig();
            config.TerrainWarningTime = defaults.TerrainWarningTime;
            config.TerrainWarningAltitude = defaults.TerrainWarningAltitude;
            config.TerrainMinDescentRate = defaults.TerrainMinDescentRate;
            config.GearWarningAltitude = defaults.GearWarningAltitude;
            config.GearWarningSpeed = defaults.GearWarningSpeed;
            config.FuelWarningPercent = defaults.FuelWarningPercent;
            config.PowerWarningPercent = defaults.PowerWarningPercent;
            config.OverheatWarningPercent = defaults.OverheatWarningPercent;
            config.HighGWarning = defaults.HighGWarning;
            config.ScreenFlashEnabled = defaults.ScreenFlashEnabled;
            config.FlashIntensity = defaults.FlashIntensity;
            ScreenMessages.PostScreenMessage("Settings reset to defaults", 2f);
        }

        // Silence methods
        public void Silence(AlertType type)
        {
            silencedAlerts.Add(type);
            silenceTimers[type] = Time.time + SILENCE_DURATION;
        }

        public void Unsilence(AlertType type)
        {
            silencedAlerts.Remove(type);
            silenceTimers.Remove(type);
        }

        public void SilenceAll()
        {
            foreach (AlertType type in System.Enum.GetValues(typeof(AlertType)))
            {
                silencedAlerts.Add(type);
                silenceTimers[type] = Time.time + SILENCE_DURATION;
            }
            ScreenMessages.PostScreenMessage("All alerts silenced for 60s", 2f);
        }

        public void UnsilenceAll()
        {
            silencedAlerts.Clear();
            silenceTimers.Clear();
            ScreenMessages.PostScreenMessage("All alerts reset", 2f);
        }

        public bool IsAlertSilenced(AlertType type)
        {
            return silencedAlerts.Contains(type);
        }

        // Public methods for toolbar
        public void ShowWindow()
        {
            showWindow = true;
        }

        public void HideWindow()
        {
            showWindow = false;
        }

        public void ToggleWindow()
        {
            showWindow = !showWindow;
        }

        public bool IsWindowVisible()
        {
            return showWindow;
        }
    }
}
