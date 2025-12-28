using UnityEngine;

namespace FlightHUD
{
    public static class HUDRenderer
    {
        #region Aircraft Symbol

        public static void DrawAircraftSymbol(float screenCenterX, float screenCenterY, float scale, Color color)
        {
            float cx = screenCenterX;
            float cy = screenCenterY;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Left wing
            GL.Vertex3(cx - 30 * scale, cy, 0);
            GL.Vertex3(cx - 8 * scale, cy, 0);

            // Left wing to center (down stroke)
            GL.Vertex3(cx - 8 * scale, cy, 0);
            GL.Vertex3(cx - 4 * scale, cy + 6 * scale, 0);

            // Center V shape
            GL.Vertex3(cx - 4 * scale, cy + 6 * scale, 0);
            GL.Vertex3(cx, cy, 0);
            GL.Vertex3(cx, cy, 0);
            GL.Vertex3(cx + 4 * scale, cy + 6 * scale, 0);

            // Right wing to center (down stroke)
            GL.Vertex3(cx + 4 * scale, cy + 6 * scale, 0);
            GL.Vertex3(cx + 8 * scale, cy, 0);

            // Right wing
            GL.Vertex3(cx + 8 * scale, cy, 0);
            GL.Vertex3(cx + 30 * scale, cy, 0);

            GL.End();
        }

        #endregion

        #region Flight Path Vector

        // Smoothed FPV position
        private static float smoothedFpvX = 0f;
        private static float smoothedFpvY = 0f;

        public static void DrawFlightPathVector(Vector2 offset, float screenCenterX, float screenCenterY,
            float pixelsPerDegree, float scale, Color color)
        {
            // Apply transforms
            float offsetX = HUDSettings.FPVNegateX ? -offset.x : offset.x;
            float offsetY = HUDSettings.FPVNegateY ? -offset.y : offset.y;

            // Apply smoothing to reduce glitchiness
            float targetX = screenCenterX + offsetX * pixelsPerDegree;
            float targetY = screenCenterY + offsetY * pixelsPerDegree;

            float smoothing = 0.15f;
            smoothedFpvX = Mathf.Lerp(smoothedFpvX, targetX, smoothing);
            smoothedFpvY = Mathf.Lerp(smoothedFpvY, targetY, smoothing);

            // Initialize on first frame
            if (smoothedFpvX == 0f) smoothedFpvX = targetX;
            if (smoothedFpvY == 0f) smoothedFpvY = targetY;

            float cx = smoothedFpvX;
            float cy = smoothedFpvY;
            float radius = 8 * scale;

            // Clamp to screen margins
            float margin = 100 * scale;
            cx = Mathf.Clamp(cx, margin, Screen.width - margin);
            cy = Mathf.Clamp(cy, margin, Screen.height - margin);

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Circle - more segments for smoother appearance
            int segments = 24;
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

            // Tail (top)
            GL.Vertex3(cx, cy - radius, 0);
            GL.Vertex3(cx, cy - radius - 10 * scale, 0);

            GL.End();
        }

        #endregion

        #region Bank Indicator

        public static void DrawBankIndicator(float roll, float screenCenterX, float screenCenterY, float scale, Color color)
        {
            // Apply transforms
            float transformedRoll = roll;
            if (HUDSettings.BankNegate)
                transformedRoll = -transformedRoll;

            float radius = 150 * scale;
            float cy = screenCenterY - 120 * scale;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Tick marks at standard angles
            float mirrorFactor = HUDSettings.BankMirror ? -1f : 1f;
            int[] tickDegrees = { -60, -45, -30, -20, -10, 0, 10, 20, 30, 45, 60 };
            foreach (int deg in tickDegrees)
            {
                float angle = (90 + deg * mirrorFactor) * Mathf.Deg2Rad;
                float x1 = screenCenterX + Mathf.Cos(angle) * radius;
                float y1 = cy + Mathf.Sin(angle) * radius;

                float tickLen = (deg % 30 == 0) ? 15 * scale : (deg % 10 == 0 ? 10 * scale : 6 * scale);
                float x2 = screenCenterX + Mathf.Cos(angle) * (radius - tickLen);
                float y2 = cy + Mathf.Sin(angle) * (radius - tickLen);

                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y2, 0);
            }

            // Arc
            for (int i = -60; i < 60; i += 5)
            {
                float angle1 = (90 + i) * Mathf.Deg2Rad;
                float angle2 = (90 + i + 5) * Mathf.Deg2Rad;

                GL.Vertex3(screenCenterX + Mathf.Cos(angle1) * radius, cy + Mathf.Sin(angle1) * radius, 0);
                GL.Vertex3(screenCenterX + Mathf.Cos(angle2) * radius, cy + Mathf.Sin(angle2) * radius, 0);
            }

