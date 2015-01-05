using System;
using UnityEngine;

namespace PlaneMode
{
    public sealed class ModulePlaneMode : PartModule
    {
        private const string ControlModeNodeKey = "controlMode";

        [KSPField(guiName = "Control Mode", guiActive = true, guiActiveEditor = true)]
        public ControlMode ControlMode;


        public override void OnLoad(ConfigNode node)
        {
            TryParseControlMode(node.GetValue(ControlModeNodeKey), out ControlMode);
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue(ControlModeNodeKey, (byte)ControlMode);
        }

        public override void OnStart(StartState state)
        {
            switch (ControlMode)
            {
                case ControlMode.Plane:
                    break;
                case ControlMode.Rocket:
                    break;
                default:
                    if (state == StartState.Editor)
                    {
                        var vesselRotation = EditorLogic.VesselRotation * Vector3.up;

                        if (vesselRotation == Vector3.up)
                        {
                            ControlMode = ControlMode.Rocket;
                        }
                        else if (vesselRotation == Vector3.forward)
                        {
                            ControlMode = ControlMode.Plane;
                        }
                        else
                        {
                            ControlMode = ControlMode.Rocket;
                        }
                    }
                    else
                    {
                        ControlMode = ControlMode.Rocket;
                    }
                    break;
            }
        }

        [KSPEvent(
            guiName = "Toggle Control Mode",
            name = "PlaneMode.ModulePlaneMode.ToggleControlMode",
            guiActive = true,
            guiActiveEditor = true
        )]
        public void ToggleControlMode()
        {
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
