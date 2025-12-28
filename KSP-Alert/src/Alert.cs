using System;

namespace KSPAlert
{
    public enum AlertPriority
    {
        Advisory = 0,    // Blue - informational
        Caution = 1,     // Amber - action needed soon
        Warning = 2      // Red - immediate action required
    }

    public enum AlertType
    {
        // Warning level (Red)
        Terrain,
        Overheat,
        Stall,
        Impact,

        // Caution level (Amber)
        GearUp,
        LowFuel,
        LowPower,
        HighG,
        CommsLost,

        // Advisory level (Blue)
        GearDown,
        OrbitAchieved,
        SASChange,
        Staging
    }

    public class Alert
    {
        public AlertType Type { get; private set; }
        public AlertPriority Priority { get; private set; }
        public string Message { get; private set; }
        public string VoiceCallout { get; private set; }
        public float Timestamp { get; private set; }
        public bool IsActive { get; set; }
        public float LastTriggered { get; set; }
        public float CooldownSeconds { get; set; }

        public Alert(AlertType type, AlertPriority priority, string message, string voiceCallout, float cooldown = 3f)
        {
            Type = type;
            Priority = priority;
            Message = message;
            VoiceCallout = voiceCallout;
            Timestamp = UnityEngine.Time.time;
            IsActive = false;
            LastTriggered = 0f;
            CooldownSeconds = cooldown;
        }

        public bool CanTrigger()
        {
            return UnityEngine.Time.time - LastTriggered >= CooldownSeconds;
        }

        public void Trigger()
        {
            IsActive = true;
            LastTriggered = UnityEngine.Time.time;
            Timestamp = UnityEngine.Time.time;
        }

        public void Clear()
        {
            IsActive = false;
        }

        public static Alert CreateTerrain()
        {
            return new Alert(AlertType.Terrain, AlertPriority.Warning,
                "TERRAIN", "TERRAIN TERRAIN PULL UP", 2f);
        }

        public static Alert CreateGearUp()
        {
            return new Alert(AlertType.GearUp, AlertPriority.Caution,
                "GEAR", "TOO LOW GEAR", 5f);
        }

        public static Alert CreateLowFuel()
        {
            return new Alert(AlertType.LowFuel, AlertPriority.Caution,
                "FUEL LOW", "FUEL LOW", 30f);
        }

        public static Alert CreateLowPower()
        {
            return new Alert(AlertType.LowPower, AlertPriority.Caution,
                "POWER LOW", "ELECTRIC LOW", 30f);
        }

        public static Alert CreateOverheat()
        {
            return new Alert(AlertType.Overheat, AlertPriority.Warning,
                "OVERHEAT", "OVERHEAT", 5f);
        }

        public static Alert CreateStall()
        {
            return new Alert(AlertType.Stall, AlertPriority.Warning,
                "STALL", "STALL STALL", 2f);
        }

        public static Alert CreateHighG()
        {
            return new Alert(AlertType.HighG, AlertPriority.Caution,
                "HIGH G", "G LIMIT", 5f);
        }

        public static Alert CreateCommsLost()
        {
            return new Alert(AlertType.CommsLost, AlertPriority.Caution,
                "NO SIGNAL", "SIGNAL LOST", 30f);
        }
    }
}
