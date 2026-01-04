using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    public enum ActivityType
    {
        LightTraining,
        NormalTraining,
        IntenseTraining,
        MatchShort,
        MatchLong,
        TournamentDay
    }

    public enum InjurySeverity
    {
        None = 0,
        Minor = 1,
        Moderate = 2,
        Severe = 3
    }

    public enum RecoveryPlan
    {
        FullRest,
        LightRehab,
        AdvancedPhysio,
        Surgery
    }

    [Serializable]
    public sealed class InjuryState
    {
        [SerializeField] private InjurySeverity severity = InjurySeverity.None;
        [SerializeField] private int remainingDays = 0;

        public InjurySeverity Severity => severity;
        public int RemainingDays => remainingDays;
        public bool IsInjured => severity != InjurySeverity.None && remainingDays > 0;

        public void Set(InjurySeverity newSeverity, int days)
        {
            severity = newSeverity;
            remainingDays = Mathf.Max(0, days);
        }

        public void Clear()
        {
            severity = InjurySeverity.None;
            remainingDays = 0;
        }

        public void ExtendDays(int extraDays)
        {
            if (!IsInjured) return;
            remainingDays = Mathf.Max(0, remainingDays + extraDays);
        }

        public void TickDay()
        {
            if (!IsInjured) return;
            remainingDays = Mathf.Max(0, remainingDays - 1);
            if (remainingDays == 0) Clear();
        }
    }

    [Serializable]
    public struct PlayerHealthSnapshot
    {
        public float fatigue;     // 0..100
        public float injuryRisk;  // 0..100
        public InjurySeverity injurySeverity;
        public int injuryDaysRemaining;
    }

    public readonly struct ActivityContext
    {
        public readonly ActivityType ActivityType;
        public readonly bool IsPlayingWhileInjured;

        public ActivityContext(ActivityType activityType, bool isPlayingWhileInjured)
        {
            ActivityType = activityType;
            IsPlayingWhileInjured = isPlayingWhileInjured;
        }
    }

    public readonly struct RecoveryContext
    {
        public readonly RecoveryPlan Plan;

        public RecoveryContext(RecoveryPlan plan)
        {
            Plan = plan;
        }
    }
}
