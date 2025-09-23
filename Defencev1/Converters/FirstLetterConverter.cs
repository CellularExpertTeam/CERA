using System.Globalization;

namespace Defencev1.Converters;

public class FirstLetterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string username && !string.IsNullOrEmpty(username))
        {
            return username.Substring(0, 1).ToUpperInvariant();
        }
        return string.Empty; // Return empty string for null or empty usernames
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

