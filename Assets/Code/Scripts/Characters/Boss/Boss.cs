using UnityEngine;
using System;
using Sirenix.OdinInspector;
using AF.TS.Weapons;
using AF.TS.Utils;

namespace AF.TS.Characters
{
    [HideMonoScript]
    public class Boss : MonoBehaviour
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
        [Tooltip(""), Unit(Units.Second)]
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

        [FoldoutGroup("References")]
        [Tooltip("List of weapons")]
        [SerializeField, ChildGameObjectsOnly, RequiredListLength(2)]
        private NewGunController[] m_weapons;

        #endregion

        #region Settings

        [FoldoutGroup("References/Movement")]
        [Tooltip("The volume inside which the boss moves while idle")]
        [SerializeField, Required]
        private BoxCollider m_idleMovementBounds;

        #endregion

        #region Debug

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

        #endregion

        #endregion

        #region Private Members -----------------------------------------------------------------

        private float m_bodyReturnTimer = 0f;
        private Quaternion m_bodyInitialRotation;
        private Vector3 m_baseLocalPosition;
        private Transform m_player;

        private HealthSystem m_healthSystem;

        #endregion

        #region Callback Methods ----------------------------------------------------------------

        private void Awake()
        {
            ServiceLocator.Register<Boss>(this);

            this.m_healthSystem = GetComponent<HealthSystem>();

            foreach (GameObject core in this.m_cores)
            {
                core.SetActive(false);
            }
        }

        private void Start()
        {
            this.m_player = ServiceLocator.Get<Character>().transform;

            this.m_bodyInitialRotation = this.m_body.localRotation;
            this.m_baseLocalPosition = this.m_mesh.localPosition;

            this.m_currentState = this.m_states[0];
            this.m_currentState.OnStart(this);
        }

        private void OnDestroy()
        {
            this.m_currentState.OnDispose();
        }

        #endregion

        #region Update Methods ------------------------------------------------------------------

        private void Update()
        {
            this.m_currentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            this.m_currentState.OnFixedUpdate();
        }

        #endregion

        #region Public Methods ------------------------------------------------------------------

        public BoxCollider IdleMovementBounds => this.m_idleMovementBounds;
        public Transform Player => this.m_player;
        public HealthSystem HealthSystem => this.m_healthSystem;
        public NewGunController[] Weapons => this.m_weapons;
        public NewGunController GetWeapon(int index) => this.m_weapons[index];

        /// <summary>
        /// Change the state of the boss by state
        /// </summary>
        /// <param name="state">New state</param>
        public void ChangeState(BaseState state)
        {
            Debug.Log($"[<Color=green>Boss</color>] Transition to state: {state.GetType().Name}");

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
                Debug.LogWarning($"[<Color=Red>Boss</color>] Invalid state index: {index}");
                return;
            }

            ChangeState(this.m_states[index]);
        }

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
            Vector3 directionToTarget = (targetPosition - this.transform.position).normalized;

            // ========== PITCH BODY ==========
            if (targetLookAt.HasValue)
            {
                this.m_bodyReturnTimer = this.m_bodyReturnDelay;

                Vector3 worldDir = (targetLookAt.Value - this.m_body.position).normalized;

                // ========== YAW (rotate root on Y axis) ==========
                Vector3 flatDir = new Vector3(worldDir.x, 0f, worldDir.z);
                if (flatDir.sqrMagnitude > 0.0001f)
                {
                    Quaternion desiredYaw = Quaternion.LookRotation(flatDir);
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, desiredYaw, Time.deltaTime * this.m_gameObjectRotationSpeed);
                }

                // ========== PITCH (rotate m_body on X axis) ==========

                // Recalculate the new direction from body to target after yaw adjustment
                Vector3 dirFromBodyToTarget = (targetLookAt.Value - this.m_body.position).normalized;

                // Convert to local space to isolate pitch
                Vector3 localDir = this.m_body.parent.InverseTransformDirection(dirFromBodyToTarget);

                // Pitch is rotation around X axis — forward = Z+
                float pitchAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

                // Reverse the pitch if you want body to "lean down" instead of up
                pitchAngle = -pitchAngle;

                Quaternion desiredPitch = Quaternion.Euler(pitchAngle, 0f, 0f);
                this.m_body.localRotation = Quaternion.Slerp(this.m_body.localRotation, desiredPitch, Time.deltaTime * this.m_bodyRotationSpeed);
            }
            else
            {
                if (this.m_bodyReturnTimer > 0)
                {
                    this.m_bodyReturnTimer -= Time.deltaTime;
                }
                else
                {
                    this.m_body.localRotation = Quaternion.Slerp(this.m_body.localRotation, this.m_bodyInitialRotation, Time.deltaTime * this.m_bodyRotationSpeed);
                }

                if ((targetPosition - this.transform.position).sqrMagnitude > 0.0001f)
                {
                    Vector3 flatDir = directionToTarget;
                    flatDir.y = 0f;

                    if (flatDir.sqrMagnitude > 0.0001f)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(flatDir);
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRot, Time.deltaTime * this.m_gameObjectRotationSpeed);
                    }
                }
            }

            // ---------- ROLL BOSS (after yaw) ----------

            // Calculate side factor for roll
            float sideFactor = Vector3.Dot(this.transform.right, directionToTarget); // -1 = left, 1 = right
            float targetRoll = -sideFactor * this.m_maxRollAngle;

            // Apply roll on top of current yaw
            Quaternion currentRot = this.transform.rotation;
            Quaternion rollOffset = Quaternion.AngleAxis(targetRoll, this.transform.forward); // roll around local Z
            Quaternion finalRot = currentRot * rollOffset;

            this.transform.rotation = Quaternion.Slerp(currentRot, finalRot, Time.deltaTime * this.m_gameObjectRotationSpeed);


            // ========== PITCH FANS ==========
            Quaternion fanTargetRot;

            if ((targetPosition - this.transform.position).sqrMagnitude > 0.0001f)
            {
                float forwardFactor = Vector3.Dot(this.transform.forward, directionToTarget); // -1 = back, 1 = forward
                float pitchAmount = Mathf.Clamp(forwardFactor, -1f, 1f) * this.m_maxFanPitchAngle;
                fanTargetRot = Quaternion.Euler(pitchAmount, 0f, 0f);
            }
            else
            {
                fanTargetRot = Quaternion.identity;
            }

            this.m_leftFan.localRotation = Quaternion.Slerp(this.m_leftFan.localRotation, fanTargetRot, Time.deltaTime * this.m_fansRotationSpeed);
            this.m_rightFan.localRotation = Quaternion.Slerp(this.m_rightFan.localRotation, fanTargetRot, Time.deltaTime * this.m_fansRotationSpeed);
        }

        public void ApplyHoverEffect()
        {
            float offsetY = Mathf.Sin(Time.time * this.m_hoverFrequency * Mathf.PI * 2f) * this.m_hoverAmplitude;
            Vector3 newLocalPos = this.m_baseLocalPosition + new Vector3(0f, offsetY, 0f);
            this.m_mesh.transform.localPosition = newLocalPos;
        }

        #endregion

        #region Gizmos Methods ------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if (m_drawGizmos != GizmosType.All)
            {
                return;
            }

            if (Application.isPlaying) this.m_currentState.OnDrawGizmos();

            DrawOpenDoorsAngles();
        }

        private void OnDrawGizmosSelected()
        {
            if (m_drawGizmos != GizmosType.Selected)
            {
                return;
            }

            if (Application.isPlaying) this.m_currentState.OnDrawGizmosSelected();

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

}