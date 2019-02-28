using IGrill.Playground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace IGrill.App.Areas
{
    public class TemperatureConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "";
            }

            int temperatureInCelcius = (int)value;

            if (Settings.TemperatureInFahranheit)
            {
                return String.Format("{0} °F", (temperatureInCelcius * 9 / 5) + 32);
            }
            return String.Format("{0} °C", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
