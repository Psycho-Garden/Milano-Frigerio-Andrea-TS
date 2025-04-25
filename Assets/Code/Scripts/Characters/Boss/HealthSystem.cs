using UnityEngine;
using System;
using Sirenix.OdinInspector;
using AF.TS.Utils;

namespace AF.TS.Characters
{
    [HideMonoScript]
    [Serializable]
    public class HealthSystem : MonoBehaviour
    {
        #region Exposed Members --------------------------------------------------------------------

        [FoldoutGroup("Stats")]
        [Tooltip("The maximum health")]
        [SerializeField, MinValue(0f)]
        private float m_maxHealth = 100f;

        [FoldoutGroup("Setting")]
        [Tooltip("")]
        [SerializeField]
        private float m_baseMultiplier = 1f;

        [FoldoutGroup("Stats")]
        [Tooltip("")]
        [SerializeField]
        private DamageType m_vulnerabilityDamage = DamageType.Physical;

        [FoldoutGroup("Stats")]
        [Tooltip("The current health")]
        [SerializeField]
        private StatusEffectType m_vulnerabilityStatusEffect = StatusEffectType.None;

        [FoldoutGroup("On Death")]
        [Tooltip("Event triggered when the character dies")]
        [SerializeField]
        private TriggerEvent m_onDeath = new();

        [FoldoutGroup("Debug")]
        [Tooltip("Current Health")]
        [ShowInInspector, ReadOnly, ProgressBar(0, "m_maxHealth", ColorGetter = "red")]
        public float CurrentHealth
        {
            get => this.m_currentHealth;
            private set
            {
                if (Mathf.Approximately(this.m_currentHealth, value)) return;

                this.m_currentHealth = value;
                OnHealthChanged?.Invoke(HealthNormalized);
            }
        }

        [FoldoutGroup("Debug")]
        [Tooltip("")]
        [Button("Take Damage")]
        private void TakeDamageButton(float damage) => TakeDamage(damage);

#if UNITY_EDITOR

        [FoldoutGroup("Debug")]
        [SerializeField, InlineProperty, HideLabel]
        private HealthSystemEditorHelper m_editorHelper = new();

        private void OnValidate()
        {
            this.m_editorHelper.OnValidate(this.gameObject);
            this.m_onDeath.TriggerEventCheckAuto(this);
        }

        private void OnTransformChildrenChanged()
        {
            this.m_editorHelper.OnTransformChildrenChanged(this.gameObject);
        }

#endif

        #endregion

        #region Private Members --------------------------------------------------------------------

        private float m_currentHealth = 0f;

        #endregion

        #region Event ------------------------------------------------------------------------------

        public event Action<float> OnHealthChanged;
        public event Action OnTakeDamage;
        public event Action OnDie;

        #endregion

        #region Callback Methods -------------------------------------------------------------------

        private void Awake()
        {
            this.m_currentHealth = this.m_maxHealth;

            foreach (var hurtbox in GetComponentsInChildren<Hurtbox>())
            {
                hurtbox.SetOwner(this);
            }
        }

        #endregion

        #region Public Methods ---------------------------------------------------------------------

        public void TakeDamage(float damage)
        {
            if (damage > 0f)
            {
                OnTakeDamage?.Invoke();
            }

            this.m_currentHealth -= damage;
            OnHealthChanged?.Invoke(HealthNormalized);

            if (this.m_currentHealth <= 0f)
            {
                OnDie?.Invoke();
                this.m_onDeath?.Invoke(this.transform);
            }
        }

        public void TakeDamage(float damage, DamageType damageType)
        {
            if ((damageType & this.m_vulnerabilityDamage) != 0)
            {
                this.m_currentHealth -= damage * this.m_baseMultiplier;
                OnHealthChanged?.Invoke(HealthNormalized);
            }
        }

        public void ApplyStatusEffect(StatusEffectType effect) { }

        public float Health => this.m_currentHealth;
        public float MaxHealth => this.m_maxHealth;
        public float HealthNormalized => this.m_currentHealth / this.m_maxHealth;

        #endregion
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
            if (m_target == null)
            {
                return;
            }

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
}