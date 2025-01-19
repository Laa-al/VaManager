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

    public string[] ModSuffix { get; } = ["rar", "var"];
    
    public const string CachePath = "82husa08517h987";
}