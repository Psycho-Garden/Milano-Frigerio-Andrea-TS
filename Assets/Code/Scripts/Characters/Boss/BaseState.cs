using UnityEngine;
using System;
using DG.Tweening;

namespace AF.TS.Characters
{
    [Serializable]
    public class BaseState : IState
    {
        protected Boss m_boss;
        protected Tween m_tween;

        public virtual void OnStart(Boss boss)
        {
            this.m_boss = boss;
        }

        public virtual void OnDispose()
        {
            if (this.m_tween != null && this.m_tween.IsActive() && this.m_tween.IsPlaying())
            {
                this.m_tween.Kill();
                this.m_tween = null;
            }
        }

        public virtual void OnFixedUpdate() { }

        public virtual void OnUpdate() { }

        public virtual void OnDrawGizmos() { }

        public virtual void OnDrawGizmosSelected() { }

        protected void LookAtSmooth(Vector3 moveTarget, Vector3? lookTarget = null)
        {
            m_boss.ApplyHoverEffect();
            m_boss.HandleMovementAndOrientation(moveTarget, lookTarget);
        }
    }
}