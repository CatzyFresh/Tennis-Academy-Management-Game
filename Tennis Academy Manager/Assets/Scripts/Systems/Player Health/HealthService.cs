using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    public sealed class HealthService
    {
        public event Action<string, PlayerHealthSnapshot> OnHealthChanged; // playerId, snapshot
        public event Action<string, InjuryState> OnInjuryChanged;          // playerId, injury

        private readonly HealthTuningSO tuning;
        private readonly System.Random rng;

        public HealthService(HealthTuningSO tuning, System.Random rng)
        {
            this.tuning = tuning ? tuning : throw new ArgumentNullException(nameof(tuning));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// Call after any training/match/tournament day.
        /// This applies fatigue, risk, and performs injury check at the end.
        /// </summary>
        public void ApplyActivity(
            string playerId,
            PlayerHealthComponent health,
            ActivityContext ctx,
            HealthModifiers modifiers)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            modifiers = ClampWithCap(modifiers);

            // 1) Fatigue gain
            float baseFatigue = tuning.GetBaseFatigueGain(ctx.ActivityType);
            float fatigueMult = 1f - modifiers.fatigueGainReduction;
            float fatigueGain = baseFatigue * Mathf.Clamp01(fatigueMult);

            if (ctx.IsPlayingWhileInjured)
                fatigueGain *= (1f + tuning.playingInjuredFatigueExtraMult);

            health.SetFatigue(health.Fatigue + fatigueGain);

            // 2) Injury risk gain (only when fatigue above risk start)
            float activityRiskFactor = tuning.GetActivityRiskFactor(ctx.ActivityType);
            float fatigueAbove = Mathf.Max(0f, health.Fatigue - tuning.fatigueRiskStart);
            float riskGain = fatigueAbove * activityRiskFactor;

            // apply risk reduction (cap already enforced)
            riskGain *= (1f - modifiers.injuryRiskReduction);

            health.SetRisk(health.InjuryRisk + riskGain);

            // 3) Injury check at END of activity
            TryTriggerInjury(playerId, health, modifiers);

            RaiseChanged(playerId, health);
        }

        /// <summary>
        /// Call once per in-game day per player.
        /// Handles injury day countdown + fatigue recovery and optional recovery plans.
        /// </summary>
        public void TickDaily(
            string playerId,
            PlayerHealthComponent health,
            RecoveryContext recoveryCtx,
            HealthModifiers modifiers)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            modifiers = ClampWithCap(modifiers);

            // Injury countdown
            int beforeDays = health.Injury.RemainingDays;
            InjurySeverity beforeSeverity = health.Injury.Severity;
            health.Injury.TickDay();

            if (beforeSeverity != health.Injury.Severity || beforeDays != health.Injury.RemainingDays)
                OnInjuryChanged?.Invoke(playerId, health.Injury);

            // Fatigue recovery
            float recovery = tuning.baseDailyFatigueRecovery + modifiers.dailyRecoveryBonus;

            recovery += recoveryCtx.Plan switch
            {
                RecoveryPlan.FullRest => 0f,
                RecoveryPlan.LightRehab => tuning.lightRehabExtraRecovery,
                RecoveryPlan.AdvancedPhysio => tuning.advancedPhysioExtraRecovery,
                RecoveryPlan.Surgery => 0f,
                _ => 0f
            };

            health.SetFatigue(health.Fatigue - Mathf.Max(0f, recovery));

            // Risk gently falls with rest (simple & readable)
            // You can tune this later; keeps risk from staying forever.
            float riskDrop = 8f; // conservative default
            if (recoveryCtx.Plan == RecoveryPlan.AdvancedPhysio) riskDrop += 4f;
            health.SetRisk(health.InjuryRisk - riskDrop);

            RaiseChanged(playerId, health);
        }

        /// <summary>
        /// Apply a paid recovery action effect on current injury duration.
        /// You can call this when player purchases Advanced Physio / Surgery, etc.
        /// </summary>
        public void ApplyRecoveryAction(
            string playerId,
            PlayerHealthComponent health,
            RecoveryPlan plan,
            HealthModifiers modifiers)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            modifiers = ClampWithCap(modifiers);

            if (!health.Injury.IsInjured) return;

            float planMult = plan switch
            {
                RecoveryPlan.AdvancedPhysio => tuning.advancedPhysioInjuryDurationMult,
                RecoveryPlan.Surgery => tuning.surgeryInjuryDurationMult,
                _ => 1f
            };

            // combine plan multiplier with modifiers injuryDurationMult
            float finalMult = planMult * modifiers.injuryDurationMult;
            int newDays = Mathf.CeilToInt(health.Injury.RemainingDays * finalMult);

            // never reduce below 1 day if still injured
            newDays = Mathf.Max(1, newDays);

            health.Injury.Set(health.Injury.Severity, newDays);
            OnInjuryChanged?.Invoke(playerId, health.Injury);
            RaiseChanged(playerId, health);
        }

        public float GetPerformanceMultiplier(PlayerHealthComponent health)
        {
            if (health == null) return 1f;

            // injury gate
            if (health.Injury.IsInjured)
            {
                return health.Injury.Severity switch
                {
                    InjurySeverity.Minor => tuning.minorInjuryPenalty,
                    InjurySeverity.Moderate => tuning.moderateInjuryPenalty,
                    InjurySeverity.Severe => tuning.severeInjuryPenalty,
                    _ => 1f
                };
            }

            // fatigue penalty
            float f = health.Fatigue;
            if (f <= 50f) return 1f;
            if (f <= 70f) return tuning.fatiguePenalty51_70;
            if (f <= 85f) return tuning.fatiguePenalty71_85;
            return tuning.fatiguePenalty86_100;
        }

        private void TryTriggerInjury(string playerId, PlayerHealthComponent health, HealthModifiers modifiers)
        {
            // If already severe injured, don't re-trigger; we already have injury state
            if (health.Injury.IsInjured) return;

            // roll
            float risk = health.InjuryRisk;
            float roll = (float)(rng.NextDouble() * 100.0);

            if (roll > risk) return;

            // severity by risk at trigger (FROZEN)
            InjurySeverity sev =
                (risk <= tuning.minorMaxRisk) ? InjurySeverity.Minor :
                (risk <= tuning.moderateMaxRisk) ? InjurySeverity.Moderate :
                InjurySeverity.Severe;

            int days = RollInjuryDays(sev, modifiers);
            health.Injury.Set(sev, days);

            OnInjuryChanged?.Invoke(playerId, health.Injury);
        }

        private int RollInjuryDays(InjurySeverity severity, HealthModifiers modifiers)
        {
            int baseDays = severity switch
            {
                InjurySeverity.Minor => tuning.minorDays.Roll(rng),
                InjurySeverity.Moderate => tuning.moderateDays.Roll(rng),
                InjurySeverity.Severe => tuning.severeDays.Roll(rng),
                _ => 0
            };

            // apply duration mult from certifications/physio traits etc.
            float finalMult = Mathf.Clamp(modifiers.injuryDurationMult, 0.1f, 2f);
            int finalDays = Mathf.CeilToInt(baseDays * finalMult);
            return Mathf.Max(1, finalDays);
        }

        private HealthModifiers ClampWithCap(HealthModifiers m)
        {
            // enforce the global cap consistently
            m.fatigueGainReduction = Mathf.Clamp(m.fatigueGainReduction, 0f, tuning.maxTotalReduction);
            m.injuryRiskReduction = Mathf.Clamp(m.injuryRiskReduction, 0f, tuning.maxTotalReduction);
            m.injuryDurationMult = Mathf.Clamp(m.injuryDurationMult, 0.1f, 2f);
            return m;
        }

        private void RaiseChanged(string playerId, PlayerHealthComponent health)
        {
            OnHealthChanged?.Invoke(playerId, health.Snapshot());
        }
    }
}
