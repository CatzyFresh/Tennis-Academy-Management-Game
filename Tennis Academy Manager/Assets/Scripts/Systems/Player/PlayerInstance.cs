using System;
using UnityEngine;
using TennisAcademyManager.Systems.Health;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.Systems.Players
{
    [Serializable]
    public sealed class PlayerInstance
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int age;

        [SerializeField] private PlayerSegment segment;
        [SerializeField] private PlayerTraitFlags traits;
        [SerializeField] private PlayerStats stats;

        [SerializeField] private PlayerStatus status;

        [SerializeField] private PlayerHealthComponent health = new PlayerHealthComponent();

        public string Id => id;
        public string DisplayName => displayName;
        public int Age => age;
        public PlayerSegment Segment => segment;
        public PlayerTraitFlags Traits => traits;
        public PlayerStats Stats => stats;
        public PlayerStatus Status => status;
        public PlayerHealthComponent Health => health;

        public PlayerInstance(string id, string displayName, int age, PlayerSegment segment, PlayerTraitFlags traits, PlayerStats stats)
        {
            this.id = id;
            this.displayName = displayName;
            this.age = age;
            this.segment = segment;
            this.traits = traits;
            this.stats = stats;
            status = PlayerStatus.Prospect;
        }

        public void SetStatus(PlayerStatus newStatus) => status = newStatus;

        public void InitializeHealth(float startingFatigue, float startingRisk)
        {
            health.SetFatigue(startingFatigue);
            health.SetRisk(startingRisk);
        }
    }
}
