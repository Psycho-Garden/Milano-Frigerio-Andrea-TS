using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DG.Tweening;

namespace AF.TS.Characters
{
    [Serializable]
    public class DroneIdleState : BaseState
    {
        [Tooltip("Delay"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_idleDelay = 2f;

        [Tooltip("Movement speed"), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0.1f)]
        private float m_moveSpeed = 1f;

        [Tooltip("Ease effect")]
        [SerializeField]
        private Ease m_moveEase = Ease.InOutSine;

        private float m_timer = 0f;
        private Transform m_body;
        private BoxCollider m_bounds;
        private bool m_moving = false;

        public override void OnStart(Boss boss)
        {
            base.OnStart(boss);

            this.m_body = boss.transform;
            this.m_bounds = boss.IdleMovementBounds;
            this.m_timer = this.m_idleDelay;
        }

        public override void OnUpdate()
        {
            if (this.m_moving || this.m_bounds == null) return;

            this.m_timer -= Time.deltaTime;
            if (this.m_timer <= 0f)
            {
                Vector3 nextTarget = GetRandomPositionInBounds();
                float distance = Vector3.Distance(this.m_body.position, nextTarget);
                float duration = distance / Mathf.Max(this.m_moveSpeed, 0.01f);

                LookAtSmooth(nextTarget);
                this.m_moving = true;

                m_tween = this.m_body.DOMove(nextTarget, duration)
                      .SetEase(this.m_moveEase)
                      .OnUpdate(() =>
                      {
                          LookAtSmooth(nextTarget);
                      })
                      .OnComplete(() =>
                      {
                          this.m_timer = this.m_idleDelay;
                          this.m_moving = false;
                      });
            }
            else
            {
                LookAtSmooth(this.m_body.position + this.m_body.forward);
            }
        }

        private Vector3 GetRandomPositionInBounds()
        {
            Vector3 center = this.m_bounds.center + this.m_bounds.transform.position;
            Vector3 size = this.m_bounds.size * 0.5f;

            float x = UnityEngine.Random.Range(-size.x, size.x);
            float y = UnityEngine.Random.Range(-size.y, size.y);
            float z = UnityEngine.Random.Range(-size.z, size.z);

            return center + new Vector3(x, y, z);
        }

        public override void OnDrawGizmos()
        {
            if (this.m_bounds != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = this.m_bounds.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(this.m_bounds.center, this.m_bounds.size);
            }
        }
    }
}