            GL.End();

            // Roll pointer triangle
            float pointerAngle = (90 + transformedRoll) * Mathf.Deg2Rad;
            float px = screenCenterX + Mathf.Cos(pointerAngle) * (radius + 5 * scale);
            float py = cy + Mathf.Sin(pointerAngle) * (radius + 5 * scale);

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            float triSize = 8 * scale;
            float perpAngle = pointerAngle + Mathf.PI / 2;

            GL.Vertex3(px + Mathf.Cos(pointerAngle) * triSize, py + Mathf.Sin(pointerAngle) * triSize, 0);
            GL.Vertex3(px + Mathf.Cos(perpAngle) * triSize * 0.5f, py + Mathf.Sin(perpAngle) * triSize * 0.5f, 0);
            GL.Vertex3(px - Mathf.Cos(perpAngle) * triSize * 0.5f, py - Mathf.Sin(perpAngle) * triSize * 0.5f, 0);

            GL.End();
        }

        #endregion

        #region Heading Tape

        public static void DrawHeadingTape(float heading, float screenCenterX, float scale, Color color)
        {
            float tapeWidth = 300 * scale;
            float tapeY = 50 * scale;
            float tickHeight = 10 * scale;

            GL.Begin(GL.LINES);
            GL.Color(color);

            float boxLeft = screenCenterX - tapeWidth / 2;
            float boxRight = screenCenterX + tapeWidth / 2;

            // Top and bottom lines of tape
            GL.Vertex3(boxLeft, tapeY, 0);
            GL.Vertex3(boxRight, tapeY, 0);
            GL.Vertex3(boxLeft, tapeY + tickHeight * 2, 0);
            GL.Vertex3(boxRight, tapeY + tickHeight * 2, 0);

            // Center pointer
            GL.Vertex3(screenCenterX, tapeY + tickHeight * 2, 0);
            GL.Vertex3(screenCenterX, tapeY + tickHeight * 2 + 8 * scale, 0);

            // Tick marks
            float pixelsPerHeadingDeg = tapeWidth / 60f;
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

        public static void DrawHeadingLabels(float heading, float screenCenterX, float scale, GUIStyle style)
        {
            float tapeWidth = 300 * scale;
            float tapeY = 50 * scale;
            float tickHeight = 10 * scale;
            float pixelsPerHeadingDeg = tapeWidth / 60f;
            float boxLeft = screenCenterX - tapeWidth / 2;
            float boxRight = screenCenterX + tapeWidth / 2;

            string[] cardinals = { "N", "E", "S", "W" };
            int[] cardinalDegs = { 0, 90, 180, 270 };

            int startDeg = Mathf.FloorToInt(heading / 10) * 10 - 30;
            for (int deg = startDeg; deg <= startDeg + 60; deg += 10)
            {
                int displayDeg = ((deg % 360) + 360) % 360;
                float offset = (deg - heading) * pixelsPerHeadingDeg;
                float x = screenCenterX + offset;

                if (x < boxLeft + 10 || x > boxRight - 10) continue;

                string label = "";
                for (int i = 0; i < 4; i++)
                {
                    if (displayDeg == cardinalDegs[i])
                    {
                        label = cardinals[i];
                        break;
                    }
                }
                if (string.IsNullOrEmpty(label))
                {
                    label = displayDeg.ToString();
                }

                GUI.Label(new Rect(x - 15, tapeY + tickHeight + 2, 30, 20), label, style);
            }
        }

        #endregion

        #region Airspeed Tape

        public static void DrawAirspeedTape(float airspeed, float screenCenterY, float scale, Color color)
        {
            float tapeX = 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;
            float tickLen = 8 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Tape frame
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

            // Tick marks
            float pixelsPerUnit = tapeHeight / 100f;
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

        public static void DrawAirspeedLabels(float airspeed, float screenCenterY, float scale, GUIStyle style)
        {
            float tapeX = 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;
            float pixelsPerUnit = tapeHeight / 100f;

            int startVal = Mathf.FloorToInt((airspeed - 50) / 20) * 20;
            for (int val = startVal; val <= startVal + 100; val += 20)
            {
                if (val < 0) continue;
                float offset = (val - airspeed) * pixelsPerUnit;
                float y = cy - offset;

                if (y < top + 10 || y > bottom - 10) continue;

                GUI.Label(new Rect(tapeX + 12 * scale, y - 10, 40, 20), val.ToString(), style);
            }

            // Current value readout
            GUI.Label(new Rect(tapeX + tapeWidth + 20 * scale, cy - 10, 50, 20), Mathf.RoundToInt(airspeed).ToString(), style);
        }

        #endregion

        #region Altitude Tape

        public static void DrawAltitudeTape(float altitude, float radarAlt, float screenCenterY, float scale, Color color)
        {
            float tapeX = Screen.width - 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;
            float tickLen = 8 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Tape frame
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

            // Adaptive scaling
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

        public static void DrawAltitudeLabels(float altitude, float screenCenterY, float scale, GUIStyle style)
        {
            float tapeX = Screen.width - 120 * scale;
            float tapeHeight = 200 * scale;
            float tapeWidth = 60 * scale;

            float cy = screenCenterY;
            float top = cy - tapeHeight / 2;
            float bottom = cy + tapeHeight / 2;

            // Adaptive scaling
            float range, increment;
            if (altitude < 1000)
            {
                range = 500f;
                increment = 100f;
            }
            else if (altitude < 10000)
            {
                range = 2000f;
                increment = 500f;
            }
            else
            {
                range = 10000f;
                increment = 2000f;
            }

            float pixelsPerUnit = tapeHeight / range;
            int startVal = Mathf.FloorToInt((altitude - range / 2) / increment) * (int)increment;

            for (float val = startVal; val <= startVal + range; val += increment)
            {
                if (val < 0) continue;
                float offset = (val - altitude) * pixelsPerUnit;
                float y = cy - offset;

                if (y < top + 10 || y > bottom - 10) continue;

                string label = val >= 1000 ? $"{val / 1000f:F1}k" : val.ToString();
                GUI.Label(new Rect(tapeX - tapeWidth - 40 * scale, y - 10, 40, 20), label, style);
            }

            // Current value readout
            string altStr = altitude >= 1000 ? $"{altitude / 1000f:F1}k" : Mathf.RoundToInt(altitude).ToString();
            GUI.Label(new Rect(tapeX - tapeWidth - 70 * scale, cy - 10, 60, 20), altStr, style);
        }

        #endregion

        #region Vertical Speed Indicator

        public static void DrawVerticalSpeedIndicator(float vertSpeed, float screenCenterY, float scale, Color color)
        {
            float x = Screen.width - 60 * scale;
            float height = 100 * scale;
            float cy = screenCenterY;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Vertical line
            GL.Vertex3(x, cy - height, 0);
            GL.Vertex3(x, cy + height, 0);

            // Center tick
            GL.Vertex3(x - 5 * scale, cy, 0);
            GL.Vertex3(x + 5 * scale, cy, 0);

            // Scale ticks
            float maxVS = 100f;
            float[] ticks = { -100, -50, 50, 100 };
            foreach (float vs in ticks)
            {
                float y = cy - (vs / maxVS) * height;
                GL.Vertex3(x - 3 * scale, y, 0);
                GL.Vertex3(x + 3 * scale, y, 0);
            }

            GL.End();

            // Pointer triangle
            float clampedVS = Mathf.Clamp(vertSpeed, -maxVS, maxVS);
            float indicatorY = cy - (clampedVS / maxVS) * height;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            GL.Vertex3(x - 10 * scale, indicatorY, 0);
            GL.Vertex3(x, indicatorY - 5 * scale, 0);
            GL.Vertex3(x, indicatorY + 5 * scale, 0);
            GL.End();
        }

        #endregion

        #region Compass Rose

        public static void DrawCompassRose(float heading, float screenCenterX, float scale, Color color)
        {
            float cy = Screen.height - 80 * scale;
            float radius = 50 * scale;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Outer circle
            int segments = 36;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

                GL.Vertex3(screenCenterX + Mathf.Cos(angle1) * radius, cy + Mathf.Sin(angle1) * radius, 0);
                GL.Vertex3(screenCenterX + Mathf.Cos(angle2) * radius, cy + Mathf.Sin(angle2) * radius, 0);
            }

            // Cardinal direction ticks (rotate with heading)
            for (int deg = 0; deg < 360; deg += 30)
            {
                float angle = (90 - (deg - heading)) * Mathf.Deg2Rad;
                float innerR = (deg % 90 == 0) ? radius - 12 * scale : radius - 8 * scale;

                GL.Vertex3(screenCenterX + Mathf.Cos(angle) * innerR, cy + Mathf.Sin(angle) * innerR, 0);
                GL.Vertex3(screenCenterX + Mathf.Cos(angle) * radius, cy + Mathf.Sin(angle) * radius, 0);
            }

            // Fixed aircraft pointer at top
            GL.Vertex3(screenCenterX, cy - radius - 5 * scale, 0);
            GL.Vertex3(screenCenterX - 5 * scale, cy - radius + 5 * scale, 0);
            GL.Vertex3(screenCenterX, cy - radius - 5 * scale, 0);
            GL.Vertex3(screenCenterX + 5 * scale, cy - radius + 5 * scale, 0);

            GL.End();
        }

        public static void DrawCompassLabels(float heading, float screenCenterX, float scale, GUIStyle style)
        {
            float cy = Screen.height - 80 * scale;
            float radius = 50 * scale;

            string[] cardinals = { "N", "E", "S", "W" };
            int[] cardinalDegs = { 0, 90, 180, 270 };

            for (int i = 0; i < 4; i++)
            {
                float angle = (90 - (cardinalDegs[i] - heading)) * Mathf.Deg2Rad;
                float x = screenCenterX + Mathf.Cos(angle) * (radius - 20 * scale);
                float y = cy + Mathf.Sin(angle) * (radius - 20 * scale);

                GUI.Label(new Rect(x - 10, y - 10, 20, 20), cardinals[i], style);
            }
        }

        #endregion

        #region Status Icons

        // Status icon colors
        private static readonly Color BrakesActiveColor = new Color(1f, 0.2f, 0.2f, 1f);    // Red
        private static readonly Color LightsActiveColor = new Color(1f, 0.9f, 0.2f, 1f);    // Yellow
        private static readonly Color GearActiveColor = new Color(0.2f, 1f, 0.4f, 1f);      // Green

        public static void DrawStatusIcons(bool brakes, bool lights, bool gear, float scale, Color color, Color dimColor)
        {
            float iconSize = 20 * scale;
            float spacing = 30 * scale;
            float startX = 80 * scale;
            float y = Screen.height - 40 * scale;

            // Brakes icon (B with box) - Red when active
            DrawBrakesIcon(startX, y, iconSize, brakes ? BrakesActiveColor : dimColor, brakes);

            // Lights icon (rays) - Yellow when active
            DrawLightsIcon(startX + spacing, y, iconSize, lights ? LightsActiveColor : dimColor, lights);

            // Gear icon (wheel) - Green when active
            DrawGearIcon(startX + spacing * 2, y, iconSize, gear ? GearActiveColor : dimColor, gear);
        }

        private static void DrawBrakesIcon(float cx, float cy, float size, Color color, bool active)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            float half = size / 2;

            // Box around
            GL.Vertex3(cx - half, cy - half, 0);
            GL.Vertex3(cx + half, cy - half, 0);
            GL.Vertex3(cx + half, cy - half, 0);
            GL.Vertex3(cx + half, cy + half, 0);
            GL.Vertex3(cx + half, cy + half, 0);
            GL.Vertex3(cx - half, cy + half, 0);
            GL.Vertex3(cx - half, cy + half, 0);
            GL.Vertex3(cx - half, cy - half, 0);

            if (active)
            {
                // "B" letter - simplified
                float letterW = size * 0.4f;
                float letterH = size * 0.6f;

                // Vertical line of B
                GL.Vertex3(cx - letterW * 0.5f, cy - letterH * 0.5f, 0);
                GL.Vertex3(cx - letterW * 0.5f, cy + letterH * 0.5f, 0);

                // Top bump
                GL.Vertex3(cx - letterW * 0.5f, cy - letterH * 0.5f, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy - letterH * 0.5f, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy - letterH * 0.5f, 0);
                GL.Vertex3(cx + letterW * 0.5f, cy - letterH * 0.25f, 0);
                GL.Vertex3(cx + letterW * 0.5f, cy - letterH * 0.25f, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy, 0);
                GL.Vertex3(cx - letterW * 0.5f, cy, 0);

                // Bottom bump
                GL.Vertex3(cx - letterW * 0.5f, cy, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy, 0);
                GL.Vertex3(cx + letterW * 0.5f, cy + letterH * 0.25f, 0);
                GL.Vertex3(cx + letterW * 0.5f, cy + letterH * 0.25f, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy + letterH * 0.5f, 0);
                GL.Vertex3(cx + letterW * 0.3f, cy + letterH * 0.5f, 0);
                GL.Vertex3(cx - letterW * 0.5f, cy + letterH * 0.5f, 0);
            }

            GL.End();
        }

        private static void DrawLightsIcon(float cx, float cy, float size, Color color, bool active)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            float radius = size * 0.3f;

            // Circle (bulb)
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;
                GL.Vertex3(cx + Mathf.Cos(angle1) * radius, cy + Mathf.Sin(angle1) * radius, 0);
                GL.Vertex3(cx + Mathf.Cos(angle2) * radius, cy + Mathf.Sin(angle2) * radius, 0);
            }

