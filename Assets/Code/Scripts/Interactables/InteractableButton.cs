using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace AF.TS.Items
{
    public class InteractableButton : InteractablePlaySound
    {
        [FoldoutGroup("VFX")]
        [Tooltip("")]
        [SerializeField, Required]
        private Transform m_button;

        [FoldoutGroup("VFX")]
        [Tooltip("Distance the button moves when pressed"), Unit(Units.Centimeter)]
        [SerializeField]
        private float m_escursion = 1f;

        [FoldoutGroup("VFX")]
        [Tooltip("Total duration of the animation"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_duration = 0.5f;

        [FoldoutGroup("VFX")]
        [Tooltip("Disables all interaction animations")]
        [SerializeField]
        private bool m_isDisabled = false;

        [FoldoutGroup("VFX")]
        [Tooltip("If true, the button stays pressed until pressed again")]
        [SerializeField]
        private bool m_isToggle = false;

        [FoldoutGroup("VFX")]
        [Tooltip("Select animation type")]
        [SerializeField]
        private AnimationType m_animationType = AnimationType.Basic;

        private bool m_isPressed = false;

        public override void Interact()
        {
            if (this.m_isDisabled)
                return;

            base.Interact();

            this.Animation();
        }

        private void Animation()
        {
            if (this.m_button == null)
                return;

            this.m_button.DOKill();

            switch (this.m_animationType)
            {
                case AnimationType.Basic:
                    this.BasicAnimation();
                    break;

                case AnimationType.Punch:
                    this.PunchAnimation();
                    break;

                case AnimationType.Shake:
                    this.ShakeAnimation();
                    break;
            }

            if (this.m_isToggle)
            {
                this.m_isPressed = !this.m_isPressed;
            }
        }

        private void BasicAnimation()
        {
            Vector3 position = this.m_button.localPosition;
            float m_targetY = position.y - Mathf.Abs(Escursion);

            if (this.m_isToggle)
            {
                float m_finalY = this.m_isPressed ? position.y : m_targetY;

                this.m_button.DOLocalMoveY(m_finalY, this.m_duration)
                             .SetEase(Ease.OutQuad);
            }
            else
            {
                this.m_button.DOLocalMoveY(m_targetY, this.m_duration * 0.5f)
                             .SetEase(Ease.OutQuad)
                             .OnComplete(() =>
                             {
                                 this.m_button.DOLocalMoveY(position.y, this.m_duration * 0.5f)
                                              .SetEase(Ease.InQuad);
                             });
            }
        }

        private void PunchAnimation()
        {
            Vector3 m_punch = new Vector3(0f, -Mathf.Abs(Escursion), 0f);
            this.m_button.DOPunchPosition(m_punch, this.m_duration, 5, 0.5f);
        }

        private void ShakeAnimation()
        {
            this.m_button.DOShakePosition(this.m_duration, new Vector3(0f, Escursion * 0.5f, 0f), 10, 90f, false, true);
        }

        private float Escursion => this.m_escursion / 100f;

        /// <summary>
        /// Sets if the button is toggle
        /// </summary>
        /// <param name="m_value">If true, the button is toggle</param>
        public void SetToggle(bool m_value)
        {
            this.m_isToggle = m_value;
        }

        /// <summary>
        /// Sets if the button is disabled
        /// </summary>
        /// <param name="m_value">If true, the button is disabled</param>
        public void SetDisabled(bool m_value)
        {
            this.m_isDisabled = m_value;
        }

        /// <summary>
        /// Sets the animation type
        /// </summary>
        /// <param name="m_type">Animation type</param>
        public void SetAnimationType(AnimationType m_type)
        {
            this.m_animationType = m_type;
        }
    }
}
