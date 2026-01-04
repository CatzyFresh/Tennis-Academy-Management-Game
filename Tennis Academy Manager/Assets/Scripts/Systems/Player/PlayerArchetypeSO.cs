using UnityEngine;
using TennisAcademyManager.Systems.Health;

namespace TennisAcademyManager.Systems.Players
{
    [CreateAssetMenu(menuName = "TAM/Players/Player Archetype", fileName = "PlayerArchetype")]
    public sealed class PlayerArchetypeSO : ScriptableObject
    {
        [Header("Identity")]
        public string archetypeName = "Junior Prospect";
        [Range(6, 45)] public int minAge = 10;
        [Range(6, 45)] public int maxAge = 16;

        [Header("Style (weights)")]
        [Range(0, 1)] public float rightHandedChance = 0.85f;
        [Range(0, 1)] public float twoHandBackhandChance = 0.80f;

        [Header("Base Stats Ranges (0-100)")]
        [Range(0, 100)] public int minServe = 20;
        [Range(0, 100)] public int maxServe = 60;
        [Range(0, 100)] public int minForehand = 20;
        [Range(0, 100)] public int maxForehand = 60;
        [Range(0, 100)] public int minBackhand = 20;
        [Range(0, 100)] public int maxBackhand = 60;
        [Range(0, 100)] public int minMovement = 20;
        [Range(0, 100)] public int maxMovement = 60;
        [Range(0, 100)] public int minMental = 20;
        [Range(0, 100)] public int maxMental = 60;

        [Header("Economy")]
        [Tooltip("One-time enrollment fee paid to academy (if your design uses it)")]
        public int enrollmentFee = 0;
        [Tooltip("Monthly fee or contract value paid by academy to player (scholarship) or player to academy. Your economy system decides direction.")]
        public int monthlyContractValue = 5000;

        [Header("Traits (probabilities)")]
        [Range(0, 1)] public float injuryProneChance = 0.10f;
        [Range(0, 1)] public float durableChance = 0.10f;
        [Range(0, 1)] public float highEnduranceChance = 0.15f;
        [Range(0, 1)] public float overtrainerChance = 0.10f;

        [Header("Health Baseline")]
        [Tooltip("Starting fatigue for newly enrolled player")]
        [Range(0, 100)] public float startingFatigue = 10f;
        [Tooltip("Starting injury risk for newly enrolled player")]
        [Range(0, 100)] public float startingRisk = 5f;
    }
}
