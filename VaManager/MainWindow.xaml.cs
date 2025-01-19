using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using VaManager.Models;
using VaManager.Models.Basic;
using VaManager.Views.ConfigManagement;
using VaManager.Views.FileManagement;

namespace VaManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Model.Content = ContentControl;
        Model.SelectedMenuItem = Model.FilePage;
    }

    private MainWindowModel Model => (MainWindowModel)DataContext;
}