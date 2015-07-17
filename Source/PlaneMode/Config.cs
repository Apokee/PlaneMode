using System;
using System.Linq;
using UnityEngine;

namespace PlaneMode
{
    internal sealed class Config
    {
        #region Singleton

        private static readonly object InstanceLock = new object();
        private static Config _instance;

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = TryParse();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        public KeyBinding ToggleControlMode { get; }
        public KeyBinding HoldControlMode { get; }
        public bool PitchInvert { get; }
        public bool EnableAppLauncherButton { get; }
        public ControlMode DefaultControlMode { get; }
        public ControlMode DefaultVabControlMode { get; }
        public ControlMode DefaultSphControlMode { get; }

        private Config(
            KeyBinding toggleControlMode,
            KeyBinding holdControlMode,
            bool pitchInvert,
            bool enableAppLauncherButton,
            ControlMode defaultControlMode,
            ControlMode defaultVabControlMode,
            ControlMode defaultSphControlMode
        )
        {
            ToggleControlMode = toggleControlMode;
            HoldControlMode = holdControlMode;
            PitchInvert = pitchInvert;
            EnableAppLauncherButton = enableAppLauncherButton;
            DefaultControlMode = defaultControlMode;
            DefaultVabControlMode = defaultVabControlMode;
            DefaultSphControlMode = defaultSphControlMode;
        }

        private static Config TryParse()
        {
            var toggleControlMode = new KeyBinding(KeyCode.None);
            var holdControlMode = new KeyBinding(KeyCode.None);
            var pitchInvert = false;
            var enableAppLauncherButton = true;
            var defaultControlMode = ControlMode.Rocket;
            var defaultVabControlMode = ControlMode.Rocket;
            var defaultSphControlMode = ControlMode.Plane;

            // LEGACY: When breaking backwards compatibility change node name to "PLANE_MODE"
            var node = GameDatabase
                .Instance
                .GetConfigNodes("PLANEMODE")
                .SingleOrDefault();

            if (node != null)
            {
                if (node.HasNode("TOGGLE_CONTROL_MODE"))
                {
                    toggleControlMode.Load(node.GetNode("TOGGLE_CONTROL_MODE"));
                }

                if (node.HasNode("HOLD_CONTROL_MODE"))
                {
                    holdControlMode.Load(node.GetNode("HOLD_CONTROL_MODE"));
                }

                if (node.HasValue("pitchInvert"))
                {
                    pitchInvert = bool.Parse(node.GetValue("pitchInvert"));
                }

                if (node.HasValue("enableAppLauncherButton"))
                {
                    enableAppLauncherButton = bool.Parse(node.GetValue("enableAppLauncherButton"));
                }

                if (node.HasValue("defaultControlMode"))
                {
                    defaultControlMode = (ControlMode)Enum.Parse(
                        typeof(ControlMode),
                        node.GetValue("defaultControlMode")
                    );
                }

                if (node.HasValue("defaultVabControlMode"))
                {
                    defaultVabControlMode = (ControlMode)Enum.Parse(
                        typeof(ControlMode),
                        node.GetValue("defaultVabControlMode")
                    );
                }

                if (node.HasValue("defaultSphControlMode"))
                {
                    defaultSphControlMode = (ControlMode)Enum.Parse(
                        typeof(ControlMode),
                        node.GetValue("defaultSphControlMode")
                    );
                }
            }

            // LEGACY: When breaking backward compatibility stop reading this node
            var legacyNode = GameDatabase
                .Instance
                .GetConfigNodes("PLANEMODE_USER_SETTINGS")
                .SingleOrDefault();

            if (legacyNode != null)
            {
                if (legacyNode.HasNode("TOGGLE_CONTROL_MODE"))
                {
                    toggleControlMode.Load(legacyNode.GetNode("TOGGLE_CONTROL_MODE"));
                }

                if (legacyNode.HasNode("HOLD_CONTROL_MODE"))
                {
                    holdControlMode.Load(legacyNode.GetNode("HOLD_CONTROL_MODE"));
                }

                if (legacyNode.HasValue("pitchInvert"))
                {
                    pitchInvert = bool.Parse(legacyNode.GetValue("pitchInvert"));
                }

                if (legacyNode.HasValue("enableAppLauncherButton"))
                {
                    enableAppLauncherButton = bool.Parse(legacyNode.GetValue("enableAppLauncherButton"));
                }
            }

            return new Config(
                toggleControlMode,
                holdControlMode,
                pitchInvert,
                enableAppLauncherButton,
                defaultControlMode,
                defaultVabControlMode,
                defaultSphControlMode
            );
        }
    }
}
