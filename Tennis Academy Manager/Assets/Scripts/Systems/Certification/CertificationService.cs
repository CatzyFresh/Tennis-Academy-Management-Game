using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class CertificationService : IGameService
    {
        public void Initialize()
        {
            Debug.Log("[CertificationService] Initialized");
        }

        // =========================
        // Start Certification
        // =========================
        public bool StartCertification(
            Coach coach,
            CertificationDefinition cert,
            EconomyService economy)
        {
            if (coach.IsInCertification) return false;

            if (cert.Prerequisite.HasValue &&
                !coach.Certifications.Contains(cert.Prerequisite.Value))
                return false;

            if (!economy.TrySpendNow(cert.Cost, $"Certification: {cert.DisplayName}"))
                return false;

            coach.ActiveCertification = cert;
            coach.CertificationMonthsRemaining = cert.DurationMonths;

            Debug.Log($"[Certification] {cert.DisplayName} started");
            return true;
        }

        // =========================
        // Monthly Progress
        // =========================
        public void OnMonthPassed(
            Coach coach,
            ReputationService reputation,
            PricingService pricing)
        {
            if (!coach.IsInCertification) return;

            coach.CertificationMonthsRemaining--;

            if (coach.CertificationMonthsRemaining > 0)
                return;

            CompleteCertification(coach, reputation, pricing);
        }

        private void CompleteCertification(
            Coach coach,
            ReputationService reputation,
            PricingService pricing)
        {
            var cert = coach.ActiveCertification;

            coach.Certifications.Add(cert.Certification);
            coach.ActiveCertification = null;

            // Apply coaching reputation bonus
            if (cert.ReputationBonus > 0)
            {
                reputation.Add(
                    ReputationComponent.Coaching,
                    cert.ReputationBonus,
                    $"Certification completed: {cert.DisplayName}"
                );
            }

            // Pricing tolerance ROI
            pricing.ApplyPricingToleranceBonus(cert.PricingToleranceBonus);

            Debug.Log($"[Certification] Completed: {cert.DisplayName}");
        }
    }
}
