using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace AF.TS.Items
{
    public class Door : InteractablePlaySound
    {
        [FoldoutGroup("Reference")]
        [Tooltip("Pivot Leaf DX")]
        [SerializeField, Required]
        private Transform m_pivotDx;

        [FoldoutGroup("Reference")]
        [Tooltip("Pivot Leaf DX")]
        [SerializeField]
        private Transform m_pivotSx;

        [FoldoutGroup("VFX")]
        [Tooltip("If true, the door will open and close automatically")]
        [SerializeField] private bool m_closable = false;

        [FoldoutGroup("VFX")]
        [Tooltip("Angle of rotation"), Unit(Units.Degree)]
        [SerializeField, Range(0, 180)]
        private float m_rotationAngle = 90f;

        [FoldoutGroup("VFX")]
        [Tooltip("Duration of the animation"), Unit(Units.Second)]
        [SerializeField]
        private float m_animationDuration = 0.5f;

        [FoldoutGroup("VFX")]
        [Tooltip("Delay before the door closes"), Unit(Units.Second)]
        [SerializeField, ShowIf("m_closable")]
        private float m_closeDelay = 3f;

        [FoldoutGroup("VFX")]
        [Tooltip("If true, the animation will be disabled")]
        [SerializeField]
        private bool m_disableAnimation = false;

        private bool m_isOpen = false;
        private Tween m_tweenDx;
        private Tween m_tweenSx;
        private Tween m_autoCloseTween;

        public override void Interact()
        {
            base.Interact();
            ToggleDoor();
        }

        private void ToggleDoor()
        {
            if (m_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void Open()
        {
            if (m_disableAnimation || m_isOpen)
                return;

            m_isOpen = true;
            AnimateDoor(true);

            m_autoCloseTween?.Kill();

            if (m_closable)
            {
                m_autoCloseTween = DOVirtual.DelayedCall(m_closeDelay, () => Close(), false);
            }
        }

        private void Close()
        {
            if (m_disableAnimation || !m_isOpen)
                return;

            m_isOpen = false;
            AnimateDoor(false);
        }

        private void AnimateDoor(bool opening)
        {
            float angle = opening ? m_rotationAngle : 0f;

            if (m_pivotDx != null)
            {
                m_tweenDx?.Kill();
                m_tweenDx = m_pivotDx.DOLocalRotate(new Vector3(0f, angle, 0f), m_animationDuration)
                                      .SetEase(Ease.OutQuad);
            }

            if (m_pivotSx != null)
            {
                m_tweenSx?.Kill();
                m_tweenSx = m_pivotSx.DOLocalRotate(new Vector3(0f, -angle, 0f), m_animationDuration)
                                      .SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
        /// Forces the door to open
        /// </summary>
        public void ForceOpen() => Open();

        /// <summary>
        /// Forces the door to close
        /// </summary>
        public void ForceClose() => Close();

        /// <summary>
        /// Sets if the door can be closed
        /// </summary>
        /// <param name="value">If true, the door can be closed</param>
        public void SetClosable(bool value)
        {
            m_closable = value;
        }

        /// <summary>
        /// Sets if the animation is enabled
        /// </summary>
        /// <param name="value">If true, the animation is enabled</param>
        public void SetAnimationEnabled(bool value)
        {
            m_disableAnimation = !value;
        }

        /// <summary>
        /// Sets the angle of rotation
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        public void SetAngle(float angle)
        {
            m_rotationAngle = Mathf.Clamp(angle, 0f, 180f);
        }

        /// <summary>
        /// Sets the duration of the animation
        /// </summary>
        /// <param name="duration">Duration of the animation</param>
        public void SetDuration(float duration)
        {
            m_animationDuration = Mathf.Max(0.01f, duration);
        }

        /// <summary>
        /// Sets the delay before the door closes
        /// </summary>
        /// <param name="delay">Delay before the door closes</param>
        public void SetCloseDelay(float delay)
        {
            m_closeDelay = Mathf.Max(0f, delay);
        }
    }

}
