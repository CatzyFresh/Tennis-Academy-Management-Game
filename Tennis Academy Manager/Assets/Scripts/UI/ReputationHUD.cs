using TMPro;
using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class ReputationHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text repText;

        private ReputationService reputation;
        private bool bound;

        private void OnEnable()
        {
            if (GameRoot.ServicesReady) Bind();
            else GameRoot.OnServicesReady += Bind;
        }

        private void OnDisable()
        {
            GameRoot.OnServicesReady -= Bind;

            if (reputation != null)
                reputation.OnReputationChanged -= OnReputationChanged;
        }

        private void Bind()
        {
            if (bound) return;
            bound = true;

            GameRoot.OnServicesReady -= Bind;

            reputation = ServiceLocator.Get<ReputationService>();
            reputation.OnReputationChanged += OnReputationChanged;

            Refresh();
        }

        private void OnReputationChanged(ReputationComponent component, int delta, string reason)
        {
            // We don't need the details for the HUD yet, but we keep them for future debugging.
            Refresh();
        }

        private void Refresh()
        {
            repText.text =
                "REPUTATION\n" +
                $"Global: {reputation.GlobalReputation}\n" +
                $"Infrastructure: {reputation.Get(ReputationComponent.Infrastructure)}\n" +
                $"Discipline: {reputation.Get(ReputationComponent.Discipline)}\n" +
                $"Care: {reputation.Get(ReputationComponent.Care)}\n" +
                $"Coaching: {reputation.Get(ReputationComponent.Coaching)}";
        }
    }
}
