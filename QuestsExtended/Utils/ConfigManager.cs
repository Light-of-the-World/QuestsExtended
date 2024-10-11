using BepInEx.Configuration;
using EFT.Communications;

namespace QuestsExtended.Utils;

public class ConfigManager
{
    public static ConfigEntry<bool> EnableProgressNotifications;
    public static ConfigEntry<ENotificationDurationType> ProgressNotificationDuration;

    public static ConfigEntry<bool> DumpQuestZones;

    public static void InitConfig(ConfigFile config)
    {
        EnableProgressNotifications = config.Bind(
            "Progress Notifications", 
            "Enable Notifications",
            true,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));
        
        ProgressNotificationDuration = config.Bind(
            "Progress Notifications", 
            "Duration of popup",
            ENotificationDurationType.Default,
            new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
        
        DumpQuestZones = config.Bind(
            "Development", 
            "Dump Quest Zones",
            false,
            new ConfigDescription("Requires loading into each map to log them to the Bepinex output.", null, new ConfigurationManagerAttributes { Order = 2 }));
    }
}