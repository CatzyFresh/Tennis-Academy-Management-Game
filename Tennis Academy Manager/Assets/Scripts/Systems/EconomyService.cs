using System.Collections.Generic;
using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class EconomyService : IGameService
    {
        public int Cash { get; private set; } = 100_000;

        private readonly List<LedgerEntry> monthlyLedger = new();

        public void Initialize()
        {
            Debug.Log("[EconomyService] Initialized");
        }

        public bool CanAfford(int amount) => Cash >= amount;

        public void Spend(int amount)
        {
            Cash -= amount;
        }

        public void Earn(int amount)
        {
            Cash += amount;
        }

        public void AddLedgerEntry(
            LedgerEntryType type,
            LedgerCategory category,
            int amount,
            string description)
        {
            monthlyLedger.Add(new LedgerEntry(type, category, amount, description));
        }

        public void RunMonthlyClose()
        {
            int net = 0;

            foreach (var entry in monthlyLedger)
            {
                net += entry.Type == LedgerEntryType.Income
                    ? entry.Amount
                    : -entry.Amount;
            }

            Cash += net;
            monthlyLedger.Clear();

            Debug.Log($"[Economy] Monthly close → Net: {net}, Cash: {Cash}");
        }
    }
}
