using UnityEngine;

namespace FlightHUD
{
    public static class HUDSettingsWindow
    {
        private static Rect windowRect = new Rect(100, 100, 320, 500);
        private static int windowId = 9876;
        private static Vector2 scrollPos = Vector2.zero;

        private static bool waitingForKey = false;

        // Foldout states for transform sections
        private static bool showPitchLadderTransforms = false;
        private static bool showPitchNumbersTransforms = false;
        private static bool showFPVTransforms = false;
        private static bool showBankTransforms = false;

        private static string[] rotationOptions = { "0", "90", "180", "270" };

        public static void Draw()
        {
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, "Flight HUD Settings");
        }

        private static void WindowFunction(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // === GENERAL SECTION ===
            GUILayout.Label("General", GetHeaderStyle());
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("HUD Enabled:", GUILayout.Width(100));
            bool newEnabled = GUILayout.Toggle(HUDSettings.HUDEnabled, HUDSettings.HUDEnabled ? "ON" : "OFF");
            if (newEnabled != HUDSettings.HUDEnabled)
            {
                HUDSettings.HUDEnabled = newEnabled;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Toggle Key:", GUILayout.Width(100));
            if (waitingForKey)
            {
                GUILayout.Label("Press a key...", GUILayout.Width(100));
                Event e = Event.current;
                if (e.isKey && e.type == EventType.KeyDown)
                {
                    HUDSettings.ToggleKey = e.keyCode;
                    waitingForKey = false;
                }
            }
            else
            {
                if (GUILayout.Button(HUDSettings.ToggleKey.ToString(), GUILayout.Width(80)))
                {
                    waitingForKey = true;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Scale: {HUDSettings.HUDScale:F1}x", GUILayout.Width(100));
            float newScale = GUILayout.HorizontalSlider(HUDSettings.HUDScale, 0.5f, 2.0f);
            if (newScale != HUDSettings.HUDScale)
            {
                HUDSettings.HUDScale = newScale;
                FlightHUDCore.Instance?.ResetStyles();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Opacity: {HUDSettings.HUDOpacity:P0}", GUILayout.Width(100));
            HUDSettings.HUDOpacity = GUILayout.HorizontalSlider(HUDSettings.HUDOpacity, 0.1f, 1.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // === COLOR SECTION ===
            GUILayout.Label("Color Scheme", GetHeaderStyle());
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(HUDSettings.ActiveScheme == ColorScheme.Green, "Green", "Button"))
                HUDSettings.ActiveScheme = ColorScheme.Green;
            if (GUILayout.Toggle(HUDSettings.ActiveScheme == ColorScheme.Amber, "Amber", "Button"))
                HUDSettings.ActiveScheme = ColorScheme.Amber;
            if (GUILayout.Toggle(HUDSettings.ActiveScheme == ColorScheme.Cyan, "Cyan", "Button"))
                HUDSettings.ActiveScheme = ColorScheme.Cyan;
            if (GUILayout.Toggle(HUDSettings.ActiveScheme == ColorScheme.White, "White", "Button"))
                HUDSettings.ActiveScheme = ColorScheme.White;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(HUDSettings.ActiveScheme == ColorScheme.Custom, "Custom", "Button"))
                HUDSettings.ActiveScheme = ColorScheme.Custom;
            GUILayout.EndHorizontal();

            if (HUDSettings.ActiveScheme == ColorScheme.Custom)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("R:", GUILayout.Width(20));
                HUDSettings.CustomR = GUILayout.HorizontalSlider(HUDSettings.CustomR, 0f, 1f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("G:", GUILayout.Width(20));
                HUDSettings.CustomG = GUILayout.HorizontalSlider(HUDSettings.CustomG, 0f, 1f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("B:", GUILayout.Width(20));
                HUDSettings.CustomB = GUILayout.HorizontalSlider(HUDSettings.CustomB, 0f, 1f);
                GUILayout.EndHorizontal();
            }

            // Color preview
            Color previewColor = HUDSettings.GetHUDColor();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preview:", GUILayout.Width(60));
            GUIStyle previewStyle = new GUIStyle(GUI.skin.box);
            previewStyle.normal.background = MakeColorTexture(previewColor);
            GUILayout.Box("", previewStyle, GUILayout.Width(50), GUILayout.Height(20));
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            // === ELEMENTS SECTION ===
            GUILayout.Label("HUD Elements", GetHeaderStyle());
            GUILayout.Space(5);

            // Pitch Ladder with transforms
            GUILayout.BeginHorizontal();
            HUDSettings.ShowPitchLadder = GUILayout.Toggle(HUDSettings.ShowPitchLadder, "Pitch Ladder", GUILayout.Width(150));
            if (GUILayout.Button(showPitchLadderTransforms ? "v" : ">", GUILayout.Width(25)))
                showPitchLadderTransforms = !showPitchLadderTransforms;
            GUILayout.EndHorizontal();

            if (showPitchLadderTransforms)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Rotation:", GUILayout.Width(80));
                // Use regular buttons instead of toggles for rotation
                for (int i = 0; i < 4; i++)
                {
                    bool isSelected = (HUDSettings.PitchLadderRotation / 90) == i;
                    GUI.backgroundColor = isSelected ? Color.green : Color.white;
                    if (GUILayout.Button(rotationOptions[i], GUILayout.Width(40)))
                        HUDSettings.PitchLadderRotation = i * 90;
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                HUDSettings.PitchLadderNegateX = GUILayout.Toggle(HUDSettings.PitchLadderNegateX, "  Negate X");
                HUDSettings.PitchLadderNegateY = GUILayout.Toggle(HUDSettings.PitchLadderNegateY, "  Negate Y");
                HUDSettings.PitchLadderMirrorX = GUILayout.Toggle(HUDSettings.PitchLadderMirrorX, "  Mirror X");
                HUDSettings.PitchLadderMirrorY = GUILayout.Toggle(HUDSettings.PitchLadderMirrorY, "  Mirror Y");

                // Reset button
                if (GUILayout.Button("  Reset Pitch Ladder"))
                {
                    HUDSettings.PitchLadderRotation = 0;
                    HUDSettings.PitchLadderNegateX = false;
                    HUDSettings.PitchLadderNegateY = false;
                    HUDSettings.PitchLadderMirrorX = false;
                    HUDSettings.PitchLadderMirrorY = false;
                }
                GUILayout.EndVertical();
            }

            // Pitch Ladder Numbers with transforms
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Pitch Ladder Numbers", GUILayout.Width(150));
            if (GUILayout.Button(showPitchNumbersTransforms ? "v" : ">", GUILayout.Width(25)))
                showPitchNumbersTransforms = !showPitchNumbersTransforms;
            GUILayout.EndHorizontal();

            if (showPitchNumbersTransforms)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Rotation:", GUILayout.Width(80));
                for (int i = 0; i < 4; i++)
                {
                    bool isSelected = (HUDSettings.PitchNumbersRotation / 90) == i;
                    GUI.backgroundColor = isSelected ? Color.green : Color.white;
                    if (GUILayout.Button(rotationOptions[i], GUILayout.Width(40)))
                        HUDSettings.PitchNumbersRotation = i * 90;
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                HUDSettings.PitchNumbersNegateX = GUILayout.Toggle(HUDSettings.PitchNumbersNegateX, "  Negate X");
                HUDSettings.PitchNumbersNegateY = GUILayout.Toggle(HUDSettings.PitchNumbersNegateY, "  Negate Y");
                HUDSettings.PitchNumbersNegate = GUILayout.Toggle(HUDSettings.PitchNumbersNegate, "  Negate Values (+/-)");

                if (GUILayout.Button("  Reset Pitch Numbers"))
                {
                    HUDSettings.PitchNumbersRotation = 0;
                    HUDSettings.PitchNumbersNegateX = false;
                    HUDSettings.PitchNumbersNegateY = false;
                    HUDSettings.PitchNumbersNegate = false;
                }
                GUILayout.EndVertical();
            }

            // FPV with transforms
            GUILayout.BeginHorizontal();
            HUDSettings.ShowFPV = GUILayout.Toggle(HUDSettings.ShowFPV, "Flight Path Vector", GUILayout.Width(150));
            if (GUILayout.Button(showFPVTransforms ? "v" : ">", GUILayout.Width(25)))
                showFPVTransforms = !showFPVTransforms;
            GUILayout.EndHorizontal();

            if (showFPVTransforms)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                HUDSettings.FPVNegateX = GUILayout.Toggle(HUDSettings.FPVNegateX, "  Negate X");
                HUDSettings.FPVNegateY = GUILayout.Toggle(HUDSettings.FPVNegateY, "  Negate Y");
                if (GUILayout.Button("  Reset FPV"))
                {
                    HUDSettings.FPVNegateX = false;
                    HUDSettings.FPVNegateY = false;
                }
                GUILayout.EndVertical();
            }

            // Bank Indicator with transforms
            GUILayout.BeginHorizontal();
            HUDSettings.ShowBankIndicator = GUILayout.Toggle(HUDSettings.ShowBankIndicator, "Bank Indicator", GUILayout.Width(150));
            if (GUILayout.Button(showBankTransforms ? "v" : ">", GUILayout.Width(25)))
                showBankTransforms = !showBankTransforms;
            GUILayout.EndHorizontal();

            if (showBankTransforms)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                HUDSettings.BankNegate = GUILayout.Toggle(HUDSettings.BankNegate, "  Negate");
                HUDSettings.BankMirror = GUILayout.Toggle(HUDSettings.BankMirror, "  Mirror");
                if (GUILayout.Button("  Reset Bank"))
                {
                    HUDSettings.BankNegate = false;
                    HUDSettings.BankMirror = false;
                }
                GUILayout.EndVertical();
            }

            HUDSettings.ShowHeadingTape = GUILayout.Toggle(HUDSettings.ShowHeadingTape, "Heading Tape");
            HUDSettings.ShowAirspeedTape = GUILayout.Toggle(HUDSettings.ShowAirspeedTape, "Airspeed Tape");
            HUDSettings.ShowAltitudeTape = GUILayout.Toggle(HUDSettings.ShowAltitudeTape, "Altitude Tape");
            HUDSettings.ShowVSI = GUILayout.Toggle(HUDSettings.ShowVSI, "Vertical Speed Indicator");
            HUDSettings.ShowCompassRose = GUILayout.Toggle(HUDSettings.ShowCompassRose, "Compass Rose");
            HUDSettings.ShowStatusIcons = GUILayout.Toggle(HUDSettings.ShowStatusIcons, "Status Icons (BRK/LTS/GER)");

            GUILayout.Space(10);
            GUILayout.Label("Data Displays", GetHeaderStyle());
            GUILayout.Space(5);

            HUDSettings.ShowGForce = GUILayout.Toggle(HUDSettings.ShowGForce, "G-Force");
            HUDSettings.ShowAOA = GUILayout.Toggle(HUDSettings.ShowAOA, "Angle of Attack");
            HUDSettings.ShowMach = GUILayout.Toggle(HUDSettings.ShowMach, "Mach Number");
            HUDSettings.ShowRadarAlt = GUILayout.Toggle(HUDSettings.ShowRadarAlt, "Radar Altitude (AGL)");

            GUILayout.Space(15);

            // === BUTTONS ===
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                HUDSettings.Save();
                ScreenMessages.PostScreenMessage("FlightHUD settings saved", 2f, ScreenMessageStyle.UPPER_CENTER);
            }
            if (GUILayout.Button("Close"))
            {
                HUDSettings.SettingsWindowVisible = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private static GUIStyle GetHeaderStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;
            return style;
        }

        private static Texture2D colorTex;

        private static Texture2D MakeColorTexture(Color color)
        {
            if (colorTex == null)
            {
                colorTex = new Texture2D(1, 1);
            }
            colorTex.SetPixel(0, 0, color);
            colorTex.Apply();
            return colorTex;
        }
    }
}
