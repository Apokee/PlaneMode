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

        #region Configuration

        private static readonly KeyBinding ToggleKey = new KeyBinding(KeyCode.None);
        private static readonly KeyBinding HoldKey = new KeyBinding(KeyCode.None);
        private static bool _pitchInvert;
        private static bool _enableAppLauncherButton;

        #endregion

        #region Interface

        private ApplicationLauncherButton _appLauncherButton;

        #endregion

        #region State

        private Vessel _currentVessel;
        private ModulePlaneMode _currentModulePlaneMode;
        private ControlMode _controlMode;

        private readonly List<IManipulator> _manipulators = new List<IManipulator>();

        #endregion

        #region MonoBehaviour

        public void Start()
        {
            Log.Trace("Entering PlaneMode.Start()");

            InitializeDefaults();
            InitializeSettings();
            InitializeInterface();

            _manipulators.Add(new GameSettingsManipulator
            {
                InvertPitch = _pitchInvert
            });

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

            if (ToggleKey.GetKeyDown() || HoldKey.GetKeyDown() || HoldKey.GetKeyUp())                
            {
                Log.Debug("ToggleKey or HoldKey pressed");

                ToggleControlMode();
            }

            Log.Trace("Leaving PlaneMode.Update()");
        }

        #endregion

        #region Event Handlers

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

            if (_enableAppLauncherButton)
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
                Strings.PlaneMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
            );

            _screenMessageRocket = new ScreenMessage(
                Strings.RocketMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
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
                    throw new ArgumentOutOfRangeException("appLauncherEvent");
            }

            Log.Trace("Leaving PlaneMode.OnAppLauncherEvent()");
        }

        private void InitializeDefaults()
        {
            Log.Trace("Entering PlaneMode.InitializeDefaults()");

            _controlMode = ControlMode.Rocket;
            _pitchInvert = false;

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

        private void SetControlMode(ControlMode newControlMode)
        {
            Log.Trace("Entering PlaneMode.SetControlMode()");
            Log.Debug("Setting control mode to {0}", newControlMode);

            if (newControlMode == ControlMode.Rocket || newControlMode == ControlMode.Plane)
            {
                if (_controlMode != newControlMode)
                {
                    Log.Debug("New control mode, {0}, is different from current control mode, {1}. Updating.",
                        newControlMode,
                        _controlMode
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
                    UpdateInterface();

                    Log.Info("Set control mode to {0}", newControlMode);
                }
                else
                {
                    Log.Debug("New control mode is same as current control mode, doing nothing");
                }
            }
            else
            {
                Log.Warning("Trying to set control mode to invalid mode: {0}", newControlMode);
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

        private static void InitializeSettings()
        {
            Log.Trace("Entering PlaneMode.InitializeSettings()");
            Log.Debug("Initializing settings");

            foreach (var settings in GameDatabase.Instance.GetConfigNodes("PLANEMODE"))
            {
                Log.Debug("Found default settings");

                ParseSettings(settings);
            }

            // LEGACY: When breaking backward compatibility stop reading this node
            foreach (var settings in GameDatabase.Instance.GetConfigNodes("PLANEMODE_USER_SETTINGS"))
            {
                Log.Debug("Found user settings");

                ParseSettings(settings);
            }

            Log.Trace("Leaving PlaneMode.InitializeSettings()");
        }

        private static void ParseSettings(ConfigNode settings)
        {
            Log.Trace("Entering PlaneMode.ParseSettings()");
            Log.Debug("Parsing settings: {0}", settings.ToString());

            try
            {
                if (settings.HasNode("TOGGLE_CONTROL_MODE"))
                {
                    Log.Debug("Loading TOGGLE_CONTROL_MODE");

                    ToggleKey.Load(settings.GetNode("TOGGLE_CONTROL_MODE"));
                }

                if (settings.HasNode("HOLD_CONTROL_MODE"))
                {
                    Log.Debug("Loading HOLD_CONTROL_MODE");

                    HoldKey.Load(settings.GetNode("HOLD_CONTROL_MODE"));
                }

                if (settings.HasValue("pitchInvert"))
                {
                    Log.Debug("Loading pitchInvert");

                    _pitchInvert = bool.Parse(settings.GetValue("pitchInvert"));
                }

                if (settings.HasValue("enableAppLauncherButton"))
                {
                    Log.Debug("Loading enableAppLauncherButton");

                    _enableAppLauncherButton = bool.Parse(settings.GetValue("enableAppLauncherButton"));
                }

                if (settings.HasValue("logLevel"))
                {
                    Log.Debug("Loading logLevel");

                    var logLevelString = settings.GetValue("logLevel");

                    try
                    {
                        Log.Level = (LogLevel)Enum.Parse(typeof(LogLevel), logLevelString, ignoreCase: true);
                    }
                    catch (ArgumentException)
                    {
                        // Enum.TryParse() was only added with .NET 4
                        Log.Warning("Failed to parse logLevel setting: {0}", logLevelString);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("Settings loading failed: {0}", e.ToString());
            }

            Log.Trace("Leaving PlaneMode.ParseSettings()");
        }

        private static Texture GetTexture(ModTexture modTexture)
        {
            Log.Trace("Entering PlaneMode.GetTexture()");
            Log.Trace("Getting texture: {0}", modTexture);

            Log.Debug("Loading texture: {0}", modTexture);


            var texture = GameDatabase.Instance.GetTexture(
                GetBaseDirectory().Name + String.Format("/Textures/{0}", modTexture), false
            );

            Log.Debug("Loaded texture: {0}", modTexture);
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
