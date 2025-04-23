using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DG.Tweening;
using UnityEngine.XR;

namespace AF.TS.Characters
{
    [HideMonoScript]
    public class Boss : MonoBehaviour, IHaveHealth
    {
        #region Exposed Members -----------------------------------------------------------------

        #region Orientation Settings

        [FoldoutGroup("Orientation")]
        [Tooltip("")]
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform m_mesh;

        [FoldoutGroup("Orientation")]
        [Tooltip("")]
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform m_body;

        [FoldoutGroup("Orientation/Fans")]
        [Tooltip("The left fan")]
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform m_leftFan;

        [FoldoutGroup("Orientation/Fans")]
        [Tooltip("The right fan")]
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform m_rightFan;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.DegreesPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_fansRotationSpeed = 5f;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.DegreesPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_bodyRotationSpeed = 5f;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.DegreesPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_gameObjectRotationSpeed = 3f;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.Degree)]
        [SerializeField, Range(0f, 45f)]
        private float m_maxRollAngle = 20f;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.Degree)]
        [SerializeField, Range(0f, 45f)]
        private float m_maxFanPitchAngle = 30f;

        [FoldoutGroup("Orientation")]
        [Tooltip(""), Unit(Units.Degree)]
        [SerializeField]
        private float m_bodyReturnDelay = 2f;

        [FoldoutGroup("Orientation")]
        [Tooltip("")]
        [SerializeField]
        private float m_lookPrecisionThreshold = 0.9f;

        [FoldoutGroup("Orientation/Hover")]
        [Tooltip("")]
        [SerializeField, MinValue(0f)]
        private float m_hoverAmplitude = 0.25f;

        [FoldoutGroup("Orientation/Hover")]
        [Tooltip("")]
        [SerializeField, MinValue(0f)]
        private float m_hoverFrequency = 1f;

        #endregion

        #region Door & Cores

        [FoldoutGroup("References")]
        [Tooltip("")]
        [SerializeField]
        private BossDoor[] m_doors;

        [FoldoutGroup("References")]
        [Tooltip("")]
        [SerializeField, ChildGameObjectsOnly]
        private GameObject[] m_cores;

        #endregion

        #region Settings

        [FoldoutGroup("References/Movement")]
        [Tooltip("The volume inside which the boss moves while idle")]
        [SerializeField, Required]
        private BoxCollider m_idleMovementBounds;

        #endregion

        [BoxGroup("States Settings")]
        [Tooltip("The states of the boss")]
        [SerializeReference]
        private BaseState[] m_states = new BaseState[0];

        [FoldoutGroup("Debug")]
        [Tooltip("The current state of the boss")]
        [ShowInInspector, ReadOnly]
        private bool m_debugLog = true;

        [FoldoutGroup("Debug")]
        [Tooltip("The current state of the boss")]
        [ShowInInspector, ReadOnly]
        private BaseState m_currentState;

        [FoldoutGroup("Debug")]
        [Tooltip("The type of gizmos to draw")]
        [SerializeField]
        private GizmosType m_drawGizmos = GizmosType.None;

#if UNITY_EDITOR

        [FoldoutGroup("Debug")]
        [SerializeField, InlineProperty, HideLabel] private HealthSystemEditorHelper m_editorHelper = new();

        private void OnValidate()
        {
            m_editorHelper.OnValidate(this.gameObject);
        }

        private void OnTransformChildrenChanged()
        {
            m_editorHelper.OnTransformChildrenChanged(this.gameObject);
        }

