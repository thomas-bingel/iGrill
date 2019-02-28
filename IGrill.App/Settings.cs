using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IGrill.Playground
{
    static class Settings
    {

        public static bool TemperatureInFahranheit {
            get
            {
                var value = ApplicationData.Current.LocalSettings.Values["TemperatureInFahrenheit"] as bool?;
                return value == null ? false : (bool)value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["TemperatureInFahrenheit"] = value;
            }
        }

        public static bool StartInFullscreen
        {
            get
            {
                var value = ApplicationData.Current.LocalSettings.Values["StartInFullscreen"] as bool?;
                return value == null ? false : (bool)value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["StartInFullscreen"] = value;
            }
        }

        public static string SelectedDeviceId
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["SelectedDeviceId"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["SelectedDeviceId"] = value;
            }
        }

    }
}
