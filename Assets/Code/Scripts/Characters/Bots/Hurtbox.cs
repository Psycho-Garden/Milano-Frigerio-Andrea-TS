using UnityEngine;
using AF.TS.Items;
using Sirenix.OdinInspector;
using AF.TS.Utils;

namespace AF.TS.Characters
{
    [HideMonoScript]
    [System.Serializable]
    public class Hurtbox : ColliderVisualizer, IIAmTarget
    {
        [BoxGroup("Health System"), PropertyOrder(-1)]
        [Tooltip("Multiplier applied to damage when this hurtbox is hit.")]
        public float damageMultiplier = 1f;

        [Tooltip("Reference to the character that owns this hurtbox.")]
        public IHaveHealth owner;

        public void TakeDamage(float baseDamage)
        {
            if (owner == null)
            {
                Debug.LogWarning($"{gameObject.name} was hit but has no owner set.");
                return;
            }

            float finalDamage = baseDamage * damageMultiplier;
            owner.TakeDamage(finalDamage);
        }
    }
}
