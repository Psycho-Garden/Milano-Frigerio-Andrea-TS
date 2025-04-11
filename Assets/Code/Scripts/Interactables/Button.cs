using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Button : MonoBehaviour
    {
        #region Exposed Members: -----------------------------------------------------------------------

        [Title("Events")]
        [Serializable]
        public class TriggerEnterEvent : UnityEvent { }

        [SerializeField, Tooltip("Event triggered when the player enters the trigger zone.")]
        private TriggerEnterEvent m_onTrigger = new();

        [FoldoutGroup("References")]
        [SerializeField, Required] Transform m_button;

        [FoldoutGroup("References")]
        [Unit(Units.Centimeter)]
        [SerializeField] float m_escursion;

        [FoldoutGroup("References")]
        [Unit(Units.Second)]
        [SerializeField, MinValue(0f)] float m_duration = 0.5f;

        [FoldoutGroup("Debug")]
        [Button("Try Interaction")]
        public void DebugTryInteraction()
        {
            TryInteraction();
        }

        #endregion

        #region Properties: ----------------------------------------------------------------------------

        private float GetEscursion => m_escursion / 100f;
        private bool IsActive = false;

        #endregion

        #region Private Methods: -----------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            TryInteraction();
        }

        private void TryInteraction()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;

            m_onTrigger?.Invoke();

            TryAnimate();
        }

        private void TryAnimate()
        {
            float originalY = m_button.localPosition.y;
            float targetY = originalY + GetEscursion;

            DOTween.Sequence()
                .Append(m_button.DOLocalMoveY(targetY, m_duration * 0.5f).SetEase(Ease.OutQuad))
                .Append(m_button.DOLocalMoveY(originalY, m_duration * 0.5f).SetEase(Ease.InQuad))
                .OnComplete(() => IsActive = false);
        }

        #endregion
    }
}