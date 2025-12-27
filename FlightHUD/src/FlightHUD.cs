using System;
using UnityEngine;

namespace FlightHUD
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightHUDMain : MonoBehaviour
    {
        public static FlightHUDMain Instance { get; private set; }
        public static HUDConfig Config { get; private set; }
        public static bool ShowSettingsGUI { get; set; }

        private Material lineMaterial;
        private GUIStyle labelStyle;
        private GUIStyle labelStyleSmall;
        private GUIStyle labelStyleCenter;

        private Vessel vessel;
        private float screenCenterX;
        private float screenCenterY;
        private float pixelsPerDegree;

        private FlightData flightData;

        public void Start()
        {
            Instance = this;
            Config = HUDConfig.Load();
            CreateLineMaterial();

            GameEvents.onVesselChange.Add(OnVesselChange);

            if (FlightGlobals.ActiveVessel != null)
            {
                vessel = FlightGlobals.ActiveVessel;
            }

            Debug.Log("[FlightHUD] Started - Press " + Config.HUDToggleKey + " to toggle, use toolbar button for settings");
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
            }
            Instance = null;
        }

        private void OnVesselChange(Vessel newVessel)
        {
            vessel = newVessel;
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

        public void Update()
        {
            if (Input.GetKeyDown(Config.HUDToggleKey))
            {
                Config.HUDEnabled = !Config.HUDEnabled;
                ScreenMessages.PostScreenMessage(
                    "Flight HUD: " + (Config.HUDEnabled ? "ON" : "OFF"),
                    2f, ScreenMessageStyle.UPPER_CENTER);
            }

            if (vessel == null || vessel != FlightGlobals.ActiveVessel)
            {
                vessel = FlightGlobals.ActiveVessel;
            }

            if (vessel != null && Config.HUDEnabled)
            {
                UpdateFlightData();
            }
        }

        private void UpdateFlightData()
        {
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 north = Vector3.ProjectOnPlane(vessel.mainBody.transform.up, up).normalized;
            Vector3 forward = vessel.transform.up;

            // Pitch
            Vector3 forwardHoriz = Vector3.ProjectOnPlane(forward, up).normalized;
            flightData.Pitch = Vector3.Angle(forwardHoriz, forward);
            if (Vector3.Dot(forward, up) < 0) flightData.Pitch = -flightData.Pitch;

            // Roll
            Vector3 right = vessel.transform.right;
            Vector3 rightHoriz = Vector3.ProjectOnPlane(right, up).normalized;
            flightData.Roll = Vector3.SignedAngle(rightHoriz, right, forward);

            // Heading
            flightData.Heading = Vector3.SignedAngle(north, forwardHoriz, up);
            if (flightData.Heading < 0) flightData.Heading += 360f;

            // Flight Path Vector
            if (vessel.srfSpeed > 5)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 velocityWorld = vessel.srf_velocity.normalized;
                    Vector3 velocityCam = cam.transform.InverseTransformDirection(velocityWorld);
                    flightData.FPVOffset = new Vector2(
                        Mathf.Atan2(velocityCam.x, velocityCam.z) * Mathf.Rad2Deg,
                        Mathf.Atan2(velocityCam.y, velocityCam.z) * Mathf.Rad2Deg);
                }
            }
            else
            {
                flightData.FPVOffset = Vector2.zero;
            }

            // AOA
            if (vessel.srfSpeed > 1)
            {
                Vector3 velocity = vessel.srf_velocity.normalized;
                flightData.AOA = Vector3.Angle(velocity, forward);
                Vector3 cross = Vector3.Cross(velocity, forward);
                if (Vector3.Dot(cross, vessel.transform.right) < 0) flightData.AOA = -flightData.AOA;
            }
            else
            {
                flightData.AOA = 0f;
            }

            // Basic telemetry
            flightData.Airspeed = (float)vessel.srfSpeed;
            flightData.Altitude = (float)vessel.altitude;
            flightData.RadarAltitude = vessel.heightFromTerrain > 0 ? (float)vessel.heightFromTerrain : flightData.Altitude;
            flightData.VerticalSpeed = (float)vessel.verticalSpeed;
            flightData.GForce = (float)vessel.geeForce;
            flightData.Mach = flightData.Airspeed / 343f;
        }

        public void OnGUI()
        {
            if (!Config.HUDEnabled || vessel == null) return;
            if (!HighLogic.LoadedSceneIsFlight) return;
            if (MapView.MapIsEnabled) return;

            if (labelStyle == null)
            {
                InitStyles();
            }

            screenCenterX = Screen.width / 2f;
            screenCenterY = Screen.height / 2f;
            pixelsPerDegree = Screen.height / (Camera.main != null ? Camera.main.fieldOfView : 60f);

            Color hudColor = Config.GetHUDColor();
            Color hudColorDim = Config.GetHUDColorDim();
            float scale = Config.HUDScale;

            GL.PushMatrix();
            lineMaterial.SetPass(0);
            GL.LoadPixelMatrix();

            if (Config.ShowBankIndicator)
                HUDElements.DrawBankIndicator(flightData.Roll, screenCenterX, screenCenterY, scale, hudColor);

            if (Config.ShowPitchLadder)
                HUDElements.DrawPitchLadder(flightData.Pitch, flightData.Roll, screenCenterX, screenCenterY, pixelsPerDegree, scale, hudColor, hudColorDim);

            if (Config.ShowAircraftSymbol)
                HUDElements.DrawAircraftSymbol(screenCenterX, screenCenterY, scale, hudColor);

            if (Config.ShowFlightPathVector)
                HUDElements.DrawFlightPathVector(flightData.FPVOffset, screenCenterX, screenCenterY, pixelsPerDegree, scale, hudColor);

            if (Config.ShowHeadingTape)
                HUDElements.DrawHeadingTape(flightData.Heading, screenCenterX, scale, hudColor);

            if (Config.ShowAirspeedTape)
                HUDElements.DrawAirspeedTape(flightData.Airspeed, screenCenterY, scale, hudColor);

            if (Config.ShowAltitudeTape)
                HUDElements.DrawAltitudeTape(flightData.Altitude, flightData.RadarAltitude, screenCenterY, scale, hudColor);

            if (Config.ShowVSI)
                HUDElements.DrawVerticalSpeedIndicator(flightData.VerticalSpeed, screenCenterY, scale, hudColor);

            if (Config.ShowCompassRose)
                HUDElements.DrawCompassRose(flightData.Heading, screenCenterX, scale, hudColor);

            GL.PopMatrix();

            DrawTextLabels(hudColor);
        }

        private void InitStyles()
        {
            Color hudColor = Config.GetHUDColor();

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = hudColor;
            labelStyle.fontSize = (int)(14 * Config.HUDScale);
            labelStyle.fontStyle = FontStyle.Bold;

            labelStyleSmall = new GUIStyle(labelStyle);
            labelStyleSmall.fontSize = (int)(11 * Config.HUDScale);

            labelStyleCenter = new GUIStyle(labelStyle);
            labelStyleCenter.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawTextLabels(Color hudColor)
        {
            float scale = Config.HUDScale;

            labelStyle.normal.textColor = hudColor;
            labelStyleSmall.normal.textColor = hudColor;
            labelStyleCenter.normal.textColor = hudColor;

            // Airspeed
            if (Config.ShowAirspeedTape)
            {
                string speedStr = flightData.Airspeed.ToString("F0");
                GUI.Label(new Rect(80 * scale, screenCenterY - 10, 60, 25), speedStr, labelStyle);

                if (flightData.Mach > 0.8f)
                {
                    GUI.Label(new Rect(80 * scale, screenCenterY + 15, 60, 20), "M" + flightData.Mach.ToString("F2"), labelStyleSmall);
                }
            }

            // Altitude
            if (Config.ShowAltitudeTape)
            {
                string altStr;
                if (flightData.Altitude >= 10000)
                    altStr = (flightData.Altitude / 1000f).ToString("F1") + "k";
                else
                    altStr = flightData.Altitude.ToString("F0");
                GUI.Label(new Rect(Screen.width - 140 * scale, screenCenterY - 10, 60, 25), altStr, labelStyle);

                if (flightData.RadarAltitude < 1000 && flightData.RadarAltitude > 0)
                {
                    GUI.Label(new Rect(Screen.width - 140 * scale, screenCenterY + 15, 60, 20),
                        "R" + flightData.RadarAltitude.ToString("F0"), labelStyleSmall);
                }
            }

            // Vertical speed
            if (Config.ShowVSI)
            {
                string vsStr = (flightData.VerticalSpeed >= 0 ? "+" : "") + flightData.VerticalSpeed.ToString("F0");
                GUI.Label(new Rect(Screen.width - 55 * scale, screenCenterY - 10, 50, 25), vsStr, labelStyleSmall);
            }

            // Heading
            if (Config.ShowHeadingTape)
            {
                string hdgStr = flightData.Heading.ToString("F0").PadLeft(3, '0');
                GUI.Label(new Rect(screenCenterX - 20, 55 * scale, 40, 25), hdgStr, labelStyleCenter);

                DrawHeadingLabels(scale);
            }

            // G-Force and AOA
            if (Config.ShowGForceAOA)
            {
                GUI.Label(new Rect(screenCenterX - 80 * scale, Screen.height - 60 * scale, 60, 25),
                    "G " + flightData.GForce.ToString("F1"), labelStyle);

                GUI.Label(new Rect(screenCenterX + 30 * scale, Screen.height - 60 * scale, 80, 25),
                    "AOA " + flightData.AOA.ToString("F1") + "\u00B0", labelStyle);
            }

            // Compass rose labels
            if (Config.ShowCompassRose)
            {
                DrawCompassLabels(scale);
            }

            // Pitch ladder labels
            if (Config.ShowPitchLadder)
            {
                DrawPitchLabels(scale);
            }
        }

        private void DrawHeadingLabels(float scale)
        {
            float tapeWidth = 300 * scale;
            float tapeY = 22 * scale;
            float pixelsPerHeadingDeg = tapeWidth / 60f;

            string[] cardinals = { "N", "E", "S", "W" };
            int[] cardinalDegrees = { 0, 90, 180, 270 };

            for (int i = 0; i < 4; i++)
            {
                float offset = Mathf.DeltaAngle(flightData.Heading, cardinalDegrees[i]) * pixelsPerHeadingDeg;
                float x = screenCenterX + offset;

                if (Mathf.Abs(offset) < tapeWidth / 2)
                {
                    GUI.Label(new Rect(x - 10, tapeY, 20, 20), cardinals[i], labelStyleCenter);
                }
            }
        }

        private void DrawCompassLabels(float scale)
        {
            float cy = Screen.height - 80 * scale;
            float radius = 50 * scale;

            string[] cardinals = { "N", "E", "S", "W" };
            int[] cardinalDegrees = { 0, 90, 180, 270 };

            for (int i = 0; i < 4; i++)
            {
                float angle = (90 - (cardinalDegrees[i] - flightData.Heading)) * Mathf.Deg2Rad;
                float x = screenCenterX + Mathf.Cos(angle) * (radius - 20 * scale);
                float y = cy + Mathf.Sin(angle) * (radius - 20 * scale);

                GUI.Label(new Rect(x - 10, y - 10, 20, 20), cardinals[i], labelStyleCenter);
            }
        }

        private void DrawPitchLabels(float scale)
        {
            float ladderWidth = 120f * scale;

            for (int deg = -90; deg <= 90; deg += 10)
            {
                if (deg == 0) continue;

                float yOffset = (deg - flightData.Pitch) * pixelsPerDegree;
                float y = screenCenterY - yOffset;

                if (y < 80 || y > Screen.height - 80) continue;

                float labelX = screenCenterX + ladderWidth + 10 * scale;
                float labelY = y - 8;

                // Apply roll rotation
                float dx = labelX - screenCenterX;
                float dy = labelY - screenCenterY;
                float rollRad = -flightData.Roll * Mathf.Deg2Rad;
                float rotX = dx * Mathf.Cos(rollRad) - dy * Mathf.Sin(rollRad);
                float rotY = dx * Mathf.Sin(rollRad) + dy * Mathf.Cos(rollRad);

                GUI.Label(new Rect(screenCenterX + rotX, screenCenterY + rotY, 30, 20),
                    deg.ToString(), labelStyleSmall);
            }
        }

        public void ToggleHUD()
        {
            Config.HUDEnabled = !Config.HUDEnabled;
            ScreenMessages.PostScreenMessage(
                "Flight HUD: " + (Config.HUDEnabled ? "ON" : "OFF"),
                2f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
