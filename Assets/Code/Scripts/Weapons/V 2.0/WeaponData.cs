using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Unity.Cinemachine;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "WeaponData", menuName = "AF/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        #region Exposed Members: -----------------------------------------------------------------------

        #region Base Definition

        [BoxGroup("Base Definition")]
        [Tooltip("Name of the weapon.")]
        [SerializeField]
        private string m_name;

        [BoxGroup("Base Definition")]
        [Tooltip("Icon used for UI and inventory.")]
        [SerializeField]
        private Sprite m_icon;

        [BoxGroup("Base Definition")]
        [Tooltip("Defines the category or handling type of the weapon.")]
        [SerializeField]
        private WeaponType m_type = WeaponType.Short;

        [BoxGroup("Base Definition")]
        [Tooltip("Available shooting modes for this weapon (e.g., semi-auto, burst, full-auto).")]
        [SerializeReference]
        private INewShootingMode[] m_shootingMode = new ShootModeBase[0];

        #endregion

        #region Shooting Settings

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Number of bullets fired per shot.")]
        [SerializeField, MinValue(1)]
        private int m_bulletPerShot = 1;

        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Delay before the first shot after pressing the trigger."), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_shootDelay = 0f;

        [FoldoutGroup("Shooting Settings")]
        [Tooltip("Minimum time between two consecutive shots."), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_shootRate = 0.25f;

        [HorizontalGroup("Shooting Settings/cooldown", width: 16f)]
        [Tooltip("Enable progressive cooldown system after continuous firing.")]
        [SerializeField, HideLabel]
        private bool m_hasCooldown = false;

        [HorizontalGroup("Shooting Settings/cooldown")]
        [Tooltip("Maximum cooldown value before the weapon overheats."), Unit(Units.Celsius)]
        [SerializeField, EnableIf("m_hasCooldown"), MinValue(0f)]
        private float m_maxCooldown = 0f;

        [BoxGroup("Shooting Settings/cooldownValue", ShowLabel = false)]
        [Tooltip("Rate at which cooldown increases when shooting.")]
        [SerializeField, ShowIf("m_hasCooldown"), MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_cooldownIncrease;

        [BoxGroup("Shooting Settings/cooldownValue")]
        [Tooltip("Rate at which cooldown decays when not shooting.")]
        [SerializeField, ShowIf("m_hasCooldown"), MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_cooldownDecay;

        #endregion

        #region Reloading Settings

        [FoldoutGroup("Reloading Settings")]
        [Tooltip("If true, the weapon automatically reloads when out of ammo.")]
        [SerializeField]
        private bool m_autoReload = false;

        [FoldoutGroup("Reloading Settings")]
        [Tooltip("Time it takes to complete a reload."), Unit(Units.Second)]
        [SerializeField]
        private float m_reloadTime = 0.25f;

        #endregion

        #region VFX

        [FoldoutGroup("VFX")]
        [Tooltip("Prefab spawned at the muzzle when the weapon fires.")]
        [SerializeField, AssetsOnly, RequiredIn(PrefabKind.All)]
        private GameObject m_muzzleFlashPrefab;

        [FoldoutGroup("VFX")]
        [Tooltip("Prefab of the casing ejected from the weapon.")]
        [SerializeField, AssetsOnly, RequiredIn(PrefabKind.All)]
        private GameObject m_casingPrefab;

        [FoldoutGroup("VFX")]
        [Tooltip("The camera shake when the weapon fires.")]
        [SerializeField]
        private NoiseSettings m_shakeCamera;

        [FoldoutGroup("VFX")]
        [Tooltip("The duration of shake effects"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_shakeDuration = 0f;

        #endregion

        #region SFX

        [FoldoutGroup("SFX")]
        [Tooltip("Sound played when the weapon fires.")]
        [SerializeField]
        private AudioClip m_shootSound;

        [FoldoutGroup("SFX")]
        [Tooltip("Sound played when trying to fire with an empty magazine.")]
        [SerializeField]
        private AudioClip m_emptySound;

        [FoldoutGroup("SFX")]
        [Tooltip("Sound played during reload.")]
        [SerializeField]
        private AudioClip m_reloadSound;

        #endregion

        #endregion

        #region Getters --------------------------------------------------------------------------------

        // Base Definition
        public string Name => m_name;
        public Sprite Icon => m_icon;
        public WeaponType Type => m_type;
        public INewShootingMode[] ShootingModes => m_shootingMode;
        public INewShootingMode ShootingMode(int index)
        {
            Type modeType = m_shootingMode[index].GetType();
            return (INewShootingMode)Activator.CreateInstance(modeType);
        }

        // Shooting Settings
        public int BulletPerShot => m_bulletPerShot;
        public float ShootDelay => m_shootDelay;
        public float ShootRate => m_shootRate;
        public bool HasCooldown => m_hasCooldown;
        public float MaxCooldown => m_maxCooldown;
        public MultiTypeValue CooldownIncrease => m_cooldownIncrease;
        public MultiTypeValue CooldownDecay => m_cooldownDecay;

        // Reloading Settings
        public bool AutoReload => m_autoReload;
        public float ReloadTime => m_reloadTime;

        // VFX
        public GameObject MuzzleFlashPrefab => m_muzzleFlashPrefab;
        public GameObject CasingPrefab => m_casingPrefab;
        public NoiseSettings ShakeCamera => m_shakeCamera;
        public float ShakeDuration => m_shakeDuration;

        // SFX
        public AudioClip ShootSound => m_shootSound;
        public AudioClip EmptySound => m_emptySound;
        public AudioClip ReloadSound => m_reloadSound;

        #endregion
    }
}