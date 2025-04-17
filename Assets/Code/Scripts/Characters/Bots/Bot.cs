using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using DG.Tweening;
using AF.TS.Weapons;
using AF.TS.Utils;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AF.TS.Characters
{
    [HideMonoScript]
    [ExecuteAlways]
    public class Bot : MonoBehaviour, IHaveHealth
    {
        #region Serialized Fields ----------------------------------------------------------------------

        [BoxGroup("References")]
        [Tooltip("Root body transform for Y rotation.")]
        [SerializeField] private Transform m_body;

        [BoxGroup("References")]
        [Tooltip("Gun handle used for aiming on X axis.")]
        [SerializeField] private Transform m_gunHandle;

        [BoxGroup("References")]
        [Tooltip("Gun GameObject (must have GunController).")]
        [SerializeField, OnValueChanged("SetGunPosition")] private GameObject m_gun;

        [BoxGroup("Aim Settings")]
        [Tooltip("Shooting range.")]
        [Unit(Units.Meter)]
        [SerializeField] private float m_range = 5f;

        [BoxGroup("Aim Settings")]
        [SerializeField] private Vector3 m_gunAimOffset = Vector3.zero;

        [BoxGroup("Aim Settings")]
        [SerializeField] private Vector3 m_aimDirection = -Vector3.up;

        [BoxGroup("Aim Settings")]
        [Unit(Units.Degree)]
        [SerializeField] private float m_aimAngle = 45f;

        [BoxGroup("Aim Settings")]
        [Unit(Units.Degree)]
        [SerializeField] private float m_aimTolerance = 10f;

        [BoxGroup("Aim Settings")]
        [Unit(Units.Second)]
        [SerializeField] private float m_bodyRotationSpeed = 0.25f;

        [BoxGroup("Aim Settings")]
        [Unit(Units.Second)]
        [SerializeField] private float m_gunRotationSpeed = 0.15f;

        [BoxGroup("Aim Settings")]
        [Tooltip("Cooldown time before reset rotation after losing target.")]
        [Unit(Units.Second)]
        [SerializeField]
        private float m_resetAimDelay = 2f;

        [BoxGroup("Health System")]
        [Tooltip("Bot's health.")]
        [SerializeField] private float m_health;

#if UNITY_EDITOR

        [BoxGroup("Health System")]
        [SerializeField, InlineProperty, HideLabel] private HealthSystemEditorHelper m_editorHelper = new();

        private void SetGunPosition()
        {
            if (m_gun != null)
            {
                m_gun.transform.SetPositionAndRotation(m_gunHandle.position, m_gunHandle.rotation);
            }
        }

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

        #region Private Fields -------------------------------------------------------------------------

        private NewGunController m_gunController;
        private Character m_character;
        private float m_targetLostTimer = 0f;
        private bool m_hasTargetInRange = false;
        private bool m_isResetting = false;

        private Vector3 m_targetPosition = Vector3.zero;

        #endregion

        #region Unity Events ---------------------------------------------------------------------------

        private void Start()
        {
            m_gun.transform.parent = m_gunHandle;
            m_gun.transform.SetPositionAndRotation(m_gunHandle.position, m_gunHandle.rotation);

            m_gunController = m_gun.GetComponent<NewGunController>();
            m_character = ServiceLocator.Get<Character>();

            foreach (var hurtbox in GetComponentsInChildren<Hurtbox>())
            {
                hurtbox.owner = this;
            }
        }

        private void Update()
        {
            if (m_character == null)
                return;

            float dist = Vector3.Distance(transform.position, m_character.transform.position);
            bool targetInRange = dist <= m_range;

            bool targetInFOV = FOVUtility.IsInFOV(transform.position, m_aimDirection, m_character.transform.position, m_aimAngle, m_aimAngle);

            if (targetInRange && targetInFOV)
            {
                // Player is in range → Reset timer and stop reset animation if any
                m_hasTargetInRange = true;
                m_targetLostTimer = 0f;

                if (m_isResetting)
                {
                    DOTween.Kill(m_body);        // Stop any ongoing reset tween
                    DOTween.Kill(m_gunHandle);
                    m_isResetting = false;
                }

                Vector3 direction = m_character.transform.position + m_gunAimOffset - transform.position;
                Ray ray = new Ray(transform.position, direction.normalized);

                Debug.DrawRay(transform.position, direction.normalized * m_range, Color.magenta);

                RotateTowardsTarget(direction);

                if (IsAimedAtTarget())
                    m_gunController.OnFireInput();
                else
                    m_gunController.OnFireRelease();
            }
            else
            {
                // Player not in range
                if (m_hasTargetInRange)
                {
                    m_targetLostTimer += Time.deltaTime;

                    if (m_targetLostTimer >= m_resetAimDelay)
                    {
                        ResetAim();
                        m_hasTargetInRange = false;
                    }
                }

                m_gunController.OnFireRelease(); // Always release fire if player out of range
            }
        }

        #endregion

        #region Aiming Logic ---------------------------------------------------------------------------

        private void RotateTowardsTarget(Vector3 direction)
        {
            m_targetPosition = m_character.transform.position + m_gunAimOffset;

            Vector3 flatDir = new Vector3(direction.x, 0f, direction.z).normalized;
            if (flatDir != Vector3.zero)
            {
                Quaternion targetBodyRotation = Quaternion.LookRotation(flatDir);
                m_body.DORotateQuaternion(targetBodyRotation, m_bodyRotationSpeed).SetEase(Ease.OutSine);
            }

            Vector3 localTargetDir = m_body.InverseTransformDirection(direction.normalized);
            float angleX = Mathf.Atan2(-localTargetDir.y, localTargetDir.z) * Mathf.Rad2Deg;

            Quaternion targetGunRotation = Quaternion.Euler(angleX, 0f, 0f);
            m_gunHandle.DOLocalRotateQuaternion(targetGunRotation, m_gunRotationSpeed).SetEase(Ease.OutSine);
        }

        private bool IsAimedAtTarget()
        {
            Vector3 toTarget = ((m_character.transform.position + m_gunAimOffset) - m_gunHandle.position).normalized;
            float angle = Vector3.Angle(m_gunHandle.forward, toTarget);
            return angle <= m_aimTolerance;
        }

        /// <summary>
        /// Resets aiming (body and gun handle) to default rotation.
        /// </summary>
        private void ResetAim()
        {
            m_isResetting = true;

            m_body.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
            m_gunHandle.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
        }

        #endregion

        #region Public Methods -------------------------------------------------------------------------

        public void TakeDamage(float damage)
        {
            Debug.Log("Bot took " + damage + " damage.");

            m_health -= damage;

            if (m_health <= 0)
            {
                this.gameObject.SetActive(false);
            }
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, m_range);

            FOVGizmoDrawer.DrawFOV(transform.position, m_aimDirection, m_range, m_aimAngle, m_aimAngle, Color.green);

            // Draw target position
            if (m_hasTargetInRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(m_targetPosition, Vector3.one * 0.1f);
            }
        }
    }

    [Serializable]
    public class HealthSystemEditorHelper
    {
#if UNITY_EDITOR
        [BoxGroup("Health System")]
        [InlineEditor(InlineEditorModes.GUIAndPreview)]
        [SerializeField] private Hurtbox[] m_hurtboxes = new Hurtbox[0];

        private GameObject m_target;

        [BoxGroup("Health System")]
        [Button("Refresh")]
        private void RefreshHurtboxes()
        {
            m_hurtboxes = this.m_target.GetComponentsInChildren<Hurtbox>();
        }

        public void OnValidate(GameObject target)
        {
            this.m_target = target;
            RefreshHurtboxes();
        }

        public void OnTransformChildrenChanged(GameObject target)
        {
            Debug.Log($"[OnTransformChildrenChanged] Children of '{target.name}' have changed.");
            this.m_target = target;
            RefreshHurtboxes();
        }
#endif
    }

    public static class FOVGizmoDrawer
    {
#if UNITY_EDITOR
        public static void DrawFOV(
            Vector3 position,
            Vector3 direction,
            float range,
            float horizontalAngle,
            float verticalAngle,
            Color gizmoColor)
        {
            // Normalize direction to avoid scaling issues
            direction.Normalize();

            // Set Gizmo color
            Gizmos.color = gizmoColor;
            Handles.color = gizmoColor;
            Handles.zTest = CompareFunction.LessEqual;

            // Horizontal boundaries
            Vector3 fallbackUp = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f
                ? Vector3.forward
                : Vector3.up;

            Vector3 localUp = Vector3.Cross(Vector3.Cross(fallbackUp, direction), direction).normalized;

            Quaternion leftRot = Quaternion.AngleAxis(-horizontalAngle / 2f, localUp);
            Quaternion rightRot = Quaternion.AngleAxis(horizontalAngle / 2f, localUp);

            Vector3 leftBoundary = leftRot * direction;
            Vector3 rightBoundary = rightRot * direction;

            Gizmos.DrawRay(position, leftBoundary * range);
            Gizmos.DrawRay(position, rightBoundary * range);

            // Vertical boundaries (using local right axis for pitch rotation)
            Vector3 rightAxis = Vector3.Cross(Vector3.up, direction).normalized;
            if (rightAxis == Vector3.zero)
                rightAxis = Vector3.right; // fallback in case direction is straight up/down

            Vector3 upBoundary = Quaternion.AngleAxis(-verticalAngle / 2f, rightAxis) * direction;
            Vector3 downBoundary = Quaternion.AngleAxis(verticalAngle / 2f, rightAxis) * direction;

            Gizmos.DrawRay(position, upBoundary * range);
            Gizmos.DrawRay(position, downBoundary * range);

            // Draw Arcs
            Handles.DrawWireArc(position, localUp, leftBoundary, horizontalAngle, range);
            Handles.DrawWireArc(position, rightAxis, upBoundary, verticalAngle, range);

        }

        public static void DrawFrustum(
        Vector3 position,
        Vector3 direction,
        float range,
        float horizontalFOV,
        float verticalFOV,
        Color gizmoColor)
        {
            direction.Normalize();

            // Setup colore e zTest per trasparenza come Gizmos
            Color semiTransparent = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.25f);
            Handles.color = semiTransparent;
            Handles.zTest = CompareFunction.LessEqual;

            // Calcolo assi locali
            Vector3 fallbackUp = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            Vector3 localRight = Vector3.Cross(fallbackUp, direction).normalized;
            Vector3 localUp = Vector3.Cross(direction, localRight).normalized;

            float hHalf = horizontalFOV * 0.5f * Mathf.Deg2Rad;
            float vHalf = verticalFOV * 0.5f * Mathf.Deg2Rad;

            // Calcola estensione orizzontale e verticale a distanza range
            float frustumHeight = Mathf.Tan(vHalf) * range;
            float frustumWidth = Mathf.Tan(hHalf) * range;

            // Vertici del rettangolo frontale del frustum
            Vector3 centerFar = position + direction * range;

            Vector3 topLeft = centerFar + localUp * frustumHeight - localRight * frustumWidth;
            Vector3 topRight = centerFar + localUp * frustumHeight + localRight * frustumWidth;
            Vector3 bottomLeft = centerFar - localUp * frustumHeight - localRight * frustumWidth;
            Vector3 bottomRight = centerFar - localUp * frustumHeight + localRight * frustumWidth;

            // Disegna raggi dal punto d'origine ai 4 angoli
            Handles.DrawLine(position, topLeft);
            Handles.DrawLine(position, topRight);
            Handles.DrawLine(position, bottomLeft);
            Handles.DrawLine(position, bottomRight);

            // Cornice frontale
            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);

            // (Opzionale) draw outline near plane o griglia
            // (puoi anche disegnare i piani con DrawSolidRectangleWithOutline, se vuoi effetti più volumetrici)
        }
