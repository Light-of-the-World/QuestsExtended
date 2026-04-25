using HarmonyLib;
using Newtonsoft.Json;
using QuestsExtended.Models;
using QuestsExtended.Quests;
using SPT.Common.Utils;
using SPT.Reflection.Utils;
//using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuestsExtended.SaveLoadRelatedClasses
{
    public class CompletedSaveData : MonoBehaviour
    {
        public static List<string> CompletedOptionals = new List<string>();
        public static List<string> CompletedMultipleChoice = new List<string>();
        public static List<string> QuestsStartedByQE = new List<string>();
        public static string DataDirectory;
        public static string CompletedOptionalsFilePath;
        public static string CompletedMultipleChoiceFilePath;
        public static string SpecialStartedQuestsFilePath;
        public static string QEQuestsFilePath;
        private static bool RanCheckLastInit = false;

        public bool hasDoneInit = false;
        public static bool hasScrubbedAFS = false;

        public void init(bool isRaid)
        {
            hasDoneInit = true;
            SetFileNames();
            LoadCompletedOptionals();
            LoadCompletedMultipleChoice();
            if (!isRaid)
            {
                if (RanCheckLastInit)
                {
                    RanCheckLastInit = false;
                    return;
                }
                CheckIfQuestNeedsToLoad();
            }
        }

        public void SetFileNames()
        {
            string baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataDirectory = Path.Combine(baseDirectory, "Data");
            DataDirectory = dataDirectory;
            string profileID = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;
            CompletedOptionalsFilePath = Path.Combine(dataDirectory, profileID + "_CompletedOptionals.json");
            CompletedMultipleChoiceFilePath = Path.Combine(dataDirectory, profileID + "_CompletedMultipleChoice.json");
            SpecialStartedQuestsFilePath = Path.Combine(dataDirectory, profileID + "_SpecialStartedQuests.json");
            string questDirectory = Path.Combine(baseDirectory, "Quests");
            QEQuestsFilePath = questDirectory;
        }
        
        //We need to create save data for when a quest has its AFS ignored by this mod. If a player does not accept that quest before closing the game, they will lose the quest, easily softlocking themselves.
        public void SaveCompletedOptionals()
        {
            /*
            if (!File.Exists(path))
                File.Create(path);
            */
            if (!File.Exists(CompletedOptionalsFilePath))
            {
                using (File.Create(CompletedOptionalsFilePath)) { } // Immediately close it
            }
            string data = JsonConvert.SerializeObject(CompletedOptionals, Formatting.Indented);
            File.WriteAllText(CompletedOptionalsFilePath, data);
            Plugin.Log.LogInfo($"Saved {CompletedOptionals.Count} optional condition(s) to file.");
        }

        public void LoadCompletedOptionals()
        {            
            if (File.Exists(CompletedOptionalsFilePath))
            {
                string json = File.ReadAllText(CompletedOptionalsFilePath);
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
            if (!Directory.Exists(DataDirectory))
                Directory.CreateDirectory(DataDirectory);
            if (!File.Exists(CompletedMultipleChoiceFilePath))
            {
                using (File.Create(CompletedMultipleChoiceFilePath)) { } // Immediately close it
            }
            string data = JsonConvert.SerializeObject(CompletedMultipleChoice, Formatting.Indented);
            File.WriteAllText(CompletedMultipleChoiceFilePath, data);
            Plugin.Log.LogInfo($"Saved {CompletedMultipleChoice.Count} completed multiple choice quests to file.");
        }

        public void LoadCompletedMultipleChoice()
        {
            if (File.Exists(CompletedMultipleChoiceFilePath))
            {
                string json = File.ReadAllText(CompletedMultipleChoiceFilePath);
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
            if (!Directory.Exists(DataDirectory))
                Directory.CreateDirectory(DataDirectory);
            /*
            if (!File.Exists(path))
                File.Create(path);
            */
            if (!File.Exists(SpecialStartedQuestsFilePath))
            {
                using (File.Create(SpecialStartedQuestsFilePath)) { } // Immediately close it
            }
            string data = JsonConvert.SerializeObject(quests, Formatting.Indented);
            File.WriteAllText(SpecialStartedQuestsFilePath, data);
            Plugin.Log.LogInfo($"Saved {quests.Count} started quests to file.");
        }

        public static void LoadQuestsThatWereStarted()
        {
            if (File.Exists(SpecialStartedQuestsFilePath))
            {
                string json = File.ReadAllText(SpecialStartedQuestsFilePath);
                QuestsStartedByQE = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                Plugin.Log.LogInfo($"Loaded {QuestsStartedByQE.Count} quest ids that were started by QE from file.");
            }
            else
            {
                Plugin.Log.LogInfo("No SpecialStartedQuests.json file found for this profile, starting fresh.");
                QuestsStartedByQE = new List<string>();
            }
        }
        public void CheckIfQuestNeedsToLoad()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string questDirectory = Path.Combine(directory, "Quests");
            string[] fileNames = Directory.GetFiles(questDirectory);
            List<CustomQuest> quests = new List<CustomQuest>();
            List<string> viewedQuests = new List<string>();
            foreach (var profileQuest in ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.QuestsData)
            {
               if (profileQuest.Status != EFT.Quests.EQuestStatus.AvailableForStart) viewedQuests.Add(profileQuest.Id);
            }
            List<string> QuestIdsToStart = new List<string>();
            foreach (string fileName in fileNames)
            {
                string json = File.ReadAllText(fileName);
                var questDict = JsonConvert.DeserializeObject<Dictionary<string, CustomQuest>>(json);
                quests.AddRange(questDict.Values);
            }
            foreach (var questid in QuestsStartedByQE)
            {
                if (viewedQuests.Contains(questid)) continue;
                CustomQuest currentQuest = new CustomQuest
                {
                    QuestName = "null"
                };
                foreach (CustomQuest quest in quests)
                {
                    if (quest.QuestId == questid)
                    {
                        currentQuest = quest;
                        break;
                    }
                }
                if (currentQuest.QuestName == "null")
                {
                    Plugin.Log.LogWarning($"Searched for a quest that does not exist: {questid}");
                    continue;
                }
                if (!currentQuest.IsMultipleChoiceStarter) continue;
                foreach (CustomCondition conditions in currentQuest.Conditions)
                {
                    if (conditions.QuestsToStart != null)
                    {
                        foreach (string id in conditions.QuestsToStart)
                        {
                            QuestIdsToStart.Add(id);
                        }
                    }
                }

            }
            if (QuestIdsToStart != null)
            {
                OptionalConditionController.SendQuestIdsForEditing<List<string>>(QuestIdsToStart);
                Plugin.Log.LogInfo($"QE Sending {QuestIdsToStart.Count} quests to have their AFS scrubbed.");
            }
            RanCheckLastInit = true;
        }
    }
}