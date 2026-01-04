using UnityEngine;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems;

namespace TennisAcademyManager.UI
{
    public class BuildCourtUI : MonoBehaviour
    {
        [SerializeField] private CourtDatabase courtDatabase;

        private EconomyService economy;
        private ReputationService reputation;
        private ConstructionService construction;

        private bool bound;

        private void OnEnable()
        {
            if (GameRoot.ServicesReady) Bind();
            else GameRoot.OnServicesReady += Bind;
        }

        private void OnDisable()
        {
            GameRoot.OnServicesReady -= Bind;
        }

        private void Bind()
        {
            if (bound) return;
            bound = true;

            GameRoot.OnServicesReady -= Bind;

            economy = ServiceLocator.Get<EconomyService>();
            reputation = ServiceLocator.Get<ReputationService>();
            construction = ServiceLocator.Get<ConstructionService>();
        }

        public void BuildRoadCourt() => Build(CourtType.RoadCourt);
        public void BuildBasicHard() => Build(CourtType.BasicHard);
        public void BuildClay() => Build(CourtType.Clay);
        public void BuildFastHard() => Build(CourtType.FastHard);
        public void BuildSlowHard() => Build(CourtType.SlowHard);
        public void BuildCarpet() => Build(CourtType.Carpet);
        public void BuildGrass() => Build(CourtType.Grass);
        public void BuildSyntheticATP() => Build(CourtType.SyntheticATP);

        private void Build(CourtType type)
        {
            if (!bound)
            {
                Debug.LogWarning("[BuildCourtUI] Not bound yet.");
                return;
            }

            var def = courtDatabase.Courts.Find(c => c.Type == type);
            if (def == null)
            {
                Debug.LogError($"[BuildCourtUI] Missing definition for {type}");
                return;
            }

            bool success = construction.BuildCourt(def, economy, reputation);

            Debug.Log(success
                ? $"[UI] Build success: {def.DisplayName}"
                : $"[UI] Not enough cash to build: {def.DisplayName}");
        }
    }
}
