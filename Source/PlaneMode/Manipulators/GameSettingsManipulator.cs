using System;

namespace PlaneMode.Manipulators
{
    internal sealed class GameSettingsManipulator : IManipulator
    {
        private readonly KeyBinding _pitchUp;
        private readonly KeyBinding _pitchDown;
        private readonly bool _pitchAxisPrimaryInverted;
        private readonly bool _pitchAxisSecondaryInverted;

        private readonly KeyBinding _rollLeft;
        private readonly KeyBinding _rollRight;
        private readonly AxisBinding _rollAxis;

        private readonly KeyBinding _yawLeft;
        private readonly KeyBinding _yawRight;
        private readonly AxisBinding _yawAxis;

        public bool InvertPitch { get; set; }

        public GameSettingsManipulator()
        {
            _pitchUp                    = GameSettings.PITCH_UP;
            _pitchDown                  = GameSettings.PITCH_DOWN;
            _pitchAxisPrimaryInverted   = GameSettings.AXIS_PITCH.primary.inverted;
            _pitchAxisSecondaryInverted = GameSettings.AXIS_PITCH.secondary.inverted;

            _rollLeft    = GameSettings.ROLL_LEFT;
            _rollRight   = GameSettings.ROLL_RIGHT;
            _rollAxis    = GameSettings.AXIS_ROLL;

            _yawLeft     = GameSettings.YAW_LEFT;
            _yawRight    = GameSettings.YAW_RIGHT;
            _yawAxis     = GameSettings.AXIS_YAW;
        }

        public void SetControlMode(ControlMode newControlMode)
        {
            switch (newControlMode)
            {
                case ControlMode.Rocket:
                    if (InvertPitch)
                    {
                        GameSettings.PITCH_UP                       = _pitchUp;
                        GameSettings.PITCH_DOWN                     = _pitchDown;
                        GameSettings.AXIS_PITCH.primary.inverted    = _pitchAxisPrimaryInverted;
                        GameSettings.AXIS_PITCH.secondary.inverted  = _pitchAxisSecondaryInverted;
                    }

                    GameSettings.ROLL_LEFT  = _rollLeft;
                    GameSettings.ROLL_RIGHT = _rollRight;
                    GameSettings.AXIS_ROLL  = _rollAxis;

                    GameSettings.YAW_LEFT   = _yawLeft;
                    GameSettings.YAW_RIGHT  = _yawRight;
                    GameSettings.AXIS_YAW   = _yawAxis;

                    break;
                case ControlMode.Plane:
                    if (InvertPitch)
                    {
                        GameSettings.PITCH_UP                       = _pitchDown;
                        GameSettings.PITCH_DOWN                     = _pitchUp;
                        GameSettings.AXIS_PITCH.primary.inverted    = !_pitchAxisPrimaryInverted;
                        GameSettings.AXIS_PITCH.secondary.inverted  = !_pitchAxisSecondaryInverted;
                    }

                    GameSettings.ROLL_LEFT  = _yawLeft;
                    GameSettings.ROLL_RIGHT = _yawRight;
                    GameSettings.AXIS_ROLL  = _yawAxis;

                    GameSettings.YAW_LEFT   = _rollLeft;
                    GameSettings.YAW_RIGHT  = _rollRight;
                    GameSettings.AXIS_YAW   = _rollAxis;

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
