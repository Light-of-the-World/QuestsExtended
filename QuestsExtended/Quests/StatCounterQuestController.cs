using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using JetBrains.Annotations;
using QuestsExtended.Models;
using UnityEngine;

namespace QuestsExtended.Quests
{
    internal class StatCounterQuestController : AbstractCustomQuestController
    {
        public StatCounterQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
        {

        }

        public void Awake()
        {
            Plugin.Log.LogInfo("StatCounterQuestController is active.");
        }
        public static void EnemyDamageProcessor(DamageInfoStruct damage, float distance)
        {
            Plugin.Log.LogInfo($"[StatsCounter] Sucessfully triggered EnemyDamageProcessor with sent information. You may now start writing the code. Body Damage was {damage.DidBodyDamage}, armour damage was {damage.DidArmorDamage} and distance was {distance}");
        }
    }
}
