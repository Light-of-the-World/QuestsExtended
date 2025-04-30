using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SPT.Reflection.Utils;
using UnityEngine;

namespace QuestsExtended.SaveLoadRelatedClasses
{
    public class CompletedChildConditions : MonoBehaviour
    {
        public static List<string> CompletedOptionals = new List<string>();

        public void init()
        {
            LoadCompletedOptionals();
        }

        public void SaveCompletedOptionals()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_CompletedOptionals.json";
            string path = Path.Combine(directory, fileName);
            if (!File.Exists(path))
                File.Create(path);
            string data = JsonConvert.SerializeObject(CompletedOptionals, Formatting.Indented);
            File.WriteAllText(path, data);
            Plugin.Log.LogInfo($"Saved {CompletedOptionals.Count} optional condition(s) to file.");
        }

        public void LoadCompletedOptionals()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_CompletedOptionals.json";
            string path = Path.Combine(directory, fileName);
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                CompletedOptionals = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                Plugin.Log.LogInfo($"Loaded {CompletedOptionals.Count} optional condition(s) from file.");
            }
            else
            {
                Plugin.Log.LogInfo("No CompletedOptionals.json file found for this profile, starting fresh.");
                CompletedOptionals = new List<string>();
            }
        }
    }
}