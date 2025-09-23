using Defencev1.Enums;
using System.Globalization;

namespace Defencev1.Converters;

public class AppOperatingModeIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppOperatingMode operatingMode)
        {
            if (operatingMode is AppOperatingMode.OnlineServices)
                return "Online";
            if (operatingMode is AppOperatingMode.LocalServices)
                return "Offline";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
