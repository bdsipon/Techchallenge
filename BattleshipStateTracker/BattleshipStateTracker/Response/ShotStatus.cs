using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipStateTracker.Response
{
    public enum ShotStatus
    {
        Invalid,
        Duplicate,
        Miss,
        Hit,
        HitAndSunk,
        Victory
    }
}
