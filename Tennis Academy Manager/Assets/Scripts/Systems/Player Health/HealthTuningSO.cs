using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    [CreateAssetMenu(menuName = "TAM/Health/Health Tuning", fileName = "HealthTuning")]
    public sealed class HealthTuningSO : ScriptableObject
    {
        [Header("Fatigue Gains (base)")]
        public float lightTrainingFatigue = 4f;
        public float normalTrainingFatigue = 8f;
        public float intenseTrainingFatigue = 14f;
        public float matchShortFatigue = 12f;
        public float matchLongFatigue = 18f;
        public float tournamentDayFatigue = 20f;

        [Header("Playing While Injured")]
        [Tooltip("Extra fatigue gain multiplier when playing while injured (e.g., 0.25 = +25%)")]
        [Range(0f, 2f)] public float playingInjuredFatigueExtraMult = 0.25f;

        [Header("Injury Risk Formula")]
        [Tooltip("Risk gain = (Fatigue - fatigueRiskStart) * activityRiskFactor")]
        [Range(0f, 100f)] public float fatigueRiskStart = 40f;

        [Tooltip("Risk factors per activity group")]
        public float trainingRiskFactor = 0.2f;
        public float matchRiskFactor = 0.4f;
        public float tournamentRiskFactor = 0.6f;

        [Header("Severity Cutoffs (based on risk at trigger)")]
        [Range(0f, 100f)] public float minorMaxRisk = 30f;
        [Range(0f, 100f)] public float moderateMaxRisk = 55f;
        // > moderateMaxRisk => Severe

        [Header("Injury Durations (days)")]
        public IntRange minorDays = new IntRange(3, 5);
        public IntRange moderateDays = new IntRange(7, 21);
        public IntRange severeDays = new IntRange(30, 90);

        [Header("Performance Penalties (multipliers)")]
        [Tooltip("Final performance multiplier when fatigue is 51-70 (e.g., 0.95 = -5%)")]
        [Range(0.1f, 1f)] public float fatiguePenalty51_70 = 0.95f;
        [Range(0.1f, 1f)] public float fatiguePenalty71_85 = 0.90f;
        [Range(0.1f, 1f)] public float fatiguePenalty86_100 = 0.80f;

        [Header("Injury Stat Penalties (multipliers)")]
        [Range(0.1f, 1f)] public float minorInjuryPenalty = 0.95f;
        [Range(0.1f, 1f)] public float moderateInjuryPenalty = 0.85f;
        [Range(0.1f, 1f)] public float severeInjuryPenalty = 0.0f; // cannot play

        [Header("Daily Fatigue Recovery")]
        public float baseDailyFatigueRecovery = 8f;

        [Header("Recovery Plan Bonuses")]
        public float lightRehabExtraRecovery = 2f;
        public float advancedPhysioExtraRecovery = 4f;

        [Tooltip("Injury duration multiplier for Advanced Physio (e.g., 0.70 = -30% days)")]
        [Range(0.1f, 1f)] public float advancedPhysioInjuryDurationMult = 0.70f;

        [Tooltip("Injury duration multiplier for Surgery (only Severe recommended; e.g., 0.60 = -40% days)")]
        [Range(0.1f, 1f)] public float surgeryInjuryDurationMult = 0.60f;

        [Header("Stacking Cap")]
        [Tooltip("Max total reduction from certifications/bonuses combined (e.g., 0.40 = 40%)")]
        [Range(0f, 0.9f)] public float maxTotalReduction = 0.40f;

        [Serializable]
        public struct IntRange
        {
            public int min;
            public int max;

            public IntRange(int min, int max)
            {
                this.min = min;
                this.max = max;
            }

            public int Roll(System.Random rng)
            {
                if (max < min) (min, max) = (max, min);
                return rng.Next(min, max + 1);
            }
        }

        public float GetBaseFatigueGain(ActivityType t)
        {
            return t switch
            {
                ActivityType.LightTraining => lightTrainingFatigue,
                ActivityType.NormalTraining => normalTrainingFatigue,
                ActivityType.IntenseTraining => intenseTrainingFatigue,
                ActivityType.MatchShort => matchShortFatigue,
                ActivityType.MatchLong => matchLongFatigue,
                ActivityType.TournamentDay => tournamentDayFatigue,
                _ => 0f
            };
        }

        public float GetActivityRiskFactor(ActivityType t)
        {
            return t switch
            {
                ActivityType.LightTraining => trainingRiskFactor,
                ActivityType.NormalTraining => trainingRiskFactor,
                ActivityType.IntenseTraining => trainingRiskFactor,
                ActivityType.MatchShort => matchRiskFactor,
                ActivityType.MatchLong => matchRiskFactor,
                ActivityType.TournamentDay => tournamentRiskFactor,
                _ => 0f
            };
        }
    }
}
