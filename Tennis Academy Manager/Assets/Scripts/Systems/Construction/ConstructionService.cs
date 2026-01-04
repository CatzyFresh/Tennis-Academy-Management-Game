using System;
using System.Collections.Generic;
using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class ConstructionService : IGameService
    {
        private readonly List<CourtInstance> builtCourts = new();

        public IReadOnlyList<CourtInstance> BuiltCourts => builtCourts;

        public event Action<CourtInstance> OnCourtBuilt;

        public void Initialize()
        {
            Debug.Log("[ConstructionService] Initialized");
        }

        public bool CanBuild(CourtDefinition def, EconomyService economy)
        {
            return economy.CanAffordNow(def.BuildCost);
        }

        public bool BuildCourt(CourtDefinition def, EconomyService economy, ReputationService reputation)
        {
            if (!CanBuild(def, economy)) return false;

            if (!economy.TrySpendNow(def.BuildCost, $"Built {def.DisplayName}"))
                return false;

            var instance = new CourtInstance(def);
            builtCourts.Add(instance);

            reputation.Add(ReputationComponent.Infrastructure, +3, $"Built {def.DisplayName}");

            Debug.Log($"[Construction] Court built: {def.DisplayName}");
            OnCourtBuilt?.Invoke(instance);
            return true;
        }


        public void ApplyMonthlyMaintenance(EconomyService economy)
        {
            int total = 0;
            foreach (var c in builtCourts)
                total += c.MonthlyMaintenance;

            if (total <= 0) return;

            economy.AddLedgerEntry(
                LedgerEntryType.Expense,
                LedgerCategory.Maintenance,
                total,
                "Court maintenance"
            );

            Debug.Log($"[Construction] Maintenance applied: ₹{total}");
        }
    }
}
