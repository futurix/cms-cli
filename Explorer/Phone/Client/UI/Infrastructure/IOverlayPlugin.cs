namespace Wave.UI
{
    public interface IOverlayPlugin
    {
        bool GoBack();

        void SignalStart();
        void SignalClosure();

        void SignalOrientationChange();
    }
}
