using UnityEngine;
using KSP.UI.Screens;

namespace KSPAlert
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AlertToolbar : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private Texture2D buttonTexture;
        private bool buttonAdded = false;

        void Start()
        {
            CreateButtonTexture();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

            if (ApplicationLauncher.Ready)
            {
                OnGUIAppLauncherReady();
            }
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
            RemoveButton();
        }

        private void CreateButtonTexture()
        {
            // Create a 38x38 texture for the toolbar button
            // This draws a simplified version of the GPWS logo
            int size = 38;
            buttonTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);

            // Fill with transparent
            Color transparent = new Color(0, 0, 0, 0);
            Color brown = new Color(0.55f, 0.35f, 0.15f, 1f);
            Color black = new Color(0.1f, 0.1f, 0.1f, 1f);
            Color red = new Color(0.9f, 0.15f, 0.15f, 1f);
            Color white = new Color(1f, 1f, 1f, 1f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    buttonTexture.SetPixel(x, y, transparent);
                }
            }

            // Draw mountain (brown triangle)
            DrawTriangle(buttonTexture, 8, 8, 30, 8, 19, 28, brown);

            // Draw aircraft silhouette (black)
            DrawAircraft(buttonTexture, black);

            // Draw exclamation mark (red)
            DrawExclamation(buttonTexture, red, white);

            buttonTexture.Apply();
        }

        private void DrawTriangle(Texture2D tex, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            // Simple triangle fill using scanline
            int minY = Mathf.Min(y1, Mathf.Min(y2, y3));
            int maxY = Mathf.Max(y1, Mathf.Max(y2, y3));

            for (int y = minY; y <= maxY; y++)
            {
                // Find x intersections at this y
                float t1 = GetXIntersection(x1, y1, x3, y3, y);
                float t2 = GetXIntersection(x2, y2, x3, y3, y);

                int startX = Mathf.RoundToInt(Mathf.Min(t1, t2));
                int endX = Mathf.RoundToInt(Mathf.Max(t1, t2));

                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private float GetXIntersection(int x1, int y1, int x2, int y2, int y)
        {
            if (y2 == y1) return x1;
            float t = (float)(y - y1) / (y2 - y1);
            return x1 + t * (x2 - x1);
        }

        private void DrawAircraft(Texture2D tex, Color color)
        {
            // Simple aircraft shape - fuselage
            for (int x = 6; x <= 24; x++)
            {
                for (int y = 10; y <= 14; y++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            // Wings
            for (int x = 12; x <= 20; x++)
            {
                int wingSpan = (x < 16) ? (16 - x) : (x - 16);
                for (int dy = -wingSpan; dy <= wingSpan; dy++)
                {
                    int y = 12 + dy;
                    if (y >= 6 && y <= 18)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }

            // Tail
            for (int x = 4; x <= 8; x++)
            {
                for (int y = 8; y <= 16; y++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        private void DrawExclamation(Texture2D tex, Color fillColor, Color outlineColor)
        {
            int centerX = 28;

            // Exclamation bar
            for (int y = 18; y <= 32; y++)
            {
                for (int x = centerX - 2; x <= centerX + 2; x++)
                {
                    tex.SetPixel(x, y, fillColor);
                }
            }

            // Exclamation dot
            for (int y = 12; y <= 16; y++)
            {
                for (int x = centerX - 2; x <= centerX + 2; x++)
                {
                    float dist = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - 14) * (y - 14));
                    if (dist <= 2.5f)
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }
        }

        private void OnGUIAppLauncherReady()
        {
            if (buttonAdded) return;

            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnButtonTrue,      // onTrue
                OnButtonFalse,     // onFalse
                null,              // onHover
                null,              // onHoverOut
                null,              // onEnable
                null,              // onDisable
                ApplicationLauncher.AppScenes.FLIGHT,
                buttonTexture
            );

            buttonAdded = true;
            Debug.Log("[KSP-Alert] Toolbar button added");
        }

        private void OnGUIAppLauncherDestroyed()
        {
            RemoveButton();
        }

        private void RemoveButton()
        {
            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
                buttonAdded = false;
            }
        }

        private void OnButtonTrue()
        {
            if (AlertPanel.Instance != null)
            {
                AlertPanel.Instance.ShowPanel();
            }
        }

        private void OnButtonFalse()
        {
            if (AlertPanel.Instance != null)
            {
                AlertPanel.Instance.HidePanel();
            }
        }
    }
}
