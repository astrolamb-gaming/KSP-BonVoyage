﻿

namespace BonVoyage
{
    /// <summary>
    /// Settings window - model part
    /// </summary>
    internal class SettingsWindowModel
    {
        private bool dewarpChecked = false;
        private bool disableRotation = false;
        private bool showBiome = false;
        private bool kspSkin = true;
        private bool kspToolbarChecked = true;
        private bool toolbarContinuedChecked = false;


        /// <summary>
        /// Constructor
        /// </summary>
        internal SettingsWindowModel()
        {
            // Load from configuration
            if (Configuration.Skin == 1)
                kspSkin = false;
            else
                kspSkin = true;
            dewarpChecked = Configuration.AutomaticDewarp;
            disableRotation = Configuration.DisableRotation;
            showBiome = Configuration.ShowBiome;
            kspToolbarChecked = Configuration.KSPToolbar;
            toolbarContinuedChecked = Configuration.ToolbarContinued;
        }


        /// <summary>
        /// Automatic dewarp checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void DewarpChecked(bool value)
        {
            dewarpChecked = value;
            Configuration.AutomaticDewarp = value;
        }


        /// <summary>
        /// Get the state of Autmatic dewarp toggle
        /// </summary>
        /// <returns></returns>
        internal bool GetDewarpToggleState()
        {
            return dewarpChecked;
        }


        /// <summary>
        /// Disable rotation checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void DisableRotationChecked(bool value)
        {
            disableRotation = value;
            Configuration.DisableRotation = value;
        }


        /// <summary>
        /// Get the state of disable rotation toggle
        /// </summary>
        /// <returns></returns>
        internal bool GeDisableRotationToggleState()
        {
            return disableRotation;
        }


        /// <summary>
        /// Show biome checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void ShowBiomeChecked(bool value)
        {
            showBiome = value;
            Configuration.ShowBiome = value;
        }


        /// <summary>
        /// Get the state of show biome toggle
        /// </summary>
        /// <returns></returns>
        internal bool GeShowBiomeToggleState()
        {
            return showBiome;
        }


        /// <summary>
        /// Change active skin
        /// </summary>
        private void ChangeSkin()
        {
            if (kspSkin)
            {
                CommonWindowProperties.ActiveSkin = UISkinManager.defaultSkin;
                Configuration.Skin = 0;
            }
            else
            {
                CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
                Configuration.Skin = 1;
            }
            CommonWindowProperties.RefreshStyles();
            BonVoyage.Instance.ResetWindows();
        }


        /// <summary>
        /// KSP skin checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void KSPSkinChecked(bool value)
        {
            kspSkin = value;
            ChangeSkin();
        }


        /// <summary>
        /// Unity skin checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void UnitySkinChecked(bool value)
        {
            kspSkin = !value;
            ChangeSkin();
        }


        /// <summary>
        /// Get the state of KSP skin toggle
        /// </summary>
        /// <returns></returns>
        internal bool GetKSPSkinToggleState()
        {
            return kspSkin;
        }


        /// <summary>
        /// Get the state of Unity skin toggle
        /// </summary>
        /// <returns></returns>
        internal bool GetUnitySkinToggleState()
        {
            return !kspSkin;
        }


        /// <summary>
        /// KSP toolbar checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void KSPToolbarChecked(bool value)
        {
            kspToolbarChecked = value;
            Configuration.KSPToolbar = value;

            if (kspToolbarChecked)
                BonVoyage.Instance.AddLauncher();
            else
                BonVoyage.Instance.RemoveAppLauncherButton();
        }


        /// <summary>
        /// Toolbar Continued checkbox
        /// </summary>
        /// <param name="value"></param>
        internal void TCChecked(bool value)
        {
            toolbarContinuedChecked = value;
            Configuration.ToolbarContinued = value;

            if (toolbarContinuedChecked)
                BonVoyage.Instance.AddLauncher();
            else
                BonVoyage.Instance.RemoveToolbarContinuedButton();
        }


        /// <summary>
        /// Get the state of KSP toolbar toggle
        /// </summary>
        /// <returns></returns>
        internal bool GetKSPToolbarToggleState()
        {
            return kspToolbarChecked;
        }


        /// <summary>
        /// Get the state of Toolbar Continued toggle
        /// </summary>
        /// <returns></returns>
        internal bool GetTCToggleState()
        {
            return toolbarContinuedChecked;
        }

    }

}
