using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TennisAcademyManager.UI
{
    public class CoachOfferItemView : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text roleText;
        [SerializeField] TMP_Text certText;
        [SerializeField] TMP_Text hireCostText;
        [SerializeField] TMP_Text salaryText;
        [SerializeField] Button hireButton;

        private TennisAcademyManager.Systems.CoachOfferDefinition offer;
        private System.Action<TennisAcademyManager.Systems.CoachOfferDefinition> onHire;

        public void Bind(
            TennisAcademyManager.Systems.CoachOfferDefinition offer,
            System.Action<TennisAcademyManager.Systems.CoachOfferDefinition> onHire)
        {
            this.offer = offer;
            this.onHire = onHire;

            nameText.text = offer.CoachName;
            roleText.text = offer.Role.ToString();
            certText.text = offer.StartingCertification.HasValue ? offer.StartingCertification.Value.ToString() : "None";
            hireCostText.text = $"Hire: ₹{offer.HiringCost}";
            salaryText.text = $"Salary: ₹{offer.ExpectedMonthlySalary}/mo";

            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() => this.onHire?.Invoke(this.offer));
        }
    }
}
