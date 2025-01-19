using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VaManager.Data.Mods;
using VaManager.Data.Tools;
using VaManager.Models;
using VaManager.Services;

namespace VaManager.Views.ModManagement;

public partial class ModBrowser
{
    public ModBrowser()
    {
        InitializeComponent();
    }

    private ModModel Model => (ModModel)DataContext;


    private void SelectOnly_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: ModDescriptor descriptor })
        {
            foreach (var mod in FileManager.Instance.ModDescriptors)
            {
                mod.IsSelected = false;
            }

            descriptor.IsSelected = true;
            FileModel.Instance.RefreshFileList();
            ModModel.Instance.RefreshModList();
        }
    }

    private void DeleteMod_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { DataContext: ModDescriptor descriptor })
        {
            FileManager.Instance.DeleteMod(descriptor);
        }
    }

    private void OpenLocalPath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFolder(sender);
    }

    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFile(sender);
    }

    private void Line_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            OpenFile(sender);
        }
    }


    private void OpenFolder(object sender)
    {
        if (sender is FrameworkElement { DataContext: ModDescriptor descriptor })
        {
            var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
            {
                Arguments = "/e,/select," + descriptor.ModPath
            };
            System.Diagnostics.Process.Start(psi);
        }
    }

    private void OpenFile(object sender)
    {
        if (sender is FrameworkElement { DataContext: ModDescriptor descriptor })
        {
            var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
            {
                Arguments = descriptor.ModPath
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
}