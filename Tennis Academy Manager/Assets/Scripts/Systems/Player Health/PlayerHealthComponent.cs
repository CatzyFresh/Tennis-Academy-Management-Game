using System;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    [Serializable]
    public sealed class PlayerHealthComponent
    {
        [SerializeField, Range(0f, 100f)] private float fatigue = 0f;
        [SerializeField, Range(0f, 100f)] private float injuryRisk = 0f;
        [SerializeField] private InjuryState injury = new InjuryState();

        public float Fatigue => fatigue;
        public float InjuryRisk => injuryRisk;
        public InjuryState Injury => injury;

        public void SetFatigue(float value) => fatigue = Mathf.Clamp(value, 0f, 100f);
        public void SetRisk(float value) => injuryRisk = Mathf.Clamp(value, 0f, 100f);

        public PlayerHealthSnapshot Snapshot()
        {
            return new PlayerHealthSnapshot
            {
                fatigue = fatigue,
                injuryRisk = injuryRisk,
                injurySeverity = injury.Severity,
                injuryDaysRemaining = injury.RemainingDays
            };
        }
    }
}