            if (active)
            {
                // Rays emanating from bulb
                float rayStart = radius + size * 0.1f;
                float rayEnd = radius + size * 0.35f;

                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * Mathf.PI * 2;
                    GL.Vertex3(cx + Mathf.Cos(angle) * rayStart, cy + Mathf.Sin(angle) * rayStart, 0);
                    GL.Vertex3(cx + Mathf.Cos(angle) * rayEnd, cy + Mathf.Sin(angle) * rayEnd, 0);
                }
            }

            GL.End();
        }

        private static void DrawGearIcon(float cx, float cy, float size, Color color, bool active)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            float wheelRadius = size * 0.35f;
            float hubRadius = size * 0.15f;

            // Outer wheel circle
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;
                GL.Vertex3(cx + Mathf.Cos(angle1) * wheelRadius, cy + Mathf.Sin(angle1) * wheelRadius, 0);
                GL.Vertex3(cx + Mathf.Cos(angle2) * wheelRadius, cy + Mathf.Sin(angle2) * wheelRadius, 0);
            }

            // Hub circle
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;
                GL.Vertex3(cx + Mathf.Cos(angle1) * hubRadius, cy + Mathf.Sin(angle1) * hubRadius, 0);
                GL.Vertex3(cx + Mathf.Cos(angle2) * hubRadius, cy + Mathf.Sin(angle2) * hubRadius, 0);
            }

            if (active)
            {
                // Spokes when gear is down
                for (int i = 0; i < 4; i++)
                {
                    float angle = (i / 4f) * Mathf.PI * 2;
                    GL.Vertex3(cx + Mathf.Cos(angle) * hubRadius, cy + Mathf.Sin(angle) * hubRadius, 0);
                    GL.Vertex3(cx + Mathf.Cos(angle) * wheelRadius, cy + Mathf.Sin(angle) * wheelRadius, 0);
                }

                // Down arrow below wheel
                float arrowY = cy + wheelRadius + size * 0.15f;
                GL.Vertex3(cx, arrowY, 0);
                GL.Vertex3(cx, arrowY + size * 0.25f, 0);
                GL.Vertex3(cx, arrowY + size * 0.25f, 0);
                GL.Vertex3(cx - size * 0.15f, arrowY + size * 0.1f, 0);
                GL.Vertex3(cx, arrowY + size * 0.25f, 0);
                GL.Vertex3(cx + size * 0.15f, arrowY + size * 0.1f, 0);
            }
            else
            {
                // Up arrow when gear is up
                float arrowY = cy - wheelRadius - size * 0.15f;
                GL.Vertex3(cx, arrowY, 0);
                GL.Vertex3(cx, arrowY - size * 0.25f, 0);
                GL.Vertex3(cx, arrowY - size * 0.25f, 0);
                GL.Vertex3(cx - size * 0.15f, arrowY - size * 0.1f, 0);
                GL.Vertex3(cx, arrowY - size * 0.25f, 0);
                GL.Vertex3(cx + size * 0.15f, arrowY - size * 0.1f, 0);
            }

            GL.End();
        }

        public static void DrawStatusLabels(bool brakes, bool lights, bool gear, float scale, GUIStyle baseStyle)
        {
            float spacing = 30 * scale;
            float startX = 80 * scale;
            float y = Screen.height - 20 * scale;

            // Create colored styles for each status
            GUIStyle brkStyle = new GUIStyle(baseStyle);
            brkStyle.normal.textColor = brakes ? BrakesActiveColor : baseStyle.normal.textColor;

            GUIStyle ltsStyle = new GUIStyle(baseStyle);
            ltsStyle.normal.textColor = lights ? LightsActiveColor : baseStyle.normal.textColor;

            GUIStyle gerStyle = new GUIStyle(baseStyle);
            gerStyle.normal.textColor = gear ? GearActiveColor : baseStyle.normal.textColor;

            GUI.Label(new Rect(startX - 10, y, 30, 20), "BRK", brkStyle);
            GUI.Label(new Rect(startX + spacing - 10, y, 30, 20), "LTS", ltsStyle);
            GUI.Label(new Rect(startX + spacing * 2 - 10, y, 30, 20), "GER", gerStyle);
        }

        #endregion
    }
}
