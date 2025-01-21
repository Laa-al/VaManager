using VaManager.Models;

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