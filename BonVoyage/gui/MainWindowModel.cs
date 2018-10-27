﻿using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Main mod's window - model part
    /// </summary>
    public class MainWindowModel
    {
        //private BonVoyage module;
        private bool activeControllersChecked = true;
        private bool disabledControllersChecked = false;

        // Displayed vessel list
        private DialogGUIVerticalLayout vesselListLayout = null;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowModel()
        {
            // Load from configuration
            CommonWindowProperties.MainWindowPosition = Configuration.MainWindowPosition;
            activeControllersChecked = Configuration.ActiveControllers;
            disabledControllersChecked = Configuration.DisabledControllers;
        }


        /// <summary>
        /// Active controllers checkbox
        /// </summary>
        /// <param name="value"></param>
        public void ActiveControllersChecked(bool value)
        {
            activeControllersChecked = value;
            Configuration.ActiveControllers = value;
            RefreshVesselListLayout();
        }


        /// <summary>
        /// Get the state of Active controllers toggle
        /// </summary>
        /// <returns></returns>
        public bool GetActiveControllersToggleState()
        {
            return activeControllersChecked;
        }


        /// <summary>
        /// Disabled controllers checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DisabledControllersChecked(bool value)
        {
            disabledControllersChecked = value;
            Configuration.DisabledControllers = value;
            RefreshVesselListLayout();
        }


        /// <summary>
        /// Get the state of Disabled controllers toggle
        /// </summary>
        /// <returns></returns>
        public bool GetDisabledControllersToggleState()
        {
            return disabledControllersChecked;
        }


        /// <summary>
        /// Switch to vessel
        /// </summary>
        /// <param name="vesselId"></param>
        public void SwitchToVessel(Guid vesselId)
        {
            for (int i = 0; i < BonVoyage.Instance.BVControllers.Count; i++)
            {
                if (BonVoyage.Instance.BVControllers[i].vessel.id == vesselId)
                {
                    Vessel v = BonVoyage.Instance.BVControllers[i].vessel;
                    if (v.loaded)
                    {
                        FlightGlobals.SetActiveVessel(v);
                    }
                    else
                    {
                        GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                        FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(v));
                    }
                }
            }
        }


        /// <summary>
        /// Create table row for controller
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>DialogGUIHorizontalLayout row or null if controller state don't equals to selected filter</returns>
        private DialogGUIHorizontalLayout CreateListLayoutRow(BVController controller)
        {
            DialogGUIHorizontalLayout row = null;

            if ((activeControllersChecked && !controller.Shutdown) || (disabledControllersChecked && controller.Shutdown))
            {
                UIStyle statusStyle;
                switch (controller.GetVesselState())
                {
                    case VesselState.Current:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_White;
                        break;
                    case VesselState.Idle:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Grey;
                        break;
                    case VesselState.ControllerDisabled:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Red;
                        break;
                    case VesselState.AwaitingSunlight:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Yellow;
                        break;
                    case VesselState.Moving:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Green;
                        break;
                    default:
                        statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Grey;
                        break;
                }

                row = new DialogGUIHorizontalLayout(
                    new DialogGUILabel(controller.vessel.GetDisplayName(), 150f),
                    new DialogGUISpace(10f),
                    new DialogGUILabel(controller.GetVesselStateText(), 70f) { guiStyle = statusStyle },
                    new DialogGUISpace(10f),
                    new DialogGUILabel(controller.vessel.mainBody.bodyDisplayName.Replace("^N", ""), 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                    new DialogGUISpace(10f),
                    new DialogGUILabel("-", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                    new DialogGUISpace(10f),
                    new DialogGUILabel("-", 90f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                    new DialogGUISpace(10f),
                    (
                        !controller.vessel.isActiveVessel
                        ?
                        TooltipExtension.DeferTooltip(new DialogGUIButton("->",
                            delegate { SwitchToVessel(controller.vessel.id); }, 22f, 16f, false)
                        { tooltipText = (Localizer.Format("#LOC_BV_SwitchToVessel") + " " + controller.vessel.GetDisplayName()) })
                        :
                        new DialogGUISpace(10f)
                    )

                );
                row.SetOptionText(controller.vessel.id.ToString()); // ID of the row (vessel ID)
            }

            return row;
        }


        /// <summary>
        /// Get layout of the list of vessels
        /// </summary>
        /// <returns></returns>
        public DialogGUIVerticalLayout GetVesselListLayout()
        {
            // Count disabled controllers
            int disabledControllersCount = 0;
            int controllersCount = BonVoyage.Instance.BVControllers.Count;
            for (int i = 0; i < controllersCount; i++)
            {
                if (BonVoyage.Instance.BVControllers[i].Shutdown)
                    disabledControllersCount++;
            }

            int listLength = 1;
            if (activeControllersChecked)
                listLength += controllersCount - disabledControllersCount;
            if (disabledControllersChecked)
                listLength += disabledControllersCount;

            DialogGUIBase[] list = new DialogGUIBase[listLength];
            list[0] = new DialogGUIContentSizer(UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize, true);

            if (listLength > 1) // anything is checked
            {
                int counter = 1;
                for (int i = 0; i < controllersCount; i++)
                {
                    DialogGUIHorizontalLayout row = CreateListLayoutRow(BonVoyage.Instance.BVControllers[i]); 
                    if (row != null)
                    {
                        list[counter] = row;
                        counter++;
                        if (counter >= listLength) // break if we are at the end of list
                            break;
                    }
                }
            }

            vesselListLayout = new DialogGUIVerticalLayout(
                CommonWindowProperties.mainListMinWidth,
                CommonWindowProperties.mainListMinHeight,
                CommonWindowProperties.mainWindowSpacing,
                CommonWindowProperties.mainListPadding,
                TextAnchor.UpperLeft,
                list
            );

            return vesselListLayout;
        }


        /// <summary>
        /// Clear layout of the list of vessels
        /// </summary>
        public void ClearVesselListLayout()
        {
            vesselListLayout = null;
        }


        /// <summary>
        /// Refresh list of vessels without reloading the list of controllers
        /// </summary>
        public void RefreshVesselListLayout()
        {
            Stack<Transform> stack = new Stack<Transform>();﻿ // some data on hierarchy of GUI components
            stack.Push(vesselListLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size

            List<DialogGUIBase> rows = vesselListLayout.children;

            // Clear list. We are skiping DialogGUIContentSizer
            while (rows.Count > 1)
            {
                DialogGUIBase child = rows.ElementAt(1); // Get child
                rows.RemoveAt(1); // Drop row
                child.uiItem.gameObject.DestroyGameObjectImmediate(); // Free memory up
            }

            // Add rows
            int controllersCount = BonVoyage.Instance.BVControllers.Count;
            for (int i = 0; i < controllersCount; i++)
            {
                DialogGUIHorizontalLayout row = CreateListLayoutRow(BonVoyage.Instance.BVControllers[i]);
                if (row != null)
                {
                    rows.Add(row);
                    rows.Last().Create(ref stack, CommonWindowProperties.ActiveSkin); // required to force the GUI creatio﻿n
                }
            }
        }

    }

}
