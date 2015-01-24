using System;

namespace PlaneMode.Manipulators
{
    internal class GameSettingsManipulator : IManipulator
    {
        // ReSharper disable NotAccessedField.Local
        private readonly KeyBinding _pitchUpStagingBinding;
        private readonly KeyBinding _pitchDownStagingBinding;
        private readonly KeyBinding _yawLeftStagingBinding;
        private readonly KeyBinding _yawRightStagingBinding;
        private readonly KeyBinding _rollLeftStagingBinding;
        private readonly KeyBinding _rollRightStagingBinding;

        private readonly AxisBinding _pitchAxisStagingBinding;
        private readonly AxisBinding _rollAxisStagingBinding;
        private readonly AxisBinding _yawAxisStagingBinding;

        private readonly KeyBinding _pitchUpDockingBinding;
        private readonly KeyBinding _pitchDownDockingBinding;
        private readonly KeyBinding _yawLeftDockingBinding;
        private readonly KeyBinding _yawRightDockingBinding;
        private readonly KeyBinding _rollLeftDockingBinding;
        private readonly KeyBinding _rollRightDockingBinding;
        private readonly AxisBinding _pitchAxisDockingBinding;
        private readonly AxisBinding _rollAxisDockingBinding;
        private readonly AxisBinding _yawAxisDockingBinding;

        private readonly bool _pitchAxisStagingInverted;
        private readonly bool _pitchAxisDockingInverted;
        // ReSharper restore NotAccessedField.Local

        public bool InvertPitch { get; set; }

        public GameSettingsManipulator()
        {
            _pitchUpStagingBinding      = GameSettings.PITCH_UP;
            _pitchDownStagingBinding    = GameSettings.PITCH_DOWN;
            _yawLeftStagingBinding      = GameSettings.YAW_LEFT;
            _yawRightStagingBinding     = GameSettings.YAW_RIGHT;
            _rollLeftStagingBinding     = GameSettings.ROLL_LEFT;
            _rollRightStagingBinding    = GameSettings.ROLL_RIGHT;
            _pitchAxisStagingBinding    = GameSettings.AXIS_PITCH;
            _rollAxisStagingBinding     = GameSettings.AXIS_ROLL;
            _yawAxisStagingBinding      = GameSettings.AXIS_YAW;

            _pitchUpDockingBinding      = GameSettings.Docking_pitchUp;
            _pitchDownDockingBinding    = GameSettings.Docking_pitchDown;
            _yawLeftDockingBinding      = GameSettings.Docking_yawLeft;
            _yawRightDockingBinding     = GameSettings.Docking_yawRight;
            _rollLeftDockingBinding     = GameSettings.Docking_rollLeft;
            _rollRightDockingBinding    = GameSettings.Docking_rollRight;
            _pitchAxisDockingBinding    = GameSettings.axis_Docking_pitch;
            _rollAxisDockingBinding     = GameSettings.axis_Docking_roll;
            _yawAxisDockingBinding      = GameSettings.axis_Docking_yaw;

            _pitchAxisStagingInverted = GameSettings.AXIS_PITCH.inverted;
            _pitchAxisDockingInverted = GameSettings.axis_Docking_pitch.inverted;
        }

        public void SetControlMode(ControlMode newControlMode)
        {
            switch (newControlMode)
            {
                case ControlMode.Rocket:
                    GameSettings.YAW_LEFT = _yawLeftStagingBinding;
                    GameSettings.YAW_RIGHT = _yawRightStagingBinding;
                    GameSettings.AXIS_YAW = _yawAxisStagingBinding;
                    GameSettings.Docking_yawLeft = _yawLeftDockingBinding;
                    GameSettings.Docking_yawRight = _yawRightDockingBinding;
                    GameSettings.axis_Docking_yaw = _yawAxisDockingBinding;

                    GameSettings.ROLL_LEFT = _rollLeftStagingBinding;
                    GameSettings.ROLL_RIGHT = _rollRightStagingBinding;
                    GameSettings.AXIS_ROLL = _rollAxisStagingBinding;
                    GameSettings.Docking_rollLeft = _rollLeftDockingBinding;
                    GameSettings.Docking_rollRight = _rollRightDockingBinding;
                    GameSettings.axis_Docking_roll = _rollAxisDockingBinding;


                    GameSettings.PITCH_UP = _pitchUpStagingBinding;
                    GameSettings.PITCH_DOWN = _pitchDownStagingBinding;
                    GameSettings.AXIS_PITCH.inverted = _pitchAxisStagingInverted;

                    GameSettings.Docking_pitchUp = _pitchUpDockingBinding;
                    GameSettings.Docking_pitchDown = _pitchDownDockingBinding;
                    GameSettings.axis_Docking_pitch.inverted = _pitchAxisDockingInverted;

                    break;
                case ControlMode.Plane:
                    GameSettings.YAW_LEFT = _rollLeftStagingBinding;
                    GameSettings.YAW_RIGHT = _rollRightStagingBinding;
                    GameSettings.AXIS_YAW = _rollAxisStagingBinding;
                    GameSettings.Docking_yawLeft = _rollLeftDockingBinding;
                    GameSettings.Docking_yawRight = _rollRightDockingBinding;
                    GameSettings.axis_Docking_yaw = _rollAxisDockingBinding;

                    GameSettings.ROLL_LEFT = _yawLeftStagingBinding;
                    GameSettings.ROLL_RIGHT = _yawRightStagingBinding;
                    GameSettings.AXIS_ROLL = _yawAxisStagingBinding;
                    GameSettings.Docking_rollLeft = _yawLeftDockingBinding;
                    GameSettings.Docking_rollRight = _yawRightDockingBinding;
                    GameSettings.axis_Docking_roll = _yawAxisDockingBinding;

                    if (InvertPitch)
                    {
                        GameSettings.PITCH_UP = _pitchDownStagingBinding;
                        GameSettings.PITCH_DOWN = _pitchUpStagingBinding;
                        GameSettings.AXIS_PITCH.inverted = !_pitchAxisStagingInverted;

                        GameSettings.Docking_pitchUp = _pitchDownDockingBinding;
                        GameSettings.Docking_pitchDown = _pitchUpDockingBinding;
                        GameSettings.axis_Docking_pitch.inverted = !_pitchAxisDockingInverted;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("newControlMode");
            }
        }

        public void OnDestroy()
        {
            SetControlMode(ControlMode.Rocket);
        }
    }
}
