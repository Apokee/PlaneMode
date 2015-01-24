using System;
using System.Reflection;

namespace PlaneMode.Manipulators
{
    internal sealed class FlightInputManipulator : IManipulator
    {
        private static readonly BindingFlags BindingFlags;

        private readonly FlightInputHandler _handler;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly FieldInfo _pitchUpField;
        private readonly FieldInfo _pitchDownField;
        private readonly FieldInfo _yawLeftField;
        private readonly FieldInfo _yawRightField;
        private readonly FieldInfo _rollLeftField;
        private readonly FieldInfo _rollRightField;
        private readonly FieldInfo _pitchAxisField;
        private readonly FieldInfo _rollAxisField;
        private readonly FieldInfo _yawAxisField;
        private readonly FieldInfo _pitchAxisStagingField;
        private readonly FieldInfo _pitchAxisDockingField;

        private readonly UIModeKeyBindingSelector _pitchUpBinding;
        private readonly UIModeKeyBindingSelector _pitchDownBinding;
        private readonly UIModeKeyBindingSelector _yawLeftBinding;
        private readonly UIModeKeyBindingSelector _yawRightBinding;
        private readonly UIModeKeyBindingSelector _rollLeftBinding;
        private readonly UIModeKeyBindingSelector _rollRightBinding;
        private readonly UIModeAxisBindingSelector _pitchAxisBinding;
        private readonly UIModeAxisBindingSelector _rollAxisBinding;
        private readonly UIModeAxisBindingSelector _yawAxisBinding;

        private readonly AxisBinding _pitchAxisStagingBinding;
        private readonly AxisBinding _pitchAxisDockingBinding;

        private readonly bool _pitchAxisStagingInverted;
        private readonly bool _pitchAxisDockingInverted;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public bool InvertPitch { get; set; }

        static FlightInputManipulator()
        {
            BindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        }

        public FlightInputManipulator(FlightInputHandler handler)
        {
            _handler = handler;

            var handlerType = typeof(FlightInputHandler);
            var axisBindingType = typeof(UIModeAxisBindingSelector);

            _pitchUpField   = handlerType.GetField("\u0010", BindingFlags);
            _pitchDownField = handlerType.GetField("\u0011", BindingFlags);
            _yawLeftField   = handlerType.GetField("\u0012", BindingFlags);
            _yawRightField  = handlerType.GetField("\u0013", BindingFlags);
            _rollLeftField  = handlerType.GetField("\u0014", BindingFlags);
            _rollRightField = handlerType.GetField("\u0015", BindingFlags);
            _pitchAxisField = handlerType.GetField("\u001E", BindingFlags);
            _rollAxisField  = handlerType.GetField("\u001F", BindingFlags);
            _yawAxisField   = handlerType.GetField("\u0020", BindingFlags);

            _pitchAxisStagingField = axisBindingType.GetField("\u0001", BindingFlags);
            _pitchAxisDockingField = axisBindingType.GetField("\u0002", BindingFlags);

            // ReSharper disable PossibleNullReferenceException
            _pitchUpBinding = (UIModeKeyBindingSelector)_pitchUpField.GetValue(_handler);

            _pitchDownBinding = (UIModeKeyBindingSelector)_pitchDownField.GetValue(_handler);
            _yawLeftBinding = (UIModeKeyBindingSelector)_yawLeftField.GetValue(_handler);
            _yawRightBinding = (UIModeKeyBindingSelector)_yawRightField.GetValue(_handler);
            _rollLeftBinding = (UIModeKeyBindingSelector)_rollLeftField.GetValue(_handler);
            _rollRightBinding = (UIModeKeyBindingSelector)_rollRightField.GetValue(_handler);

            _pitchAxisBinding = (UIModeAxisBindingSelector)_pitchAxisField.GetValue(_handler);
            _rollAxisBinding = (UIModeAxisBindingSelector)_rollAxisField.GetValue(_handler);
            _yawAxisBinding = (UIModeAxisBindingSelector)_yawAxisField.GetValue(_handler);

            _pitchAxisStagingBinding = (AxisBinding)_pitchAxisStagingField.GetValue(_pitchAxisBinding);
            _pitchAxisDockingBinding = (AxisBinding)_pitchAxisDockingField.GetValue(_pitchAxisBinding);

            _pitchAxisStagingInverted = _pitchAxisStagingBinding.inverted;
            _pitchAxisDockingInverted = _pitchAxisDockingBinding.inverted;
            // ReSharper restore PossibleNullReferenceException
        }

        public void SetControlMode(ControlMode newControlMode)
        {
            switch (newControlMode)
            {
                case ControlMode.Rocket:
                    _yawLeftField.SetValue(_handler, _yawLeftBinding);
                    _yawRightField.SetValue(_handler, _yawRightBinding);
                    _yawAxisField.SetValue(_handler, _yawAxisBinding);

                    _rollLeftField.SetValue(_handler, _rollLeftBinding);
                    _rollRightField.SetValue(_handler, _rollRightBinding);
                    _rollAxisField.SetValue(_handler, _rollAxisBinding);

                    _pitchUpField.SetValue(_handler, _pitchUpBinding);
                    _pitchDownField.SetValue(_handler, _pitchDownBinding);

                    _pitchAxisStagingBinding.inverted = _pitchAxisStagingInverted;
                    _pitchAxisDockingBinding.inverted = _pitchAxisDockingInverted;
                    break;
                case ControlMode.Plane:
                    _yawLeftField.SetValue(_handler, _rollLeftBinding);
                    _yawRightField.SetValue(_handler, _rollRightBinding);
                    _yawAxisField.SetValue(_handler, _rollAxisBinding);

                    _rollLeftField.SetValue(_handler, _yawLeftBinding);
                    _rollRightField.SetValue(_handler, _yawRightBinding);
                    _rollAxisField.SetValue(_handler, _yawAxisBinding);

                    if (InvertPitch)
                    {
                        _pitchUpField.SetValue(_handler, _pitchDownBinding);
                        _pitchDownField.SetValue(_handler, _pitchUpBinding);

                        _pitchAxisStagingBinding.inverted = !_pitchAxisStagingInverted;
                        _pitchAxisDockingBinding.inverted = !_pitchAxisDockingInverted;
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
