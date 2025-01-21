using System.Windows.Media.Imaging;

namespace VaManager.Resources;

public class GlobalResources
{
    private double _unit = 1;
    public static GlobalResources Instance { get; } = new();

    public double Unit
    {
        get => _unit;
        set => _unit = Math.Max(1, value);
    }

    public double SizeSmall => Unit * 24;
    public double SizeMedium => Unit * 36;
    public double SizeLarge => Unit * 48;
    public double FontSizeSmall => Unit * 18;
    public double FontSizeMedium => Unit * 24;
    public double FontSizeLarge => Unit * 36;

    public BitmapImage DefaultImage { get; } = new();
    public BitmapImage FolderImage { get; } = new();

    public const string CachePath = "82husa08517h987";
    
    public static string GetFileCacheFolder()
    {
        var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = System.IO.Path.Combine(userFolderPath, @"AppData\Local\Temp", CachePath);
        return path;
    }
}