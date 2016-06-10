namespace PlaneMode
{
    public interface IManipulator
    {
        bool InvertPitch { get; set; }

        void SetControlMode(ControlMode newControlMode);
        void OnDestroy();
    }
}
