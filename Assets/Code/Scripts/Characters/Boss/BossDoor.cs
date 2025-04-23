using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DG.Tweening;

namespace AF.TS.Characters
{
    [Serializable]
    public class BossDoor
    {
        [BoxGroup("References")]
        [Tooltip("The pivot of the door")]
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform m_pivot;

        [FoldoutGroup("Settings")]
        [Tooltip("The open rotation of the door")]
        [SerializeField]
        private Vector3 m_openRotation = Vector3.zero;

        [FoldoutGroup("Settings")]
        [Tooltip("The animation duration")]
        [SerializeField, MinValue(0f)]
        private float m_duration = 0.5f;

        [FoldoutGroup("Settings")]
        [Tooltip("The animation Ease")]
        [SerializeField]
        private Ease m_ease = Ease.OutQuad;

        private Vector3 m_closeRotation;
        private bool m_isOpen = false;

        public void Open()
        {
            if (this.m_isOpen || this.m_pivot == null)
            {
                return;
            }

            this.m_isOpen = true;
            this.m_pivot.DOComplete();

            this.m_closeRotation = this.m_pivot.localEulerAngles;

            Vector3 target = this.m_closeRotation + this.m_openRotation;
            this.m_pivot.DOLocalRotate(target, this.m_duration).SetEase(this.m_ease);
        }

        public void Close()
        {
            if (!this.m_isOpen || this.m_pivot == null)
            {
                return;
            }

            this.m_isOpen = false;
            this.m_pivot.DOComplete();

            this.m_pivot.DOLocalRotate(this.m_closeRotation, this.m_duration).SetEase(this.m_ease);
        }

        public Transform Pivot => this.m_pivot;
        public Vector3 OpenRotation => this.m_openRotation;
    }
}