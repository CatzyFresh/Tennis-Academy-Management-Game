using UnityEngine;
using TennisAcademyManager.Systems;
using TennisAcademyManager.Systems.Health;
using System.Collections.Generic;
using TennisAcademyManager.Systems.Tournaments;
using TennisAcademyManager.Systems.City;

namespace TennisAcademyManager.Settings
{
    [CreateAssetMenu(menuName = "TAM/Config/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Demand")]
        public DemandConfig DemandConfig;

        [Header("Health")]
        public HealthTuningSO HealthTuning;

        [Header("City")]
        public CityConfigSO CityConfig;

        [Header("Tournaments")]
        [Tooltip("Weekly tournament templates used by TournamentService.RunWeeklyCycle().")]

        public List<TournamentTemplateSO> TournamentTemplates = new();
        // Later:
        // public PricingConfig PricingConfig;
        // public InjuryConfig InjuryConfig;
    }
}
