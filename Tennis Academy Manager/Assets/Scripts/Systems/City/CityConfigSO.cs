using UnityEngine;

namespace TennisAcademyManager.Systems.City
{
    [CreateAssetMenu(menuName = "TAM/Config/City Config", fileName = "CityConfig")]
    public class CityConfigSO : ScriptableObject
    {
        public CityType cityType = CityType.Tier2City;

        [Header("Demand")]
        [Tooltip("Multiplies daily inquiries.")]
        [Range(0.5f, 2.0f)] public float demandMultiplier = 1.0f;

        [Header("Fee tolerance")]
        [Tooltip("Higher = players tolerate higher fees before care penalty hits.")]
        [Range(0.7f, 1.5f)] public float feeToleranceMultiplier = 1.0f;

        [Header("Costs")]
        [Tooltip("Multiplies monthly maintenance costs (courts etc.).")]
        [Range(0.7f, 1.8f)] public float maintenanceMultiplier = 1.0f;

        [Tooltip("Multiplies coach salaries.")]
        [Range(0.7f, 2.0f)] public float salaryMultiplier = 1.0f;

        [Header("Competition pressure")]
        [Tooltip("Higher = more competitive market: reduces conversion/retention unless reputation is strong.")]
        [Range(0.7f, 1.3f)] public float competitionPressure = 1.0f;

        [Header("Tournaments")]
        [Tooltip("Higher = easier access to tournaments (more templates available / less travel penalty).")]
        [Range(0.7f, 1.3f)] public float tournamentAccessibility = 1.0f;

        [Tooltip("Multiplies travel costs.")]
        [Range(0.7f, 2.0f)] public float travelCostIndex = 1.0f;

        public static CityConfigSO CreatePreset(CityType type)
        {
            // Not used at runtime; just describes intended tuning.
            // Create assets in Unity instead.
            return null;
        }
    }
}