#endif



        #endregion

        #region Private Members -----------------------------------------------------------------

        private float m_bodyReturnTimer = 0f;
        private Quaternion m_bodyInitialRotation;
        private Vector3 m_baseLocalPosition;

        #endregion

        #region Callback Methods ----------------------------------------------------------------

        private void Awake()
        {
            foreach (GameObject core in this.m_cores)
            {
                core.SetActive(false);
            }
        }

        private void Start()
        {
            m_bodyInitialRotation = m_body.localRotation;
            m_baseLocalPosition = m_mesh.localPosition;

            m_currentState = m_states[0];
            m_currentState.OnStart(this);
        }

        private void OnDestroy()
        {
            m_currentState.OnDispose();
        }

        #endregion

        #region Update Methods ------------------------------------------------------------------

        private void Update()
        {
            m_currentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            m_currentState.OnFixedUpdate();
        }

        #endregion

        #region Public Methods ------------------------------------------------------------------

        public BoxCollider IdleMovementBounds => m_idleMovementBounds;

        /// <summary>
        /// Change the state of the boss by state
        /// </summary>
        /// <param name="state">New state</param>
        public void ChangeState(BaseState state)
        {
            Debug.Log($"[<Color=green>Boss</colore>] Transition to state: {state.GetType().Name}");

            this.m_currentState.OnDispose();
            this.m_currentState = state;
            this.m_currentState.OnStart(this);
        }

        /// <summary>
        /// Change the state of the boss by index
        /// </summary>
        /// <param name="index">Index of the state in the array</param>
        public void ChangeState(int index)
        {
            if (index < 0 || index >= this.m_states.Length)
            {
                Debug.LogWarning($"[<Color=Red>Boss</colore>] Invalid state index: {index}");
                return;
            }

            ChangeState(this.m_states[index]);
        }

        public void TakeDamage(float damage)
        {
            throw new NotImplementedException();
        }

        public void ApplyStatusEffect(StatusEffectType effect) { }

        [FoldoutGroup("Debug")]
        [Tooltip("")]
        [Button("Open Doors")]
        public void OpenDoors()
        {
            foreach (BossDoor door in this.m_doors)
            {
                door.Open();
            }

            foreach (GameObject core in this.m_cores)
            {
                core.SetActive(true);
            }
        }

        [FoldoutGroup("Debug")]
        [Tooltip("")]
        [Button("Close Doors")]
        public void CloseDoors()
        {
            foreach (BossDoor door in this.m_doors)
            {
                door.Close();
            }

            foreach (GameObject core in this.m_cores)
            {
                core.SetActive(false);
            }
        }

        public void HandleMovementAndOrientation(Vector3 targetPosition, Vector3? targetLookAt = null)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;

            // ========== PITCH BODY ==========
            if (targetLookAt.HasValue)
            {
                m_bodyReturnTimer = m_bodyReturnDelay;

                Vector3 dirLook = (targetLookAt.Value - m_body.position).normalized;
                Quaternion desiredBodyRot = Quaternion.LookRotation(dirLook);
                m_body.rotation = Quaternion.Slerp(m_body.rotation, desiredBodyRot, Time.deltaTime * m_bodyRotationSpeed);

                float dot = Vector3.Dot(m_body.forward, dirLook);
                if (dot < m_lookPrecisionThreshold)
                {
                    // Too "Out axis" → Rotate the root
                    Quaternion desiredRootRot = Quaternion.LookRotation(dirLook);
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRootRot, Time.deltaTime * m_gameObjectRotationSpeed);
                }
            }
            else
            {
                if (m_bodyReturnTimer > 0)
                {
                    m_bodyReturnTimer -= Time.deltaTime;
                }
                else
                {
                    m_body.localRotation = Quaternion.Slerp(m_body.localRotation, m_bodyInitialRotation, Time.deltaTime * m_bodyRotationSpeed);
                }
            }

            // ========== ROLL BOSS ==========
            float sideFactor = Vector3.Dot(transform.right, directionToTarget); // -1 = sx, 1 = dx
            float targetRoll = -sideFactor * m_maxRollAngle;
            Quaternion currentRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(currentRot.eulerAngles.x, currentRot.eulerAngles.y, targetRoll);
            transform.localRotation = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * m_gameObjectRotationSpeed);

            // ========== PITCH FANS ==========
            Quaternion fanTargetRot;

            if ((targetPosition - transform.position).sqrMagnitude > 0.0001f)
            {
                float forwardFactor = Vector3.Dot(transform.forward, directionToTarget); // -1 = back, 1 = forward
                float pitchAmount = Mathf.Clamp(forwardFactor, -1f, 1f) * m_maxFanPitchAngle;
                fanTargetRot = Quaternion.Euler(pitchAmount, 0f, 0f);
            }
            else
            {
                fanTargetRot = Quaternion.identity;
            }

            m_leftFan.localRotation = Quaternion.Slerp(m_leftFan.localRotation, fanTargetRot, Time.deltaTime * m_fansRotationSpeed);
            m_rightFan.localRotation = Quaternion.Slerp(m_rightFan.localRotation, fanTargetRot, Time.deltaTime * m_fansRotationSpeed);
        }

        public void ApplyHoverEffect()
        {
            float offsetY = Mathf.Sin(Time.time * m_hoverFrequency * Mathf.PI * 2f) * m_hoverAmplitude;
            Vector3 newLocalPos = m_baseLocalPosition + new Vector3(0f, offsetY, 0f);
            m_mesh.transform.localPosition = newLocalPos;
        }

        #endregion

        #region Gizmos Methods ------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if (m_drawGizmos != GizmosType.All)
            {
                return;
            }

            m_currentState.OnDrawGizmos();

            DrawOpenDoorsAngles();
        }

        private void OnDrawGizmosSelected()
        {
            if (m_drawGizmos != GizmosType.Selected)
            {
                return;
            }

            m_currentState.OnDrawGizmosSelected();

            DrawOpenDoorsAngles();
        }

        private void DrawOpenDoorsAngles()
        {

            foreach (BossDoor door in m_doors)
            {
                if (door.Pivot == null)
                    continue;

                Quaternion rotation = door.Pivot.rotation * Quaternion.Euler(door.OpenRotation);

                Vector3 direction = rotation * Vector3.down;

                Gizmos.color = Color.green;
                Gizmos.DrawRay(door.Pivot.position, direction * 0.5f);
            }
        }

        #endregion
    }

    [Serializable]
    public enum GizmosType { None, All, Selected }

    public interface IState
    {
        public void OnStart(Boss boss);
        public void OnUpdate();
        public void OnFixedUpdate();
        public void OnDispose();
        public void OnDrawGizmos();
        public void OnDrawGizmosSelected();
    }

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