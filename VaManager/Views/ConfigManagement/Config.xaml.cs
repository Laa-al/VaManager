using System.IO;
using System.Windows;
using System.Windows.Controls;
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

    private void ClickButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.SaveConfig();
    }

    private void AnalyzeButton_OnClick(object sender, RoutedEventArgs e)
    {
        FileManager.Instance.ReanalyzeFromConfig();
    }

    private void RestoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.LoadConfig();
    }
}