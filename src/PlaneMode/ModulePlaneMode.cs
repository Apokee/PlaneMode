using System;
using UnityEngine;

namespace PlaneMode
{
    public sealed class ModulePlaneMode : PartModule
    {
        private const string ControlModeNodeKey = "controlMode";

        [KSPField(guiName = "Control Mode", guiActive = true, guiActiveEditor = true)]
        public ControlMode ControlMode = ControlMode.Rocket;

        public override void OnLoad(ConfigNode node)
        {
            Log.Trace("Entering ModulePlaneMode.OnLoad()");

            TryParseControlMode(node.GetValue(ControlModeNodeKey), out ControlMode);

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

            Log.Trace("Leaving ModulePlaneMode.OnStart()");
        }

        [KSPEvent(
            guiName = "Toggle Control Mode",
            name = "PlaneMode.ModulePlaneMode.ToggleControlMode",
            guiActive = true,
            guiActiveEditor = true
        )]
        public void ToggleControlMode()
        {
            Log.Trace("Entering ModulePlaneMode.ToggleControlMode()");

            switch(ControlMode)
            {
                case ControlMode.Rocket:
                    ControlMode = ControlMode.Plane;
                    break;
                case ControlMode.Plane:
                    ControlMode = ControlMode.Rocket;
                    break;
                default:
                    ControlMode = ControlMode.Rocket;
                    break;
            }

            Log.Info($"Toggled control mode for {part.partInfo.title} to {ControlMode}");
            Log.Trace("Leaving ModulePlaneMode.ToggleControlMode()");
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

        public void SetControlMode(ControlMode controlMode)
        {
            Log.Trace("Entering ModulePlaneMode.SetControlMode()");

            ControlMode = controlMode;

            Log.Info($"Changed control mode for {part.partInfo.title} to {ControlMode}");
            Log.Trace("Leaving ModulePlaneMode.SetControlMode()");
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool TryParseControlMode(string s, out ControlMode result)
        {
            result = default(ControlMode);

            byte b;
            if (Byte.TryParse(s, out b) && Enum.IsDefined(typeof(ControlMode), b))
            {
                result = (ControlMode)b;
                return true;
            }

            return false;
        }
    }
}
