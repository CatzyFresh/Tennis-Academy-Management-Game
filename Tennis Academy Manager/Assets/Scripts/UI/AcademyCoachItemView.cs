using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TennisAcademyManager.Systems;
using System.Collections.Generic;

namespace TennisAcademyManager.UI
{
    public class AcademyCoachItemView : MonoBehaviour
    {
        [SerializeField] TMP_Text roleText;
        [SerializeField] TMP_Text salaryText;
        [SerializeField] TMP_Text certsText;
        [SerializeField] TMP_Text statusText;

        [SerializeField] Button fireButton;

        [Header("Raise")]
        [SerializeField] TMP_InputField raiseInput;
        [SerializeField] Button applyRaiseButton;

        [Header("Certification")]
        [SerializeField] TMP_Dropdown certDropdown;
        [SerializeField] Button sendCertButton;

        private Coach coach;

        public void Bind(
            Coach coach,
            List<CertificationDefinition> certOptions,
            System.Action<Coach> onFire,
            System.Action<Coach, int> onRaise,
            System.Action<Coach, CertificationDefinition> onSendCert)
        {
            this.coach = coach;

            roleText.text = coach.Role.ToString();
            salaryText.text = $"₹{coach.MonthlySalary}/mo";
            certsText.text = coach.Certifications.Count > 0 ? string.Join(", ", coach.Certifications) : "None";
            statusText.text = coach.IsInCertification
                ? $"In Cert ({coach.CertificationMonthsRemaining} mo)"
                : "Active";

            fireButton.onClick.RemoveAllListeners();
            fireButton.onClick.AddListener(() => onFire?.Invoke(this.coach));

            applyRaiseButton.onClick.RemoveAllListeners();
            applyRaiseButton.onClick.AddListener(() =>
            {
                if (int.TryParse(raiseInput.text, out var newSalary))
                    onRaise?.Invoke(this.coach, newSalary);
            });

            certDropdown.ClearOptions();
            var names = new System.Collections.Generic.List<string>();
            foreach (var c in certOptions) names.Add(c.DisplayName);
            if (names.Count == 0) names.Add("No certs");
            certDropdown.AddOptions(names);

            sendCertButton.onClick.RemoveAllListeners();
            sendCertButton.onClick.AddListener(() =>
            {
                if (certOptions.Count == 0) return;
                var chosen = certOptions[certDropdown.value];
                onSendCert?.Invoke(this.coach, chosen);
            });
        }
    }
}
