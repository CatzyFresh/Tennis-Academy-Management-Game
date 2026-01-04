using System;

namespace TennisAcademyManager.Systems
{
    [Serializable]
    public class CourtInstance
    {
        public CourtType Type;
        public int BuildCost;
        public int MonthlyMaintenance;
        public float InjuryRisk;
        public int Capacity;

        public CourtInstance(CourtDefinition def)
        {
            Type = def.Type;
            BuildCost = def.BuildCost;
            MonthlyMaintenance = def.MonthlyMaintenance;
            InjuryRisk = def.InjuryRisk;
            Capacity = def.Capacity;
        }
    }
}
