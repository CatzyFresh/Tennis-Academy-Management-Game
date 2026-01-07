using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TennisAcademyManager.UI.Common
{
    /// <summary>
    /// Reusable base for any hoverable UI card (scale + optional glow hook).
    /// Requires an EventSystem + GraphicRaycaster on the Canvas.
    /// </summary>
    public abstract class HoverableCardBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Animation")]
        [SerializeField] private RectTransform target;
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private Tween _scaleTween;
        private Vector3 _defaultScale;

        protected virtual void Awake()
        {
            if (target == null) target = transform as RectTransform;
            _defaultScale = target.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayHover(true);
            OnHoverChanged(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayHover(false);
            OnHoverChanged(false);
        }

        private void PlayHover(bool hover)
        {
            if (target == null) return;

            _scaleTween?.Kill();
            var to = hover ? _defaultScale * hoverScale : _defaultScale;
            _scaleTween = target.DOScale(to, duration).SetEase(ease).SetUpdate(true);
        }

        protected abstract void OnHoverChanged(bool isHovering);

        protected virtual void OnDisable()
        {
            _scaleTween?.Kill();
            if (target != null) target.localScale = _defaultScale;
        }
    }
}
