using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TennisAcademyManager.UI
{
    public class CoachHiringPanel : MonoBehaviour
    {
        [SerializeField] Transform contentRoot;
        [SerializeField] CoachOfferItemView itemPrefab;
        [SerializeField] TMP_Text feedbackText;

        [Header("Data")]
        [SerializeField] List<TennisAcademyManager.Systems.CoachOfferDefinition> offers;

        private TennisAcademyManager.Systems.CoachService coachService;
        private TennisAcademyManager.Systems.EconomyService economy;

        public void Init(
            TennisAcademyManager.Systems.CoachService coachService,
            TennisAcademyManager.Systems.EconomyService economy)
        {
            this.coachService = coachService;
            this.economy = economy;

            Refresh();
        }

        public void Refresh()
        {
            foreach (Transform child in contentRoot) Destroy(child.gameObject);

            foreach (var offer in offers)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                item.Bind(offer, OnHireClicked);
            }
        }

        private void OnHireClicked(TennisAcademyManager.Systems.CoachOfferDefinition offer)
        {
            bool ok = coachService.HireCoach(offer, economy);
            feedbackText.text = ok ? $"Hired {offer.CoachName}!" : "Hire failed (insufficient funds?)";
        }
    }
}
