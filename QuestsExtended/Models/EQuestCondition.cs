namespace QuestsExtended.Models;

public enum EQuestCondition
{
    /*
    InspectLock,
    PickLock,
    PickLockFailed,
    RepairLock,
    RepairLockFailed,
    BreakLock,
    HackDoor,
    HackDoorFailed,
    */
    FixLightBleed,
    FixHeavyBleed,
    FixFracture,
    HealthLoss,
    HealthGain,
    DestroyBodyPart,
    RestoreBodyPart,
    Die,
    EncumberedTimeInSeconds,
    OverEncumberedTimeInSeconds,
    //All below this line have been added by Light. SE = Self Explanitory
    FixAnyBleed, //Combines Light and Heavy bleed
    MoveDistance, //SE
    MoveDistanceWhileCrouched, //I will need to look into how the game decides what is and isn't 'crouched'
    MoveDistanceWhileProne, //SE
    MoveDistanceWhileSilent, //Moving slow enough to earn Covert Movement xp
    KillWhileADS, //SE
    KillWhileProne, //SE
    SearchContainer, //SE
    LootItem,  //SE
}