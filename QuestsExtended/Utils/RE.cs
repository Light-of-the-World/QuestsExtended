using System;
using System.Linq;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Utils;

namespace QuestsExtended.Utils;


/// <summary>
/// Class to assist with getting obfuscated types at runtime 
/// </summary>
/// <exception cref="MemberNotFoundException"></exception>
public static class RE
{
    public static Type BleedType;
    public static Type LightBleedType;
    public static Type HeavyBleedType;
    public static Type FractureType;
    public static Type PainType;
    public static Type MedEffectType;
    public static Type StimulatorType;

    public static Type JumpType;
    public static Type IdleType;
    public static Type RunType;
    public static Type SprintType;
    public static Type BreachDoorType;
    
    public static void CacheTypes()
    {
        BleedType = AccessTools.Inner(typeof(ActiveHealthController), "Bleeding");
        LightBleedType = AccessTools.Inner(typeof(ActiveHealthController), "LightBleeding");
        HeavyBleedType = AccessTools.Inner(typeof(ActiveHealthController), "HeavyBleeding");
        FractureType = AccessTools.Inner(typeof(ActiveHealthController), "Fracture");
        PainType = AccessTools.Inner(typeof(ActiveHealthController), "Pain");
        MedEffectType = AccessTools.Inner(typeof(ActiveHealthController), "MedEffect");
        StimulatorType = AccessTools.Inner(typeof(ActiveHealthController), "Stimulator");

        JumpType = PatchConstants.EftTypes
            .FirstOrDefault(t => t.GetMethod("ApplyMovementAndRotation") is not null);
        
        IdleType = PatchConstants.EftTypes
            .SingleOrDefault(t => t.GetFields()
                .FirstOrDefault(m => m.Name == "gclass683_0") is not null);
        
        RunType = PatchConstants.EftTypes
            .FirstOrDefault(t => t.GetMethod("HasNoInputForLongTime") is not null);
        
        SprintType = PatchConstants.EftTypes
            .FirstOrDefault(t => t.GetMethod("ChangeSpeed") is not null 
                                 && t.GetField("StationaryWeapon") is null);
        
        BreachDoorType = PatchConstants.EftTypes
            .FirstOrDefault(t => t.GetMethod("ExecuteDoorInteraction") is not null 
                                 && t.GetField("KickTime") is not null);
        
        if (BleedType is null || FractureType is null || StimulatorType is null)
        {
            throw new MemberNotFoundException("Could not find HealthController nested types");
        }
        
        Plugin.Log.LogError("Cached Reflection Objects");
    }
}