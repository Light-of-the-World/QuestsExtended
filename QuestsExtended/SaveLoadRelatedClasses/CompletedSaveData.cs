using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SPT.Reflection.Utils;
using UnityEngine;

namespace QuestsExtended.SaveLoadRelatedClasses
{
    public class CompletedSaveData : MonoBehaviour
    {
        public static List<string> CompletedOptionals = new List<string>();
        public static List<string> CompletedMultipleChoice = new List<string>();
        public static List<string> QuestsStartedByQE = new List<string>();
        public bool hasDoneInit = false;
        public static bool hasScrubbedAFS = false;

        public void init()
        {
            hasDoneInit = true;
            LoadCompletedOptionals();
            LoadCompletedMultipleChoice();
        }
        
        //We need to create save data for when a quest has its AFS ignored by this mod. If a player does not accept that quest before closing the game, they will lose the quest, easily softlocking themselves.
        public void SaveCompletedOptionals()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_CompletedOptionals.json";
            string path = Path.Combine(directory, fileName);
            /*
            if (!File.Exists(path))
                File.Create(path);
            */
            if (!File.Exists(path))
            {
                using (File.Create(path)) { } // Immediately close it
            }
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

        public void SaveCompletedMultipleChoice()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_CompletedMultipleChoice.json";
            string path = Path.Combine(directory, fileName);
            /*
            if (!File.Exists(path))
                File.Create(path);
            */
            if (!File.Exists(path))
            {
                using (File.Create(path)) { } // Immediately close it
            }
            string data = JsonConvert.SerializeObject(CompletedMultipleChoice, Formatting.Indented);
            File.WriteAllText(path, data);
            Plugin.Log.LogInfo($"Saved {CompletedMultipleChoice.Count} completed multiple choice quests to file.");
        }

        public void LoadCompletedMultipleChoice()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_CompletedMultipleChoice.json";
            string path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                CompletedMultipleChoice = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                Plugin.Log.LogInfo($"Loaded {CompletedMultipleChoice.Count} completed multiple choice quests from file.");
            }
            else
            {
                Plugin.Log.LogInfo("No CompletedMultipleChoice.json file found for this profile, starting fresh.");
                CompletedMultipleChoice = new List<string>();
            }
        }

        public void LogQuestThatWasStarted(List<string> quests)
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_SpecialStartedQuests.json";
            string path = Path.Combine(directory, fileName);
            /*
            if (!File.Exists(path))
                File.Create(path);
            */
            if (!File.Exists(path))
            {
                using (File.Create(path)) { } // Immediately close it
            }
            string data = JsonConvert.SerializeObject(quests, Formatting.Indented);
            File.WriteAllText(path, data);
            Plugin.Log.LogInfo($"Saved {quests.Count} started quests to file.");
        }

        public static void LoadQuestsThatWereStarted()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Combine(directory, "Data");
            string fileName = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId + "_SpecialStartedQuests.json";
            string path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                QuestsStartedByQE = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                Plugin.Log.LogInfo($"Loaded {QuestsStartedByQE.Count} quest ids that were started by QE from file.");
            }
            else
            {
                Plugin.Log.LogInfo("No SpecialStartedQuests.json file found for this profile, starting fresh.");
                QuestsStartedByQE = new List<string>();
            }
        }
    }
}