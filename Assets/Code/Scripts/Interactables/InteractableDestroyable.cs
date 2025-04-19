using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Utils;
using AF.TS.Weapons;

namespace AF.TS.Items
{
    public class InteractableDestroyable : InteractablePlaySound
    {
        [FoldoutGroup("VFX")]
        [Tooltip("Particles to spawn when the object is destroyed")]
        [SerializeField, AssetsOnly, RequiredIn(PrefabKind.All)] 
        private GameObject m_object = null;

        [FoldoutGroup("VFX")]
        [Tooltip("Time to wait before destroying the object"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)] 
        private float m_duration = 1f;

        public virtual void Start()
        {
            ServiceLocator.Get<ObjectPooler>().InitializePool(this.m_object, 2);
        }

        public override void Interact()
        {
            base.Interact();

            if (this.m_object != null)
            {
                ServiceLocator.Get<ObjectPooler>().Get(this.m_object.name, this.m_duration).transform.position = this.transform.position;
            }

            this.gameObject.SetActive(false);
        }
    }
}
