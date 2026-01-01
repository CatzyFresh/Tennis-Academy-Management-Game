using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class EconomyService : IGameService
    {
        public int Money { get; private set; } = 100_000;

        public void Initialize()
        {
            Debug.Log("[EconomyService] Initialized");
        }

        public bool CanAfford(int amount) => Money >= amount;

        public void Spend(int amount)
        {
            Money -= amount;
        }

        public void Earn(int amount)
        {
            Money += amount;
        }
    }
}
