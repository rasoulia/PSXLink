using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSXLink.MVVM.Data
{
    public class SettingRepository
    {
        public static void SaveSetting(Setting? setting)
        {
            FolderRepository.CreateFolder();
            string? fileName = @"Settings\Settings.json";
            JsonSerializerOptions? options = new JsonSerializerOptions { WriteIndented = true };
            string jsonSetting = JsonSerializer.Serialize(setting, options);
            File.WriteAllText(fileName, jsonSetting);
        }

        public static void LoadSetting()
        {
            string? fileName = @"Settings\Settings.json";

            if (!File.Exists(fileName))
            {
                SaveSetting(Setting.Instance());
            }

            string jsonSetting = File.ReadAllText("Settings\\Settings.json");
            Setting? setting = JsonSerializer.Deserialize<Setting>(jsonSetting);

            Setting.Instance().CheckOnly = setting!.CheckOnly;
            Setting.Instance().CheckVersion = setting!.CheckVersion;
        }
    }
}
