using System;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class GunController : MonoBehaviour
    {
        #region Exposed Members: -----------------------------------------------------------------------

        [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        private WeaponDefinition m_weaponDefinition;

        [FoldoutGroup("References")]
        [Tooltip("")]
        [SerializeField] Transform m_trigger;

        [FoldoutGroup("References")]
        [Tooltip("")]
        [SerializeField] Transform m_slide;

        [FoldoutGroup("Debug")]
        [Tooltip("If true, the gun will be visible in the scene view")]
        [SerializeField]
        private bool m_debug = false;

        [FoldoutGroup("Debug/Logs")]
        [Tooltip("If true, print log messages")]
        [SerializeField]
        private bool m_Log = false;

        [FoldoutGroup("Debug/Logs")]
        [Tooltip("If true, print warning messages")]
        [SerializeField]
        private bool m_LogWarning = false;

        [FoldoutGroup("Debug/Logs")]
        [Tooltip("If true, print error messages")]
        [SerializeField]
        private bool m_LogError = false;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [ShowInInspector, ReadOnly]
        private IShootingMode m_shootingMode;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [ShowInInspector, ReadOnly]
        private int m_currentAmmo = 0;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Reload Ammo")]
        public void DebugReload()
        {
            TryReload();
        }

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Shoot")]
        public void DebugShoot()
        {
            OnFireInput();
        }

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Changing Shooting Mode")]
        public void DebugChangeShootingMode()
        {
            TryChangeShootingMode();
        }

        [FoldoutGroup("Debug/Gizmos")]
        [Tooltip("If true, the gun will be visible in the scene view")]
        [SerializeField]
        private bool m_drawGizmos = false;

        #endregion

        #region Members: -------------------------------------------------------------------------------


        private int indexShootingMode = 0;
        private bool m_asSlideLocked = false;
        private bool m_isInReload = false;

        private AudioSource m_audioSource;

        #endregion

        #region Properties: ----------------------------------------------------------------------------

        public WeaponDefinition Definition => m_weaponDefinition;

        public float NextShootTime { get; set; }

        public int CurrentAmmo { get => m_currentAmmo; set => m_currentAmmo = value; }

        public bool IsReloading => m_isInReload;

        #endregion

        #region Events: --------------------------------------------------------------------------------

        public event Action OnShoot;
        public event Action OnReloadStart;
        public event Action OnReloadComplete;

        #endregion

        #region Initializers: --------------------------------------------------------------------------

        private void Awake()
        {
            m_shootingMode = m_weaponDefinition.GetShootingModes[0];
            m_shootingMode.Init(this);

            m_currentAmmo = m_weaponDefinition.GetMagazineSize;
        }

        private void Start()
        {
            m_audioSource = GetComponent<AudioSource>();
        }

        #endregion

        #region Update Methods: ------------------------------------------------------------------------

        private void Update()
        {
            m_shootingMode.OnUpdate();
        }

        #endregion

        #region Logic Methods: -------------------------------------------------------------------------

        public void VisualFeedback()
        {
            GunUtilities.TrySound(m_audioSource, m_weaponDefinition.GetShootSFX);

            if (m_slide == null)
            {
                if (m_LogWarning) Debug.LogWarning($"Weapon {m_weaponDefinition.name} has no slide");
                return;
            }

            if (m_currentAmmo <= 0 && m_weaponDefinition.GetAsSlideLock)
            {
                GunUtilities.AnimateSlide(m_slide, m_weaponDefinition.GetBackEscursion, m_weaponDefinition.GetFireRate * 0.5f);
                m_asSlideLocked = true;
            }
            else
            {
                GunUtilities.AnimateSlideFull(m_slide, m_weaponDefinition.GetBackEscursion, m_weaponDefinition.GetFireRate * 0.5f);
            }
        }

        public void ShootLogic()
        {
            OnShoot?.Invoke();

            if (m_weaponDefinition.GetPrefabBullet == null)
            {
                if (m_LogError) Debug.LogError($"Weapon {m_weaponDefinition.name} has no bullet prefab");
                return;
            }

            if (m_weaponDefinition.GetMuzzlePoint.Length <= 0)
            {
                if (m_LogError) Debug.LogError($"Weapon {m_weaponDefinition.name} has no muzzle flash points");
                return;
            }

            if (m_Log) Debug.Log($"Weapon {m_weaponDefinition.name} is shooting");

            for (int i = 0; i < m_weaponDefinition.GetMuzzlePoint.Length; i++)
            {
                if (m_currentAmmo <= 0)
                {
                    if (m_Log) Debug.Log($"Weapon {m_weaponDefinition.name} is out of ammo");
                    break;
                }

                m_currentAmmo--;

                Point point = m_weaponDefinition.GetMuzzlePoint[i];

                if (m_weaponDefinition.GetMuzzleFlash != null)
                {
                    GunUtilities.Flash(
                        this.transform,
                        m_weaponDefinition.GetMuzzleFlash.name,
                        point.Position,
                        point.Rotation
                    );
                }

                GunUtilities.Shoot(
                    this.transform,
                    m_weaponDefinition.GetPrefabBullet.name,
                    point.Position,
                    point.Rotation
                ).Init(
                    m_weaponDefinition.GetBulletSpeed,
                    m_weaponDefinition.GetBulletRange,
                    m_weaponDefinition.GetParabolicMotion,
                    m_weaponDefinition.GetBulletDamage
                );

                if (m_weaponDefinition.GetCasingEjectPoint != null &&
                    i < m_weaponDefinition.GetCasingEjectPoint.Length &&
                    m_weaponDefinition.GetPrefabCase != null)
                {
                    point = m_weaponDefinition.GetCasingEjectPoint[i];
                    GunUtilities.EjectCasing(
                        this.transform,
                        m_weaponDefinition.GetPrefabCase.name,
                        point.Position,
                        point.Rotation
                    );
                }
            }
        }

        #endregion

        #region Input Methods: -------------------------------------------------------------------------

        public void OnFireInput()
        {

            if (m_currentAmmo <= 0 && m_weaponDefinition.GetAutoReload)
            {
                TryReload();
            }

            m_shootingMode.Shoot();
        }

        public void OnFireRelease()
        {
            if (m_shootingMode is FullAutoShooter fa)
            {
                fa.StopFiring();
            }
        }

        #endregion

        #region Public Methods: ------------------------------------------------------------------------

        public void TryReload()
        {
            if (m_currentAmmo >= m_weaponDefinition.GetMagazineSize || m_isInReload)
            {
                return;
            }

            OnReloadStart?.Invoke();

            m_isInReload = true;

            GunUtilities.TrySound(m_audioSource, m_weaponDefinition.GetReloadSFX);

            if (m_weaponDefinition.GetAsSlideLock && m_asSlideLocked)
            {
                GunUtilities.AnimateSlide(
                    m_slide,
                    -m_weaponDefinition.GetBackEscursion,
                    m_weaponDefinition.GetFireRate * 0.5f
                );
                m_asSlideLocked = false;
            }

            float reloadDuration = m_weaponDefinition.GetReloadSpeed;

            DOVirtual.DelayedCall(reloadDuration, () =>
            {
                m_currentAmmo = m_weaponDefinition.GetMagazineSize;
                m_isInReload = false;
                OnReloadComplete?.Invoke();
            }, false);
        }

        public void TryChangeShootingMode()
        {
            if (m_weaponDefinition.GetShootingModes.Length <= 1)
            {
                return;
            }

            indexShootingMode++;
            if (indexShootingMode > m_weaponDefinition.GetShootingModes.Length - 1) indexShootingMode = 0;
            m_shootingMode = m_weaponDefinition.GetShootingModes[indexShootingMode];
            m_shootingMode.Init(this);
        }

        #endregion

        #region Gizmo Methods: -------------------------------------------------------------------------

        private void OnDrawGizmosSelected()
        {
            if (!m_drawGizmos)
            {
                return;
            }

            // Muzzle
            foreach (var point in m_weaponDefinition.GetMuzzlePoint)
            {
                GizmosExtension.DrawAxis(this.transform, point, 0.05f);
            }

            // Casing 
            foreach (var point in m_weaponDefinition.GetCasingEjectPoint)
            {
                GizmosExtension.DrawAxis(this.transform, point, 0.05f);
            }

            // ik
            GizmosExtension.DrawAxis(this.transform, m_weaponDefinition.GetLeftHandIK, 0.05f);
        }

        #endregion

    }
}