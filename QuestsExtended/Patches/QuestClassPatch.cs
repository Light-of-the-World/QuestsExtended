using System.Linq;
using System.Reflection;
using EFT.Communications;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Utils;
using SPT.Reflection.Patching;

namespace QuestsExtended.Patches;

public class QuestClassPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return PatchConstants.EftTypes.FirstOrDefault(t => t.GetEvent("OnConditionQuestTimeExpired") != null)
            .GetMethod("SetConditionCurrentValue");
    }

    [PatchPostfix]
    private static void Postfix(IConditionCounter conditional, EQuestStatus status, Condition condition, float value, bool notify)
    {
        NotificationManagerClass.DisplayMessageNotification(
            $"Progress updated on {condition.id.Localized()} to {value:F1}",
            ENotificationDurationType.Default,
            ENotificationIconType.Quest);
        
        Plugin.Log.LogDebug($"Incrementing {condition.id.Localized()} by {value}");
    }
}