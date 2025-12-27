using System.IO;
using UnityEngine;
using KSP.UI.Screens;

namespace FlightHUD
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ToolbarButton : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private Texture2D iconTexture;
        private bool buttonAdded = false;

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
            }
        }

        private void OnGUIAppLauncherReady()
        {
            if (buttonAdded) return;

            iconTexture = CreateIconTexture();

            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnLeftClick,      // onTrue (left click when off)
                OnLeftClick,      // onFalse (left click when on)
                null,             // onHover
                null,             // onHoverOut
                null,             // onEnable
                null,             // onDisable
                ApplicationLauncher.AppScenes.FLIGHT,
                iconTexture
            );

            buttonAdded = true;
            Debug.Log("[FlightHUD] Toolbar button added");
        }

        private void OnGUIAppLauncherDestroyed()
        {
            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
                buttonAdded = false;
            }
        }

        private void OnLeftClick()
        {
            if (FlightHUDMain.Instance != null)
            {
                FlightHUDMain.Instance.ToggleHUD();
            }
        }

        public void Update()
        {
            // Right-click detection for settings
            if (toolbarButton != null && Input.GetMouseButtonDown(1))
            {
                // Check if mouse is over the toolbar button area
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                if (IsMouseOverButton(mousePos))
                {
                    FlightHUDMain.ShowSettingsGUI = !FlightHUDMain.ShowSettingsGUI;
                }
            }
        }

        private bool IsMouseOverButton(Vector2 mousePos)
        {
            if (toolbarButton == null) return false;

            // Get button position - approximate check for toolbar area
            // The toolbar is typically in the top-right corner
            float buttonSize = 38f;
            float toolbarX = Screen.width - 300f; // Approximate toolbar area

            return mousePos.x > toolbarX && mousePos.y < buttonSize + 10;
        }

        private Texture2D CreateIconTexture()
        {
            // Try to load from file first
            string iconPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FlightHUD/icon_toolbar.png");
            if (File.Exists(iconPath))
            {
                Texture2D tex = new Texture2D(38, 38);
                ImageConversion.LoadImage(tex, File.ReadAllBytes(iconPath));
                return tex;
            }

            // Generate icon programmatically - green -v- symbol
            Texture2D texture = new Texture2D(38, 38, TextureFormat.ARGB32, false);

            // Clear to transparent
            Color transparent = new Color(0, 0, 0, 0);
            Color green = new Color(0f, 1f, 0.5f, 1f);

            for (int y = 0; y < 38; y++)
            {
                for (int x = 0; x < 38; x++)
                {
                    texture.SetPixel(x, y, transparent);
                }
            }

            // Draw -v- symbol
            // Left wing (horizontal line)
            for (int x = 4; x < 14; x++)
            {
                texture.SetPixel(x, 19, green);
                texture.SetPixel(x, 18, green);
            }

            // Right wing (horizontal line)
            for (int x = 24; x < 34; x++)
            {
                texture.SetPixel(x, 19, green);
                texture.SetPixel(x, 18, green);
            }

            // V shape - left diagonal
            for (int i = 0; i < 8; i++)
            {
                int x = 14 + i;
                int y = 19 - i;
                texture.SetPixel(x, y, green);
                texture.SetPixel(x, y - 1, green);
                texture.SetPixel(x + 1, y, green);
            }

            // V shape - right diagonal
            for (int i = 0; i < 8; i++)
            {
                int x = 24 - i;
                int y = 19 - i;
                texture.SetPixel(x, y, green);
                texture.SetPixel(x, y - 1, green);
                texture.SetPixel(x - 1, y, green);
            }

            texture.Apply();
            return texture;
        }
    }
}
