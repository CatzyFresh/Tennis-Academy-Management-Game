using System;

namespace TennisAcademyManager.Systems.Players
{
    public enum PlayerStatus
    {
        Prospect,   // inquiry created but not enrolled
        Active,     // enrolled in academy
        Injured,    // currently injured (derived from health)
        Recovering, // on rehab plan
        Released    // left academy
    }

    public enum Handedness { Right, Left }
    public enum BackhandType { OneHand, TwoHand }

    [Flags]
    public enum PlayerTraitFlags
    {
        None = 0,
        InjuryProne = 1 << 0,
        Durable = 1 << 1,
        HighEndurance = 1 << 2,
        Overtrainer = 1 << 3
    }
}
