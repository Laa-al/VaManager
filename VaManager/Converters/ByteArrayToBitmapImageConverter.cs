using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VaManager.Converters;

public class ByteArrayToBitmapImageConverter : IValueConverter
{

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes || bytes.Length == 0 || parameter is false)
        {
            return DependencyProperty.UnsetValue;
        }

        var stream = new MemoryStream(bytes);
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
        catch (Exception)
        {
            stream.Close();
            return DependencyProperty.UnsetValue;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BitmapImage image) return null;
        using var stream = new MemoryStream();
        image.StreamSource.CopyTo(stream);
        return stream.ToArray();
    }
}