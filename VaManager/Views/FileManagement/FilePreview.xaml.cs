using System.Windows;
using System.Windows.Controls;
using VaManager.Data.Files;
using VaManager.Models;

namespace VaManager.Views.FileManagement;

public partial class FilePreview : UserControl
{
    public FilePreview()
    {
        InitializeComponent();
    }

    private void NavigateToMod(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: FileDescriptor descriptor })
        {
            ModModel.Instance.ModSelect = descriptor.Mod;
            MainWindowModel.Instance.SelectedMenuItem = MainWindowModel.Instance.ModPage;
        }
    }
    
    private void Resize(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        Grid.SetValue(WidthProperty, Math.Max(Grid.ActualWidth - e.HorizontalChange, 20));
    }
}