using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Win32;
using Serilog;
using VaManager.Data;
using VaManager.Data.Files;
using VaManager.Data.Mods;
using VaManager.Extensions;
using VaManager.Models;
using VaManager.Resources;

namespace VaManager.Views.FileManagement;

public partial class FileBrowser
{
    public FileBrowser()
    {
        InitializeComponent();
    }

    private FileModel Model => (FileModel)DataContext;

    #region Navigate
    
    private void KeyEvent_Navigate(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Model.NavigateToPath(Model.ExplorerPath);
        }
    }

    private void RoutedEvent_Navigate(object sender, RoutedEventArgs e)
    {
        Model.NavigateToPath(Model.ExplorerPath);
    }

    private void RoutedEvent_NavigateBack(object sender, RoutedEventArgs e)
    {
        var path = Model.FolderOpen?.Parent?.Path;
        if (path is not null)
        {
            Model.NavigateToPath(path);
        }
    }

    private void RoutedEvent_NavigateRefresh(object sender, RoutedEventArgs e)
    {
        Model.RefreshFileList();
    }

    private void RoutedEvent_NavigateBefore(object sender, RoutedEventArgs e)
    {
        Model.NavigateBefore();
    }

    private void RoutedEvent_NavigateNext(object sender, RoutedEventArgs e)
    {
        Model.NavigateNext();
    }

    #endregion

    #region Data Grid Event

    private void ActionInvokeWithContext(object sender, Action<ItemDescriptor> action)
    {
        if (sender is FrameworkElement { DataContext: ItemDescriptor descriptor })
        {
            action(descriptor);
        }
    }

    private void RoutedEvent_OpenItem(object sender, RoutedEventArgs e)
    {
        ActionInvokeWithContext(sender, OpenItem);
    }

    private void MouseButtonEvent_OpenItem(object sender, MouseButtonEventArgs e)
    {
        ActionInvokeWithContext(sender, OpenItem);
    }

    #endregion

    #region Function

    private void OpenItem(ItemDescriptor descriptor)
    {
        switch (descriptor)
        {
            case FolderDescriptor folderDescriptor:
                Model.FolderOpen = folderDescriptor;
                break;
            case FileDescriptor fileDescriptor:
                fileDescriptor.OpenLocalFile();
                break;
        }
    }
    
    #endregion
}