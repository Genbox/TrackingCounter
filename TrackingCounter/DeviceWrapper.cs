using SharpPcap.WinPcap;

namespace TrackingCounter
{
    public class DeviceWrapper
    {
        public WinPcapDevice Device;

        public DeviceWrapper(WinPcapDevice device)
        {
            Device = device;
        }

        public override string ToString()
        {
            return Device.Interface.FriendlyName;
        }
    }
}