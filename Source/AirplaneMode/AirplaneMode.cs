using System;
using AirplaneMode.Extensions;
using UnityEngine;

namespace AirplaneMode
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AirplaneMode : MonoBehaviour
    {
        #region Constants

        private const string ModDirectory = "AirplaneMode";
        private const float ScreenMessageDurationSeconds = 5;

        private const string TexturePathAirplane = ModDirectory + "/" + "airplane_mode";
        private const string TexturePathRocket = ModDirectory + "/" + "rocket_mode";

        private ScreenMessage _screenMessageAirplane;
        private ScreenMessage _screenMessageRocket;

        #endregion

        #region Configuration

        private static readonly KeyBinding ToggleKey = new KeyBinding(KeyCode.ScrollLock);
        private static readonly KeyBinding HoldKey = new KeyBinding(KeyCode.Home);

        private bool _pitchInvert;

        #endregion

        #region Toolbar

        private bool _toolbarInstalled;
        private IButton _controlModeButton;

        #endregion

        #region State

        private Vessel _currentVessel;
        private ControlMode _controlMode;

        #endregion

        #region MonoBehaviour

        public void OnDestroy()
        {
            if (_toolbarInstalled)
            {
                _controlModeButton.OnClick -= OnControlModeButtonOnClick;
                _controlModeButton.Destroy();
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

        private void OnControlModeButtonOnClick(ClickEvent e)
        {
            ToggleControlMode();
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
            if (ToolbarManager.ToolbarAvailable)
            {
                _toolbarInstalled = true;

                _controlModeButton = ToolbarManager.Instance.add(GetType().Name, "_controlModeButton");
                _controlModeButton.TexturePath = TexturePathRocket;
                _controlModeButton.ToolTip = Strings.SwitchToAirplaneMode;

                _controlModeButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);

                _controlModeButton.OnClick += OnControlModeButtonOnClick;
            }
            else
            {
                _toolbarInstalled = false;
            }

            _screenMessageAirplane = new ScreenMessage(
                Strings.AirplaneMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );

            _screenMessageRocket = new ScreenMessage(
                Strings.RocketMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );
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
            if (_toolbarInstalled)
            {
                switch(_controlMode)
                {
                    case ControlMode.Airplane:
                        _controlModeButton.TexturePath = TexturePathAirplane;
                        _controlModeButton.ToolTip = Strings.SwitchToRocketMode;
                        break;
                    case ControlMode.Rocket:
                        _controlModeButton.TexturePath = TexturePathRocket;
                        _controlModeButton.ToolTip = Strings.SwitchToAirplaneMode;
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

        #endregion

        #region Nested Types

        private enum ControlMode
        {
            Airplane,
            Rocket
        }

        #endregion
    }
}
