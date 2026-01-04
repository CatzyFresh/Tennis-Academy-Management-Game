using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class AcademyCoachesPanel : MonoBehaviour
    {
        [SerializeField] Transform contentRoot;
        [SerializeField] AcademyCoachItemView itemPrefab;
        [SerializeField] TMP_Text feedbackText;

        [Header("Certification Options (all)")]
        [SerializeField] List<CertificationDefinition> allCerts;

        private CoachService coachService;
        private CertificationService certService;
        private EconomyService economy;
        private ReputationService reputation;
        private PricingService pricing;

        public void Init(
            CoachService coachService,
            CertificationService certService,
            EconomyService economy,
            ReputationService reputation,
            PricingService pricing)
        {
            this.coachService = coachService;
            this.certService = certService;
            this.economy = economy;
            this.reputation = reputation;
            this.pricing = pricing;

            coachService.OnRosterChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (coachService != null) coachService.OnRosterChanged -= Refresh;
        }

        public void Refresh()
        {
            foreach (Transform child in contentRoot) Destroy(child.gameObject);

            foreach (var coach in coachService.Coaches)
            {
                var certOptionsForRole = GetCertOptionsFor(coach);

                var item = Instantiate(itemPrefab, contentRoot);
                item.Bind(
                    coach,
                    certOptionsForRole,
                    onFire: OnFire,
                    onRaise: OnRaise,
                    onSendCert: OnSendCertification
                );
            }
        }

        private List<CertificationDefinition> GetCertOptionsFor(Coach coach)
        {
            var list = new List<CertificationDefinition>();
            foreach (var c in allCerts)
            {
                if (c == null) continue;
                // Optional: gate by prerequisite is handled in CertificationService anyway
                list.Add(c);
            }
            return list;
        }

        private void OnFire(Coach coach)
        {
            bool ok = coachService.FireCoach(coach);
            feedbackText.text = ok ? "Coach fired." : "Cannot fire (maybe in certification).";
        }

        private void OnRaise(Coach coach, int newSalary)
        {
            bool ok = coachService.GiveRaise(coach, newSalary);
            feedbackText.text = ok ? "Raise applied." : "Raise failed.";
        }

        private void OnSendCertification(Coach coach, CertificationDefinition cert)
        {
            bool ok = certService.StartCertification(coach, cert, economy);
            feedbackText.text = ok ? $"Sent for {cert.DisplayName}" : "Certification start failed (prereq/cost/in progress).";
        }

        // You’ll call this from your month system:
        public void OnMonthPassed()
        {
            foreach (var coach in coachService.Coaches)
                certService.OnMonthPassed(coach, reputation, pricing);

            Refresh();
        }
    }
}
