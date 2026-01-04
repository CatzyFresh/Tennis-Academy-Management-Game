using System;
using System.Collections.Generic;
using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class PricingService : IGameService
    {
        private readonly Dictionary<PlayerSegment, PricingProfile> pricing = new();

        public event Action<PlayerSegment> OnPricingChanged;

        public void Initialize()
        {
            Debug.Log("[PricingService] Initialized");
        }

        // ✅ Inject base fees + optional limits
        public void SetBaseFeesFromDemandConfig(DemandConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[PricingService] DemandConfig is NULL!");
                return;
            }

            pricing.Clear();

            pricing[PlayerSegment.HobbyKids] = new PricingProfile(config.feeHobbyKids);
            pricing[PlayerSegment.CompetitiveJuniors] = new PricingProfile(config.feeCompetitiveJuniors);
            pricing[PlayerSegment.EliteProspects] = new PricingProfile(config.feeEliteProspects);
            pricing[PlayerSegment.Adults] = new PricingProfile(config.feeAdults);

            Debug.Log("[PricingService] Base fees loaded from DemandConfig");
        }

        public PricingProfile Get(PlayerSegment segment) => pricing[segment];

        // -----------------------
        // Recommendation Logic
        // -----------------------
        public void RecalculateRecommendations(
            ReputationService reputation,
            ConstructionService construction)
        {
            if (pricing.Count == 0) return; // config not injected yet

            float rep01 = reputation.GlobalReputation / 100f;
            float repMult = Mathf.Lerp(0.9f, 1.25f, rep01);

            float infraMult = 1f + Mathf.Clamp(construction.BuiltCourts.Count, 0, 6) * 0.05f;

            foreach (var kv in pricing)
            {
                var p = kv.Value;
                p.RecommendedPrice = Mathf.RoundToInt(p.BasePrice * repMult * infraMult);

                // Clamp current price inside new bounds
                if (!p.CanSetPrice(p.CurrentPrice))
                    p.CurrentPrice = p.RecommendedPrice;

                OnPricingChanged?.Invoke(kv.Key);
            }
        }

        // -----------------------
        // Manual Override
        // -----------------------
        public bool TrySetPrice(PlayerSegment segment, int newPrice)
        {
            if (!pricing.ContainsKey(segment)) return false;

            var p = pricing[segment];
            if (!p.CanSetPrice(newPrice)) return false;

            p.CurrentPrice = newPrice;
            OnPricingChanged?.Invoke(segment);
            return true;
        }

        // -----------------------
        // Demand multipliers
        // -----------------------
        public float GetEnrollmentMultiplier(PlayerSegment segment)
        {
            if (!pricing.ContainsKey(segment)) return 1f;

            var p = pricing[segment];
            float ratio = (float)p.CurrentPrice / p.RecommendedPrice;

            if (ratio > 1f)
                return Mathf.Lerp(1f, 0.6f, Mathf.Clamp01((ratio - 1f) / 0.2f));

            return Mathf.Lerp(1f, 1.1f, Mathf.Clamp01((1f - ratio) / 0.2f));
        }

        public float GetRetentionMultiplier(PlayerSegment segment)
        {
            if (!pricing.ContainsKey(segment)) return 1f;

            var p = pricing[segment];
            float ratio = (float)p.CurrentPrice / p.RecommendedPrice;

            if (ratio > 1f)
                return Mathf.Lerp(1f, 0.7f, Mathf.Clamp01((ratio - 1f) / 0.2f));

            return 1f;
        }

        public float GetLoadPressureMultiplier(PlayerSegment segment)
        {
            if (!pricing.ContainsKey(segment)) return 1f;

            var p = pricing[segment];
            float ratio = (float)p.CurrentPrice / p.RecommendedPrice;

            if (ratio < 1f)
                return Mathf.Lerp(1f, 1.3f, Mathf.Clamp01((1f - ratio) / 0.2f));

            return 1f;
        }

        // -----------------------
        // Reputation hook
        // -----------------------
        public float GetOverpricingRatio(PlayerSegment segment)
        {
            if (!pricing.ContainsKey(segment)) return 1f;

            var p = pricing[segment];
            return (float)p.CurrentPrice / p.RecommendedPrice;
        }

        public int GetCarePenalty(PlayerSegment segment)
        {
            float ratio = GetOverpricingRatio(segment);

            if (ratio > 1.20f) return 2;
            if (ratio > 1.10f) return 1;
            return 0;
        }

        // -----------------------
        // ROI hook (Certification)
        // -----------------------
        public void ApplyPricingToleranceBonus(float bonusPct)
        {
            foreach (var kv in pricing)
                kv.Value.MaxIncreasePct += bonusPct;

            Debug.Log($"[Pricing] Pricing tolerance increased by {bonusPct * 100f}%");
        }
    }
}
