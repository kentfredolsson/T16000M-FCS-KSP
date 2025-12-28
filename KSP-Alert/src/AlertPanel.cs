using System.Collections.Generic;
using UnityEngine;

namespace KSPAlert
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AlertPanel : MonoBehaviour
    {
        private bool showPanel = true;
        private bool panelMinimized = false;
        private Rect panelRect;
        private int windowId;

        // Silenced alerts (temporary until condition clears)
        private HashSet<AlertType> silencedAlerts = new HashSet<AlertType>();
        private Dictionary<AlertType, float> silenceTimers = new Dictionary<AlertType, float>();

        // Styles
        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle alertLabelStyle;
        private GUIStyle silenceButtonStyle;
        private GUIStyle volumeButtonStyle;
        private GUIStyle minimizedStyle;
        private bool stylesInitialized = false;

        // Textures
        private Texture2D panelBg;
        private Texture2D warningBg;
        private Texture2D cautionBg;
        private Texture2D advisoryBg;
        private Texture2D silencedBg;

        private const float PANEL_WIDTH = 220f;
        private const float ALERT_HEIGHT = 35f;
        private const float SILENCE_DURATION = 60f; // Auto-unsilence after 60 seconds

        void Awake()
        {
            windowId = GetInstanceID();
            panelRect = new Rect(Screen.width - PANEL_WIDTH - 20, 100, PANEL_WIDTH, 200);
            CreateTextures();
        }

        void Update()
        {
            // Toggle panel with F11
            if (Input.GetKeyDown(KeyCode.F11))
            {
                showPanel = !showPanel;
            }

            // Update silence timers
            UpdateSilenceTimers();
        }

        private void UpdateSilenceTimers()
        {
            var expired = new List<AlertType>();

            foreach (var kvp in silenceTimers)
            {
                if (Time.time > kvp.Value)
                {
                    expired.Add(kvp.Key);
                }
            }

            foreach (var type in expired)
            {
                silencedAlerts.Remove(type);
                silenceTimers.Remove(type);
            }
        }

        private void CreateTextures()
        {
            panelBg = MakeTexture(new Color(0.1f, 0.1f, 0.15f, 0.85f));
            warningBg = MakeTexture(new Color(0.7f, 0.1f, 0.1f, 0.9f));
            cautionBg = MakeTexture(new Color(0.7f, 0.5f, 0.1f, 0.9f));
            advisoryBg = MakeTexture(new Color(0.1f, 0.3f, 0.6f, 0.9f));
            silencedBg = MakeTexture(new Color(0.3f, 0.3f, 0.3f, 0.7f));
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

            panelStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = panelBg },
                padding = new RectOffset(5, 5, 25, 5)
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            alertLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                padding = new RectOffset(5, 0, 0, 0)
            };

            silenceButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(2, 2, 2, 2)
            };

            volumeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(2, 2, 2, 2)
            };

            minimizedStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = panelBg, textColor = Color.white }
            };

            stylesInitialized = true;
        }

        void OnGUI()
        {
            if (!showPanel) return;
            if (!HighLogic.LoadedSceneIsFlight) return;

            InitializeStyles();

            if (panelMinimized)
            {
                DrawMinimizedPanel();
            }
            else
            {
                panelRect = GUILayout.Window(
                    windowId,
                    panelRect,
                    DrawPanel,
                    "",
                    panelStyle,
                    GUILayout.Width(PANEL_WIDTH)
                );
            }
        }

        private void DrawMinimizedPanel()
        {
            var config = AlertManager.Instance?.Config;
            int activeCount = GetActiveAlertCount();

            string label = activeCount > 0 ? $"ALERTS ({activeCount})" : "ALERTS";
            Color bgColor = activeCount > 0 ? new Color(0.7f, 0.2f, 0.2f, 0.9f) : new Color(0.2f, 0.4f, 0.2f, 0.9f);

            Texture2D bg = MakeTexture(bgColor);
            GUIStyle style = new GUIStyle(minimizedStyle) { normal = { background = bg } };

            Rect minRect = new Rect(panelRect.x, panelRect.y, 80, 30);
            if (GUI.Button(minRect, label, style))
            {
                panelMinimized = false;
            }
        }

        private int GetActiveAlertCount()
        {
            // This would need to be exposed from AlertManager
            // For now, approximate
            return 0;
        }

        private void DrawPanel(int id)
        {
            var config = AlertManager.Instance?.Config;
            if (config == null) return;

            // Header with controls
            GUILayout.BeginHorizontal();

            // Minimize button
            if (GUILayout.Button("_", GUILayout.Width(25), GUILayout.Height(20)))
            {
                panelMinimized = true;
            }

            GUILayout.Label("KSP-ALERT", headerStyle);

            // Master toggle
            string masterLabel = config.Enabled ? "ON" : "OFF";
            Color masterColor = config.Enabled ? Color.green : Color.red;
            GUI.contentColor = masterColor;
            if (GUILayout.Button(masterLabel, GUILayout.Width(35), GUILayout.Height(20)))
            {
                config.Enabled = !config.Enabled;
            }
            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Volume control
            DrawVolumeControl(config);

            GUILayout.Space(10);

            // Alert type rows
            DrawAlertRow("TERRAIN", AlertType.Terrain, config.TerrainEnabled, warningBg);
            DrawAlertRow("GEAR", AlertType.GearUp, config.GearEnabled, cautionBg);
            DrawAlertRow("FUEL", AlertType.LowFuel, config.FuelEnabled, cautionBg);
            DrawAlertRow("POWER", AlertType.LowPower, config.PowerEnabled, cautionBg);
            DrawAlertRow("HEAT", AlertType.Overheat, config.OverheatEnabled, warningBg);
            DrawAlertRow("G-FORCE", AlertType.HighG, config.HighGEnabled, cautionBg);
            DrawAlertRow("COMMS", AlertType.CommsLost, config.CommsEnabled, cautionBg);

            GUILayout.Space(10);

            // Bottom buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SILENCE ALL", GUILayout.Height(25)))
            {
                SilenceAll();
            }
            if (GUILayout.Button("RESET", GUILayout.Height(25)))
            {
                UnsilenceAll();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("F10: Settings | F11: Toggle Panel", GUI.skin.label);

            GUI.DragWindow();
        }

        private void DrawVolumeControl(AlertConfig config)
        {
            GUILayout.BeginHorizontal();

            // Mute button
            string muteLabel = config.AudioEnabled ? "AUDIO" : "MUTED";
            Color muteColor = config.AudioEnabled ? Color.white : Color.red;
            GUI.contentColor = muteColor;
            if (GUILayout.Button(muteLabel, GUILayout.Width(55), GUILayout.Height(22)))
            {
                config.AudioEnabled = !config.AudioEnabled;
            }
            GUI.contentColor = Color.white;

            // Volume down
            if (GUILayout.Button("-", volumeButtonStyle, GUILayout.Width(25), GUILayout.Height(22)))
            {
                config.MasterVolume = Mathf.Max(0f, config.MasterVolume - 0.1f);
            }

            // Volume bar
            float vol = config.MasterVolume;
            GUILayout.Label($"{vol * 100:F0}%", headerStyle, GUILayout.Width(45));

            // Volume up
            if (GUILayout.Button("+", volumeButtonStyle, GUILayout.Width(25), GUILayout.Height(22)))
            {
                config.MasterVolume = Mathf.Min(1f, config.MasterVolume + 0.1f);
            }

            GUILayout.EndHorizontal();

            // Volume slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(60);
            config.MasterVolume = GUILayout.HorizontalSlider(config.MasterVolume, 0f, 1f, GUILayout.Width(140));
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

            GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(ALERT_HEIGHT));

            // Status indicator
            Color statusColor = enabled && !isSilenced ? Color.green : Color.gray;
            GUI.contentColor = statusColor;
            GUILayout.Label(enabled && !isSilenced ? ">" : "-", GUILayout.Width(15));
            GUI.contentColor = Color.white;

            // Alert name
            string displayLabel = isSilenced ? $"{label} (SILENCED)" : label;
            GUILayout.Label(displayLabel, alertLabelStyle, GUILayout.Width(110));

            // Silence/Unsilence button
            if (isSilenced)
            {
                if (GUILayout.Button("RST", silenceButtonStyle, GUILayout.Width(35), GUILayout.Height(25)))
                {
                    Unsilence(type);
                }
            }
            else
            {
                if (GUILayout.Button("SIL", silenceButtonStyle, GUILayout.Width(35), GUILayout.Height(25)))
                {
                    Silence(type);
                }
            }

            GUILayout.EndHorizontal();
        }

        public void Silence(AlertType type)
        {
            silencedAlerts.Add(type);
            silenceTimers[type] = Time.time + SILENCE_DURATION;
            ScreenMessages.PostScreenMessage($"{type} alert silenced for 60s", 2f);
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

        public static AlertPanel Instance { get; private set; }

        void Start()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            Instance = null;
        }
    }
}