#endif
    }

    public static class FOVUtility
    {
        /// <summary>
        /// Checks if a target is within a 3D field of view (frustum) defined by origin, forward direction, and FOV angles.
        /// </summary>
        /// <param name="origin">Position of the observer.</param>
        /// <param name="forward">Forward direction of the observer (will be normalized).</param>
        /// <param name="targetPosition">World position of the target to test.</param>
        /// <param name="horizontalFOV">Horizontal field of view in degrees.</param>
        /// <param name="verticalFOV">Vertical field of view in degrees.</param>
        /// <returns>True if the target is inside the FOV cone.</returns>
        public static bool IsInFOV(
            Vector3 origin,
            Vector3 forward,
            Vector3 targetPosition,
            float horizontalFOV,
            float verticalFOV)
        {
            forward.Normalize();

            Vector3 directionToTarget = (targetPosition - origin).normalized;

            // Horizontal angle (XZ projection)
            Vector3 forwardH = new Vector3(forward.x, 0, forward.z).normalized;
            Vector3 toTargetH = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
            float angleH = Vector3.Angle(forwardH, toTargetH);

            // Local right and up axes
            Vector3 fallback = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(fallback, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            // Project the direction onto the up-forward plane
            Vector3 projected = Vector3.ProjectOnPlane(directionToTarget, right).normalized;
            float angleV = Vector3.Angle(forward, projected);

            // Preserve sign
            float sign = Mathf.Sign(Vector3.Dot(projected, up));
            angleV *= sign;

            bool inHorizontal = angleH <= horizontalFOV * 0.5f;
            bool inVertical = Mathf.Abs(angleV) <= verticalFOV * 0.5f;

            return inHorizontal && inVertical;
        }
    }

    public interface IHaveHealth
    {
        void TakeDamage(float damage);
    }
}
