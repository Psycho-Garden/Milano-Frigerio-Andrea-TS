using UnityEngine;
using UnityEngine.Events;
using System;
using Sirenix.OdinInspector;
using AF.TS.Utils;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class NewBullet : MonoBehaviour
    {
        #region Exposed Members ----------------------------------------------------------------

        [BoxGroup("Reference")]
        [Tooltip("The bullet's data")]
        [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        private BulletData m_bulletData;

        [BoxGroup("Reference")]
        [Tooltip("The bullet's behaviour")]
        [SerializeReference, InlineProperty, HideLabel]
        private BulletBehaviour m_bulletBehaviour;

        [Serializable]
        public class TriggerEnterEvent : UnityEvent { }

        [SerializeField, Tooltip("Event triggered when the bullet collides with something")]
        private TriggerEnterEvent m_onCollision = new();

        #endregion

        #region Members ------------------------------------------------------------------------

        private Collider m_collider;
        private Rigidbody m_rigidbody;

        protected float m_travelledDistance = 0f;
        protected float m_timeAlive = 0f;

        protected RaycastHit[] m_collisions = new RaycastHit[1];

        #endregion

        #region Validation ---------------------------------------------------------------------
#if UNITY_EDITOR

        private void OnBulletDataChangedRegister()
        {
            this.m_bulletData.OnEditorValidate -= OnBulletDataChanged;

            this.m_bulletData.OnEditorValidate += OnBulletDataChanged;
        }

        private void OnBulletDataChanged()
        {
            if (this.m_bulletData.PhysicType == PhysicsMode.Physics)
            {
                if (this.m_collider == null)
                {
                    if (TryGetComponent(out this.m_collider) == false)
                    {
                        this.m_collider = gameObject.AddComponent<SphereCollider>();
                    }

                    this.m_collider.isTrigger = true;

                    SphereCollider sphereCollider = (SphereCollider)this.m_collider;
                    sphereCollider.radius = this.m_bulletData.Radius;
                }

                if (this.m_rigidbody == null)
                {
                    if (TryGetComponent(out this.m_rigidbody) == false)
                    {
                        this.m_rigidbody = gameObject.AddComponent<Rigidbody>();
                    }

                    this.m_rigidbody.isKinematic = true;
                }
            }
            else
            {
                if (this.m_collider != null)
                {
                    DestroyImmediate(this.m_collider);
                    this.m_collider = null;

                }

                if (this.m_rigidbody != null)
                {
                    DestroyImmediate(this.m_rigidbody);
                    this.m_rigidbody = null;
                }
            }
        }

        private void OnValidate()
        {
            if (this.m_bulletData == null)
            {
                Debug.LogError("Bullet Data is null");
                return;
            }

            OnBulletDataChangedRegister();
        }

#endif
        #endregion

        #region Initialization -----------------------------------------------------------------

        private void Start()
        {
            this.m_bulletBehaviour?.OnStart(this);
        }

        private void OnEnable()
        {
            this.m_bulletBehaviour?.OnEnable();

            this.m_timeAlive = 0f;
            this.m_travelledDistance = 0f;
        }

        private void OnDisable()
        {
            this.m_bulletBehaviour?.OnDisable();
        }

        #endregion

        #region Update Methods -----------------------------------------------------------------

        private void Update()
        {
            this.m_bulletBehaviour?.OnUpdate();

            this.m_timeAlive += Time.deltaTime;

            if (this.m_bulletData.HasRange && this.m_travelledDistance > this.m_bulletData.Range)
            {
                OnDispose();
            }

            if (this.m_bulletData.HasLifeTime && this.m_timeAlive > this.m_bulletData.LifeTime)
            {
                OnDispose();
            }

            if (this.m_bulletData.PhysicType == PhysicsMode.Simulated)
            {
                CheckCollision();
            }
        }

        private void CheckCollision()
        {
            int hits = Physics.SphereCastNonAlloc(this.transform.position, this.m_bulletData.Radius, this.transform.forward, this.m_collisions, 0f, this.m_bulletData.ImpactLayers);

            if (hits > 0)
            {
                OnCollision(this.m_collisions[0].collider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            OnCollision(other);
        }

        private void OnDispose()
        {
            this.m_bulletBehaviour?.OnDisable();

            ServiceLocator.Get<ObjectPooler>().ReturnToPool(this.gameObject);
        }

        #endregion

        #region Private Methods ----------------------------------------------------------------

        private void OnCollision(Collider other)
        {
            m_onCollision?.Invoke();

            // Damage passed to interactable
            if (other.TryGetComponent(out IInteractable<float> hurtbox))
            {
                hurtbox.Interact(this.m_bulletData.Damage);
            }
            else if(other.TryGetComponent(out IInteractable<string> interactable))
            {
                interactable.Interact(this.gameObject.tag);
            }

            if (this.m_bulletData.ImpactEffect != null)
            {
                ServiceLocator.Get<ObjectPooler>().Get(this.m_bulletData.ImpactEffect.name, 0.1f).transform.SetPositionAndRotation(this.m_collisions[0].point, Quaternion.Euler(-this.m_collisions[0].normal));
            }

            if (this.m_bulletData.ImpactSound != null)
            {
                ServiceLocator.Get<ObjectPooler>().Get("AudioSource", 0.5f).GetComponent<AudioSource>().PlayOneShot(this.m_bulletData.ImpactSound);
            }

            OnDispose();
        }

        #endregion

        #region Public Methods -----------------------------------------------------------------

        public BulletData BulletData => this.m_bulletData;

        public Rigidbody GetRigidbody => this.m_rigidbody;

        public float TravelledDistance
        {
            get => m_travelledDistance;
            set => m_travelledDistance = value;
        }

        public void ChangeBehavior(BulletBehaviour bulletBehaviour)
        {
            this.m_bulletBehaviour = bulletBehaviour;
            this.m_bulletBehaviour?.OnStart(this);
        }

        #endregion

        #region Gizmos Methods -----------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if (this.m_bulletData == null)
            {
                return;
            }

            this.m_bulletBehaviour?.OnDrawGizmos();

            if (this.m_bulletData.DrawBody)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(this.transform.position, this.BulletData.Radius);
            }
        }

        #endregion

    }

    public class BulletBehaviour : IBaseUnityCallback<NewBullet>
    {
        [SerializeField, HideInInspector]
        protected NewBullet m_parent;

        public BulletBehaviour() { }

        public virtual void OnStart(NewBullet parent)
        {
            this.m_parent = parent;
        }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnUpdate() { }

        public virtual void OnDispose() { }

        public virtual void OnDrawGizmos() { }
    }

    public class NormalBulletBehaviour : BulletBehaviour
    {
        protected Vector3 m_velocity = Vector3.zero;

        public override void OnStart(NewBullet parent)
        {
            base.OnStart(parent);
            this.m_velocity = this.m_parent.transform.forward * this.m_parent.BulletData.Speed;

            if (this.m_parent.BulletData.PhysicType == PhysicsMode.Physics)
            {
                var rb = this.m_parent.GetRigidbody;
                if (rb != null)
                    rb.linearVelocity = this.m_velocity;
            }
        }

        public override void OnUpdate()
        {
            if (this.m_parent.BulletData.PhysicType == PhysicsMode.Simulated)
            {
                BulletBehaviorUtilities.SimulatedMovement(this.m_parent, ref m_velocity);
            }
            // In Physics mode, Rigidbody handles movement � no update needed
        }
    }

    public class HomingBulletBehaviour : BulletBehaviour
    {
        [SerializeField] protected Transform m_target = null;
        [SerializeField] private float m_turnSpeed = 10f;
        [SerializeField] private bool m_ignoreGravity = true;

        public void SetTarget(Transform target) => this.m_target = target;

        public override void OnStart(NewBullet parent)
        {
            base.OnStart(parent);

            if (this.m_parent.BulletData.PhysicType == PhysicsMode.Physics)
            {
                var rb = this.m_parent.GetRigidbody;
                if (rb != null)
                {
                    rb.useGravity = !this.m_ignoreGravity;
                    rb.linearVelocity = this.m_parent.transform.forward * this.m_parent.BulletData.Speed;
                }
            }
        }

        public override void OnUpdate()
        {
            if (this.m_target == null)
                return;

            if (this.m_parent.BulletData.PhysicType == PhysicsMode.Simulated)
            {
                BulletBehaviorUtilities.SimulatedHoming(this.m_parent, this.m_target, this.m_turnSpeed);
            }
            else
            {
                BulletBehaviorUtilities.PhysicsHoming(this.m_parent, this.m_target, this.m_turnSpeed);
            }
        }
    }

    public static class BulletBehaviorUtilities
    {
        public static void SimulatedMovement(NewBullet bullet, ref Vector3 velocity)
        {
            float delta = Time.deltaTime;
            bullet.transform.position += velocity * delta;
            bullet.TravelledDistance += velocity.magnitude * delta;
        }

        public static void SimulatedHoming(NewBullet bullet, Transform target, float turnSpeed)
        {
            float delta = Time.deltaTime;

            Vector3 toTarget = (target.position - bullet.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            bullet.transform.rotation = Quaternion.RotateTowards(
                bullet.transform.rotation,
                targetRotation,
                turnSpeed * delta
            );

            Vector3 velocity = bullet.transform.forward * bullet.BulletData.Speed;
            bullet.transform.position += velocity * delta;
            bullet.TravelledDistance += velocity.magnitude * delta;
        }

        public static void PhysicsHoming(NewBullet bullet, Transform target, float turnSpeed)
        {
            var rb = bullet.GetRigidbody;
            if (rb == null) return;

            Vector3 toTarget = (target.position - bullet.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            bullet.transform.rotation = Quaternion.RotateTowards(
                bullet.transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            rb.linearVelocity = bullet.transform.forward * bullet.BulletData.Speed;
        }
    }

    public interface IInteractable<T>
    {
        public void Interact(T obj);
    }
}
