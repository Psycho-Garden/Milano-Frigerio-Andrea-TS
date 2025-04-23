using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Items;
using AF.TS.Utils;
using AF.TS.Weapons;

namespace AF.TS.Characters
{
    [HideMonoScript]
    [System.Serializable]
    public class Hurtbox : ColliderVisualizer, IIAmTarget
    {
        [BoxGroup("Health System"), PropertyOrder(-1)]
        [Tooltip("Multiplier applied to damage when this hurtbox is hit.")]
        [SerializeField]
        private float m_damageMultiplier = 1f;

        [BoxGroup("Health System"), PropertyOrder(-1)]
        [Tooltip("Reference to the character that owns this hurtbox.")]
        [ShowInInspector, ReadOnly]
        private IHaveHealth m_owner;

        public void SetOwner(IHaveHealth owner)
        {
            this.m_owner = owner;
        }

        public void TakeDamage(DamageData data)
        {
            if (this.m_owner == null)
            {
                Debug.LogWarning($"{gameObject.name} was hit but has no owner set.");
                return;
            }

            float finalDamage = DamageCalculator.CalculateFinalDamageByTravel(data, this.m_damageMultiplier);
            this.m_owner.TakeDamage(finalDamage);

            if (data.statusEffect != StatusEffectType.None)
            {
                this.m_owner.ApplyStatusEffect(data.statusEffect);
            }
        }
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Magic,
        Poison
    }

    public enum StatusEffectType
    {
        None,
        Burn,
        Freeze,
        Poison
    }

    public struct DamageData
    {
        public float baseDamage;
        public float criticalMultiplier;
        public DamageType damageType;
        public StatusEffectType statusEffect;
        public GameObject source;
        public Vector3 hitPoint;
        public bool isBlocked;
        public bool isDoT;

        public static DamageData Create(float damage, GameObject source, DamageType type = DamageType.Physical)
        {
            return new DamageData
            {
                baseDamage = damage,
                criticalMultiplier = 1f,
                damageType = type,
                statusEffect = StatusEffectType.None,
                source = source,
                hitPoint = Vector3.zero,
                isBlocked = false,
                isDoT = false
            };
        }
    }

    public static class DamageCalculator
    {
        public static float CalculateFinalDamage(DamageData data, float hurtboxMultiplier)
        {
            float damage = data.baseDamage * data.criticalMultiplier;
            damage *= hurtboxMultiplier;

            if (data.isBlocked)
                damage *= 0.2f;

            return Mathf.Max(0, damage);
        }

        public static float CalculateFinalDamageByTravel(DamageData data, float hurtboxMultiplier)
        {
            float damage = data.baseDamage * data.criticalMultiplier;
            damage *= hurtboxMultiplier;

            if (data.source.TryGetComponent(out NewBullet component))
            {
                if (component.BulletData.HasRange)
                {
                    damage /= component.TravelledDistance / component.BulletData.Range;
                }
            }

            if (data.isBlocked)
                damage *= 0.2f;

            return Mathf.Max(0, damage);
        }
    }
}
