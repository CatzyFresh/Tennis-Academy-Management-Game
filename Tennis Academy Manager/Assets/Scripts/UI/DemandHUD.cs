using TMPro;
using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class DemandHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text demandText;

        private DemandService demand;

        private void OnEnable()
        {
            // If boot already happened, bind immediately
            if (GameRoot.ServicesReady) Bind();
            else GameRoot.OnServicesReady += Bind;
        }

        private void OnDisable()
        {
            GameRoot.OnServicesReady -= Bind;

            if (demand != null)
                demand.OnDemandUpdated -= Refresh;
        }

        private void Bind()
        {
            // prevent double bind
            GameRoot.OnServicesReady -= Bind;

            demand = ServiceLocator.Get<DemandService>();
            demand.OnDemandUpdated += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            demandText.text =
                "DEMAND\n" +
                $"TOTAL ACTIVE: {demand.TotalActive()}";
        }
    }
}
