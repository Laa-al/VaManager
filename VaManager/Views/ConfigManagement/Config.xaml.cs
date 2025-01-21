using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using VaManager.Models;
using VaManager.Services;

namespace VaManager.Views.ConfigManagement;

public partial class Config
{
    public Config()
    {
        InitializeComponent();
    }

    private ConfigModel Model => (ConfigModel)DataContext;

    private void SelectMainFolder(object sender, RoutedEventArgs e)
    {
        var openFolder = new CommonOpenFileDialog();
        openFolder.AllowNonFileSystemItems = true;
        openFolder.Multiselect = false;
        openFolder.IsFolderPicker = true;
        openFolder.Title = "Select main folder";

        if (openFolder.ShowDialog() == CommonFileDialogResult.Ok)
        {
            Model.Config.MainFolderPath = openFolder.FileName;
        }
    }

    private void SelectModFolder(object sender, RoutedEventArgs e)
    {
        var openFolder = new CommonOpenFileDialog();
        openFolder.AllowNonFileSystemItems = true;
        openFolder.Multiselect = false;
        openFolder.IsFolderPicker = true;
        openFolder.Title = "Select mod folder";

        if (openFolder.ShowDialog() == CommonFileDialogResult.Ok &&
            !Model.Config.ExtraModPaths.Contains(openFolder.FileName))
        {
            Model.Config.ExtraModPaths.Add(openFolder.FileName);
        }
    }

    private void DeleteModFolder(object sender, RoutedEventArgs e)
    {
        if (ModPathsListView.SelectedItem is string path)
        {
            Model.Config.ExtraModPaths.Remove(path);
        }
    }

    private void RoutedEvent_ClearCache(object sender, RoutedEventArgs e)
    {
        FileManager.ClearLocalFileCache();
    }

    private void RoutedEvent_SaveConfig(object sender, RoutedEventArgs e)
    {
        Model.SaveConfig();
    }

    private void RoutedEvent_Analyze(object sender, RoutedEventArgs e)
    {
        FileManager.Instance.ReanalyzeFromConfig();
    }

    private void RoutedEvent_LoadConfig(object sender, RoutedEventArgs e)
    {
        Model.LoadConfig();
    }
}