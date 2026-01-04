using System;
using System.Collections.Generic;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems.City;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class CoachService : IGameService
    {
        private readonly List<Coach> coaches = new();

        public IReadOnlyList<Coach> Coaches => coaches;

        public event Action OnRosterChanged;

        public void Initialize()
        {
            Debug.Log("[CoachService] Initialized");
            OnRosterChanged?.Invoke();
        }

        // =========================
        // Hiring (NEW overload)
        // =========================
        public bool HireCoach(CoachOfferDefinition offer, EconomyService economy)
        {
            if (offer == null) return false;

            if (!economy.TrySpendNow(offer.HiringCost, $"Hire Coach: {offer.CoachName}"))
                return false;

            var coach = new Coach(offer.Role, offer.ExpectedMonthlySalary, offer.Capacity);

            if (offer.StartingCertification.HasValue)
                coach.Certifications.Add(offer.StartingCertification.Value);

            coaches.Add(coach);

            Debug.Log($"[Coach] Hired {offer.CoachName} ({offer.Role})");
            OnRosterChanged?.Invoke();
            return true;
        }

        public bool HireCoach(CoachRole role, EconomyService economy)
        {
            var (salary, capacity) = GetDefaults(role);
            var coach = new Coach(role, salary, capacity);
            coaches.Add(coach);

            Debug.Log($"[Coach] Hired {role} | Salary ₹{salary} | Capacity {capacity}");
            OnRosterChanged?.Invoke();
            return true;
        }

        // =========================
        // Fire (NEW)
        // =========================
        public bool FireCoach(Coach coach)
        {
            if (coach == null) return false;
            if (coach.IsInCertification) return false; // rule: cannot fire while in cert

            bool removed = coaches.Remove(coach);
            if (removed) OnRosterChanged?.Invoke();
            return removed;
        }

        // =========================
        // Raise (NEW)
        // =========================
        public bool GiveRaise(Coach coach, int newMonthlySalary)
        {
            if (coach == null) return false;
            if (newMonthlySalary < 0) return false;

            coach.MonthlySalary = newMonthlySalary;
            OnRosterChanged?.Invoke();
            return true;
        }

        // =========================
        // Monthly Salary
        // =========================
        public void ApplyMonthlySalaries(EconomyService economy)
        {
            int baseTotal = 0;
            foreach (var c in coaches)
                baseTotal += c.MonthlySalary;

            if (baseTotal <= 0) return;

            // City salary multiplier applied exactly once here.
            var city = ServiceLocator.Get<CityService>();
            float s = city != null ? city.SalaryMult : 1f;

            int total = Mathf.RoundToInt(baseTotal * s);

            economy.AddLedgerEntry(
                LedgerEntryType.Expense,
                LedgerCategory.Salaries,
                total,
                "Coach salaries"
            );

            Debug.Log($"[Coach] Salaries applied: ₹{total} (base ₹{baseTotal}, city x{s:0.00})");
        }

        // =========================
        // Load Calculation
        // =========================
        public void RecalculateLoad(int totalActivePlayers, PricingService pricing)
        {
            int totalCapacity = 0;
            foreach (var c in coaches)
                totalCapacity += c.Capacity;

            if (totalCapacity <= 0)
            {
                foreach (var c in coaches)
                    c.CurrentLoad = totalActivePlayers;
                return;
            }

            float baseLoad = (float)totalActivePlayers / totalCapacity;

            foreach (var c in coaches)
            {
                float pricePressure = pricing.GetLoadPressureMultiplier(PlayerSegment.HobbyKids);
                c.CurrentLoad = Mathf.RoundToInt(c.Capacity * baseLoad * pricePressure);
            }
        }

        // =========================
        // Defaults (MVP)
        // =========================
        private (int salary, int capacity) GetDefaults(CoachRole role)
        {
            return role switch
            {
                CoachRole.HeadCoach => (30000, 20),
                CoachRole.AssistantCoach => (18000, 15),
                CoachRole.FitnessCoach => (20000, 25),
                CoachRole.MentalCoach => (15000, 30),
                CoachRole.Physio => (22000, 40),
                _ => (15000, 10)
            };
        }
    }
}
