using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using VaManager.Extensions;
using VaManager.Models.Basic;

namespace VaManager.Models;

public class ConfigModel : ViewModelBase<ConfigModel>
{
    public static SerializableConfig ConfigInstance => Instance.Config;
    
    public SerializableConfig Config { get; set; } = new();

    #region Save & Load

    private const string ConfigFileName = "config.json";

    public void LoadConfig()
    {
        try
        {
            if (!File.Exists(ConfigFileName)) return;
            var configJson = File.ReadAllText(ConfigFileName);
            var config = JsonSerializer.Deserialize<SerializableConfig>(configJson);
            if (config is null)
            {
                Log.Warning("Config file is empty.");
                return;
            }
            Config = config;
        }
        catch (Exception e)
        {
            Log.Error(e,"Could not load config file.");
        }
    }


    public void SaveConfig()
    {
        var json = JsonSerializer.Serialize(Config);
        File.WriteAllText(ConfigFileName, json);
    }

    #endregion
}