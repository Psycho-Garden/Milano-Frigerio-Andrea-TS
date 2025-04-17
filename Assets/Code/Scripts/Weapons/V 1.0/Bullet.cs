using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Utils;
using AF.TS.Items;
namespace AF.TS.Weapons
{
    /// <summary>
    /// Handles the movement and lifecycle of a projectile bullet.
    /// Supports parabolic vertical offset via an AnimationCurve.
    /// Returns to the object pool upon collision or exceeding range.
    /// </summary>
    [HideMonoScript]
    public class Bullet : MonoBehaviour
    {
        #region Private Fields ------------------------------------------------------------------

        [ShowInInspector, ReadOnly]
        private Vector3 m_startPosition;

        [ShowInInspector, ReadOnly]
        private float m_distanceTraveled = 0f;

        private AnimationCurve m_parabolicCurve;
        private float m_speed;
        private float m_range;

        private float m_damage;

        private bool m_isInitialized = false;

        #endregion

        #region Initialization -------------------------------------------------------------------

        /// <summary>
        /// Initializes the bullet parameters.
        /// </summary>
        /// <param name="startPosition">Starting position of the bullet.</param>
        /// <param name="speed">Movement speed of the bullet.</param>
        /// <param name="range">Maximum distance the bullet can travel.</param>
        /// <param name="parabolicCurve">Curve to apply parabolic height offset.</param>
        public void Init(float speed, float range, AnimationCurve parabolicCurve, float damage)
        {
            m_startPosition = this.transform.position;
            m_speed = speed;
            m_range = range;
            m_parabolicCurve = parabolicCurve;

            m_damage = damage;

            m_isInitialized = true;
        }

        public void OnEnable()
        {
            m_distanceTraveled = 0f;
        }

        #endregion

        #region Unity Events ---------------------------------------------------------------------

        /// <summary>
        /// Handles bullet collision with any trigger collider.
        /// </summary>
        /// <param name="other">The collider the bullet has hit.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IIAmTarget>(out var target))
            {
                target.TakeDamage(m_damage);
            }

            OnDispose();
        }

        /// <summary>
        /// Updates bullet position along forward direction with optional parabolic arc.
        /// </summary>
        private void Update()
        {
            if (!m_isInitialized)
            {
                return;
            }

            float delta = m_speed * Time.deltaTime;
            m_distanceTraveled += delta;

            // Normalize distance on 100m scale for Curve evaluation
            float normalized = Mathf.Clamp01(m_distanceTraveled / 100f);
            float heightOffset = m_parabolicCurve.Evaluate(normalized);

            // Calculate forward direction movement
            Vector3 forward = transform.forward;
            Vector3 basePosition = m_startPosition + forward * m_distanceTraveled;
            Vector3 finalPosition = basePosition + Vector3.up * heightOffset;

            transform.position = finalPosition;

            // Dispose bullet if beyond max range
            if (m_distanceTraveled >= m_range)
            {
                OnDispose();
            }
        }

        #endregion

        #region Private Methods ------------------------------------------------------------------

        /// <summary>
        /// Returns the bullet to the object pool.
        /// </summary>
        private void OnDispose()
        {
            ServiceLocator.Get<ObjectPooler>().ReturnToPool(gameObject);

            m_isInitialized = false;
        }

        #endregion
    }
}
