using System;
using System.Linq;
using EFT.HealthSystem;
using HarmonyLib;

namespace QuestsExtended.Utils;

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
    
    static RE()
    {
        BleedType = AccessTools.Inner(typeof(ActiveHealthController), "LightBleeding");
        LightBleedType = AccessTools.Inner(typeof(ActiveHealthController), "LightBleeding");
        HeavyBleedType = AccessTools.Inner(typeof(ActiveHealthController), "HeavyBleeding");
        FractureType = AccessTools.Inner(typeof(ActiveHealthController), "Fracture");
        PainType = AccessTools.Inner(typeof(ActiveHealthController), "Pain");
        MedEffectType = AccessTools.Inner(typeof(ActiveHealthController), "MedEffect");
        StimulatorType = AccessTools.Inner(typeof(ActiveHealthController), "MedEffect");

        JumpType = AccessTools.AllTypes()
            .SingleOrDefault(t => t.GetMethods()
                .FirstOrDefault(m => m.Name == "ApplyMovementAndRotation") is not null);
        
        IdleType = AccessTools.AllTypes()
            .SingleOrDefault(t => t.GetFields()
                .FirstOrDefault(m => m.Name == "gclass683_0") is not null);
        
        RunType = AccessTools.AllTypes()
            .SingleOrDefault(t => t.GetMethods()
                .FirstOrDefault(m => m.Name == "HasNoInputForLongTime") is not null);
        
        SprintType = AccessTools.AllTypes()
            .SingleOrDefault(t => t.GetMethods()
                .FirstOrDefault(m => m.Name == "ChangeSpeed") is not null && 
                                  t.GetFields().SingleOrDefault(f => f.Name == "StationaryWeapon") is null);
        
        BreachDoorType = AccessTools.AllTypes()
            .SingleOrDefault(t => t.GetMethods()
                .FirstOrDefault(m => m.Name == "ExecuteDoorInteraction") is not null && 
                                  t.GetFields().SingleOrDefault(f => f.Name == "KickTime") is not null);
        
        if (BreachDoorType is null || SprintType is null || RunType is null)
        {
            throw new MemberNotFoundException("Could not find HealthController nested types");
        }
    }
}