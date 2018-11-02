﻿using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Enum of vessel states
    /// </summary>
    public enum VesselState
    {
        Idle = 0,
        ControllerDisabled = 1,
        Current = 2,
        Moving = 3,
        AwaitingSunlight = 4
    }


    /// <summary>
    /// Result for display in the Control Window
    /// </summary>
    public struct DisplayedSystemCheckResult
    {
        public string Label;
        public string Text;
        public string Tooltip;
    }


    /// <summary>
    /// Basic controller
    /// </summary>
    public class BVController
    {
        #region Public properties

        public Vessel vessel; // Vessel containing BonVoyageModule

        public bool Shutdown
        {
            get { return shutdown; }
            set
            {
                shutdown = value;
                if (shutdown)
                    State = VesselState.ControllerDisabled;
                else
                    State = VesselState.Idle;
            }
        }

        public bool Active {  get { return active; } }

        public double RemainingDistanceToTarget { get { return distanceToTarget - distanceTravelled; } }
        public virtual double AverageSpeed { get { return 0; } }
        public event EventHandler OnStateChanged;

        #endregion


        #region Private and protected properties

        protected ConfigNode BVModule; // Config node of BonVoyageModule
        protected List<DisplayedSystemCheckResult> displayedSystemCheckResults;
        protected int mainStarIndex; // Vessel's main star's index in the FlightGlobals.Bodies

        // Config values
        private bool active = false;
        private bool shutdown = false;
        private double targetLatitude = 0;
        private double targetLongitude = 0;
        private double distanceToTarget = 0;
        private double distanceTravelled = 0;
        private double lastTimeUpdated = 0;
        // Config values
        
        private List<PathUtils.WayPoint> path = null; // Path to destination

        private VesselState _state;
        public VesselState State
        {
            get { return _state; }
            set
            {
                _state = value;
                if (OnStateChanged != null)
                    OnStateChanged(this, EventArgs.Empty);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public BVController(Vessel v, ConfigNode module)
        {
            vessel = v;
            BVModule = module;
            displayedSystemCheckResults = new List<DisplayedSystemCheckResult>();

            // Load values from config if it isn't the first run of the mod (we are reseting vessel on the first run)
            if (!Configuration.FirstRun)
            {
                active = bool.Parse(BVModule.GetValue("active") != null ? BVModule.GetValue("active") : "false");
                shutdown = bool.Parse(BVModule.GetValue("shutdown") != null ? BVModule.GetValue("shutdown") : "false");
                targetLatitude = double.Parse(BVModule.GetValue("targetLatitude") != null ? BVModule.GetValue("targetLatitude") : "0");
                targetLongitude = double.Parse(BVModule.GetValue("targetLongitude") != null ? BVModule.GetValue("targetLongitude") : "0");
                distanceToTarget = double.Parse(BVModule.GetValue("distanceToTarget") != null ? BVModule.GetValue("distanceToTarget") : "0");
                distanceTravelled = double.Parse(BVModule.GetValue("distanceTravelled") != null ? BVModule.GetValue("distanceTravelled") : "0");
                lastTimeUpdated = double.Parse(BVModule.GetValue("lastTimeUpdated") != null ? BVModule.GetValue("lastTimeUpdated") : "0");
                if (BVModule.GetValue("pathEncoded") != null)
                    path = PathUtils.DecodePath(BVModule.GetValue("pathEncoded"));
            }

            State = VesselState.Idle;
            if (shutdown)
                State = VesselState.ControllerDisabled;

            mainStarIndex = 0; // In the most cases The Sun
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public virtual int GetControllerType()
        {
            return -1;
        }


        #region Main window texts

        /// <summary>
        /// Get vessel state
        /// </summary>
        /// <returns></returns>
        public VesselState GetVesselState()
        {
            if (vessel.isActiveVessel)
                return VesselState.Current;
            return State;
        }


        /// <summary>
        /// Get textual reprezentation of the vessel status
        /// </summary>
        /// <returns></returns>
        public string GetVesselStateText()
        {
            if (vessel.isActiveVessel)
                return Localizer.Format("#LOC_BV_Status_Current");
            switch (State)
            {
                case VesselState.Idle:
                    return Localizer.Format("#LOC_BV_Status_Idle");
                case VesselState.ControllerDisabled:
                    return Localizer.Format("#LOC_BV_Status_Disabled");
                case VesselState.AwaitingSunlight:
                    return Localizer.Format("#LOC_BV_Status_AwaitingSunlight");
                case VesselState.Moving:
                    return Localizer.Format("#LOC_BV_Status_Moving");
                default:
                    return Localizer.Format("#LOC_BV_Status_Idle");
            }
        }

        #endregion


        #region Status window texts

        public virtual List<DisplayedSystemCheckResult> GetDisplayedSystemCheckResults()
        {
            if (displayedSystemCheckResults == null) // Just to be sure
                displayedSystemCheckResults = new List<DisplayedSystemCheckResult>();

            displayedSystemCheckResults.Clear();

            DisplayedSystemCheckResult result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_TargetLat"),
                Text = targetLatitude.ToString("0.####"),
                Tooltip = ""
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_TargetLon"),
                Text = targetLongitude.ToString("0.####"),
                Tooltip = ""
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_Distance"),
                Text = Tools.ConvertDistanceToText(RemainingDistanceToTarget),
                Tooltip = ""
            };
            displayedSystemCheckResults.Add(result);

            return displayedSystemCheckResults;
        }

        #endregion


        #region Pathfinder

        /// <summary>
        /// Find a route to the target
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public virtual bool FindRoute(double lat, double lon)
        {
            return FindRoute(lat, lon, TileTypes.Land | TileTypes.Ocean);
        }


        /// <summary>
        /// Find a route to the target using only specified tile types (route on land, water or both)
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="tileType"></param>
        /// <returns></returns>
        protected bool FindRoute(double targetLat, double targetLon, TileTypes tileType)
        {
            bool result = false;

            PathFinder pathFinder = new PathFinder(vessel.latitude, vessel.longitude, targetLat, targetLon, vessel.mainBody, tileType);
            pathFinder.FindPath();

            double dist = pathFinder.GetDistance();
            if (dist > 0) // Path found
            {
                targetLatitude = targetLat;
                targetLongitude = targetLon;
                distanceToTarget = dist;
                path = PathUtils.HexToWaypoint(pathFinder.path);
                result = true;
            }
            else // Path not found
                result = false;

            return result;
        }

        #endregion


        /// <summary>
        /// Check the systems
        /// </summary>
        public virtual void SystemCheck()
        {
            mainStarIndex = Tools.GetMainStar(vessel).flightGlobalsIndex;
        }


        /// <summary>
        /// Activate autopilot
        /// </summary>
        public virtual bool Activate()
        {
            if (distanceToTarget == 0)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_Warning_NoRoute", 5f)).color = Color.yellow;
                return false;
            }

            BonVoyageModule module = vessel.FindPartModuleImplementing<BonVoyageModule>();
            if (module != null)
            {
                distanceTravelled = 0;
                active = true;

                module.active = active;
                module.targetLatitude = targetLatitude;
                module.targetLongitude = targetLongitude;
                module.distanceToTarget = distanceToTarget;
                module.distanceTravelled = distanceTravelled;
                module.pathEncoded = PathUtils.EncodePath(path);

                BonVoyage.Instance.AutopilotActivated(true);
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_BonVoyage"), 5f);
            }

            return active;
        }


        /// <summary>
        /// Deactivate autopilot
        /// </summary>
        public virtual bool Deactivate()
        {
            BonVoyageModule module = vessel.FindPartModuleImplementing<BonVoyageModule>();
            if (module != null)
            {
                active = false;
                targetLatitude = 0;
                targetLongitude = 0;
                distanceToTarget = 0;
                distanceTravelled = 0;
                path = null;

                module.active = active;
                module.targetLatitude = targetLatitude;
                module.targetLongitude = targetLongitude;
                module.distanceToTarget = distanceToTarget;
                module.distanceTravelled = distanceTravelled;
                module.pathEncoded = "";

                BonVoyage.Instance.AutopilotActivated(false);
            }

            return !active;
        }

    }

}
