using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace AF.TS.Items
{
    public class InteractableTarget : InteractablePlaySound
    {
        [FoldoutGroup("VFX")]
        [Tooltip("Pivot to animate")]
        [SerializeField, Required, SceneObjectsOnly]
        private Transform m_pivot = null;

        [FoldoutGroup("VFX")]
        [Tooltip("Animation style")]
        [SerializeField]
        private AnimationType m_animationType = AnimationType.Basic;

        [FoldoutGroup("VFX")]
        [Tooltip("Rotation intensity in degrees"), Unit(Units.Degree)]
        [SerializeField, Range(0f, 90f)]
        private float m_rotationAmount = 30f;

        [FoldoutGroup("VFX")]
        [Tooltip("Total duration of the animation"), Unit(Units.Second)]
        [SerializeField, MinValue(0.01f)]
        private float m_duration = 0.3f;

        public override void Interact()
        {
            base.Interact();
            this.PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (this.m_pivot == null)
            {
                return;
            }

            this.m_pivot.DOKill(); // Stop any previous animation

            switch (this.m_animationType)
            {
                case AnimationType.Basic:
                    this.PlayBasic();
                    break;

                case AnimationType.Punch:
                    this.PlayPunch();
                    break;

                case AnimationType.Shake:
                    this.PlayShake();
                    break;
            }
        }

        private void PlayBasic()
        {
            // Reset rotation to start clean
            this.m_pivot.localRotation = Quaternion.identity;

            this.m_pivot.DOLocalRotate(
                    new Vector3(this.m_rotationAmount, 0f, 0f),
                    this.m_duration * 0.5f
                )
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
        }

        private void PlayPunch()
        {
            this.m_pivot.localRotation = Quaternion.identity;

            this.m_pivot.DOPunchRotation(
                    new Vector3(this.m_rotationAmount, 0f, 0f),
                    this.m_duration,
                    6,
                    0.4f
                );
        }

        private void PlayShake()
        {
            this.m_pivot.localRotation = Quaternion.identity;

            this.m_pivot.DOShakeRotation(
                    this.m_duration,
                    new Vector3(this.m_rotationAmount, 0f, 0f),
                    10,
                    90f,
                    false
                );
        }

        public void SetAnimationType(AnimationType type)
        {
            this.m_animationType = type;
        }

        public void SetDuration(float duration)
        {
            this.m_duration = Mathf.Max(0.01f, duration);
        }

        public void SetIntensity(float amount)
        {
            this.m_rotationAmount = Mathf.Clamp(amount, 0f, 90f);
        }
    }
}