using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TennisAcademyManager.Systems.City;
using TennisAcademyManager.UI.Common;

namespace TennisAcademyManager.UI.CitySelection
{
    public class CityOptionCardView : HoverableCardBase
    {
        [Header("UI")]
        [SerializeField] private TMP_Text cityNameText;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Button selectButton;

        [Header("Traits (Optional)")]
        [Tooltip("Assign the TMP for the first trait pill/label (e.g., High Budget).")]
        [SerializeField] private TMP_Text traitAText;

        [Tooltip("Assign the TMP for the second trait pill/label (e.g., Large Population).")]
        [SerializeField] private TMP_Text traitBText;

        [Header("Glow (UIFX)")]
        [Tooltip("Drag the UIFX Glow component here (whatever component you added).")]
        [SerializeField] private Component glowComponent;

        [Tooltip("Name of the float property to drive glow (common: 'energy', 'intensity', 'Strength').")]
        [SerializeField] private string glowFloatPropertyName = "ExpFalloffEnergy"; 

        [Header("Glow Values")]
        [SerializeField] private float glowIdle = 0f;
        [SerializeField] private float glowHover = 0.8f;
        [SerializeField] private float glowSelected = 1.2f;

        private CityConfigSO _config;
        private bool _isSelected;

        private Action<CityConfigSO> _onHoverEnter;
        private Action<CityConfigSO> _onHoverExit;
        private Action<CityConfigSO> _onSelectPressed;

        private PropertyInfo _glowProp;

        public CityConfigSO Config => _config;

        public void Bind(
            CityConfigSO config,
            CityTraits traits,
            Action<CityConfigSO> onHoverEnter,
            Action<CityConfigSO> onHoverExit,
            Action<CityConfigSO> onSelectPressed)
        {
            _config = config;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;
            _onSelectPressed = onSelectPressed;

            // Title
            if (cityNameText != null)
                cityNameText.text = GetDisplayName(config);

            // Traits (optional)
            if (traitAText != null) traitAText.text = traits.traitA;
            if (traitBText != null) traitBText.text = traits.traitB;

            // Select button
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() =>
                {
                    if (_config != null)
                        _onSelectPressed?.Invoke(_config);
                });
            }

            CacheGlowProperty();
            SetSelected(false);
            ApplyGlow(glowIdle);
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            ApplyGlow(_isSelected ? glowSelected : glowIdle);
        }

        protected override void OnHoverChanged(bool isHovering)
        {
            if (_config == null) return;

            if (isHovering)
            {
                // Hover glow overrides idle (but not selected)
                if (!_isSelected) ApplyGlow(glowHover);
                _onHoverEnter?.Invoke(_config);
            }
            else
            {
                // Return to selected or idle
                ApplyGlow(_isSelected ? glowSelected : glowIdle);
                _onHoverExit?.Invoke(_config);
            }
        }

        private void CacheGlowProperty()
        {
            if (glowComponent == null || string.IsNullOrWhiteSpace(glowFloatPropertyName))
            {
                _glowProp = null;
                return;
            }

            _glowProp = glowComponent.GetType().GetProperty(
                glowFloatPropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (_glowProp == null || _glowProp.PropertyType != typeof(float))
            {
                Debug.LogWarning(
                    $"[CityOptionCardView] Glow property '{glowFloatPropertyName}' not found (float) on {glowComponent.GetType().Name}. " +
                    $"Update 'glowFloatPropertyName' in Inspector.");
                _glowProp = null;
            }
        }

        private void ApplyGlow(float value)
        {
            if (glowComponent == null || _glowProp == null) return;

            try
            {
                _glowProp.SetValue(glowComponent, value);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CityOptionCardView] Failed setting glow: {e.Message}");
            }
        }

        private static string GetDisplayName(CityConfigSO cfg)
        {
            if (cfg == null) return "City";

            return cfg.cityType switch
            {
                CityType.Tier1Metro => "Metro",
                CityType.Tier2City => "City",
                CityType.Tier3Town => "Town",
                _ => cfg.cityType.ToString()
            };
        }
    }
}
