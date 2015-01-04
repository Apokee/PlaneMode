using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AirplaneMode.Extensions;
using UnityEngine;

namespace AirplaneMode
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AirplaneMode : MonoBehaviour
    {
        #region Constants

        private const float ScreenMessageDurationSeconds = 5;

        private ScreenMessage _screenMessageAirplane;
        private ScreenMessage _screenMessageRocket;

        #endregion

        #region Configuration

        private static readonly KeyBinding ToggleKey = new KeyBinding(KeyCode.ScrollLock);
        private static readonly KeyBinding HoldKey = new KeyBinding(KeyCode.Home);

        private bool _pitchInvert;

        #endregion

        #region Interface

        private static readonly object TextureCacheLock = new object();
        private static readonly Dictionary<ModTexture, Texture> TextureCache = new Dictionary<ModTexture, Texture>();

        private ApplicationLauncherButton _appLauncherButton;

        #endregion

        #region State

        private Vessel _currentVessel;
        private ControlMode _controlMode;

        #endregion

        #region MonoBehaviour

        public void OnDestroy()
        {
            if (_appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_appLauncherButton);
            }

            if (_currentVessel != null)
            {
                OnVesselChange(null);
            }
        }

        public void Start()
        {
            InitializeConfiguration();
            InitializeDefaults();
            InitializeInterface();

            GameEvents.onVesselChange.Add(OnVesselChange);
            OnVesselChange(FlightGlobals.ActiveVessel);
        }

        public void Update()
        {
            if (ToggleKey.GetKeyDown() || HoldKey.GetKeyDown() || HoldKey.GetKeyUp())                
            {
                ToggleControlMode();
            }
        }

        #endregion

        #region Event Handlers

        private void OnVesselChange(Vessel vessel)
        {
            if (_currentVessel != null)
            {
                // ReSharper disable once DelegateSubtraction
                vessel.OnPreAutopilotUpdate -= OnPreAutopilotUpdate;
            }

            if (vessel != null)
            {
                vessel.OnPreAutopilotUpdate += OnPreAutopilotUpdate;
            }

            _currentVessel = vessel;
        }

        private void OnPreAutopilotUpdate(FlightCtrlState flightCtrlState)
        {
            switch (_controlMode)
            {
                case ControlMode.Airplane:
                    var yaw = flightCtrlState.yaw;
                    var roll = flightCtrlState.roll;
                    var pitch = flightCtrlState.pitch;

                    // Overriding the SAS and Autopilot seems kind of hacky but it appears to work correctly

                    if (ShouldOverrideControls(flightCtrlState))
                    {
                        FlightGlobals.ActiveVessel.Autopilot.SAS.ManualOverride(true);
                        FlightGlobals.ActiveVessel.Autopilot.Enabled = false;

                        flightCtrlState.yaw = roll;
                        flightCtrlState.roll = yaw;

                        if (_pitchInvert)
                        {
                            flightCtrlState.pitch = -pitch;
                        }
                    }
                    else
                    {
                        FlightGlobals.ActiveVessel.Autopilot.SAS.ManualOverride(false);
                        FlightGlobals.ActiveVessel.Autopilot.Enabled = true;
                    }
                    break;
                case ControlMode.Rocket:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Helpers

        private void InitializeConfiguration()
        {
            try
            {
                foreach (var config in GameDatabase.Instance.GetConfigNodes("airplane_mode_config"))
                {
                    if (config.HasNode("TOGGLE_CONTROL_MODE"))
                    {
                        ToggleKey.Load(config.GetNode("TOGGLE_CONTROL_MODE"));
                    }

                    if (config.HasNode("HOLD_CONTROL_MODE"))
                    {
                        HoldKey.Load(config.GetNode("HOLD_CONTROL_MODE"));
                    }

                    if (config.HasValue("pitch_invert"))
                    {
                        _pitchInvert = bool.Parse(config.GetValue("pitch_invert"));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("[AirplaneMode]: Config file loading failed: " + e);
            }
        }

        private void InitializeInterface()
        {
            _appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                () => OnAppLauncherEvent(AppLauncherEvent.OnTrue),
                () => OnAppLauncherEvent(AppLauncherEvent.OnFalse),
                () => OnAppLauncherEvent(AppLauncherEvent.OnHover),
                () => OnAppLauncherEvent(AppLauncherEvent.OnHoverOut),
                () => OnAppLauncherEvent(AppLauncherEvent.OnEnable),
                () => OnAppLauncherEvent(AppLauncherEvent.OnDisable),
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                GetTexture(ModTexture.AppLauncherRocket)
            );

            _screenMessageAirplane = new ScreenMessage(
                Strings.AirplaneMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );

            _screenMessageRocket = new ScreenMessage(
                Strings.RocketMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );
        }

        private void OnAppLauncherEvent(AppLauncherEvent appLauncherEvent)
        {
            switch (appLauncherEvent)
            {
                case AppLauncherEvent.OnTrue:
                    ToggleControlMode();
                    break;
                case AppLauncherEvent.OnFalse:
                    ToggleControlMode();
                    break;
                case AppLauncherEvent.OnHover:
                    break;
                case AppLauncherEvent.OnHoverOut:
                    break;
                case AppLauncherEvent.OnEnable:
                    break;
                case AppLauncherEvent.OnDisable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("appLauncherEvent");
            }
        }

        private void InitializeDefaults()
        {
            _pitchInvert = false;
            _controlMode = ControlMode.Rocket;
        }

        private bool ShouldOverrideControls(FlightCtrlState flightCtrlState)
        {
            return (!flightCtrlState.pitch.IsZero() && _pitchInvert)
                || !flightCtrlState.roll.IsZero()
                || !flightCtrlState.yaw.IsZero();
        }

        private void ToggleControlMode()
        {
            switch (_controlMode)
            {
                case ControlMode.Airplane:
                    _controlMode = ControlMode.Rocket;
                    break;
                case ControlMode.Rocket:
                    _controlMode = ControlMode.Airplane;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            UpdateToolbar();
            ShowMessageControlMode();
        }

        private void UpdateToolbar()
        {
            if (_appLauncherButton != null)
            {
                switch(_controlMode)
                {
                    case ControlMode.Airplane:
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherAirplane));
                        break;
                    case ControlMode.Rocket:
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherRocket));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ShowMessageControlMode()
        {
            ScreenMessages.RemoveMessage(_screenMessageAirplane);
            ScreenMessages.RemoveMessage(_screenMessageRocket);

            switch (_controlMode)
            {
                case ControlMode.Airplane:
                    ScreenMessages.PostScreenMessage(_screenMessageAirplane);
                    break;
                case ControlMode.Rocket:
                    ScreenMessages.PostScreenMessage(_screenMessageRocket);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Texture GetTexture(ModTexture modTexture)
        {
            if (!TextureCache.ContainsKey(modTexture))
            {
                lock (TextureCacheLock)
                {
                    if (!TextureCache.ContainsKey(modTexture))
                    {
                        var texture = new Texture2D(38, 38, TextureFormat.RGBA32, false);

                        texture.LoadImage(File.ReadAllBytes(Path.Combine(
                            GetBaseDirectory().FullName, String.Format("Textures/{0}.png", modTexture)
                        )));

                        TextureCache[modTexture] = texture;
                    }
                }
            }

            return TextureCache[modTexture];
        }

        private static DirectoryInfo GetBaseDirectory()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent;
        }

        #endregion

        #region Nested Types

        private enum AppLauncherEvent
        {
            OnTrue,
            OnFalse,
            OnHover,
            OnHoverOut,
            OnEnable,
            OnDisable,
        }

        private enum ModTexture
        {
            AppLauncherAirplane,
            AppLauncherRocket,
        }

        private enum ControlMode
        {
            Airplane,
            Rocket
        }

        #endregion
    }
}
