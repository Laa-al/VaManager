using MaterialDesignThemes.Wpf;

namespace VaManager.Models.Basic;

public class MenuItemModel(string? title, Type pageType, PackIconKind selectedIcon, PackIconKind unselectedIcon)
    : ViewModelBase
{
    public string? Title { get; set; } = title;
    public PackIconKind SelectedIcon { get; set; } = selectedIcon;
    public PackIconKind UnselectedIcon { get; set; } = unselectedIcon;
    // public Type PageType { get; set; } = pageType;
    public Type PageType { get; set; } = pageType;
}