using UnityEngine;
using Unity.Cinemachine;
using Sirenix.OdinInspector;
using System;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [CreateAssetMenu(menuName = "AF/Weapons/Weapon Definition")]
    [Obsolete("Use WeaponData instead", true)]
    public class WeaponDefinition : ScriptableObject
    {
        #region Fields ------------------------------------------------------------------

        [Title("Base Info")]

        [Tooltip("Name of the weapon, used for HUD and debugging.")]
        [SerializeField]
        private string m_gunName;

        [Tooltip("Defines whether the weapon is short (e.g. pistol) or long (e.g. rifle).")]
        [SerializeField]
        private WeaponType m_weaponType = WeaponType.Short;

        [Tooltip("Defines if the weapon fires in semi-automatic or automatic mode.")]
        [SerializeReference]
        private IShootingMode[] m_shootingModes;

        [Tooltip("Icon displayed in the HUD or inventory UI.")]
        [SerializeField]
        private Sprite m_gunIcon;

        // Structural -----
        [FoldoutGroup("Structural")]
        [Tooltip("Point at which at the weapon's muzzle when firing.")]
        [SerializeField, ValidateInput(nameof(HasAtLeastOneMuzzlePoint), "There must be at least one muzzle point.")]
        private Point[] m_muzzlePoint = new Point[1];

        [FoldoutGroup("Structural")]
        [Tooltip("Point at which the casing is spawned when firing.")]
        [SerializeField]
        private Point[] m_casingEjectPoint = new Point[0];

        [FoldoutGroup("Structural")]
        [Tooltip("")]
        [SerializeField]
        [AssetSelector(Paths = "Assets/Level/Prefabs", FlattenTreeView = true)]
        private GameObject m_prefabBullet;

        // Shooting -----
        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Rate of fire. Time (in seconds) between each shot.")]
        [Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_fireRate = 0.25f;

        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Delay before the first shot is fired. Useful for charged or delayed weapons.")]
        [Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_fireDelay = 0f;

        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Cooldown duration after firing, before the weapon can fire again.")]
        [Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_fireCooldownSpeed = 0f;

        // Ammo & Reload -----
        [FoldoutGroup("Ammo & Reload")]
        [Tooltip("Bullet speed in meters per second.")]
        [Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0)]
        private float m_bulletSpeed = 10f;

        [FoldoutGroup("Ammo & Reload")]
        [Tooltip("Maximum number of bullets per magazine.")]
        [SerializeField, MinValue(1)]
        private int m_magazineSize = 10;

        [FoldoutGroup("Ammo & Reload")]
        [Tooltip("Time in seconds to reload the weapon.")]
        [Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_reloadSpeed = 0f;

        [FoldoutGroup("Ammo & Reload")]
        [Tooltip("If true, the weapon will reload automatically when the magazine is empty.")]
        [SerializeField]
        private bool m_autoReload = false;

        // Damage -----
        [FoldoutGroup("Damage")]
        [Tooltip("Base damage dealt by each bullet.")]
        [SerializeField]
        private float m_bulletDamage = 10f;

        // Accuracy -----
        [FoldoutGroup("Accuracy")]
        [Tooltip("Maximum distance bullets can travel.")]
        [Unit(Units.Meter)]
        [SerializeField, MinValue(0f)]
        private float m_bulletRange;

        [FoldoutGroup("Accuracy")]
        [Tooltip("Parabolic motion applied to bullets to simulate gravity (calculated on 100m).")]
        [SerializeField]
        private AnimationCurve m_parabolicMotion;

        // Visual & Audio Effects -----
        [FoldoutGroup("Effects")]
        [BoxGroup("Effects/VFX")]
        [Tooltip("Particle effect spawned at the weapon's muzzle when firing.")]
        [SerializeField]
        private GameObject m_muzzleFlash;

        [BoxGroup("Effects/VFX")]
        [Tooltip("Effect spawned at the point of impact (e.g. hit particles).")]
        [SerializeField]
        private GameObject m_impactEffect;

        [BoxGroup("Effects/VFX")]
        [Tooltip("Effect spawned at the weapon's casing when firing.")]
        [SerializeField]
        [AssetSelector(Paths = "Assets/Level/Prefabs", FlattenTreeView = true)]
        private GameObject m_prefabCase;

        [BoxGroup("Effects/VFX")]
        [Tooltip("Camera shake effect applied when shooting (Cinemachine Noise Profile).")]
        [SerializeField]
        private NoiseSettings m_shootCameraShake;

        [BoxGroup("Effects/SFX")]
        [Tooltip("Audio clip played when shooting.")]
        [SerializeField]
        private AudioClip m_shootSFX;

        [BoxGroup("Effects/SFX")]
        [Tooltip("Audio clip played when reloading.")]
        [SerializeField]
        private AudioClip m_reloadSFX;

        [BoxGroup("Effects/Slide Animation")]
        [Tooltip("If true, when the weapon finish bullets, the slider will lock in the back position.")]
        [SerializeField]
        private bool m_asSlideLock = false;

        [BoxGroup("Effects/Slide Animation")]
        [Tooltip("Transform used in slide animation.")]
        [Unit(Units.Centimeter)]
        [SerializeField]
        private float m_backEscursion = 0f;

        // IK & Positioning -----
        [FoldoutGroup("IK & Positioning")]
        [Tooltip("Target transform for the left hand (used in long weapons with IK).")]
        [SerializeField, Required]
        private Point m_leftHandIK;


        #endregion

        #region Validation -----------------------------------------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureMuzzlePointsConsistency();
        }

        private void EnsureMuzzlePointsConsistency()
        {
            if (m_muzzlePoint == null)
                m_muzzlePoint = new Point[1];

            int desiredLength = Mathf.Max(1, m_casingEjectPoint?.Length ?? 0);
            if (m_muzzlePoint.Length < desiredLength)
            {
                var newArray = new Point[desiredLength];
                for (int i = 0; i < desiredLength; i++)
                {
                    newArray[i] = i < m_muzzlePoint.Length ? m_muzzlePoint[i] : new Point();
                }
                m_muzzlePoint = newArray;
            }
        }

        private bool HasAtLeastOneMuzzlePoint()
        {
            return m_muzzlePoint != null && m_muzzlePoint.Length > 0;
        }
#endif

        #endregion

        #region Getters -----------------------------------------------------------------

        // Base Info
        public string GetGunName => m_gunName;
        public WeaponType GetWeaponType => m_weaponType;
        public IShootingMode[] GetShootingModes => m_shootingModes;
        public Sprite GetIcon => m_gunIcon;

        // Structural
        public int GetNumberOfBarrels => m_muzzlePoint.Length;
        public Point[] GetMuzzlePoint => m_muzzlePoint;
        public Point[] GetCasingEjectPoint => m_casingEjectPoint;
        public GameObject GetPrefabBullet => m_prefabBullet;

        // Shooting
        public float GetFireRate => m_fireRate;
        public float GetFireDelay => m_fireDelay;
        public float GetFireCooldownSpeed => m_fireCooldownSpeed;

        // Ammo & Reload
        public float GetBulletSpeed => m_bulletSpeed;
        public int GetMagazineSize => m_magazineSize;
        public float GetReloadSpeed => m_reloadSpeed;
        public bool GetAutoReload => m_autoReload;

        // Damage
        public float GetBulletDamage => m_bulletDamage;

        // Accuracy
        public float GetBulletRange => m_bulletRange;
        public AnimationCurve GetParabolicMotion => m_parabolicMotion;

        // Visual & Audio Effects
        public GameObject GetMuzzleFlash => m_muzzleFlash;
        public GameObject GetImpactEffect => m_impactEffect;
        public GameObject GetPrefabCase => m_prefabCase;
        public NoiseSettings GetShootCameraShake => m_shootCameraShake;
        public AudioClip GetShootSFX => m_shootSFX;
        public AudioClip GetReloadSFX => m_reloadSFX;

        // Slide Animation
        public bool GetAsSlideLock => m_asSlideLock;
        public float GetBackEscursion => m_backEscursion / 100f;

        // IK & Positioning
        public Point GetLeftHandIK => m_leftHandIK;

        #endregion

    }
}
