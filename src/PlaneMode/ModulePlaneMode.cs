using System;
using UnityEngine;

namespace PlaneMode
{
    public sealed class ModulePlaneMode : PartModule
    {
        private const string ControlModeNodeKey = "controlMode";

        public ControlMode ControlMode { get; private set; }

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Control Mode", isPersistant = true)]
        [UI_Cycle(
            affectSymCounterparts = UI_Scene.None,
            controlEnabled = true,
            scene = UI_Scene.All,
            stateNames = new[] { "Rocket", "Plane" }
        )]
        private int _controlMode = -1;

        public override void OnLoad(ConfigNode node)
        {
            Log.Trace("Entering ModulePlaneMode.OnLoad()");

            ControlMode controlMode;
            TryParseControlMode(node.GetValue(ControlModeNodeKey), out controlMode);
            ControlMode = controlMode;

            if (part?.partInfo != null)
            {
                Log.Debug($"Part {part.partInfo.title} loaded ControlMode: {ControlMode}");
            }

            Log.Trace("Leaving ModulePlaneMode.OnLoad()");
        }

        public override void OnSave(ConfigNode node)
        {
            Log.Trace("Entering ModulePlaneMode.OnSave()");

            node.AddValue(ControlModeNodeKey, (byte)ControlMode);

            Log.Debug($"Part {part.partInfo.title} saved ControlMode: {ControlMode}");

            Log.Trace("Leaving ModulePlaneMode.OnSave()");
        }

        public override void OnStart(StartState state)
        {
            Log.Trace("Entering ModulePlaneMode.OnStart()");
            Log.Debug($"Part {part.partInfo.title} is starting in state {state}");

            switch (ControlMode)
            {
                case ControlMode.Plane:
                    break;
                case ControlMode.Rocket:
                    break;
                default:
                    Log.Debug($"Part {part.partInfo.title} does not have a valid ControlMode: {ControlMode}");

                    if (state == StartState.Editor)
                    {
                        var vesselRotation = EditorLogic.VesselRotation * Vector3.up;

                        Log.Debug($"Part {part.partInfo.title} is in Editor with vesselRotation: {vesselRotation}");

                        if (vesselRotation == Vector3.up)
                        {
                            Log.Debug(
                                $"Setting part {part.partInfo.title} control mode to " +
                                $"{Config.Instance.DefaultVabControlMode} because it's in the VAB"
                            );

                            ControlMode = Config.Instance.DefaultVabControlMode;
                        }
                        else if (vesselRotation == Vector3.forward)
                        {
                            Log.Debug(
                                $"Setting part {part.partInfo.title} control mode to " +
                                $"{Config.Instance.DefaultSphControlMode} because it's in the SPH"
                            );

                            ControlMode = Config.Instance.DefaultSphControlMode;
                        }
                        else
                        {
                            Log.Debug(
                                $"Setting part {part.partInfo.title} control mode to " +
                                $"{Config.Instance.DefaultControlMode} because we don't know where it is"
                            );

                            ControlMode = Config.Instance.DefaultControlMode;
                        }
                    }
                    else
                    {
                        Log.Debug(
                            $"Setting part {part.partInfo.title} control mode to {Config.Instance.DefaultControlMode} " +
                            "because it's not in the editor"
                        );

                        ControlMode = Config.Instance.DefaultControlMode;
                    }
                    break;
            }

            switch (ControlMode)
            {
                case ControlMode.Rocket:
                    _controlMode = 0;
                    break;
                case ControlMode.Plane:
                    _controlMode = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Log.Trace("Leaving ModulePlaneMode.OnStart()");
        }

        [KSPAction("Control Mode: Toggle")]
        public void ActionToggleControlMode(KSPActionParam p)
        {
            ToggleControlMode();
        }

        [KSPAction("Control Mode: Rocket")]
        public void ActionSetRocketControlMode(KSPActionParam p)
        {
            SetControlMode(ControlMode.Rocket);
        }

        [KSPAction("Control Mode: Plane")]
        public void ActionSetPlaneControlMode(KSPActionParam p)
        {
            SetControlMode(ControlMode.Plane);
        }

        public void FixedUpdate()
        {
            switch (_controlMode)
            {
                case 0:
                    if (ControlMode != ControlMode.Rocket)
                        SetControlMode(ControlMode.Rocket);
                    break;
                case 1:
                    if (ControlMode != ControlMode.Plane)
                        SetControlMode(ControlMode.Plane);
                    break;
                default:
                    Log.Warning($"Invalid {nameof(_controlMode)}: {_controlMode}");
                    break;
            }
        }

        public void SetControlMode(ControlMode controlMode)
        {
            Log.Trace("Entering ModulePlaneMode.SetControlMode()");

            ControlMode = controlMode;

            switch (controlMode)
            {
                case ControlMode.Rocket:
                    _controlMode = 0;
                    break;
                case ControlMode.Plane:
                    _controlMode = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(controlMode), controlMode, null);
            }

            Log.Info($"Changed control mode for {part.partInfo.title} to {ControlMode}");
            Log.Trace("Leaving ModulePlaneMode.SetControlMode()");
        }

        public void ToggleControlMode()
        {
            Log.Trace("Entering ModulePlaneMode.ToggleControlMode()");

            switch (ControlMode)
            {
                case ControlMode.Rocket:
                    SetControlMode(ControlMode.Plane);
                    break;
                case ControlMode.Plane:
                    SetControlMode(ControlMode.Rocket);
                    break;
                default:
                    SetControlMode(ControlMode.Rocket);
                    break;
            }

            Log.Info($"Toggled control mode for {part.partInfo.title} to {ControlMode}");
            Log.Trace("Leaving ModulePlaneMode.ToggleControlMode()");
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool TryParseControlMode(string s, out ControlMode result)
        {
            result = default(ControlMode);

            byte b;
            if (byte.TryParse(s, out b) && Enum.IsDefined(typeof(ControlMode), b))
            {
                result = (ControlMode) b;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
