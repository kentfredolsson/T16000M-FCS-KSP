using System;
using UnityEngine;

namespace FlightHUD
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class HUDSettingsGUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(Screen.width - 320, 100, 300, 450);
        private int currentTab = 0;
        private string[] tabs = { "Display", "Colors", "Elements" };

        private Vector2 scrollPosition;

        public void OnGUI()
        {
            if (!FlightHUDMain.ShowSettingsGUI) return;
            if (FlightHUDMain.Config == null) return;

            windowRect = GUILayout.Window(
                GetHashCode(),
                windowRect,
                DrawSettingsWindow,
                "FlightHUD Settings",
                GUILayout.Width(300),
                GUILayout.Height(450));
        }

        private void DrawSettingsWindow(int id)
        {
            HUDConfig config = FlightHUDMain.Config;

            currentTab = GUILayout.Toolbar(currentTab, tabs);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            switch (currentTab)
            {
                case 0:
                    DrawDisplayTab(config);
                    break;
                case 1:
                    DrawColorsTab(config);
                    break;
                case 2:
                    DrawElementsTab(config);
                    break;
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                config.Save();
                ScreenMessages.PostScreenMessage("FlightHUD settings saved", 2f, ScreenMessageStyle.UPPER_CENTER);
            }
            if (GUILayout.Button("Reset"))
            {
                ResetToDefaults(config);
            }
            if (GUILayout.Button("Close"))
            {
                FlightHUDMain.ShowSettingsGUI = false;
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void DrawDisplayTab(HUDConfig config)
        {
            GUILayout.Label("HUD Visibility", GUI.skin.box);
            config.HUDEnabled = GUILayout.Toggle(config.HUDEnabled, "HUD Enabled");

            GUILayout.Space(10);
            GUILayout.Label("Toggle Hotkey: " + config.HUDToggleKey.ToString());

            GUILayout.Space(15);
            GUILayout.Label("Scale", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Size: " + config.HUDScale.ToString("F1"));
            config.HUDScale = GUILayout.HorizontalSlider(config.HUDScale, 0.5f, 2.0f, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            GUILayout.Label("Opacity", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Alpha: " + config.HUDOpacity.ToString("F1"));
            config.HUDOpacity = GUILayout.HorizontalSlider(config.HUDOpacity, 0.3f, 1.0f, GUILayout.Width(150));
            GUILayout.EndHorizontal();
        }

        private void DrawColorsTab(HUDConfig config)
        {
            GUILayout.Label("Color Scheme", GUI.skin.box);

            string[] schemeNames = Enum.GetNames(typeof(ColorScheme));
            int currentScheme = (int)config.ActiveScheme;

            for (int i = 0; i < schemeNames.Length; i++)
            {
                bool isSelected = (currentScheme == i);
                bool newSelected = GUILayout.Toggle(isSelected, schemeNames[i]);
                if (newSelected && !isSelected)
                {
                    config.ActiveScheme = (ColorScheme)i;
                }
            }

            if (config.ActiveScheme == ColorScheme.Custom)
            {
                GUILayout.Space(15);
                GUILayout.Label("Custom RGB", GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.Label("R: " + config.CustomColorR.ToString("F2"), GUILayout.Width(60));
                config.CustomColorR = GUILayout.HorizontalSlider(config.CustomColorR, 0f, 1f, GUILayout.Width(150));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("G: " + config.CustomColorG.ToString("F2"), GUILayout.Width(60));
                config.CustomColorG = GUILayout.HorizontalSlider(config.CustomColorG, 0f, 1f, GUILayout.Width(150));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("B: " + config.CustomColorB.ToString("F2"), GUILayout.Width(60));
                config.CustomColorB = GUILayout.HorizontalSlider(config.CustomColorB, 0f, 1f, GUILayout.Width(150));
                GUILayout.EndHorizontal();
            }

            // Preview color
            GUILayout.Space(15);
            GUILayout.Label("Preview", GUI.skin.box);
            Color previewColor = config.GetHUDColor();
            GUIStyle previewStyle = new GUIStyle(GUI.skin.box);
            previewStyle.normal.background = MakeColorTexture(previewColor);
            GUILayout.Box("", previewStyle, GUILayout.Height(30));
        }

        private void DrawElementsTab(HUDConfig config)
        {
            GUILayout.Label("HUD Elements", GUI.skin.box);

            config.ShowPitchLadder = GUILayout.Toggle(config.ShowPitchLadder, "Pitch Ladder");
            config.ShowFlightPathVector = GUILayout.Toggle(config.ShowFlightPathVector, "Flight Path Vector");
            config.ShowAircraftSymbol = GUILayout.Toggle(config.ShowAircraftSymbol, "Aircraft Symbol (-v-)");
            config.ShowBankIndicator = GUILayout.Toggle(config.ShowBankIndicator, "Bank Indicator");

            GUILayout.Space(10);
            GUILayout.Label("Tapes & Readouts", GUI.skin.box);

            config.ShowHeadingTape = GUILayout.Toggle(config.ShowHeadingTape, "Heading Tape");
            config.ShowAirspeedTape = GUILayout.Toggle(config.ShowAirspeedTape, "Airspeed Tape");
            config.ShowAltitudeTape = GUILayout.Toggle(config.ShowAltitudeTape, "Altitude Tape");
            config.ShowVSI = GUILayout.Toggle(config.ShowVSI, "Vertical Speed Indicator");

            GUILayout.Space(10);
            GUILayout.Label("Additional", GUI.skin.box);

            config.ShowGForceAOA = GUILayout.Toggle(config.ShowGForceAOA, "G-Force & AOA Display");
            config.ShowCompassRose = GUILayout.Toggle(config.ShowCompassRose, "Compass Rose");
        }

        private void ResetToDefaults(HUDConfig config)
        {
            config.HUDEnabled = true;
            config.HUDScale = 1.0f;
            config.HUDOpacity = 0.9f;
            config.ActiveScheme = ColorScheme.Green;
            config.CustomColorR = 0f;
            config.CustomColorG = 1f;
            config.CustomColorB = 0.5f;

            config.ShowPitchLadder = true;
            config.ShowFlightPathVector = true;
            config.ShowAircraftSymbol = true;
            config.ShowAirspeedTape = true;
            config.ShowAltitudeTape = true;
            config.ShowHeadingTape = true;
            config.ShowBankIndicator = true;
            config.ShowVSI = true;
            config.ShowGForceAOA = true;
            config.ShowCompassRose = true;

            ScreenMessages.PostScreenMessage("FlightHUD settings reset to defaults", 2f, ScreenMessageStyle.UPPER_CENTER);
        }

        private Texture2D MakeColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
    }
}
