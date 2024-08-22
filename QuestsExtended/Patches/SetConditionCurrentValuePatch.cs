using System.Linq;
using System.Reflection;
using EFT.Communications;
using EFT.Quests;
using HarmonyLib;
using QuestsExtended.Utils;
using SPT.Reflection.Utils;
using SPT.Reflection.Patching;

namespace QuestsExtended.Patches;

public class SetConditionCurrentValuePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return PatchConstants.EftTypes.FirstOrDefault(t => t.GetEvent("OnConditionQuestTimeExpired") != null)
            .GetMethod("SetConditionCurrentValue");
    }

    [PatchPostfix]
    private static void Postfix(IConditionCounter conditional, EQuestStatus status, Condition condition, float value, bool notify)
    {
        if (!ConfigManager.EnableProgressNotifications.Value) return;
        
        NotificationManagerClass.DisplayMessageNotification(
            $"Progress updated on {condition.id.Localized()} to {value:F1}",
            ConfigManager.ProgressNotificationDuration.Value,
            ENotificationIconType.Quest);
        
        Plugin.Log.LogDebug($"Incrementing {condition.id.Localized()} by {value}");
    }
}