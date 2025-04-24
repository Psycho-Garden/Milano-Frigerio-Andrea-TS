using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Utils;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [System.Serializable]
    public class AmmoMagazine
    {
        [FoldoutGroup("Settings")]
        [Tooltip("The bullet prefab")]
        [SerializeField, Required, AssetsOnly, RequiredIn(PrefabKind.All)] 
        private GameObject m_bulletPrefab;

        [FoldoutGroup("Settings")]
        [Tooltip("The maximum amount of ammo the magazine can hold")]
        [SerializeField, MinValue(0)] 
        private int m_magazineSize = 0;

        [FoldoutGroup("Settings")]
        [Tooltip("The amount of ammo the magazine starts with")]
        [SerializeField, PropertyRange(0, "m_magazineSize")] 
        private int m_startAmmo = 0;

        [FoldoutGroup("Settings")]
        [Tooltip("If true, the magazine is infinite")]
        [SerializeField] 
        private bool m_isInfinite = false;

        private int m_currentAmmo = 0;

        public void Init()
        {
            this.m_currentAmmo = this.m_startAmmo;

            ServiceLocator.Get<ObjectPooler>().InitializePool(this.m_bulletPrefab, this.m_magazineSize);
        }

        public void GetAmmo()
        {
            GetAmmo(1);
        }

        public void GetAmmo(int amount)
        {
            if (m_isInfinite)
            {
                return;
            }

            this.m_currentAmmo -= amount;
        }

        public void RefillAll()
        {
            this.m_currentAmmo = m_magazineSize;
        }

        public void Refill(int amount)
        {
            this.m_currentAmmo += amount;
            this.m_currentAmmo = Mathf.Clamp(this.m_currentAmmo, 0, this.m_magazineSize);
        }

        public bool Empty() => this.m_currentAmmo <= 0;
        public bool Full() => this.m_currentAmmo >= this.m_magazineSize;
        public bool HasAmmo() => this.m_currentAmmo > 0;
        public bool HasAmmo(int amount) => this.m_currentAmmo >= amount;
        public bool HasSpace() => this.m_currentAmmo < this.m_magazineSize;
        public bool HasSpace(int amount) => this.m_currentAmmo - amount >= 0;

        public GameObject BulletPrefab => this.m_bulletPrefab;
        public int MagazineCapacity => this.m_magazineSize;
        public int CurrentAmmo => this.m_currentAmmo;

    }
}