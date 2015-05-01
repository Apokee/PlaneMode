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

            if (part != null && part.partInfo != null)
            {
                Log.Debug("Part {0} loaded ControlMode: {1}", part.partInfo.title, ControlMode);
            }

            Log.Trace("Leaving ModulePlaneMode.OnLoad()");
        }

        public override void OnSave(ConfigNode node)
        {
            Log.Trace("Entering ModulePlaneMode.OnSave()");

            node.AddValue(ControlModeNodeKey, (byte)ControlMode);

            Log.Debug("Part {0} saved ControlMode: {1}", part.partInfo.title, ControlMode);

            Log.Trace("Leaving ModulePlaneMode.OnSave()");
        }

        public override void OnStart(StartState state)
        {
            Log.Trace("Entering ModulePlaneMode.OnStart()");
            Log.Debug("Part {0} is starting in state {1}", part.partInfo.title, state);

            switch (ControlMode)
            {
                case ControlMode.Plane:
                    break;
                case ControlMode.Rocket:
                    break;
                default:
                    Log.Debug("Part {0} does not have a valid ControlMode: {1}", part.partInfo.title, ControlMode);

                    if (state == StartState.Editor)
                    {
                        var vesselRotation = EditorLogic.VesselRotation * Vector3.up;

                        Log.Debug("Part {0} is in Editor with vesselRotation: {1}",
                            part.partInfo.title,
                            vesselRotation
                        );

                        if (vesselRotation == Vector3.up)
                        {
                            Log.Debug("Setting part {0} control mode to Rocket because it's in the VAB",
                                part.partInfo.title
                            );

                            ControlMode = ControlMode.Rocket;
                        }
                        else if (vesselRotation == Vector3.forward)
                        {
                            Log.Debug("Setting part {0} control mode to Plane because it's in the SPH",
                                part.partInfo.title
                            );

                            ControlMode = ControlMode.Plane;
                        }
                        else
                        {
                            Log.Debug("Setting part {0} control mode to Rocket because we don't know where it is",
                                part.partInfo.title
                            );

                            ControlMode = ControlMode.Rocket;
                        }
                    }
                    else
                    {
                        Log.Debug("Setting part {0} control mode to Rocket because it's not in the editor",
                                part.partInfo.title
                            );

                        ControlMode = ControlMode.Rocket;
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

            Log.Info("Changed control mode for {0} to {1}", part.partInfo.title, ControlMode);

            Log.Trace("Leaving ModulePlaneMode.ToggleControlMode()");
        }

        public void SetControlMode(ControlMode controlMode)
        {
            Log.Trace("Entering ModulePlaneMode.SetControlMode()");

            ControlMode = controlMode;

            Log.Info("Changed control mode for {0} to {1}", part.partInfo.title, ControlMode);
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
