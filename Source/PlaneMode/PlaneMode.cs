using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PlaneMode.Manipulators;
using UnityEngine;

namespace PlaneMode
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class PlaneMode : MonoBehaviour
    {
        #region Constants

        private const float ScreenMessageDurationSeconds = 5;

        private ScreenMessage _screenMessagePlane;
        private ScreenMessage _screenMessageRocket;

        #endregion

        #region Interface

        private ApplicationLauncherButton _appLauncherButton;

        #endregion

        #region State

        private Vessel _currentVessel;
        private ModulePlaneMode _currentModulePlaneMode;
        private ControlMode _controlMode;
        private ControlMode? _prePauseControlMode;

        private readonly List<IManipulator> _manipulators = new List<IManipulator>();

        #endregion

        #region MonoBehaviour

        public void Start()
        {
            Log.Trace("Entering PlaneMode.Start()");

            InitializeDefaults();
            InitializeInterface();

            _manipulators.Add(new GameSettingsManipulator
            {
                InvertPitch = Config.Instance.PitchInvert
            });

            GameEvents.onGamePause.Add(OnGamePause);
            GameEvents.onGameUnpause.Add(OnGameUnpause);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

            GameEvents.onVesselChange.Add(OnVesselChange);
            OnVesselChange(FlightGlobals.ActiveVessel);

            Log.Trace("Leaving PlaneMode.Start()");
        }

        public void OnDestroy()
        {
            Log.Trace("Entering PlaneMode.OnDestroy()");

            if (_appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_appLauncherButton);
                Log.Debug("Removed Application Launcher button");
            }

            GameEvents.onVesselChange.Remove(OnVesselChange);
            OnVesselChange(null);

            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.onGameUnpause.Remove(OnGameUnpause);
            GameEvents.onGamePause.Remove(OnGamePause);

            foreach (var manipulator in _manipulators)
            {
                manipulator.OnDestroy();
            }

            _manipulators.Clear();

            Log.Trace("Leaving PlaneMode.OnDestroy()");
        }

        public void Update()
        {
            Log.Trace("Entering PlaneMode.Update()");

            Part storedReferenceTransformPart = null;
            Part currentReferenceTransformPart = null;

            if (_currentModulePlaneMode != null)
            {
                storedReferenceTransformPart = _currentModulePlaneMode.part;
            }

            if (_currentVessel != null)
            {
                currentReferenceTransformPart = _currentVessel.GetReferenceTransformPart();
            }

            if (storedReferenceTransformPart != currentReferenceTransformPart)
            {
                Log.Debug("storedReferenceTransformPart does not equal currentReferenceTransformPart");

                OnReferenceTransfomPartChange(currentReferenceTransformPart);
            }

            if (_currentModulePlaneMode != null)
            {
                if (_controlMode != _currentModulePlaneMode.ControlMode)
                {
                    Log.Debug("_controlMode does not equal _currentModulePlaneMode.ControlMode");

                    SetControlMode(_currentModulePlaneMode.ControlMode);
                }
            }

            if (
                Config.Instance.ToggleControlMode.GetKeyDown() ||
                Config.Instance.HoldControlMode.GetKeyDown() ||
                Config.Instance.HoldControlMode.GetKeyUp()
            )                
            {
                Log.Debug("ToggleKey or HoldKey pressed");

                ToggleControlMode();
            }

            Log.Trace("Leaving PlaneMode.Update()");
        }

        #endregion

        #region Event Handlers

        private void OnGamePause()
        {
            Log.Trace("Entering PlaneMode.OnGamePause()");

            if (_controlMode != ControlMode.Rocket)
            {
                Log.Info("Game paused while not in Rocket mode, swapping to Rocket mode while paused");
                _prePauseControlMode = _controlMode;
                SetControlMode(ControlMode.Rocket, disableInterfaceUpdate: true);
            }

            Log.Trace("Leaving PlaneMode.OnGamePause()");
        }

        private void OnGameUnpause()
        {
            Log.Trace("Entering PlaneMode.OnGameUnpause()");

            if (_prePauseControlMode != null && _prePauseControlMode != _controlMode)
            {
                SetControlMode(_prePauseControlMode.Value, disableInterfaceUpdate: true);

                Log.Info($"Game unpaused, reverted back to {_prePauseControlMode.Value} mode");

                _prePauseControlMode = null;
            }

            Log.Trace("Leaving PlaneMode.OnGameUnpause()");
        }

        private void OnGameSettingsApplied()
        {
            Log.Trace("Entering PlaneMode.OnGameSettingsApplied()");
            Log.Debug("GameSettings have been saved");

            if (_controlMode != ControlMode.Rocket)
            {
                Log.Info("GameSettings were saved while not in Rocket mode, swapping to Rocket mode and re-saving");

                var origControlMode = _controlMode;

                SetControlMode(ControlMode.Rocket, disableInterfaceUpdate: true);
                GameSettings.SaveSettings();
                SetControlMode(origControlMode, disableInterfaceUpdate: true);

                Log.Info($"GameSettings saved in Rocket mode, reverted to {_controlMode} mode");
            }

            Log.Trace("Leaving PlaneMode.OnGameSettingsApplied()");
        }

        private void OnVesselChange(Vessel vessel)
        {
            Log.Trace("Entering PlaneMode.OnVesselChange()");
            Log.Debug("Vessel has changed");

            if (vessel != null)
            {
                Log.Debug("new vessel is not null, triggering OnReferenceTransfomPartChange event");
                OnReferenceTransfomPartChange(vessel.GetReferenceTransformPart());
            }
            else
            {
                Log.Debug("new vessel is null, triggering OnReferenceTransfomPartChange event");
                OnReferenceTransfomPartChange(null);
            }

            Log.Debug("Updating _currentVessel");
            _currentVessel = vessel;

            Log.Trace("Leaving PlaneMode.OnVesselChange()");
        }

        // Psuedo-event from checking Update()
        private void OnReferenceTransfomPartChange(Part part)
        {
            Log.Trace("Entering PlaneMode.OnReferenceTransfomPartChange()");
            Log.Debug("ReferenceTransformPart has changed");

            if (part != null)
            {
                Log.Debug("part is not null, finding ModulePlaneMode on: " + part.partInfo.title);
                var modulePlaneMode = part.FindModuleImplementing<ModulePlaneMode>();

                if (modulePlaneMode != null)
                {
                    Log.Debug("Found ModulePlaneMode, updating _currentModulePlaneMode and calling SetControlMode()");

                    _currentModulePlaneMode = modulePlaneMode;
                    SetControlMode(_currentModulePlaneMode.ControlMode);
                }
            }
            else
            {
                Log.Debug("part is null, updating _currentModulePlaneMode");
                _currentModulePlaneMode = null;
            }

            Log.Trace("Leaving PlaneMode.OnReferenceTransfomPartChange()");
        }

        #endregion

        #region Helpers

        private void InitializeInterface()
        {
            Log.Trace("Entering PlaneMode.InitializeInterface()");

            if (Config.Instance.EnableAppLauncherButton)
            {
                Log.Debug("Adding Application Launcher button");

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
            }

            _screenMessagePlane = new ScreenMessage(
                "Plane Mode", ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );

            _screenMessageRocket = new ScreenMessage(
                "Rocket Mode", ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );

            Log.Trace("Leaving PlaneMode.InitializeInterface()");
        }

        private void OnAppLauncherEvent(AppLauncherEvent appLauncherEvent)
        {
            Log.Trace("Entering PlaneMode.OnAppLauncherEvent()");

            switch (appLauncherEvent)
            {
                case AppLauncherEvent.OnTrue:
                    Log.Debug("Application Launcher button changed to True mode, setting control mode to Plane");
                    SetControlMode(ControlMode.Plane);
                    break;
                case AppLauncherEvent.OnFalse:
                    Log.Debug("Application Launcher button changed to False mode, setting control mode to Rocket");
                    SetControlMode(ControlMode.Rocket);
                    break;
                case AppLauncherEvent.OnHover:
                    break;
                case AppLauncherEvent.OnHoverOut:
                    break;
                case AppLauncherEvent.OnEnable:
                    Log.Debug("Application Launcher button is enabled, updating interface");
                    UpdateInterface();
                    break;
                case AppLauncherEvent.OnDisable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appLauncherEvent));
            }

            Log.Trace("Leaving PlaneMode.OnAppLauncherEvent()");
        }

        private void InitializeDefaults()
        {
            Log.Trace("Entering PlaneMode.InitializeDefaults()");

            _controlMode = ControlMode.Rocket;

            Log.Trace("Leaving PlaneMode.InitializeDefaults()");
        }

        private void ToggleControlMode()
        {
            Log.Trace("Entering PlaneMode.ToggleControlMode()");

            switch (_controlMode)
            {
                case ControlMode.Plane:
                    Log.Debug("Toggling ControlMode from Plane to Rocket");
                    SetControlMode(ControlMode.Rocket);
                    break;
                case ControlMode.Rocket:
                    Log.Debug("Toggling ControlMode from Rocket to Plane");
                    SetControlMode(ControlMode.Plane);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Log.Trace("Leaving PlaneMode.ToggleControlMode()");
        }

        private void SetControlMode(ControlMode newControlMode, bool disableInterfaceUpdate = false)
        {
            Log.Trace("Entering PlaneMode.SetControlMode()");
            Log.Debug($"Setting control mode to {newControlMode}");

            if (newControlMode == ControlMode.Rocket || newControlMode == ControlMode.Plane)
            {
                if (_controlMode != newControlMode)
                {
                    Log.Debug(
                        $"New control mode, {newControlMode}, is different from current control mode, " +
                        $"{_controlMode}. Updating."
                    );

                    foreach (var manipulator in _manipulators)
                    {
                        manipulator.SetControlMode(newControlMode);
                    }

                    _controlMode = newControlMode;

                    if (_currentModulePlaneMode != null)
                    {
                        Log.Debug("_currentModulePlaneMode is not null, updating its control mode");

                        _currentModulePlaneMode.SetControlMode(newControlMode);
                    }

                    Log.Debug("Updating interface");

                    if (!disableInterfaceUpdate)
                    {
                        UpdateInterface();
                    }

                    Log.Info($"Set control mode to {newControlMode}");
                }
                else
                {
                    Log.Debug("New control mode is same as current control mode, doing nothing");
                }
            }
            else
            {
                Log.Warning($"Trying to set control mode to invalid mode: {newControlMode}");
            }

            Log.Trace("Leaving PlaneMode.SetControlMode()");
        }

        private void UpdateInterface()
        {
            Log.Trace("Entering PlaneMode.UpdateInterface()");
            Log.Debug("Updating interface");

            UpdateAppLauncher();
            ShowMessageControlMode();

            Log.Trace("Leaving PlaneMode.UpdateInterface()");
        }

        private void UpdateAppLauncher()
        {
            Log.Trace("Entering PlaneMode.UpdateAppLauncher()");

            /* 
             * There appears to be a slight issue when a vessel is first loaded whose initial reference transform part
             * is in plane mode. The AppLauncher button's texture will be set to Plane but it's not 'enabled' as if
             * SetTrue() was not called on it. Clicking the button again in this state keeps it in Plane mode and
             * enables the button. It's as if the texture gets set correctly but the initial call to SetTrue() fails
             * for some reason..
             */

            if (_appLauncherButton != null)
            {
                Log.Debug("Updating Application Launcher");

                switch (_controlMode)
                {
                    case ControlMode.Plane:
                        Log.Debug("Updating Application Launcher button to Plane mode");
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherPlane));
                        _appLauncherButton.SetTrue(makeCall: false);
                        break;
                    case ControlMode.Rocket:
                        Log.Debug("Updating Application Launcher button to Rocket mode");
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherRocket));
                        _appLauncherButton.SetFalse(makeCall: false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Log.Trace("Leaving PlaneMode.UpdateAppLauncher()");
        }

        private void ShowMessageControlMode()
        {
            Log.Trace("Entering PlaneMode.ShowMessageControlMode()");
            Log.Debug("Showing screen message");

            Log.Debug("Removing any existing messages");
            ScreenMessages.RemoveMessage(_screenMessagePlane);
            ScreenMessages.RemoveMessage(_screenMessageRocket);

            switch (_controlMode)
            {
                case ControlMode.Plane:
                    Log.Debug("Showing Plane Mode message");
                    ScreenMessages.PostScreenMessage(_screenMessagePlane);
                    break;
                case ControlMode.Rocket:
                    Log.Debug("Showing Rocket Mode message");
                    ScreenMessages.PostScreenMessage(_screenMessageRocket);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Log.Trace("Leaving PlaneMode.ShowMessageControlMode()");
        }

        private static Texture GetTexture(ModTexture modTexture)
        {
            Log.Trace("Entering PlaneMode.GetTexture()");
            Log.Trace($"Getting texture: {modTexture}");

            Log.Debug($"Loading texture: {modTexture}");

            var texture = GameDatabase
                .Instance
                .GetTexture($"{GetBaseDirectory().Name}/Textures/{modTexture}", false);

            Log.Debug($"Loaded texture: {modTexture}");
            Log.Trace("Leaving PlaneMode.GetTexture()");

            return texture;
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
            AppLauncherPlane,
            AppLauncherRocket,
        }

        #endregion
    }
}
