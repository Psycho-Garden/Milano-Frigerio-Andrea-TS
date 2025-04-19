using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Items
{
    public class InteractableEsplosive : InteractableDestroyable
    {
        [FoldoutGroup("Effect")]
        [Tooltip("Explosion radius"), Unit(Units.Meter)]
        [SerializeField, MinValue(0f)]
        private float m_explosionRadius = 1f;

        [FoldoutGroup("Effect")]
        [Tooltip("Force applied to nearby rigidbodies"), Unit(Units.Newton)]
        [SerializeField, MinValue(0f)]
        private float m_explosionForce = 500f;

        [FoldoutGroup("Effect")]
        [Tooltip("Maximum damage at the center of the explosion")]
        [SerializeField, MinValue(0f)]
        private float m_damage = 100f;


        [FoldoutGroup("Effect")]
        [Tooltip("Maximum collider detect in the range")]
        [SerializeField, MinValue(0)]
        private int m_maxCollider = 5;

        Collider[] hits;

        public override void Start()
        {
            base.Start();
            hits = new Collider[m_maxCollider];
        }

        public override void Interact()
        {
            Explode();
            base.Interact();
        }

        protected virtual void Explode()
        {
            Vector3 explosionCenter = this.transform.position;

            Physics.OverlapSphereNonAlloc(explosionCenter, this.m_explosionRadius, hits);

            foreach (Collider m_hit in hits)
            {
                if (m_hit == null || m_hit.transform == this.transform)
                {
                    continue;
                }

                // Add explosion force to the rigidbody if there is one
                if (m_hit.attachedRigidbody != null)
                {
                    m_hit.attachedRigidbody.AddExplosionForce(this.m_explosionForce, explosionCenter, this.m_explosionRadius, this.m_damage, ForceMode.Impulse);
                }

                if (m_hit.TryGetComponent(out Interactable interactable))
                {
                    interactable.Interact();
                }

                //IDamageable m_damageable = m_hit.GetComponent<IDamageable>();
                //if (m_damageable != null)
                //{
                //    float m_distance = Vector3.Distance(explosionCenter, m_hit.transform.position);
                //    float m_damageFactor = 1f - Mathf.Clamp01(m_distance / this.m_explosionRadius);
                //    float m_finalDamage = this.m_damage * m_damageFactor;

                //    m_damageable.TakeDamage(m_finalDamage);
                //}
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, this.m_explosionRadius);
        }
    }
}
