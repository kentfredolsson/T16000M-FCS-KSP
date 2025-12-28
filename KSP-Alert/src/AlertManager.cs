using System.Collections.Generic;
using UnityEngine;

namespace KSPAlert
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AlertManager : MonoBehaviour
    {
        public static AlertManager Instance { get; private set; }

        public AlertConfig Config { get; private set; }

        private Dictionary<AlertType, Alert> alerts;
        private List<Alert> activeAlerts;

        private AlertDisplay display;
        private AlertAudio audio;

        // Vessel data cache
        private Vessel vessel;
        private double altitudeAGL;
        private double verticalSpeed;
        private double surfaceSpeed;
        private double fuelPercent;
        private double powerPercent;
        private double gForce;
        private double maxPartTemp;
        private bool hasGear;
        private bool gearDeployed;
        private bool hasComms;

        private float lastUpdateTime;
        private const float UPDATE_INTERVAL = 0.1f; // 10 Hz update rate

        void Awake()
        {
            Instance = this;
            Config = new AlertConfig();
            Config.Load();

            InitializeAlerts();
            activeAlerts = new List<Alert>();
        }

        void Start()
        {
            display = gameObject.AddComponent<AlertDisplay>();
            audio = gameObject.AddComponent<AlertAudio>();

            Debug.Log("[KSP-Alert] Alert system initialized");
        }

        void OnDestroy()
        {
            Config.Save();
            Instance = null;
        }

        private void InitializeAlerts()
        {
            alerts = new Dictionary<AlertType, Alert>
            {
                { AlertType.Terrain, Alert.CreateTerrain() },
                { AlertType.GearUp, Alert.CreateGearUp() },
                { AlertType.LowFuel, Alert.CreateLowFuel() },
                { AlertType.LowPower, Alert.CreateLowPower() },
                { AlertType.Overheat, Alert.CreateOverheat() },
                { AlertType.Stall, Alert.CreateStall() },
                { AlertType.HighG, Alert.CreateHighG() },
                { AlertType.CommsLost, Alert.CreateCommsLost() }
            };
        }

        void Update()
        {
            if (!Config.Enabled) return;
            if (!HighLogic.LoadedSceneIsFlight) return;

            vessel = FlightGlobals.ActiveVessel;
            if (vessel == null) return;

            // Throttle updates
            if (Time.time - lastUpdateTime < UPDATE_INTERVAL) return;
            lastUpdateTime = Time.time;

            UpdateVesselData();
            CheckAllAlerts();
        }

        private void UpdateVesselData()
        {
            altitudeAGL = vessel.radarAltitude;
            verticalSpeed = vessel.verticalSpeed;
            surfaceSpeed = vessel.srfSpeed;
            gForce = vessel.geeForce;

            // Calculate fuel percentage
            double totalFuel = 0;
            double maxFuel = 0;
            foreach (var part in vessel.parts)
            {
                foreach (var resource in part.Resources)
                {
                    if (resource.resourceName == "LiquidFuel" ||
                        resource.resourceName == "Oxidizer" ||
                        resource.resourceName == "MonoPropellant")
                    {
                        totalFuel += resource.amount;
                        maxFuel += resource.maxAmount;
                    }
                }
            }
            fuelPercent = maxFuel > 0 ? (totalFuel / maxFuel) * 100 : 100;

            // Calculate power percentage
            double totalPower = 0;
            double maxPower = 0;
            foreach (var part in vessel.parts)
            {
                foreach (var resource in part.Resources)
                {
                    if (resource.resourceName == "ElectricCharge")
                    {
                        totalPower += resource.amount;
                        maxPower += resource.maxAmount;
                    }
                }
            }
            powerPercent = maxPower > 0 ? (totalPower / maxPower) * 100 : 100;

            // Check for landing gear using action groups
            hasGear = false;
            gearDeployed = vessel.ActionGroups[KSPActionGroup.Gear];

            // Check if vessel has any gear parts
            foreach (var part in vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module.moduleName == "ModuleWheelDeployment" ||
                        module.moduleName == "ModuleLandingGear" ||
                        module.moduleName == "ModuleLandingLeg")
                    {
                        hasGear = true;
                        break;
                    }
                }
                if (hasGear) break;
            }

            // Check max part temperature
            maxPartTemp = 0;
            foreach (var part in vessel.parts)
            {
                double tempPercent = part.temperature / part.maxTemp * 100;
                if (tempPercent > maxPartTemp)
                {
                    maxPartTemp = tempPercent;
                }
            }

            // Check comms
            hasComms = vessel.Connection != null && vessel.Connection.IsConnected;
        }

        private void CheckAllAlerts()
        {
            activeAlerts.Clear();

            CheckTerrainAlert();
            CheckGearAlert();
            CheckFuelAlert();
            CheckPowerAlert();
            CheckOverheatAlert();
            CheckHighGAlert();
            CheckCommsAlert();

            // Sort by priority (highest first)
            activeAlerts.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            // Update display
            display?.UpdateAlerts(activeAlerts);
        }

        private bool IsAlertSilenced(AlertType type)
        {
            // Check both old panel and new main window for backwards compatibility
            if (AlertMainWindow.Instance != null && AlertMainWindow.Instance.IsAlertSilenced(type))
                return true;
            if (AlertPanel.Instance != null && AlertPanel.Instance.IsAlertSilenced(type))
                return true;
            return false;
        }

        private void CheckTerrainAlert()
        {
            if (!Config.TerrainEnabled) return;

            var alert = alerts[AlertType.Terrain];

            // Only trigger if descending toward terrain
            bool inDanger = altitudeAGL < Config.TerrainWarningAltitude &&
                           verticalSpeed < Config.TerrainMinDescentRate &&
                           vessel.situation == Vessel.Situations.FLYING;

            if (inDanger)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.Terrain))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckGearAlert()
        {
            if (!Config.GearEnabled) return;
            if (!hasGear) return;

            var alert = alerts[AlertType.GearUp];

            // Warn if low, slow, descending, and gear not deployed
            bool needsGear = altitudeAGL < Config.GearWarningAltitude &&
                            surfaceSpeed < Config.GearWarningSpeed &&
                            verticalSpeed < -1 &&
                            !gearDeployed &&
                            vessel.situation == Vessel.Situations.FLYING;

            if (needsGear)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.GearUp))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckFuelAlert()
        {
            if (!Config.FuelEnabled) return;

            var alert = alerts[AlertType.LowFuel];

            if (fuelPercent < Config.FuelWarningPercent)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.LowFuel))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckPowerAlert()
        {
            if (!Config.PowerEnabled) return;

            var alert = alerts[AlertType.LowPower];

            if (powerPercent < Config.PowerWarningPercent)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.LowPower))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckOverheatAlert()
        {
            if (!Config.OverheatEnabled) return;

            var alert = alerts[AlertType.Overheat];

            if (maxPartTemp > Config.OverheatWarningPercent)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.Overheat))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckHighGAlert()
        {
            if (!Config.HighGEnabled) return;

            var alert = alerts[AlertType.HighG];

            if (gForce > Config.HighGWarning)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.HighG))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        private void CheckCommsAlert()
        {
            if (!Config.CommsEnabled) return;

            var alert = alerts[AlertType.CommsLost];

            if (!hasComms && vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                if (alert.CanTrigger() && !IsAlertSilenced(AlertType.CommsLost))
                {
                    alert.Trigger();
                    audio?.PlayAlert(alert);
                }
                activeAlerts.Add(alert);
            }
            else
            {
                alert.Clear();
            }
        }

        public void TriggerAlert(AlertType type)
        {
            if (alerts.ContainsKey(type))
            {
                var alert = alerts[type];
                alert.Trigger();
                audio?.PlayAlert(alert);
            }
        }

        public void TriggerTestAlert(AlertType type)
        {
            // Force trigger for testing, bypass cooldown
            if (alerts.ContainsKey(type))
            {
                var alert = alerts[type];
                alert.LastTriggered = 0f; // Reset cooldown
                alert.Trigger();
                audio?.PlayAlert(alert);
                activeAlerts.Add(alert);
                display?.UpdateAlerts(activeAlerts);
            }
        }
    }
}
