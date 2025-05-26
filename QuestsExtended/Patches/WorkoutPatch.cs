using EFT;
using EFT.Hideout;
using EFT.Interactive;
using HarmonyLib;
using QuestsExtended.Models;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuestsExtended.Patches
{
    internal class WorkoutPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.StartQte));
        }
        [PatchPostfix]
        private static void Postfix(ref HideoutPlayerOwner owner)
        {
            if (owner.Player.ProfileId == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Id)
            {
                GameObject persistentObject = GameObject.Find("PersistentCounterObject");

                if (persistentObject == null)
                {
                    persistentObject = new GameObject("PersistentCounterObject");
                    UnityEngine.Object.DontDestroyOnLoad(persistentObject);
                    Plugin.Log.LogInfo("Created PersistentCounterObject");
                }
                WorkoutCounter counter = persistentObject.GetOrAddComponent<WorkoutCounter>();
                UnityEngine.Object.DontDestroyOnLoad(persistentObject);
                Plugin.Log.LogInfo("Player is beginning workout");
                counter.counter += 1;
            }
        }
    }
}
