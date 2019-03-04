using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace IGrillLibrary
{
    public static class IGrillFactory
    {
        public static IGrill FromDeviceInformation(DeviceInformation device)
        {
            if (device.Name.StartsWith("iGrill_mini") || device.Name.StartsWith("iGrill Mini"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrillMini, device.Id);
            }
            else if (device.Name.StartsWith("iGrill_V2"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrill2, device.Id);
            }
            else if (device.Name.StartsWith("iGrill_V3"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrill3, device.Id);
            }
            else
            {
                throw new Exception("Unknown device with name " + device.Name);
            }
        }
    }
}
