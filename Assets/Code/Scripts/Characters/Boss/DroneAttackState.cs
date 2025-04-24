using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;
using AF.TS.Weapons;

namespace AF.TS.Characters
{
    [Serializable]
    public class DroneAttackState : BaseState
    {
        #region Exposed Members -----------------------------------------------------------------

        [FoldoutGroup("Settings")]
        [Tooltip("Movement speed while orbiting"), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_moveSpeed = 4f;

        [FoldoutGroup("Settings")]
        [Tooltip("Desired orbit distance from player"), Unit(Units.Meter)]
        [SerializeField, MinValue(0f)]
        private float m_orbitRadius = 5f;

        [FoldoutGroup("Settings")]
        [Tooltip("Offset applied when aiming at player (e.g., bust height)")]
        [SerializeField]
        private Vector3 m_lookOffset = Vector3.up;

        [FoldoutGroup("Settings")]
        [Tooltip("Index of the overload state")]
        [SerializeField, MinValue(0)]
        private int m_nextStateIndex = 2;

        [FoldoutGroup("Settings")]
        [Tooltip("Layers considered obstacles for raycast")]
        [SerializeField]
        private LayerMask m_obstacleMask = Physics.DefaultRaycastLayers;

        [FoldoutGroup("Aiming")]
        [Tooltip("The smoothing factor applied to the predicted target position\n Higher = snappier, lower = smoother")]
        [SerializeField, MinValue(0f)]
        private float m_predictTargetPositionSmoothing = 10f;

        [FoldoutGroup("Aiming")]
        [Tooltip("The maximum speed of the predicted target position"), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_maxPredictSpeed = 6f;

        #endregion

        #region Private Members -----------------------------------------------------------------

        private BoxCollider m_bounds;
        private Transform m_body;
        private Tween m_moveTween;
        private bool m_isMoving = false;
        private bool m_isFiring = false;
        private Vector3 m_predictedAimPoint = Vector3.zero;

        private Vector3 m_lastPlayerPosition;
        private Vector3 m_estimatedVelocity;

        private RaycastHit[] m_raycastHits = new RaycastHit[1];

        public DroneAttackState()
        {
        }

        #endregion

        #region State Methods -------------------------------------------------------------------

        public override void OnStart(Boss boss)
        {
            base.OnStart(boss);

            this.m_lastPlayerPosition = this.m_boss.Player.position;

            this.m_body = boss.transform;
            this.m_bounds = boss.IdleMovementBounds;
            this.TriggerWeapons(true);
            this.SelectNextPosition();
        }

        public override void OnUpdate()
        {
            if (this.m_boss.Player == null || this.m_bounds == null)
                return;

            this.m_predictedAimPoint = PredictTargetPosition();

            this.LookAtSmooth(this.m_body.position + this.m_body.forward, m_predictedAimPoint);
            this.CheckLineOfSight(m_predictedAimPoint);

            if (!this.m_isMoving && this.m_moveTween == null)
            {
                this.SelectNextPosition();
            }

            if (this.m_boss != null && this.m_boss.HealthSystem.HealthNormalized <= 0.5f)
            {
                this.m_boss.ChangeState(this.m_nextStateIndex);
            }
        }

        public override void OnDispose()
        {
            this.TriggerWeapons(false);
            this.m_moveTween?.Kill();
            this.m_moveTween = null;
            this.m_isMoving = false;
        }

        #endregion

        #region Private Methods -----------------------------------------------------------------

        #region Aiming

        private void CheckLineOfSight(Vector3 target)
        {
            Vector3 origin = this.m_body.position;
            Vector3 direction = (target - origin).normalized;
            float distance = Vector3.Distance(origin, target);

            Debug.DrawRay(origin, direction * distance, Color.red);

            int hitCount = Physics.RaycastNonAlloc(origin, direction, this.m_raycastHits, distance, this.m_obstacleMask);
            if (hitCount > 0 && this.m_isFiring)
            {
                this.TriggerWeapons(false);
            }
            else if (!this.m_isFiring)
            {
                this.TriggerWeapons(true);
            }
        }

        private void TriggerWeapons(bool state)
        {
            this.m_isFiring = state;

            if (state)
            {
                foreach (NewGunController weapon in this.m_boss.Weapons)
                {
                    weapon.OnFireInput();
                }
            }
            else
            {
                foreach (NewGunController weapon in this.m_boss.Weapons)
                {
                    weapon.OnFireRelease();
                }
            }

        }

        #endregion

        #region Movement

        private Vector3 PredictTargetPosition()
        {
            Vector3 currentPos = this.m_boss.Player.position;

            // Compute raw velocity
            Vector3 rawVelocity = (currentPos - this.m_lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            this.m_lastPlayerPosition = currentPos;

            // Smooth velocity (lerp toward new velocity)
            this.m_estimatedVelocity = Vector3.Lerp(this.m_estimatedVelocity, rawVelocity, Time.deltaTime * this.m_predictTargetPositionSmoothing);

            // Optional clamp to prevent overshooting
            Vector3 clampedVelocity = Vector3.ClampMagnitude(this.m_estimatedVelocity, this.m_maxPredictSpeed);

            // Predict position based on move speed and distance
            float distance = Vector3.Distance(this.m_body.position, currentPos);
            float estimatedTime = distance / Mathf.Max(this.m_moveSpeed, 0.01f);

            return currentPos + clampedVelocity * estimatedTime + this.m_lookOffset;
        }

        private void SelectNextPosition()
        {
            Vector3 offset = UnityEngine.Random.insideUnitSphere;
            offset.y = 0;
            offset = offset.normalized * this.m_orbitRadius;

            Vector3 target = this.m_boss.Player.position + offset;
            target = this.GetPositionOutsideSphereInsideBounds();

            float distance = Vector3.Distance(this.m_body.position, target);
            float duration = distance / Mathf.Max(this.m_moveSpeed, 0.01f);

            this.m_isMoving = true;
            this.m_moveTween = this.m_body.DOMove(target, duration)
                .SetEase(Ease.InOutSine)
                .OnUpdate(() =>
                {
                    this.LookAtSmooth(target, this.m_predictedAimPoint);
                })
                .OnComplete(() =>
                {
                    this.m_isMoving = false;
                    this.m_moveTween = null;
                });
        }

        private Vector3 ClampOutsideBounds(Vector3 point)
        {
            Vector3 center = m_predictedAimPoint;
            Vector3 size = this.m_bounds.size * 0.5f;

            Vector3 clamped = point;

            if (Mathf.Abs(point.x - center.x) < size.x &&
                Mathf.Abs(point.y - center.y) < size.y &&
                Mathf.Abs(point.z - center.z) < size.z)
            {
                Vector3 dir = (point - center).normalized;
                clamped = center + dir * size.magnitude;
            }

            return clamped;
        }

        private Vector3 GetPositionOutsideSphereInsideBounds()
        {
            Vector3 center = this.m_bounds.center + this.m_bounds.transform.position;
            Vector3 size = this.m_bounds.size * 0.5f;
            Vector3 playerPos = m_predictedAimPoint;

            const int maxAttempts = 20;

            for (int i = 0; i < maxAttempts; i++)
            {
                float x = UnityEngine.Random.Range(-size.x, size.x);
                float y = UnityEngine.Random.Range(-size.y, size.y);
                float z = UnityEngine.Random.Range(-size.z, size.z);

                Vector3 localPoint = new Vector3(x, y, z);
                Vector3 worldPoint = center + localPoint;

                float sqrDistance = (worldPoint - playerPos).sqrMagnitude;

                if (sqrDistance >= this.m_orbitRadius * this.m_orbitRadius)
                {
                    return worldPoint;
                }
            }

            // Fallback if nothing found
            Vector3 dir = (this.m_body.position - playerPos).normalized;
            Vector3 fallback = playerPos + dir * (this.m_orbitRadius * 1.2f);
            return this.ClampOutsideBounds(fallback);
        }

        #endregion

        #endregion

        #region Gizmos --------------------------------------------------------------------------

        public override void OnDrawGizmos()
        {
            if (this.m_boss.Player == null)
            {
                return;
            }

            DrawTargetPosition(this.m_boss.Player.position + this.m_lookOffset, Color.red);
            DrawTargetPosition(this.m_predictedAimPoint, Color.magenta);
        }

        private void DrawTargetPosition(Vector3 position, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, this.m_orbitRadius);
            Gizmos.DrawSphere(position, 0.1f);
        }

        #endregion

    }
}