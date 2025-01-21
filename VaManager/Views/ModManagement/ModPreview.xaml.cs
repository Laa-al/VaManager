using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VaManager.Data.Files;
using VaManager.Models;

namespace VaManager.Views.ModManagement;

public partial class ModPreview : UserControl
{
    public ModPreview()
    {
        InitializeComponent();
    }

    private void SelectFile(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
        if (sender is FrameworkElement { DataContext: FileDescriptor descriptor })
        {
            var model = FileModel.Instance;
            model.NavigateTo(descriptor.Folder);
            model.ItemSelect = descriptor;

            var mainWindow = MainWindowModel.Instance;
            mainWindow.SelectedMenuItem = mainWindow.FilePage;
        }
    }

    private void Resize(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        Grid.SetValue(WidthProperty, Math.Max(Grid.ActualWidth - e.HorizontalChange, 20));
    }
}