using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using VaManager.Models.Basic;
using VaManager.Views.ConfigManagement;
using VaManager.Views.FileManagement;
using VaManager.Views.ModManagement;

namespace VaManager.Models;

public class MainWindowModel : ViewModelBase<MainWindowModel>
{
    private MenuItemModel _selectedMenuItem;

    public MainWindowModel()
    {
        MenuItems =
        [
            FilePage,
            ModPage,
            ConfigPage
        ];
        _selectedMenuItem = ConfigPage;
    }

    public List<MenuItemModel> MenuItems { get; set; }

    #region Menu Items

    public MenuItemModel FilePage { get; } =
        new("文件", typeof(FileExplorer), PackIconKind.File, PackIconKind.FileOutline);

    public MenuItemModel ModPage { get; } =
        new("Mod", typeof(ModExplorer), PackIconKind.ZipBox, PackIconKind.ZipBoxOutline);

    public MenuItemModel ConfigPage { get; } = new("配置", typeof(Config), PackIconKind.Cog, PackIconKind.CogOutline);

    #endregion

    public MenuItemModel SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (SetProperty(ref _selectedMenuItem, value))
            {
                if (Content is not null)
                {
                    Content.Content = new Frame
                    {
                        Content = Activator.CreateInstance(_selectedMenuItem.PageType),
                    };
                }
            }
        }
    }

    public Visibility Visibility { get; set; } = Visibility.Hidden;

    public ContentControl? Content { get; set; }


    public void ShowLoading()
    {
        Visibility = Visibility.Visible;
    }

    public void HideLoading()
    {
        Visibility = Visibility.Hidden;
    }
}