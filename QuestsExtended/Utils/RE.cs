using System;
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

        foreach (var t in PatchConstants.EftTypes) {
            if (t.GetMethod("ApplyMovementAndRotation") != null) { 
                JumpType = t; 
                return; }
        }

        foreach (var t in PatchConstants.EftTypes)
        {
            foreach (var m in t.GetFields())
            {
                if (m.Name == "gclass683_0")
                {
                    IdleType = t;
                    return;
                }
            }
        }

        foreach (var t in PatchConstants.EftTypes)
        {
            if (t.GetMethod("HasNoInputForLongTime") != null)
            {
                RunType = t;
                return;
            }
        }

        foreach (var t in PatchConstants.EftTypes)
        {
            if (t.GetMethod("ChangeSpeed") != null && t.GetField("KickTime") == null)
            {
                RunType = t;
                return;
            }
        }

        foreach (var t in PatchConstants.EftTypes)
        {
            if (t.GetMethod("ExecuteDoorInteraction") != null && t.GetField("StationaryWeapon") != null)
            {
                BreachDoorType = t;
                return;
            }
        }
        if (BleedType is null || FractureType is null || StimulatorType is null)
        {
            throw new MemberNotFoundException("Could not find HealthController nested types");
        }
        
        Plugin.Log.LogInfo("Cached Reflection Objects");
    }
}