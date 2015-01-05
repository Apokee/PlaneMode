using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        #endregion

        #region Interface

        private static readonly object TextureCacheLock = new object();
        private static readonly Dictionary<ModTexture, Texture> TextureCache = new Dictionary<ModTexture, Texture>();

        private ApplicationLauncherButton _appLauncherButton;

        #endregion

        #region State

        private Vessel _currentVessel;
        private ModulePlaneMode _currentModulePlaneMode;
        private ControlMode _controlMode;

        #endregion

        #region MonoBehaviour

        public void Start()
        {
            InitializeDefaults();
            InitializeSettings();
            InitializeInterface();

            GameEvents.onVesselChange.Add(OnVesselChange);
            OnVesselChange(FlightGlobals.ActiveVessel);
        }

        public void OnDestroy()
        {
            if (_appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_appLauncherButton);
            }

            GameEvents.onVesselChange.Remove(OnVesselChange);
            OnVesselChange(null);
        }

        public void Update()
        {
            if (_currentModulePlaneMode != null)
            {
                var currentReferenceTransformPart = _currentVessel.GetReferenceTransformPart();

                if (_currentModulePlaneMode.part != currentReferenceTransformPart)
                {
                    OnReferenceTransfomPartChange(currentReferenceTransformPart);
                }
            }

            if (_currentModulePlaneMode != null)
            {
                if (_controlMode != _currentModulePlaneMode.ControlMode)
                {
                    SetControlMode(_currentModulePlaneMode.ControlMode);
                }
            }

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
                _currentVessel.OnPreAutopilotUpdate -= OnPreAutopilotUpdate;

            }

            if (vessel != null)
            {
                vessel.OnPreAutopilotUpdate += OnPreAutopilotUpdate;

                var referenceTransformPart = vessel.GetReferenceTransformPart();
                if (referenceTransformPart != null)
                {
                    OnReferenceTransfomPartChange(referenceTransformPart);
                }
            }
            else
            {
                OnReferenceTransfomPartChange(null);
            }

            _currentVessel = vessel;
        }

        // Psuedo-event from checking Update()
        private void OnReferenceTransfomPartChange(Part part)
        {
            if (part != null)
            {
                var modulePlaneMode = part.FindModuleImplementing<ModulePlaneMode>();

                if (modulePlaneMode != null)
                {
                    _currentModulePlaneMode = modulePlaneMode;
                    SetControlMode(_currentModulePlaneMode.ControlMode);
                }
            }
            else
            {
                _currentModulePlaneMode = null;
            }
        }

        private void OnPreAutopilotUpdate(FlightCtrlState flightCtrlState)
        {
            switch (_controlMode)
            {
                case ControlMode.Plane:
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

            _screenMessagePlane = new ScreenMessage(
                Strings.PlaneMode, ScreenMessageDurationSeconds, ScreenMessageStyle.LOWER_CENTER
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
                    SetControlMode(ControlMode.Plane);
                    break;
                case AppLauncherEvent.OnFalse:
                    SetControlMode(ControlMode.Rocket);
                    break;
                case AppLauncherEvent.OnHover:
                    break;
                case AppLauncherEvent.OnHoverOut:
                    break;
                case AppLauncherEvent.OnEnable:
                    UpdateInterface();
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

        private void ToggleControlMode()
        {
            switch (_controlMode)
            {
                case ControlMode.Plane:
                    SetControlMode(ControlMode.Rocket);
                    break;
                case ControlMode.Rocket:
                    SetControlMode(ControlMode.Plane);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetControlMode(ControlMode newControlMode)
        {
            _controlMode = newControlMode;

            if (_currentModulePlaneMode != null)
            {
                _currentModulePlaneMode.SetControlMode(newControlMode);
            }

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            UpdateAppLauncher();
            ShowMessageControlMode();
        }

        private void UpdateAppLauncher()
        {
            /* 
             * There appears to be a slight issue when a vessel is first loaded whose initial reference transform part
             * is in plane mode. The AppLauncher button's texture will be set to Plane but it's not 'enabled' as if
             * SetTrue() was not called on it. Clicking the button again in this state keeps it in Plane mode and
             * enables the button. It's as if the texture gets set correctly but the initial call to SetTrue() fails
             * for some reason..
             */

            if (_appLauncherButton != null)
            {
                switch(_controlMode)
                {
                    case ControlMode.Plane:
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherPlane));
                        _appLauncherButton.SetTrue(makeCall: false);
                        break;
                    case ControlMode.Rocket:
                        _appLauncherButton.SetTexture(GetTexture(ModTexture.AppLauncherRocket));
                        _appLauncherButton.SetFalse(makeCall: false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ShowMessageControlMode()
        {
            ScreenMessages.RemoveMessage(_screenMessagePlane);
            ScreenMessages.RemoveMessage(_screenMessageRocket);

            switch (_controlMode)
            {
                case ControlMode.Plane:
                    ScreenMessages.PostScreenMessage(_screenMessagePlane);
                    break;
                case ControlMode.Rocket:
                    ScreenMessages.PostScreenMessage(_screenMessageRocket);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool ShouldOverrideControls(FlightCtrlState flightCtrlState)
        {
            return (!flightCtrlState.pitch.IsZero() && _pitchInvert)
                || !flightCtrlState.roll.IsZero()
                || !flightCtrlState.yaw.IsZero();
        }

        private static void InitializeSettings()
        {
            foreach (var settings in GameDatabase.Instance.GetConfigNodes("PLANEMODE_DEFAULT_SETTINGS"))
            {
                ParseSettings(settings);
            }

            foreach (var settings in GameDatabase.Instance.GetConfigNodes("PLANEMODE_USER_SETTINGS"))
            {
                ParseSettings(settings);
            }
        }

        private static void ParseSettings(ConfigNode settings)
        {
            try
            {
                if (settings.HasNode("TOGGLE_CONTROL_MODE"))
                {
                    ToggleKey.Load(settings.GetNode("TOGGLE_CONTROL_MODE"));
                }

                if (settings.HasNode("HOLD_CONTROL_MODE"))
                {
                    HoldKey.Load(settings.GetNode("HOLD_CONTROL_MODE"));
                }

                if (settings.HasValue("pitchInvert"))
                {
                    _pitchInvert = bool.Parse(settings.GetValue("pitchInvert"));
                }

                if (settings.HasValue("logLevel"))
                {
                    try
                    {
                        Log.Level = (LogLevel)Enum.Parse(
                            typeof(LogLevel), settings.GetValue("logLevel"), ignoreCase: true
                        );
                    }
                    catch (ArgumentException)
                    {
                        // Enum.TryParse() was only added with .NET 4
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[PlaneMode]: Settings loading failed: " + e);
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
            AppLauncherPlane,
            AppLauncherRocket,
        }

        #endregion
    }
}
