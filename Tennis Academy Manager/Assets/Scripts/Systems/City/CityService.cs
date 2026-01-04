using TennisAcademyManager.Core;

namespace TennisAcademyManager.Systems.City
{
    public sealed class CityService : IGameService
    {
        public CityConfigSO Config { get; private set; }

        public void Initialize() { }

        public void SetConfig(CityConfigSO config)
        {
            Config = config;
        }

        // Convenience accessors (safe defaults)
        public float DemandMult => Config ? Config.demandMultiplier : 1f;
        public float FeeToleranceMult => Config ? Config.feeToleranceMultiplier : 1f;
        public float MaintenanceMult => Config ? Config.maintenanceMultiplier : 1f;
        public float SalaryMult => Config ? Config.salaryMultiplier : 1f;
        public float CompetitionPressure => Config ? Config.competitionPressure : 1f;
        public float TournamentAccessibility => Config ? Config.tournamentAccessibility : 1f;
        public float TravelCostIndex => Config ? Config.travelCostIndex : 1f;
    }
}
