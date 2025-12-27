using UnityEngine;

namespace FlightHUD
{
    public struct FlightData
    {
        public float Pitch;
        public float Roll;
        public float Heading;
        public float Airspeed;
        public float Mach;
        public float VerticalSpeed;
        public Vector2 FPVOffset;
        public float Altitude;
        public float RadarAltitude;
        public float GForce;
        public float AOA;
    }

    public static class HUDElements
    {
        public static void DrawPitchLadder(float pitch, float roll, float screenCenterX, float screenCenterY,
            float pixelsPerDegree, float scale, Color color, Color colorDim)
        {
            float ladderWidth = 120f * scale;
            float gapWidth = 40f * scale;

            GL.PushMatrix();
            Matrix4x4 rotMatrix = Matrix4x4.TRS(
                new Vector3(screenCenterX, screenCenterY, 0),
                Quaternion.Euler(0, 0, roll),
                Vector3.one);
            GL.MultMatrix(rotMatrix * Matrix4x4.TRS(new Vector3(-screenCenterX, -screenCenterY, 0), Quaternion.identity, Vector3.one));

            for (int deg = -90; deg <= 90; deg += 5)
            {
                if (deg == 0) continue;

                float yOffset = (deg - pitch) * pixelsPerDegree;
                float y = screenCenterY - yOffset;

                if (y < 50 || y > Screen.height - 50) continue;

                bool isMajor = (deg % 10 == 0);
                float width = isMajor ? ladderWidth : ladderWidth * 0.6f;
                Color lineColor = isMajor ? color : colorDim;

                GL.Begin(GL.LINES);
                GL.Color(lineColor);

                if (deg > 0)
                {
                    // Solid lines for positive pitch (above horizon)
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - gapWidth, y, 0);
                    GL.Vertex3(screenCenterX + gapWidth, y, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);

                    // End caps pointing up
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y + 8 * scale, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y + 8 * scale, 0);
                }
                else
                {
                    // Dashed lines for negative pitch (below horizon)
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

                    // End caps pointing down
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y - 8 * scale, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y - 8 * scale, 0);
                }

                GL.End();
            }

            // Horizon line
            float horizonY = screenCenterY + pitch * pixelsPerDegree;
            if (horizonY > 50 && horizonY < Screen.height - 50)
            {
                GL.Begin(GL.LINES);
                GL.Color(color);
                GL.Vertex3(screenCenterX - ladderWidth * 1.5f, horizonY, 0);
                GL.Vertex3(screenCenterX - gapWidth, horizonY, 0);
                GL.Vertex3(screenCenterX + gapWidth, horizonY, 0);
                GL.Vertex3(screenCenterX + ladderWidth * 1.5f, horizonY, 0);
                GL.End();
            }

            GL.PopMatrix();
        }

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

        public static void DrawFlightPathVector(Vector2 offset, float screenCenterX, float screenCenterY,
            float pixelsPerDegree, float scale, Color color)
        {
            float cx = screenCenterX + offset.x * pixelsPerDegree;
            float cy = screenCenterY - offset.y * pixelsPerDegree;
            float radius = 8 * scale;

            // Clamp to screen margins
            float margin = 100 * scale;
            cx = Mathf.Clamp(cx, margin, Screen.width - margin);
            cy = Mathf.Clamp(cy, margin, Screen.height - margin);

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Circle
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

            // Tail (top)
            GL.Vertex3(cx, cy - radius, 0);
            GL.Vertex3(cx, cy - radius - 10 * scale, 0);

            GL.End();
        }

        public static void DrawBankIndicator(float roll, float screenCenterX, float screenCenterY, float scale, Color color)
        {
            float radius = 150 * scale;
            float cy = screenCenterY - 120 * scale;

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Tick marks
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
            float pointerAngle = (90 - roll) * Mathf.Deg2Rad;
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

        public static void DrawHeadingTape(float heading, float screenCenterX, float scale, Color color)
        {
            float tapeWidth = 300 * scale;
            float tapeY = 50 * scale;
            float tickHeight = 10 * scale;
            float pixelsPerHeadingDeg = tapeWidth / 60f;

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
    }
}
