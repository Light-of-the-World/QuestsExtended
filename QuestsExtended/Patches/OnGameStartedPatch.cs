using System.Reflection;
using EFT;
using HarmonyLib;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;

namespace QuestsExtended.Patches;

internal class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }
    
    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        __instance.GetOrAddComponent<QuestExtendedController>();
    }
}