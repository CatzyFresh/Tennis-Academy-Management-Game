using System;
using System.Collections.Generic;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems.Players;
using TennisAcademyManager.Systems.City;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class DemandService : IGameService
    {
        private DemandConfig config;

        private readonly Dictionary<PlayerSegment, DemandFunnelSnapshot> funnel = new();

        public event Action OnDemandUpdated;

        public void Initialize()
        {
            foreach (PlayerSegment s in Enum.GetValues(typeof(PlayerSegment)))
                funnel[s] = new DemandFunnelSnapshot();

            Debug.Log("[DemandService] Initialized");
        }

        public void SetConfig(DemandConfig demandConfig)
        {
            config = demandConfig;

            if (config == null)
                Debug.LogError("[DemandService] Missing DemandConfig! Assign it inside GameConfig.");
        }

        public DemandFunnelSnapshot Get(PlayerSegment segment) => funnel[segment];

        public int TotalActive()
        {
            int total = 0;
            foreach (var kv in funnel) total += kv.Value.Active;
            return total;
        }

        // =========================
        // Daily loop
        // =========================
        public void OnDayPassed(
            CalendarService calendar,
            ReputationService reputation,
            ConstructionService construction)
        {
            if (config == null) return;

            float seasonMult = GetSeasonMultiplier(calendar.CurrentSeason);

            int courts = construction.BuiltCourts.Count;
            float infraMult = 1f + Mathf.Clamp(courts, 0, 6) * 0.08f;

            float rep01 = Mathf.Clamp01(reputation.GlobalReputation / 100f);
            float repMult = Mathf.Lerp(0.75f, 1.35f, rep01);

            var city = ServiceLocator.Get<CityService>();
            float cityDemandMult = city != null ? city.DemandMult : 1f;

            // Competition pressure slightly reduces conversions unless reputation is strong
            // We'll apply this pressure later in ConvertTrialToEnroll + ApplyRetention.
            float expected = config.baseInquiriesPerDay * seasonMult * infraMult * repMult * cityDemandMult;


            int inquiriesToday = SamplePoissonLike(expected);

            AllocateInquiries(inquiriesToday);

            ConvertInquiryToTrial(reputation);
            ConvertTrialToEnroll(reputation); // <-- now spawns players too

            OnDemandUpdated?.Invoke();
        }

        // =========================
        // Monthly loop
        // =========================
        public void OnMonthPassed(
            EconomyService economy,
            ReputationService reputation)
        {
            if (config == null) return;

            int income = 0;
            income += funnel[PlayerSegment.HobbyKids].Active * config.feeHobbyKids;
            income += funnel[PlayerSegment.CompetitiveJuniors].Active * config.feeCompetitiveJuniors;
            income += funnel[PlayerSegment.EliteProspects].Active * config.feeEliteProspects;
            income += funnel[PlayerSegment.Adults].Active * config.feeAdults;

            if (income > 0)
            {
                economy.AddLedgerEntry(
                    LedgerEntryType.Income,
                    LedgerCategory.PlayerFees,
                    income,
                    "Monthly player fees"
                );
            }

            ApplyRetention(reputation); // <-- now releases players too

            OnDemandUpdated?.Invoke();
        }

        // -------------------------
        // Helpers
        // -------------------------
        private float GetSeasonMultiplier(SeasonPhase phase)
        {
            return phase switch
            {
                SeasonPhase.PreSeason => config.preSeasonMult,
                SeasonPhase.Competition => config.competitionMult,
                SeasonPhase.Monsoon => config.monsoonMult,
                SeasonPhase.Peak => config.peakMult,
                _ => config.offSeasonMult
            };
        }

        private void AllocateInquiries(int inquiries)
        {
            int hobby = Mathf.RoundToInt(inquiries * config.hobbyKidsShare);
            int comp = Mathf.RoundToInt(inquiries * config.competitiveJuniorsShare);
            int elite = Mathf.RoundToInt(inquiries * config.eliteProspectsShare);
            int adults = Mathf.Max(0, inquiries - (hobby + comp + elite));

            funnel[PlayerSegment.HobbyKids].Inquiries += hobby;
            funnel[PlayerSegment.CompetitiveJuniors].Inquiries += comp;
            funnel[PlayerSegment.EliteProspects].Inquiries += elite;
            funnel[PlayerSegment.Adults].Inquiries += adults;
        }

        private void ConvertInquiryToTrial(ReputationService reputation)
        {
            float rep01 = Mathf.Clamp01(reputation.GlobalReputation / 100f);
            float mult = Mathf.Lerp(0.85f, 1.20f, rep01);

            foreach (var kv in funnel)
            {
                var snap = kv.Value;
                if (snap.Inquiries <= 0) continue;

                float p = Mathf.Clamp01(config.inquiryToTrial * mult);
                int moved = Binomial(snap.Inquiries, p);
                snap.Inquiries -= moved;
                snap.Trials += moved;
            }
        }

        private void ConvertTrialToEnroll(ReputationService reputation)
        {
            var pricing = ServiceLocator.Get<PricingService>();
            var players = ServiceLocator.Get<PlayerService>(); 
            var city = ServiceLocator.Get<CityService>();

            float rep01 = Mathf.Clamp01(reputation.GlobalReputation / 100f);
            float repMult = Mathf.Lerp(0.80f, 1.25f, rep01);

            foreach (var kv in funnel)
            {
                var segment = kv.Key;
                var snap = kv.Value;

                if (snap.Trials <= 0)
                    continue;

                float priceMult = pricing != null
                    ? pricing.GetEnrollmentMultiplier(segment)
                    : 1f;

                float finalProbability =
                    Mathf.Clamp01(config.trialToEnroll * repMult * priceMult);

                float competition = city != null ? city.CompetitionPressure : 1f;
               
                // Better rep cancels pressure a bit
                float pressureMult = Mathf.Lerp(1f / competition, 1f, rep01);
                finalProbability *= pressureMult;
                finalProbability = Mathf.Clamp01(finalProbability);


                int moved = Binomial(snap.Trials, finalProbability);

                snap.Trials -= moved;

                // Funnel count (existing behavior)
                snap.Active += moved;

                // NEW: spawn real player instances into roster
                players?.EnrollGeneratedPlayers(segment, moved);
            }
        }

        private void ApplyRetention(ReputationService reputation)
        {
            var pricing = ServiceLocator.Get<PricingService>();
            var players = ServiceLocator.Get<PlayerService>();
            var city = ServiceLocator.Get<CityService>();

            float rep01 = Mathf.Clamp01(reputation.GlobalReputation / 100f);
            float repMult = Mathf.Lerp(0.92f, 1.06f, rep01);

            foreach (var kv in funnel)
            {
                var segment = kv.Key;
                var snap = kv.Value;

                if (snap.Active <= 0)
                    continue;

                float priceMult = pricing != null
                    ? pricing.GetRetentionMultiplier(segment)
                    : 1f;

                float finalRetention =
                    Mathf.Clamp01(config.monthlyRetention * repMult * priceMult);

                float competition = city != null ? city.CompetitionPressure : 1f;

                // Better rep cancels pressure a bit
                float pressureMult = Mathf.Lerp(1f / competition, 1f, rep01);
                finalRetention *= pressureMult;
                finalRetention = Mathf.Clamp01(finalRetention);


                int retained = Binomial(snap.Active, finalRetention);
                int churned = snap.Active - retained;

                snap.Active = retained;

                // NEW: release churned players from roster
                players?.ReleaseActivePlayers(segment, churned);

                if (churned >= 5)
                {
                    Debug.Log($"[Demand] Churn spike: {segment} churned {churned} players");
                }
            }
        }

        private int Binomial(int n, float p)
        {
            int x = 0;
            for (int i = 0; i < n; i++)
                if (UnityEngine.Random.value < p) x++;
            return x;
        }

        private int SamplePoissonLike(float lambda)
        {
            int baseVal = Mathf.FloorToInt(lambda);
            float frac = lambda - baseVal;
            return baseVal + (UnityEngine.Random.value < frac ? 1 : 0);
        }
    }
}
