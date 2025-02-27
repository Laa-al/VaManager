﻿using System.Windows;
using System.Windows.Input;
using VaManager.Data.Mods;
using VaManager.Models;
using VaManager.Services;

namespace VaManager.Views.ModManagement;

public partial class ModBrowser
{
    public ModBrowser()
    {
        InitializeComponent();
    }

    #region Data Grid Event

    private void ActionInvokeWithContext(object sender, Action<ModDescriptor> action)
    {
        if (sender is FrameworkElement { DataContext: ModDescriptor descriptor })
        {
            action(descriptor);
        }
    }

    private void RoutedEvent_FilterOnly(object sender, RoutedEventArgs e)
    {
        ActionInvokeWithContext(sender, FilterOnly);
    }

    private void RoutedEvent_DeleteFile(object sender, RoutedEventArgs e)
    {
        List<ModDescriptor> descriptors = [];
        foreach (var item in ModDataGrid.SelectedItems)
        {
            if (item is ModDescriptor descriptor)
            {
                descriptors.Add(descriptor);
            }
        }

        foreach (var descriptor in descriptors)
        {
            DeleteFile(descriptor);
        }
        
        // ActionInvokeWithContext(sender, DeleteFile);
    }

    private void RoutedEvent_OpenAndSelectFile(object sender, RoutedEventArgs e)
    {
        ActionInvokeWithContext(sender, OpenFolderAndSelectFile);
    }

    private void RoutedEvent_OpenFile(object sender, RoutedEventArgs e)
    {
        ActionInvokeWithContext(sender, OpenFile);
    }

    private void MouseButtonEvent_OpenFile(object sender, MouseButtonEventArgs e)
    {
        ActionInvokeWithContext(sender, OpenFile);
    }

    #endregion

    #region Function

    private void OpenFolderAndSelectFile(ModDescriptor descriptor)
    {
        FileManager.OpenFolderAndSelectFile(descriptor.ModPath);
    }

    private void OpenFile(ModDescriptor descriptor)
    {
        FileManager.OpenFileOrFolder(descriptor.ModPath);
    }

    private void DeleteFile(ModDescriptor descriptor)
    {
        FileManager.Instance.DeleteMod(descriptor);
    }
    
    private void FilterOnly(ModDescriptor descriptor)
    {
        foreach (var mod in FileManager.Instance.ModDescriptors)
        {
            mod.IsSelected = false;
        }

        descriptor.IsSelected = true;
        FileModel.Instance.RefreshFileList();
    }

    #endregion
}