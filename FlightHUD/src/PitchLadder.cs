using System.Collections.Generic;
using UnityEngine;

namespace FlightHUD
{
    public static class PitchLadder
    {
        private static List<LabelInfo> pendingLabels = new List<LabelInfo>();

        private struct LabelInfo
        {
            public float X;
            public float Y;
            public string Text;
        }

        public static void Draw(float pitch, float roll, float screenCenterX, float screenCenterY,
                                float pixelsPerDegree, float scale, Color color, Color colorDim)
        {
            pendingLabels.Clear();

            // Apply transforms from settings
            float transformedPitch = pitch;
            float transformedRoll = roll;

            if (HUDSettings.PitchLadderNegateY)
                transformedPitch = -transformedPitch;
            if (HUDSettings.PitchLadderNegateX)
                transformedRoll = -transformedRoll;

            // Add rotation offset
            transformedRoll += HUDSettings.PitchLadderRotation;

            float ladderWidth = 100f * scale;
            float minorWidth = 60f * scale;
            float gapWidth = 35f * scale;
            float capLen = 8f * scale;
            float dashLen = 8f * scale;

            // Mirror factors
            float mirrorX = HUDSettings.PitchLadderMirrorX ? -1f : 1f;
            float mirrorY = HUDSettings.PitchLadderMirrorY ? -1f : 1f;

            // Setup rotation matrix for roll (for all ladder elements including horizon)
            GL.PushMatrix();
            Matrix4x4 rollMatrix = Matrix4x4.TRS(
                new Vector3(screenCenterX, screenCenterY, 0),
                Quaternion.Euler(0, 0, transformedRoll),
                Vector3.one
            );
            GL.MultMatrix(rollMatrix * Matrix4x4.TRS(
                new Vector3(-screenCenterX, -screenCenterY, 0),
                Quaternion.identity, Vector3.one));

            // Draw horizon line (now inside the roll matrix so it rotates with the ladder)
            float horizonY = screenCenterY - transformedPitch * pixelsPerDegree * mirrorY;
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

            // Draw pitch ladder rungs
            for (int deg = -90; deg <= 90; deg += 5)
            {
                if (deg == 0) continue;

                float y = screenCenterY - (transformedPitch + deg) * pixelsPerDegree * mirrorY;

                if (y < 50 || y > Screen.height - 50) continue;

                bool isMajor = (deg % 10 == 0);
                float width = isMajor ? ladderWidth : minorWidth;
                Color lineColor = isMajor ? color : colorDim;

                GL.Begin(GL.LINES);
                GL.Color(lineColor);

                if (deg > 0)
                {
                    // Above horizon (positive pitch): solid lines with caps pointing down toward horizon
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - gapWidth, y, 0);

                    GL.Vertex3(screenCenterX + gapWidth, y, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);

                    // Caps point down (toward horizon which is below)
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y + capLen, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y + capLen, 0);
                }
                else
                {
                    // Below horizon (negative pitch): dashed lines with caps pointing up toward horizon
                    for (float x = screenCenterX - width; x < screenCenterX - gapWidth; x += dashLen * 2)
                    {
                        float endX = Mathf.Min(x + dashLen, screenCenterX - gapWidth);
                        GL.Vertex3(x, y, 0);
                        GL.Vertex3(endX, y, 0);
                    }

                    for (float x = screenCenterX + gapWidth; x < screenCenterX + width; x += dashLen * 2)
                    {
                        float endX = Mathf.Min(x + dashLen, screenCenterX + width);
                        GL.Vertex3(x, y, 0);
                        GL.Vertex3(endX, y, 0);
                    }

                    // Caps point up (toward horizon which is above)
                    GL.Vertex3(screenCenterX - width, y, 0);
                    GL.Vertex3(screenCenterX - width, y - capLen, 0);
                    GL.Vertex3(screenCenterX + width, y, 0);
                    GL.Vertex3(screenCenterX + width, y - capLen, 0);
                }

                GL.End();

                if (isMajor)
                {
                    pendingLabels.Add(new LabelInfo
                    {
                        X = width + 5 * scale,
                        Y = y - screenCenterY,
                        Text = deg.ToString()
                    });
                }
            }

            GL.PopMatrix();
        }

        public static void DrawLabels(float pitch, float roll, float screenCenterX, float screenCenterY,
                                      float pixelsPerDegree, float scale, GUIStyle style)
        {
            // Apply SEPARATE transforms for numbers (not the ladder transforms)
            float transformedRoll = roll;
            if (HUDSettings.PitchNumbersNegateX)
                transformedRoll = -transformedRoll;
            transformedRoll += HUDSettings.PitchNumbersRotation;

            float rollRad = transformedRoll * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rollRad);
            float sin = Mathf.Sin(rollRad);

            // Negate Y for position calculation
            float yFactor = HUDSettings.PitchNumbersNegateY ? -1f : 1f;

            foreach (var label in pendingLabels)
            {
                float labelY = label.Y * yFactor;
                float rotX = label.X * cos - labelY * sin;
                float rotY = label.X * sin + labelY * cos;

                float finalX = screenCenterX + rotX;
                float finalY = screenCenterY + rotY;

                // Negate the displayed value if requested
                string displayText = label.Text;
                if (HUDSettings.PitchNumbersNegate)
                {
                    int val;
                    if (int.TryParse(label.Text, out val))
                    {
                        displayText = (-val).ToString();
                    }
                }

                GUI.Label(new Rect(finalX, finalY - 8, 30, 20), displayText, style);
            }
        }
    }
}
