using UnityEngine;
using KSP.UI.Screens;

namespace FlightHUD
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HUDToolbar : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private Texture2D iconTexture;

        public void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

            if (ApplicationLauncher.Ready)
            {
                OnGUIAppLauncherReady();
            }
        }

        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);

            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
            }

            if (iconTexture != null)
            {
                Destroy(iconTexture);
                iconTexture = null;
            }
        }

        private void OnGUIAppLauncherReady()
        {
            if (toolbarButton != null) return;

            iconTexture = CreateIconTexture();

            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnLeftClick,
                OnLeftClick,
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.FLIGHT,
                iconTexture
            );
        }

        private void OnGUIAppLauncherDestroyed()
        {
            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
            }
        }

        private void OnLeftClick()
        {
            HUDSettings.SettingsWindowVisible = !HUDSettings.SettingsWindowVisible;
        }

        private Texture2D CreateIconTexture()
        {
            int size = 38;
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);

            Color transparent = new Color(0, 0, 0, 0);
            Color yellow = new Color(0.94f, 0.76f, 0.25f, 1f);  // Golden yellow #F0C040
            Color green = new Color(0.56f, 0.93f, 0.56f, 1f);   // Light green #90EE90

            // Fill with transparent background
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, transparent);
                }
            }

            int cx = size / 2;

            // === TOP PART: Yellow sun/circle with wings ===
            int circleY = 26;  // Center Y of circle (near top)
            int circleR = 8;   // Radius

            // Draw circle ring
            for (int angle = 0; angle < 360; angle += 3)
            {
                float rad = angle * Mathf.Deg2Rad;
                int px = cx + Mathf.RoundToInt(Mathf.Cos(rad) * circleR);
                int py = circleY + Mathf.RoundToInt(Mathf.Sin(rad) * circleR);
                SetPixelThick(tex, px, py, yellow);
            }

            // Small triangle pointer at bottom of circle
            for (int i = 0; i < 4; i++)
            {
                tex.SetPixel(cx - i, circleY - circleR - i, yellow);
                tex.SetPixel(cx + i, circleY - circleR - i, yellow);
            }

            // Left wing from circle
            for (int i = 0; i < 10; i++)
            {
                SetPixelThick(tex, cx - circleR - 2 - i, circleY, yellow);
            }

            // Right wing from circle
            for (int i = 0; i < 10; i++)
            {
                SetPixelThick(tex, cx + circleR + 2 + i, circleY, yellow);
            }

            // === BOTTOM PART: Green V with tail ===
            int vCenterY = 12;  // Center of V (lower part)

            // V shape - left leg
            for (int i = 0; i < 10; i++)
            {
                int px = cx - 10 + i;
                int py = vCenterY + 6 - i;
                SetPixelThick(tex, px, py, green);
            }

            // V shape - right leg
            for (int i = 0; i < 10; i++)
            {
                int px = cx + 10 - i;
                int py = vCenterY + 6 - i;
                SetPixelThick(tex, px, py, green);
            }

            // Vertical tail extending down from V
            for (int i = 0; i < 10; i++)
            {
                SetPixelThick(tex, cx, vCenterY - 4 - i, green);
            }

            // Small wings on V
            for (int i = 0; i < 6; i++)
            {
                tex.SetPixel(cx - 10 - i, vCenterY + 4, green);
                tex.SetPixel(cx + 10 + i, vCenterY + 4, green);
            }

            tex.Apply();
            return tex;
        }

        private void SetPixelThick(Texture2D tex, int x, int y, Color color)
        {
            if (x >= 0 && x < tex.width - 1 && y >= 0 && y < tex.height - 1)
            {
                tex.SetPixel(x, y, color);
                tex.SetPixel(x + 1, y, color);
                tex.SetPixel(x, y + 1, color);
                tex.SetPixel(x + 1, y + 1, color);
            }
        }
    }
}
