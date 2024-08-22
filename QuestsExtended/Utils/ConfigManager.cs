using BepInEx.Configuration;
using EFT.Communications;

namespace QuestsExtended.Utils;

public class ConfigManager
{
    public static ConfigEntry<bool> EnableProgressNotifications;
    public static ConfigEntry<ENotificationDurationType> ProgressNotificationDuration;

    public static void InitConfig(ConfigFile config)
    {
        EnableProgressNotifications = config.Bind(
            "Progress Notifications", 
            "Enable Notifications",
            true);
        
        ProgressNotificationDuration = config.Bind(
            "Progress Notifications", 
            "Duration of popup",
            ENotificationDurationType.Default,
            "How long a notification remains on your screen. These are BSG values.");
    }
}