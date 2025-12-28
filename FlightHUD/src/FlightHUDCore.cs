using UnityEngine;

namespace FlightHUD
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightHUDCore : MonoBehaviour
    {
        public static FlightHUDCore Instance { get; private set; }

        // Rendering
        private Material lineMaterial;
        private float screenCenterX;
        private float screenCenterY;
        private float pixelsPerDegree;

        // Styles
        private GUIStyle labelStyle;
        private GUIStyle labelStyleSmall;
        private bool stylesInitialized = false;

        public void Start()
        {
            Instance = this;
            CreateLineMaterial();
            HUDSettings.Load();
            Debug.Log("[FlightHUD] Started");
        }

        public void OnDestroy()
        {
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
            }
            HUDSettings.Save();
            Instance = null;
            Debug.Log("[FlightHUD] Destroyed");
        }

        private void CreateLineMaterial()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = HUDSettings.GetHUDColor();
            labelStyle.fontSize = Mathf.RoundToInt(12 * HUDSettings.HUDScale);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            labelStyleSmall = new GUIStyle(labelStyle);
            labelStyleSmall.fontSize = Mathf.RoundToInt(10 * HUDSettings.HUDScale);

            stylesInitialized = true;
        }

        public void ResetStyles()
        {
            stylesInitialized = false;
        }

        public void Update()
        {
            // Toggle HUD with hotkey
            if (Input.GetKeyDown(HUDSettings.ToggleKey))
            {
                HUDSettings.HUDEnabled = !HUDSettings.HUDEnabled;
                ScreenMessages.PostScreenMessage(
                    "Flight HUD: " + (HUDSettings.HUDEnabled ? "ON" : "OFF"),
                    2f, ScreenMessageStyle.UPPER_CENTER);
            }

            // Update flight data if HUD is enabled
            if (HUDSettings.HUDEnabled && FlightGlobals.ActiveVessel != null)
            {
                FlightData.Update();
            }
        }

        public void OnGUI()
        {
            // Always draw settings window if visible (even when HUD is off)
            if (HUDSettings.SettingsWindowVisible)
            {
                HUDSettingsWindow.Draw();
            }

            if (!HUDSettings.HUDEnabled) return;
            if (FlightGlobals.ActiveVessel == null) return;
            if (MapView.MapIsEnabled) return;

            InitStyles();

            // Update screen metrics
            screenCenterX = Screen.width / 2f;
            screenCenterY = Screen.height / 2f;

            // Calculate pixels per degree based on camera FOV
            Camera cam = Camera.main;
            if (cam != null)
            {
                pixelsPerDegree = Screen.height / cam.fieldOfView;
            }
            else
            {
                pixelsPerDegree = Screen.height / 60f;
            }

            float scale = HUDSettings.HUDScale;
            Color color = HUDSettings.GetHUDColor();
            Color colorDim = new Color(color.r, color.g, color.b, color.a * 0.6f);

            // Update label style colors
            labelStyle.normal.textColor = color;
            labelStyleSmall.normal.textColor = color;

            // Setup GL rendering
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix();

            // Draw HUD elements
            if (HUDSettings.ShowPitchLadder)
            {
                PitchLadder.Draw(FlightData.Pitch, FlightData.Roll, screenCenterX, screenCenterY, pixelsPerDegree, scale, color, colorDim);
            }

            HUDRenderer.DrawAircraftSymbol(screenCenterX, screenCenterY, scale, color);

            if (HUDSettings.ShowFPV)
            {
                HUDRenderer.DrawFlightPathVector(FlightData.FPVOffset, screenCenterX, screenCenterY, pixelsPerDegree, scale, color);
            }

            if (HUDSettings.ShowBankIndicator)
            {
                HUDRenderer.DrawBankIndicator(FlightData.Roll, screenCenterX, screenCenterY, scale, color);
            }

            if (HUDSettings.ShowHeadingTape)
            {
                HUDRenderer.DrawHeadingTape(FlightData.Heading, screenCenterX, scale, color);
            }

            if (HUDSettings.ShowAirspeedTape)
            {
                HUDRenderer.DrawAirspeedTape(FlightData.Airspeed, screenCenterY, scale, color);
            }

            if (HUDSettings.ShowAltitudeTape)
            {
                HUDRenderer.DrawAltitudeTape(FlightData.AltitudeASL, FlightData.AltitudeAGL, screenCenterY, scale, color);
            }

            if (HUDSettings.ShowVSI)
            {
                HUDRenderer.DrawVerticalSpeedIndicator(FlightData.VerticalSpeed, screenCenterY, scale, color);
            }

            if (HUDSettings.ShowCompassRose)
            {
                HUDRenderer.DrawCompassRose(FlightData.Heading, screenCenterX, scale, color);
            }

            if (HUDSettings.ShowStatusIcons)
            {
                HUDRenderer.DrawStatusIcons(FlightData.BrakesOn, FlightData.LightsOn, FlightData.GearDeployed, scale, color, colorDim);
            }

            GL.PopMatrix();

            // Draw text labels (outside GL matrix)
            if (HUDSettings.ShowPitchLadder)
            {
                PitchLadder.DrawLabels(FlightData.Pitch, FlightData.Roll, screenCenterX, screenCenterY, pixelsPerDegree, scale, labelStyleSmall);
            }

            if (HUDSettings.ShowHeadingTape)
            {
                HUDRenderer.DrawHeadingLabels(FlightData.Heading, screenCenterX, scale, labelStyleSmall);
            }

            if (HUDSettings.ShowAirspeedTape)
            {
                HUDRenderer.DrawAirspeedLabels(FlightData.Airspeed, screenCenterY, scale, labelStyleSmall);
            }

            if (HUDSettings.ShowAltitudeTape)
            {
                HUDRenderer.DrawAltitudeLabels(FlightData.AltitudeASL, screenCenterY, scale, labelStyleSmall);
            }

            if (HUDSettings.ShowCompassRose)
            {
                HUDRenderer.DrawCompassLabels(FlightData.Heading, screenCenterX, scale, labelStyleSmall);
            }

            if (HUDSettings.ShowStatusIcons)
            {
                GUIStyle dimStyle = new GUIStyle(labelStyleSmall);
                dimStyle.normal.textColor = colorDim;
                HUDRenderer.DrawStatusLabels(FlightData.BrakesOn, FlightData.LightsOn, FlightData.GearDeployed, scale, dimStyle);
            }

            // Draw text readouts
            DrawTextReadouts(scale, color);
        }

        private void DrawTextReadouts(float scale, Color color)
        {
            GUIStyle style = new GUIStyle(labelStyle);
            style.normal.textColor = color;

            float bottomY = Screen.height - 50 * scale;
            float leftX = 20 * scale;

            // Left side readouts
            float yOffset = 0;

            if (HUDSettings.ShowGForce)
            {
                GUI.Label(new Rect(leftX, bottomY - yOffset, 100, 20), $"G: {FlightData.GForce:F1}", style);
                yOffset += 20 * scale;
            }

            if (HUDSettings.ShowAOA)
            {
                GUI.Label(new Rect(leftX, bottomY - yOffset, 100, 20), $"AOA: {FlightData.AOA:F1}", style);
                yOffset += 20 * scale;
            }

            if (HUDSettings.ShowMach)
            {
                GUI.Label(new Rect(leftX, bottomY - yOffset, 100, 20), $"M: {FlightData.MachNumber:F2}", style);
                yOffset += 20 * scale;
            }

            // Right side - radar altitude
            if (HUDSettings.ShowRadarAlt)
            {
                float rightX = Screen.width - 120 * scale;
                string radarStr = FlightData.AltitudeAGL < 1000
                    ? $"R {FlightData.AltitudeAGL:F0}"
                    : $"R {FlightData.AltitudeAGL / 1000:F1}k";
                GUI.Label(new Rect(rightX, bottomY, 100, 20), radarStr, style);
            }
        }
    }
}
