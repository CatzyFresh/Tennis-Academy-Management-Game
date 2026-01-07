using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TennisAcademyManager.Core;
using TennisAcademyManager.Systems.City;
using TennisAcademyManager.UI.Common;

namespace TennisAcademyManager.UI.CitySelection
{
    public class CitySelectionScreen : MonoBehaviour
    {
        [Header("Cards (3)")]
        [SerializeField] private CityOptionCardView metroCard;
        [SerializeField] private CityOptionCardView cityCard;
        [SerializeField] private CityOptionCardView townCard;

        [Header("Configs (3)")]
        [SerializeField] private CityConfigSO tier1MetroConfig;
        [SerializeField] private CityConfigSO tier2CityConfig;
        [SerializeField] private CityConfigSO tier3TownConfig;

        [Header("Background Preview (behind cards)")]
        [Tooltip("Drag 'City Image Background' Image here.")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite metroBackground;
        [SerializeField] private Sprite cityBackground;
        [SerializeField] private Sprite townBackground;

        [Header("Popup")]
        [SerializeField] private ConfirmationPopup confirmationPopup;

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        

        // What user clicked Select on (pending confirm)
        private CityConfigSO _pendingConfirm;

        // What is actually confirmed/locked in (so we can revert visuals on cancel)
        private CityConfigSO _confirmedSelection;

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
            GameRoot.OnServicesReady -= Bind;

            if (metroCard == null || cityCard == null || townCard == null)
            {
                Debug.LogError("[CitySelectionScreen] Card references missing.");
                return;
            }

            if (tier1MetroConfig == null || tier2CityConfig == null || tier3TownConfig == null)
            {
                Debug.LogError("[CitySelectionScreen] CityConfigSO references missing.");
                return;
            }

            // Bind cards with hover + select callbacks
            metroCard.Bind(
                tier1MetroConfig,
                CityTraitCatalog.GetTraitsFor(CityType.Tier1Metro),
                OnCardHoverEnter,
                OnCardHoverExit,
                OnSelectPressed
            );

            cityCard.Bind(
                tier2CityConfig,
                CityTraitCatalog.GetTraitsFor(CityType.Tier2City),
                OnCardHoverEnter,
                OnCardHoverExit,
                OnSelectPressed
            );

            townCard.Bind(
                tier3TownConfig,
                CityTraitCatalog.GetTraitsFor(CityType.Tier3Town),
                OnCardHoverEnter,
                OnCardHoverExit,
                OnSelectPressed
            );

            // Back button => Main Menu
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackButtonClicked);
            }

            // Read current selection from service if already set (continue game / reload)
            //if (ServiceLocator.TryGet<CityService>(out var cityService) && cityService.Config != null)
            //{
            //    _confirmedSelection = cityService.Config;
            //    SetSelectedVisual(_confirmedSelection);
            //    PreviewBackgroundFor(_confirmedSelection);
            //}
            //else
            //{
            //    _confirmedSelection = null;
            //    SetSelectedVisual(null);
            //    PreviewBackgroundFor(tier1MetroConfig); // default background
            //}
        }

        private void OnCardHoverEnter(CityConfigSO cfg)
        {
            // Hover changes the background preview
            PreviewBackgroundFor(cfg);
        }

        private void OnCardHoverExit(CityConfigSO cfg)
        {
            // Optional: revert to confirmed background on hover exit
            // If you want to keep last hovered background, comment this out.
            //if (_confirmedSelection != null)
            //    PreviewBackgroundFor(_confirmedSelection);
        }

        private void OnSelectPressed(CityConfigSO cfg)
        {
            if (cfg == null) return;

            _pendingConfirm = cfg;

            // Optional: show "selected" glow while popup is open
            SetSelectedVisual(cfg);

            if (confirmationPopup == null)
            {
                Debug.LogError("[CitySelectionScreen] ConfirmationPopup reference missing.");
                return;
            }

            // On cancel/close -> revert glow back to confirmed state (or none)
            confirmationPopup.Show(
                onConfirm: ConfirmSelection,
                onCancel: CancelSelection
            );
        }

        private void CancelSelection()
        {
            _pendingConfirm = null;

            // Revert visuals so only 0 or the confirmed card is selected
            SetSelectedVisual(null);

            // Revert background too (optional)
            if (_confirmedSelection != null)
                PreviewBackgroundFor(_confirmedSelection);
        }

        private void ConfirmSelection()
        {
            if (_pendingConfirm == null)
            {
                Debug.LogWarning("[CitySelectionScreen] Confirm clicked but pending selection is null.");
                return;
            }

            var cityService = ServiceLocator.Get<CityService>();
            cityService.SetConfig(_pendingConfirm);

            if (ServiceLocator.TryGet<GameConfigService>(out var configService) && configService.Config != null)
                configService.Config.CityConfig = _pendingConfirm;

            _confirmedSelection = _pendingConfirm;
            _pendingConfirm = null;

            Debug.Log($"[CitySelectionScreen] City confirmed: {_confirmedSelection.cityType}");

            GameRoot.Instance.ChangeState<AcademyHubState>();
        }

        private void SetSelectedVisual(CityConfigSO cfg)
        {
            // If cfg is null => all go idle
            metroCard.SetSelected(cfg != null && cfg == tier1MetroConfig);
            cityCard.SetSelected(cfg != null && cfg == tier2CityConfig);
            townCard.SetSelected(cfg != null && cfg == tier3TownConfig);
        }

        private void PreviewBackgroundFor(CityConfigSO cfg)
        {
            if (backgroundImage == null || cfg == null) return;

            if (cfg == tier1MetroConfig && metroBackground != null) backgroundImage.sprite = metroBackground;
            else if (cfg == tier2CityConfig && cityBackground != null) backgroundImage.sprite = cityBackground;
            else if (cfg == tier3TownConfig && townBackground != null) backgroundImage.sprite = townBackground;
        }

        private void OnBackButtonClicked()
        {
            // Return to Main Menu state (your existing flow)
            gameObject.SetActive(false);

            if (GameRoot.Instance != null)
                GameRoot.Instance.ChangeState<MainMenuState>();
            else
                Debug.LogError("[CitySelectionScreen] GameRoot.Instance is null. Cannot go back to Main Menu.");
        }
    }
}
