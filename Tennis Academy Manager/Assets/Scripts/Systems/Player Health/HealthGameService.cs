using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems.Health
{
    public sealed class HealthGameService : IGameService
    {
        public HealthService Health { get; private set; }

        private HealthTuningSO tuning;
        private System.Random rng;

        public HealthGameService(HealthTuningSO tuning)
        {
            this.tuning = tuning;
        }

        public void Initialize()
        {
            if (tuning == null)
                Debug.LogError("[HealthGameService] Missing HealthTuningSO reference!");

            rng = new System.Random();
            Health = new HealthService(tuning, rng);

            Debug.Log("[HealthGameService] Initialized");
        }
    }
}
