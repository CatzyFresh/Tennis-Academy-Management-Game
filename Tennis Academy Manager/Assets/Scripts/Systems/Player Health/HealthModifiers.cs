using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    [Serializable]
    public struct HealthModifiers
    {
        // reductions are expressed as 0..1 (0.10 = -10%)
        [Range(0f, 1f)] public float fatigueGainReduction;     // reduces fatigue gain
        [Range(0f, 1f)] public float injuryRiskReduction;      // reduces injury risk gain
        [Range(-50f, 50f)] public float dailyRecoveryBonus;    // added to daily recovery
        [Range(0.1f, 2f)] public float injuryDurationMult;     // multiplies injury duration (0.8 => -20%)

        public static HealthModifiers Default => new HealthModifiers
        {
            fatigueGainReduction = 0f,
            injuryRiskReduction = 0f,
            dailyRecoveryBonus = 0f,
            injuryDurationMult = 1f
        };

        public static HealthModifiers Combine(HealthModifiers a, HealthModifiers b, float maxReductionCap)
        {
            // cap reductions so they don't stack infinitely (FROZEN rule)
            float fatigueRed = Mathf.Min(a.fatigueGainReduction + b.fatigueGainReduction, maxReductionCap);
            float riskRed = Mathf.Min(a.injuryRiskReduction + b.injuryRiskReduction, maxReductionCap);

            return new HealthModifiers
            {
                fatigueGainReduction = fatigueRed,
                injuryRiskReduction = riskRed,
                dailyRecoveryBonus = a.dailyRecoveryBonus + b.dailyRecoveryBonus,
                injuryDurationMult = a.injuryDurationMult * b.injuryDurationMult
            };
        }
    }

    public interface IHealthModifierProvider
    {
        HealthModifiers GetHealthModifiers(); // from coaches, facilities, traits, etc.
    }
}
