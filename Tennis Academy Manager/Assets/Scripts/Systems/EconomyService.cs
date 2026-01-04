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

        // =========================
        // Immediate (paid NOW)
        // =========================
        public bool CanAffordNow(int amount) => Cash >= amount;

        public bool TrySpendNow(int amount, string reason = "")
        {
            if (amount <= 0) return true;
            if (Cash < amount) return false;

            Cash -= amount;
            Debug.Log($"[Economy] Spent now: ₹{amount} ({reason}) | Cash: ₹{Cash}");
            return true;
        }

        public void EarnNow(int amount, string reason = "")
        {
            if (amount <= 0) return;

            Cash += amount;
            Debug.Log($"[Economy] Earned now: ₹{amount} ({reason}) | Cash: ₹{Cash}");
        }

        // =========================
        // Ledger (paid at month close)
        // =========================
        public void AddLedgerEntry(
            LedgerEntryType type,
            LedgerCategory category,
            int amount,
            string description)
        {
            if (amount <= 0) return;
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
