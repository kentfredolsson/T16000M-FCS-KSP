using System;
using UnityEngine;

namespace T16000M_FCS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightHUD : MonoBehaviour
    {
        // Configuration reference
        private Config config;

        // Rendering
        private Material lineMaterial;
        private GUIStyle labelStyle;
        private GUIStyle labelStyleSmall;
        private GUIStyle labelStyleCenter;

        // State
        private bool showHUD = true;
        private Vessel vessel;

        // Screen metrics
        private float screenCenterX;
        private float screenCenterY;
        private float pixelsPerDegree;

        // HUD colors
        private Color hudColor;
        private Color hudColorDim;

        public void Start()
        {
            config = Config.Load();
            CreateLineMaterial();

            GameEvents.onVesselChange.Add(OnVesselChange);

            if (FlightGlobals.ActiveVessel != null)
            {
                vessel = FlightGlobals.ActiveVessel;
            }

            Debug.Log("[T16000M_FCS] HUD Started - Press " + config.HUDKey + " to toggle");
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
            }
        }

        private void OnVesselChange(Vessel newVessel)
        {
            vessel = newVessel;
        }

        private void CreateLineMaterial()
        {
            // Unity's built-in shader for colored lines
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
            // Toggle HUD
            if (Input.GetKeyDown(config.HUDKey))
            {
                showHUD = !showHUD;
                ScreenMessages.PostScreenMessage(
                    "HUD: " + (showHUD ? "ON" : "OFF"),
                    2f, ScreenMessageStyle.UPPER_CENTER);
            }

            // Update vessel reference
            if (vessel == null || vessel != FlightGlobals.ActiveVessel)
            {
                vessel = FlightGlobals.ActiveVessel;
            }
        }

        public void OnGUI()
        {
            if (!showHUD || vessel == null || !config.HUDEnabled) return;
            if (!HighLogic.LoadedSceneIsFlight) return;

            // Don't show in map view
            if (MapView.MapIsEnabled) return;

            // Initialize styles if needed
            if (labelStyle == null)
            {
                InitStyles();
            }

            // Update screen metrics
            screenCenterX = Screen.width / 2f;
            screenCenterY = Screen.height / 2f;
            pixelsPerDegree = Screen.height / (Camera.main != null ? Camera.main.fieldOfView : 60f);

            // Update colors from config
            hudColor = new Color(config.HUDColorR, config.HUDColorG, config.HUDColorB, config.HUDOpacity);
            hudColorDim = new Color(config.HUDColorR, config.HUDColorG, config.HUDColorB, config.HUDOpacity * 0.6f);

            // Draw all HUD elements
            DrawHUD();
        }

        private void InitStyles()
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = hudColor;
            labelStyle.fontSize = (int)(14 * config.HUDScale);
            labelStyle.fontStyle = FontStyle.Bold;

            labelStyleSmall = new GUIStyle(labelStyle);
            labelStyleSmall.fontSize = (int)(11 * config.HUDScale);

            labelStyleCenter = new GUIStyle(labelStyle);
            labelStyleCenter.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawHUD()
        {
            // Get flight data
            float pitch = GetPitch();
            float roll = GetRoll();
            float heading = GetHeading();
            float airspeed = (float)vessel.srfSpeed;
            float altitude = (float)vessel.altitude;
            float radarAlt = vessel.heightFromTerrain > 0 ? (float)vessel.heightFromTerrain : altitude;
            float vertSpeed = (float)vessel.verticalSpeed;
            float gForce = (float)vessel.geeForce;
            float aoa = GetAOA();
            float mach = airspeed / 343f; // Approximate speed of sound

            // Calculate flight path vector position
            Vector2 fpvOffset = GetFlightPathVectorOffset();

            // Begin GL drawing
            GL.PushMatrix();
            lineMaterial.SetPass(0);
            GL.LoadPixelMatrix();

            // Draw elements (order matters for layering)
            DrawBankIndicator(roll);
            DrawPitchLadder(pitch, roll);
            DrawAircraftSymbol();
            DrawFlightPathVector(fpvOffset);
            DrawHeadingTape(heading);
            DrawAirspeedTape(airspeed, mach);
            DrawAltitudeTape(altitude, radarAlt);
            DrawVerticalSpeedIndicator(vertSpeed);

            GL.PopMatrix();

            // Draw text labels (after GL drawing)
            DrawTextLabels(airspeed, altitude, radarAlt, vertSpeed, gForce, aoa, heading, mach);
        }

        // ============================================
        // Flight Data Calculations
        // ============================================

        private float GetPitch()
        {
            if (vessel == null) return 0f;
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 forward = vessel.transform.up; // In KSP, transform.up is forward
            Vector3 right = vessel.transform.right;

            // Project forward onto horizontal plane
            Vector3 forwardHoriz = Vector3.ProjectOnPlane(forward, up).normalized;
            float pitch = Vector3.Angle(forwardHoriz, forward);
            if (Vector3.Dot(forward, up) < 0) pitch = -pitch;

            return pitch;
        }

        private float GetRoll()
        {
            if (vessel == null) return 0f;
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 right = vessel.transform.right;

            // Project vessel right onto plane perpendicular to up
            Vector3 rightHoriz = Vector3.ProjectOnPlane(right, up).normalized;
            float roll = Vector3.SignedAngle(rightHoriz, right, vessel.transform.up);

            return roll;
        }

        private float GetHeading()
        {
            if (vessel == null) return 0f;
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 north = Vector3.ProjectOnPlane(vessel.mainBody.transform.up, up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(vessel.transform.up, up).normalized;

            float heading = Vector3.SignedAngle(north, forward, up);
            if (heading < 0) heading += 360f;

            return heading;
        }

        private float GetAOA()
        {
            if (vessel == null || vessel.srfSpeed < 1) return 0f;

            Vector3 velocity = vessel.srf_velocity.normalized;
            Vector3 forward = vessel.transform.up;

            float aoa = Vector3.Angle(velocity, forward);
            Vector3 cross = Vector3.Cross(velocity, forward);
            if (Vector3.Dot(cross, vessel.transform.right) < 0) aoa = -aoa;

            return aoa;
        }

        private Vector2 GetFlightPathVectorOffset()
        {
            if (vessel == null || vessel.srfSpeed < 5) return Vector2.zero;

            // Get velocity in camera space
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;

            Vector3 velocityWorld = vessel.srf_velocity.normalized;
            Vector3 velocityCam = cam.transform.InverseTransformDirection(velocityWorld);

            // Convert to screen offset in degrees
            float offsetX = Mathf.Atan2(velocityCam.x, velocityCam.z) * Mathf.Rad2Deg;
            float offsetY = Mathf.Atan2(velocityCam.y, velocityCam.z) * Mathf.Rad2Deg;

            return new Vector2(offsetX, offsetY);
        }

        // ============================================
        // Drawing Methods
        // ============================================

        private void DrawPitchLadder(float pitch, float roll)
        {
            float scale = config.HUDScale;
            float ladderWidth = 120f * scale;
            float gapWidth = 40f * scale;

            // Save matrix and apply rotation for roll
            GL.PushMatrix();
            Matrix4x4 rotMatrix = Matrix4x4.TRS(
                new Vector3(screenCenterX, screenCenterY, 0),
                Quaternion.Euler(0, 0, roll),
                Vector3.one);
            GL.MultMatrix(rotMatrix * Matrix4x4.TRS(new Vector3(-screenCenterX, -screenCenterY, 0), Quaternion.identity, Vector3.one));

            // Draw pitch lines from -90 to +90 in 5 degree increments
            for (int deg = -90; deg <= 90; deg += 5)
            {
                if (deg == 0) continue; // Horizon line handled separately

                float yOffset = (deg - pitch) * pixelsPerDegree;
                float y = screenCenterY - yOffset;

                // Only draw if on screen
                if (y < 50 || y > Screen.height - 50) continue;

                bool isMajor = (deg % 10 == 0);
                float width = isMajor ? ladderWidth : ladderWidth * 0.6f;
                Color color = isMajor ? hudColor : hudColorDim;

                GL.Begin(GL.LINES);
                GL.Color(color);

                if (deg > 0)
                {
                    // Above horizon - solid lines
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - gapWidth, y, 0);
                    GL.Vertex3(screenCenterX + gapWidth, y, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);

                    // End caps pointing down
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y + 8 * scale, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y + 8 * scale, 0);
                }
                else
                {
                    // Below horizon - dashed lines
                    float dashLen = 8f * scale;
                    for (float x = screenCenterX - width; x < screenCenterX - gapWidth; x += dashLen * 2)
                    {
                        GL.Vertex3(x, y, 0);
                        GL.Vertex3(Mathf.Min(x + dashLen, screenCenterX - gapWidth), y, 0);
                    }
                    for (float x = screenCenterX + gapWidth; x < screenCenterX + width; x += dashLen * 2)
                    {
                        GL.Vertex3(x, y, 0);
                        GL.Vertex3(Mathf.Min(x + dashLen, screenCenterX + width), y, 0);
                    }

                    // End caps pointing up
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y - 8 * scale, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y - 8 * scale, 0);
                }

                GL.End();
            }

            // Draw horizon line
            float horizonY = screenCenterY + pitch * pixelsPerDegree;
            if (horizonY > 50 && horizonY < Screen.height - 50)
            {
                GL.Begin(GL.LINES);
                GL.Color(hudColor);
                GL.Vertex3(screenCenterX - ladderWidth * 1.5f, horizonY, 0);
                GL.Vertex3(screenCenterX - gapWidth, horizonY, 0);
                GL.Vertex3(screenCenterX + gapWidth, horizonY, 0);
                GL.Vertex3(screenCenterX + ladderWidth * 1.5f, horizonY, 0);
                GL.End();
            }

            GL.PopMatrix();
        }

        private void DrawAircraftSymbol()
        {
            float scale = config.HUDScale;
            float cx = screenCenterX;
            float cy = screenCenterY;

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // W-shape aircraft symbol
            // Left wing
            GL.Vertex3(cx - 30 * scale, cy, 0);
            GL.Vertex3(cx - 8 * scale, cy, 0);

            // Left dip
            GL.Vertex3(cx - 8 * scale, cy, 0);
            GL.Vertex3(cx - 4 * scale, cy + 6 * scale, 0);

            // Center
            GL.Vertex3(cx - 4 * scale, cy + 6 * scale, 0);
            GL.Vertex3(cx, cy, 0);
            GL.Vertex3(cx, cy, 0);
            GL.Vertex3(cx + 4 * scale, cy + 6 * scale, 0);

            // Right dip
            GL.Vertex3(cx + 4 * scale, cy + 6 * scale, 0);
            GL.Vertex3(cx + 8 * scale, cy, 0);

            // Right wing
            GL.Vertex3(cx + 8 * scale, cy, 0);
            GL.Vertex3(cx + 30 * scale, cy, 0);

            GL.End();
        }

        private void DrawFlightPathVector(Vector2 offset)
        {
            float scale = config.HUDScale;
            float cx = screenCenterX + offset.x * pixelsPerDegree;
            float cy = screenCenterY - offset.y * pixelsPerDegree;
            float radius = 8 * scale;

            // Clamp to screen bounds
            float margin = 100 * scale;
            cx = Mathf.Clamp(cx, margin, Screen.width - margin);
            cy = Mathf.Clamp(cy, margin, Screen.height - margin);

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Draw circle
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

                GL.Vertex3(cx + Mathf.Cos(angle1) * radius, cy + Mathf.Sin(angle1) * radius, 0);
                GL.Vertex3(cx + Mathf.Cos(angle2) * radius, cy + Mathf.Sin(angle2) * radius, 0);
            }

            // Left wing
            GL.Vertex3(cx - radius, cy, 0);
            GL.Vertex3(cx - radius - 15 * scale, cy, 0);

            // Right wing
            GL.Vertex3(cx + radius, cy, 0);
            GL.Vertex3(cx + radius + 15 * scale, cy, 0);

            // Top fin
            GL.Vertex3(cx, cy - radius, 0);
            GL.Vertex3(cx, cy - radius - 10 * scale, 0);

            GL.End();
        }

        private void DrawBankIndicator(float roll)
        {
            float scale = config.HUDScale;
            float radius = 150 * scale;
            float cy = screenCenterY - 120 * scale;

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Draw arc from -60 to +60 degrees
            int[] tickDegrees = { -60, -45, -30, -20, -10, 0, 10, 20, 30, 45, 60 };

            foreach (int deg in tickDegrees)
            {
                float angle = (90 + deg) * Mathf.Deg2Rad;
                float x1 = screenCenterX + Mathf.Cos(angle) * radius;
                float y1 = cy + Mathf.Sin(angle) * radius;

                float tickLen = (deg % 30 == 0) ? 15 * scale : (deg % 10 == 0 ? 10 * scale : 6 * scale);
                float x2 = screenCenterX + Mathf.Cos(angle) * (radius - tickLen);
                float y2 = cy + Mathf.Sin(angle) * (radius - tickLen);

                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y2, 0);
            }

            // Draw arc segments
            for (int i = -60; i < 60; i += 5)
            {
                float angle1 = (90 + i) * Mathf.Deg2Rad;
                float angle2 = (90 + i + 5) * Mathf.Deg2Rad;

                GL.Vertex3(screenCenterX + Mathf.Cos(angle1) * radius, cy + Mathf.Sin(angle1) * radius, 0);
                GL.Vertex3(screenCenterX + Mathf.Cos(angle2) * radius, cy + Mathf.Sin(angle2) * radius, 0);
            }

            GL.End();

            // Draw roll pointer (triangle)
            float pointerAngle = (90 - roll) * Mathf.Deg2Rad;
            float px = screenCenterX + Mathf.Cos(pointerAngle) * (radius + 5 * scale);
            float py = cy + Mathf.Sin(pointerAngle) * (radius + 5 * scale);

            GL.Begin(GL.TRIANGLES);
            GL.Color(hudColor);

            float triSize = 8 * scale;
            float perpAngle = pointerAngle + Mathf.PI / 2;

            GL.Vertex3(px + Mathf.Cos(pointerAngle) * triSize, py + Mathf.Sin(pointerAngle) * triSize, 0);
            GL.Vertex3(px + Mathf.Cos(perpAngle) * triSize * 0.5f, py + Mathf.Sin(perpAngle) * triSize * 0.5f, 0);
            GL.Vertex3(px - Mathf.Cos(perpAngle) * triSize * 0.5f, py - Mathf.Sin(perpAngle) * triSize * 0.5f, 0);

            GL.End();
        }

        private void DrawHeadingTape(float heading)
        {
            float scale = config.HUDScale;
            float tapeWidth = 300 * scale;
            float tapeY = 50 * scale;
            float tickHeight = 10 * scale;
            float pixelsPerHeadingDeg = tapeWidth / 60f; // Show 60 degrees of heading

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Draw heading tape box
            float boxLeft = screenCenterX - tapeWidth / 2;
            float boxRight = screenCenterX + tapeWidth / 2;
            GL.Vertex3(boxLeft, tapeY, 0);
            GL.Vertex3(boxRight, tapeY, 0);
            GL.Vertex3(boxLeft, tapeY + tickHeight * 2, 0);
            GL.Vertex3(boxRight, tapeY + tickHeight * 2, 0);

            // Center tick
            GL.Vertex3(screenCenterX, tapeY + tickHeight * 2, 0);
            GL.Vertex3(screenCenterX, tapeY + tickHeight * 2 + 8 * scale, 0);

            // Draw heading ticks
            int startDeg = Mathf.FloorToInt(heading / 5) * 5 - 30;
            for (int deg = startDeg; deg <= startDeg + 60; deg += 5)
            {
                int displayDeg = ((deg % 360) + 360) % 360;
                float offset = (deg - heading) * pixelsPerHeadingDeg;
                float x = screenCenterX + offset;

                if (x < boxLeft || x > boxRight) continue;

                bool isMajor = (displayDeg % 10 == 0);
                float len = isMajor ? tickHeight : tickHeight * 0.5f;

                GL.Vertex3(x, tapeY, 0);
                GL.Vertex3(x, tapeY + len, 0);
            }

            GL.End();
        }

        private void DrawAirspeedTape(float airspeed, float mach)
        {
            float scale = config.HUDScale;
            float tapeX = 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;
            float tickLen = 8 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Tape outline
            GL.Vertex3(tapeX, top, 0);
            GL.Vertex3(tapeX, bottom, 0);
            GL.Vertex3(tapeX, top, 0);
            GL.Vertex3(tapeX + tapeWidth, top, 0);
            GL.Vertex3(tapeX, bottom, 0);
            GL.Vertex3(tapeX + tapeWidth, bottom, 0);

            // Current value pointer
            GL.Vertex3(tapeX + tapeWidth, cy - 8 * scale, 0);
            GL.Vertex3(tapeX + tapeWidth + 15 * scale, cy, 0);
            GL.Vertex3(tapeX + tapeWidth + 15 * scale, cy, 0);
            GL.Vertex3(tapeX + tapeWidth, cy + 8 * scale, 0);

            // Draw ticks
            float pixelsPerUnit = tapeHeight / 100f; // Show 100 m/s range
            int startVal = Mathf.FloorToInt((airspeed - 50) / 10) * 10;

            for (int val = startVal; val <= startVal + 100; val += 10)
            {
                if (val < 0) continue;
                float offset = (val - airspeed) * pixelsPerUnit;
                float y = cy - offset;

                if (y < top || y > bottom) continue;

                bool isMajor = (val % 50 == 0);
                float len = isMajor ? tickLen * 1.5f : tickLen;

                GL.Vertex3(tapeX, y, 0);
                GL.Vertex3(tapeX + len, y, 0);
            }

            GL.End();
        }

        private void DrawAltitudeTape(float altitude, float radarAlt)
        {
            float scale = config.HUDScale;
            float tapeX = Screen.width - 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;
            float tickLen = 8 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Tape outline
            GL.Vertex3(tapeX, top, 0);
            GL.Vertex3(tapeX, bottom, 0);
            GL.Vertex3(tapeX - tapeWidth, top, 0);
            GL.Vertex3(tapeX, top, 0);
            GL.Vertex3(tapeX - tapeWidth, bottom, 0);
            GL.Vertex3(tapeX, bottom, 0);

            // Current value pointer
            GL.Vertex3(tapeX - tapeWidth, cy - 8 * scale, 0);
            GL.Vertex3(tapeX - tapeWidth - 15 * scale, cy, 0);
            GL.Vertex3(tapeX - tapeWidth - 15 * scale, cy, 0);
            GL.Vertex3(tapeX - tapeWidth, cy + 8 * scale, 0);

            // Determine scale based on altitude
            float range, increment;
            if (altitude < 1000)
            {
                range = 500f;
                increment = 50f;
            }
            else if (altitude < 10000)
            {
                range = 2000f;
                increment = 200f;
            }
            else
            {
                range = 10000f;
                increment = 1000f;
            }

            float pixelsPerUnit = tapeHeight / range;
            int startVal = Mathf.FloorToInt((altitude - range / 2) / increment) * (int)increment;

            for (float val = startVal; val <= startVal + range; val += increment)
            {
                if (val < 0) continue;
                float offset = (val - altitude) * pixelsPerUnit;
                float y = cy - offset;

                if (y < top || y > bottom) continue;

                bool isMajor = (val % (increment * 5) == 0) || (increment >= 1000);
                float len = isMajor ? tickLen * 1.5f : tickLen;

                GL.Vertex3(tapeX - len, y, 0);
                GL.Vertex3(tapeX, y, 0);
            }

            GL.End();
        }

        private void DrawVerticalSpeedIndicator(float vertSpeed)
        {
            float scale = config.HUDScale;
            float x = Screen.width - 60 * scale;
            float height = 100 * scale;
            float cy = screenCenterY;

            GL.Begin(GL.LINES);
            GL.Color(hudColor);

            // Vertical line
            GL.Vertex3(x, cy - height, 0);
            GL.Vertex3(x, cy + height, 0);

            // Zero mark
            GL.Vertex3(x - 5 * scale, cy, 0);
            GL.Vertex3(x + 5 * scale, cy, 0);

            // Ticks at intervals
            float maxVS = 100f; // m/s scale
            float[] ticks = { -100, -50, 50, 100 };
            foreach (float vs in ticks)
            {
                float y = cy - (vs / maxVS) * height;
                GL.Vertex3(x - 3 * scale, y, 0);
                GL.Vertex3(x + 3 * scale, y, 0);
            }

            // Current VS indicator (clamped)
            float clampedVS = Mathf.Clamp(vertSpeed, -maxVS, maxVS);
            float indicatorY = cy - (clampedVS / maxVS) * height;

            // Draw filled triangle
            GL.End();
            GL.Begin(GL.TRIANGLES);
            GL.Color(hudColor);
            GL.Vertex3(x - 10 * scale, indicatorY, 0);
            GL.Vertex3(x, indicatorY - 5 * scale, 0);
            GL.Vertex3(x, indicatorY + 5 * scale, 0);
            GL.End();
        }

        private void DrawTextLabels(float airspeed, float altitude, float radarAlt,
            float vertSpeed, float gForce, float aoa, float heading, float mach)
        {
            float scale = config.HUDScale;

            // Update label style colors
            labelStyle.normal.textColor = hudColor;
            labelStyleSmall.normal.textColor = hudColor;
            labelStyleCenter.normal.textColor = hudColor;

            // Airspeed value
            string speedStr = airspeed.ToString("F0");
            GUI.Label(new Rect(80 * scale, screenCenterY - 10, 60, 25), speedStr, labelStyle);

            // Mach number (if supersonic)
            if (mach > 0.8f)
            {
                GUI.Label(new Rect(80 * scale, screenCenterY + 15, 60, 20), "M" + mach.ToString("F2"), labelStyleSmall);
            }

            // Altitude value
            string altStr;
            if (altitude >= 10000)
                altStr = (altitude / 1000f).ToString("F1") + "k";
            else
                altStr = altitude.ToString("F0");
            GUI.Label(new Rect(Screen.width - 140 * scale, screenCenterY - 10, 60, 25), altStr, labelStyle);

            // Radar altitude (if low)
            if (radarAlt < 1000 && radarAlt > 0)
            {
                GUI.Label(new Rect(Screen.width - 140 * scale, screenCenterY + 15, 60, 20),
                    "R" + radarAlt.ToString("F0"), labelStyleSmall);
            }

            // Vertical speed
            string vsStr = (vertSpeed >= 0 ? "+" : "") + vertSpeed.ToString("F0");
            GUI.Label(new Rect(Screen.width - 55 * scale, screenCenterY - 10, 50, 25), vsStr, labelStyleSmall);

            // Heading value (center box)
            string hdgStr = heading.ToString("F0").PadLeft(3, '0');
            GUI.Label(new Rect(screenCenterX - 20, 55 * scale, 40, 25), hdgStr, labelStyleCenter);

            // Heading cardinal directions
            DrawHeadingLabels(heading, scale);

            // G-force
            GUI.Label(new Rect(screenCenterX - 80 * scale, Screen.height - 60 * scale, 60, 25),
                "G " + gForce.ToString("F1"), labelStyle);

            // AOA
            GUI.Label(new Rect(screenCenterX + 30 * scale, Screen.height - 60 * scale, 60, 25),
                "AOA " + aoa.ToString("F1") + "°", labelStyle);

            // Pitch ladder degree labels
            DrawPitchLabels(scale);
        }

        private void DrawHeadingLabels(float heading, float scale)
        {
            float tapeWidth = 300 * scale;
            float tapeY = 22 * scale;
            float pixelsPerHeadingDeg = tapeWidth / 60f;

            string[] cardinals = { "N", "E", "S", "W" };
            int[] cardinalDegrees = { 0, 90, 180, 270 };

            for (int i = 0; i < 4; i++)
            {
                float offset = Mathf.DeltaAngle(heading, cardinalDegrees[i]) * pixelsPerHeadingDeg;
                float x = screenCenterX + offset;

                if (Mathf.Abs(offset) < tapeWidth / 2)
                {
                    GUI.Label(new Rect(x - 10, tapeY, 20, 20), cardinals[i], labelStyleCenter);
                }
            }

            // Numeric labels every 30 degrees
            int startDeg = Mathf.FloorToInt(heading / 30) * 30 - 30;
            for (int deg = startDeg; deg <= startDeg + 90; deg += 30)
            {
                int displayDeg = ((deg % 360) + 360) % 360;
                float offset = Mathf.DeltaAngle(heading, deg) * pixelsPerHeadingDeg;
                float x = screenCenterX + offset;

                if (Mathf.Abs(offset) < tapeWidth / 2 && displayDeg % 90 != 0)
                {
                    GUI.Label(new Rect(x - 15, tapeY, 30, 20), displayDeg.ToString(), labelStyleSmall);
                }
            }
        }

        private void DrawPitchLabels(float scale)
        {
            if (vessel == null) return;

            float pitch = GetPitch();
            float roll = GetRoll();
            float ladderWidth = 120f * scale;

            // Draw degree labels next to pitch ladder lines
            for (int deg = -90; deg <= 90; deg += 10)
            {
                if (deg == 0) continue;

                float yOffset = (deg - pitch) * pixelsPerDegree;
                float y = screenCenterY - yOffset;

                if (y < 80 || y > Screen.height - 80) continue;

                // Apply roll rotation to label position
                float labelX = screenCenterX + ladderWidth + 10 * scale;
                float labelY = y - 8;

                // Rotate around screen center
                float dx = labelX - screenCenterX;
                float dy = labelY - screenCenterY;
                float rollRad = -roll * Mathf.Deg2Rad;
                float rotX = dx * Mathf.Cos(rollRad) - dy * Mathf.Sin(rollRad);
                float rotY = dx * Mathf.Sin(rollRad) + dy * Mathf.Cos(rollRad);

                GUI.Label(new Rect(screenCenterX + rotX, screenCenterY + rotY, 30, 20),
                    deg.ToString(), labelStyleSmall);
            }
        }
    }
}
