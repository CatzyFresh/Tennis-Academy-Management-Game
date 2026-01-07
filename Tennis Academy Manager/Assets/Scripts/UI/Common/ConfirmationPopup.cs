using System;
using UnityEngine;
using UnityEngine.UI;

namespace TennisAcademyManager.UI.Common
{
    public class ConfirmationPopup : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button closeButton;      // X button
        [SerializeField] private Button confirmButton;    // Confirm City button

        [Header("Optional")]
        [Tooltip("If you have a dark dimmer/backdrop assign it here.")]
        [SerializeField] private GameObject dimmer;

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            // Wire static close listeners once.
            if (closeButton != null)
                closeButton.onClick.AddListener(Cancel);
        }

        /// <summary>
        /// Shows the popup. Confirm calls onConfirm. Close/backdrop calls onCancel.
        /// </summary>
        public void Show(Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(Confirm);
            }

            gameObject.SetActive(true);
            dimmer.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            dimmer.SetActive(false);
            ClearCallbacks();
        }

        private void Confirm()
        {
            gameObject.SetActive(false);
            dimmer.SetActive(false);
            var cb = _onConfirm;
            ClearCallbacks();
            cb?.Invoke();
        }

        private void Cancel()
        {
            gameObject.SetActive(false);
            dimmer.SetActive(false);
            var cb = _onCancel;
            ClearCallbacks();
            cb?.Invoke();
        }

        private void ClearCallbacks()
        {
            _onConfirm = null;
            _onCancel = null;
        }
    }
}
