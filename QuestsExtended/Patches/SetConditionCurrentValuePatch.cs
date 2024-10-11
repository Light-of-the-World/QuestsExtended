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
        return PatchConstants.EftTypes.SingleCustom(t => t.GetEvent("OnConditionQuestTimeExpired") != null)
            .GetMethod("SetConditionCurrentValue");
    }

    [PatchPostfix]
    private static void Postfix(IConditionCounter conditional, EQuestStatus status, Condition condition, float value, bool notify)
    {
        if (!ConfigManager.EnableProgressNotifications.Value) return;
        if (value > condition.value) return;
        
        NotificationManagerClass.DisplayMessageNotification(
            $"{condition.id.Localized()} progress updated {value:F1}/{condition.value}",
            ConfigManager.ProgressNotificationDuration.Value,
            ENotificationIconType.Quest);
        
        Plugin.Log.LogDebug($"Incrementing {condition.id.Localized()} by {value}");
    }
}