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

    private void NavigateInput_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Model.NavigateToPath(Model.ExplorerPath);
        }
    }

    private void NavigateButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.NavigateToPath(Model.ExplorerPath);
    }

    private void NavigateBackButton_OnClick(object sender, RoutedEventArgs e)
    {
        var path = Model.FolderSelect?.Parent?.Path;
        if (path is not null)
        {
            Model.NavigateToPath(path);
        }
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.RefreshFileList();
    }

    private void NavigateBeforeButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.NavigateBefore();
    }

    private void NavigateNextButton_OnClick(object sender, RoutedEventArgs e)
    {
        Model.NavigateNext();
    }

    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        EnsureSelectItem(sender);
    }

    private void Item_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            EnsureSelectItem(sender);
        }
    }

    private void EnsureSelectItem(object sender)
    {
        if (sender is FrameworkElement { DataContext: ItemDescriptor descriptor })
        {
            switch (descriptor)
            {
                case FolderDescriptor folderDescriptor:
                    Model.FolderSelect = folderDescriptor;
                    break;
                case FileDescriptor fileDescriptor:
                    if (fileDescriptor.Mod is null)
                    {
                        var mainPath = ConfigModel.ConfigInstance.MainFolderPath;
                        var filePath = fileDescriptor.Path["/root/".Length..];
                        var path = Path.Combine(mainPath, filePath);

                        OpenFile(path.Replace('/','\\'));
                    }
                    else
                    {
                        Task.Run(() =>
                        {
                            var environment = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                            var path = Path.Combine(environment, "AppData/Local/Temp",
                                GlobalResources.CachePath, fileDescriptor.Mod.Guid,
                                fileDescriptor.Mod.ProgramVersion,
                                fileDescriptor.Path[1..]).Replace('/','\\');
                            if (File.Exists(path))
                            {
                                OpenFile(path);
                            }
                            else
                            {
                                fileDescriptor.UsingStream(s =>
                                {
                                    try
                                    {
                                        var folder = Path.GetDirectoryName(path);
                                        if (folder is null) return;

                                        if (!Directory.Exists(folder))
                                        {
                                            CreateDirectory(folder);
                                        }

                                        File.WriteAllBytes(path, s.ReadAllBytes());

                                        OpenFile(path);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error(e, "Failed to upzip mod.");
                                    }

                                    return;

                                    void CreateDirectory(string p)
                                    {
                                        var pre = Path.GetDirectoryName(p);
                                        if (!Directory.Exists(pre))
                                            CreateDirectory(pre!);
                                        Directory.CreateDirectory(p);
                                    }
                                });
                            }
                        });
                    }

                    break;
            }
        }
    }


    private void OpenFile(string path)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
        {
            Arguments = path
        };
        System.Diagnostics.Process.Start(psi);
    }
}