using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace IGrill.Playground
{
    static class Settings
    {
        private static IPropertySet Values
        {
            get { return ApplicationData.Current.LocalSettings.Values; }
        }

        public static string MqttHost
        {
            get
            {
                return !(Values["MqttHost"] is string value) ? "192.168.0.69" : value;
            }
            set
            {
                Values["MqttHost"] = value;
            }
        }

        public static int MqttPort
        {
            get
            {
                return !(Values["MqttPort"] is int value) ? 1883 : value;
            }
            set
            {
                Values["MqttPort"] = value;
            }
        }

        public static bool TemperatureInFahranheit {
            get
            {
                var value = Values["TemperatureInFahrenheit"] as bool?;
                return value == null ? false : (bool)value;
            }
            set
            {
                Values["TemperatureInFahrenheit"] = value;
            }
        }

        public static bool StartInFullscreen
        {
            get
            {
                var value = Values["StartInFullscreen"] as bool?;
                return value == null ? false : (bool)value;
            }
            set
            {
                Values["StartInFullscreen"] = value;
            }
        }

        public static string SelectedDeviceId
        {
            get
            {
                return Values["SelectedDeviceId"] as string;
            }
            set
            {
                Values["SelectedDeviceId"] = value;
            }
        }

    
    }
}
