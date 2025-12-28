using UnityEngine;

namespace FlightHUD
{
    public static class FlightData
    {
        // Attitude
        public static float Pitch { get; private set; }
        public static float Roll { get; private set; }
        public static float Heading { get; private set; }

        // Speed
        public static float Airspeed { get; private set; }
        public static float MachNumber { get; private set; }
        public static float VerticalSpeed { get; private set; }

        // Altitude
        public static float AltitudeASL { get; private set; }
        public static float AltitudeAGL { get; private set; }

        // Flight dynamics
        public static float GForce { get; private set; }
        public static float AOA { get; private set; }
        public static float Sideslip { get; private set; }

        // Flight path vector offset (degrees from nose)
        public static Vector2 FPVOffset { get; private set; }

        // Vehicle status
        public static bool BrakesOn { get; private set; }
        public static bool LightsOn { get; private set; }
        public static bool GearDeployed { get; private set; }

        // Smoothed FPV for stable display
        private static Vector2 smoothedFPV = Vector2.zero;
        private static bool fpvInitialized = false;

        public static void Update()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null) return;

            // Reference vectors
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 north = Vector3.ProjectOnPlane(vessel.mainBody.transform.up, up).normalized;
            Vector3 forward = vessel.transform.up;  // KSP: vessel "up" is forward/nose
            Vector3 right = vessel.transform.right;

            // Pitch: angle between forward and horizontal plane
            Vector3 forwardHoriz = Vector3.ProjectOnPlane(forward, up).normalized;
            if (forwardHoriz.sqrMagnitude > 0.001f)
            {
                Pitch = Vector3.SignedAngle(forwardHoriz, forward, Vector3.Cross(up, forwardHoriz));
            }

            // Roll: angle between vessel right and horizon right
            Vector3 horizRight = Vector3.Cross(up, forwardHoriz).normalized;
            if (horizRight.sqrMagnitude > 0.001f)
            {
                Roll = Vector3.SignedAngle(horizRight, right, forward);
            }

            // Heading: compass direction
            Heading = Vector3.SignedAngle(north, forwardHoriz, up);
            if (Heading < 0) Heading += 360f;

            // Airspeed (surface velocity magnitude)
            Airspeed = (float)vessel.srfSpeed;

            // Mach number
            MachNumber = (float)vessel.mach;

            // Vertical speed
            VerticalSpeed = (float)vessel.verticalSpeed;

            // Altitude
            AltitudeASL = (float)vessel.altitude;
            AltitudeAGL = (float)vessel.radarAltitude;

            // G-Force
            GForce = (float)vessel.geeForce;

            // Angle of Attack and Sideslip
            if (vessel.srfSpeed > 1)
            {
                Vector3 velocity = vessel.srf_velocity.normalized;
                AOA = Vector3.SignedAngle(velocity, forward, right);
                Sideslip = Vector3.SignedAngle(velocity, forward, vessel.transform.forward);
            }
            else
            {
                AOA = 0;
                Sideslip = 0;
            }

            // Flight Path Vector offset (where velocity vector is relative to nose)
            if (vessel.srfSpeed > 10)
            {
                Vector3 velocity = vessel.srf_velocity.normalized;

                // Calculate FPV in vessel-relative coordinates for stability
                // Project velocity onto vessel's local coordinate system
                float fpvRight = Vector3.Dot(velocity, right);  // Sideslip component
                float fpvUp = Vector3.Dot(velocity, vessel.transform.forward);  // Vertical component relative to nose
                float fpvForward = Vector3.Dot(velocity, forward);  // Forward component

                // Convert to angles (in degrees) - only if we have meaningful forward velocity
                if (fpvForward > 0.1f)
                {
                    float rawYaw = Mathf.Atan2(fpvRight, fpvForward) * Mathf.Rad2Deg;
                    float rawPitch = Mathf.Atan2(fpvUp, fpvForward) * Mathf.Rad2Deg;

                    Vector2 rawFPV = new Vector2(rawYaw, rawPitch);

                    // Apply heavy smoothing to prevent jumps
                    if (!fpvInitialized)
                    {
                        smoothedFPV = rawFPV;
                        fpvInitialized = true;
                    }
                    else
                    {
                        // Only update if the change is reasonable (not a sudden jump)
                        float maxDelta = 5f;  // Max degrees per frame
                        Vector2 delta = rawFPV - smoothedFPV;

                        if (delta.magnitude > maxDelta)
                        {
                            // Large jump detected - smooth more aggressively
                            smoothedFPV = Vector2.Lerp(smoothedFPV, rawFPV, 0.05f);
                        }
                        else
                        {
                            // Normal smoothing
                            smoothedFPV = Vector2.Lerp(smoothedFPV, rawFPV, 0.2f);
                        }
                    }

                    FPVOffset = smoothedFPV;
                }
                else
                {
                    // Flying backwards or very slow forward - fade to center
                    smoothedFPV = Vector2.Lerp(smoothedFPV, Vector2.zero, 0.1f);
                    FPVOffset = smoothedFPV;
                }
            }
            else
            {
                // Too slow - smoothly return to center
                smoothedFPV = Vector2.Lerp(smoothedFPV, Vector2.zero, 0.1f);
                FPVOffset = smoothedFPV;
                fpvInitialized = false;
            }

            // Vehicle status
            BrakesOn = vessel.ActionGroups[KSPActionGroup.Brakes];
            LightsOn = vessel.ActionGroups[KSPActionGroup.Light];
            GearDeployed = vessel.ActionGroups[KSPActionGroup.Gear];
        }
    }
}
