using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class SystemAgent
    {
        public const DeviceGroup DefaultDeviceGroup = DeviceGroup.Res_480x800;

        public LocationObserver Location { get; private set; }

        public DeviceGroup CurrentDeviceGroup
        {
            get { return currentDeviceGroup; }
        }

        public DeviceGroup[] SupportedDeviceGroups
        {
            get { return new DeviceGroup[] { DeviceGroup.Res_480x800, DeviceGroup.Res_800x480 }; }
        }

        private DeviceGroup currentDeviceGroup = DefaultDeviceGroup;

        public SystemAgent()
        {
            Location = new LocationObserver();
        }
        
        #region Screen orientation

        public void OnOrientationChange(bool portrait)
        {
            currentDeviceGroup = portrait ? DeviceGroup.Res_480x800 : DeviceGroup.Res_800x480; //TODO: notifications
        }

        #endregion
    }
}
