using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestsExtended.Models;

[Flags]

public enum EQuestConditionMisc1
{
    FixAnyMalfunction = 1 << 0,
    FixARMalfunction = 1 << 1,
    FixDMRMalfunction = 1 << 2,
    FixLMGMalfunction = 1 << 3,
    FixPistolMalfunction = 1 << 4,
    FixShotgunMalfunction = 1 << 5,
    FixSMGMalfunction= 1 << 6,
    FixSniperMalfunction = 1 << 7,
}
