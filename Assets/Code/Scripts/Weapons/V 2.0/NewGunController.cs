using System;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using AF.TS.Audio;

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class NewGunController : MonoBehaviour
    {
        #region Exposed Members: -----------------------------------------------------------------------

        [InfoBox("The <color=red>Red</color> fields are to be implemented", InfoMessageType.Warning)] // TODO: removed when implemented all fields
        [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Boxed), Required, AssetsOnly]
        private WeaponData m_weaponData;

        [FoldoutGroup("Magazine Settings")]
        [Tooltip("List of ammo magazines in this weapon.")]
        [SerializeField]
        private AmmoMagazine[] m_ammoMagazines;

        #region Structure Settings

        [FoldoutGroup("Structure Settings")]
        [Tooltip("Defines one or more muzzle points where bullets are spawned from.")]
        [SerializeField, ValidateInput(nameof(HasAtLeastOneMuzzlePoint), "There must be at least one muzzle point.")]
        private Point[] m_muzzlePoint = new Point[1];

        [FoldoutGroup("Structure Settings")]
        [Tooltip("Defines one or more extractor points where casings are ejected from.")]
        [SerializeField]
        private Point[] m_extractorPoint;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("Structure Settings/IK")]
        [BoxGroup("Structure Settings/IK/Right")]
        [Tooltip("Right hand IK reference point for animation and hand alignment.")]
        [SerializeField, InlineProperty, HideLabel]
        private Point m_IKRightPoint;

        [GUIColor("red")] //TODO: Need to be implmented
        [BoxGroup("Structure Settings/IK/Left")]
        [Tooltip("Left hand IK reference point for animation and hand alignment.")]
        [SerializeField, InlineProperty, HideLabel]
        private Point m_IKLeftPoint;

        [BoxGroup("Structure Settings/Slide")]
        [Tooltip("Slide movement definitions for visual recoil or mechanical parts.")]
        [SerializeField]
        private Slide[] m_slides;

        [BoxGroup("Structure Settings/Slide")]
        [Tooltip("If true, the slide is locked back after the last shot.")]
        [SerializeField]
        private bool m_asSlideLock = false;

        #endregion

        #region Edit

        [FoldoutGroup("Edit Settings")]
        [Tooltip("Enable gizmo editing for muzzle points in Scene view.")]
        [SerializeField, ToggleLeft]
        private bool m_editMuzzle = false;

        [FoldoutGroup("Edit Settings")]
        [Tooltip("Enable gizmo editing for extractor points in Scene view.")]
        [SerializeField, ToggleLeft]
        private bool m_editExtractor = false;

        #endregion

        #region Debug

        [FoldoutGroup("Debug")]
        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("Shooting mode index")]
        [ShowInInspector]
        public GunState CurrentState => this.m_currentState;

        [FoldoutGroup("Debug")]
        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("Shooting mode index")]
        [ShowInInspector, ReadOnly]
        public INewShootingMode CurrentShootingMode => this.m_currentShootingMode;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("Magazine index")]
        [ShowInInspector]
        public int CurrentMagazineIndex => this.m_currentMagazineIndex;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("Magazine index")]
        [ShowInInspector]
        public int CurrentAmmo => this.m_weaponData != null && this.m_ammoMagazines.Length > 0 ? this.m_ammoMagazines[this.m_currentMagazineIndex].CurrentAmmo : 0;

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [ShowInInspector, ReadOnly, ProgressBar(0f, 1f, DrawValueLabel = false)]
        private float Cooldown
        {
            get
            {
                Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();

                if (this.m_weaponData != null)
                {
                    return this.m_weaponData.MaxCooldown > 0f ? this.m_currentCooldown / this.m_weaponData.MaxCooldown : 0f;
                }

                return 0f;
            }
        }

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Reload Ammo")]
        public void DebugReload() => TryReload();

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Fire")]
        public void DebugShoot() => OnFireInput();

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Release Fire")]
        public void DebugShoot2() => OnFireRelease();

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Changing Shooting Mode")]
        public void DebugChangeShootingMode() => TryChangeShootingMode();

        [FoldoutGroup("Debug/Gameplay")]
        [Tooltip("")]
        [Button("Next Magazine")]
        public void DebugNextMagazine() => TryChangeMagazine();

        #endregion

        #endregion

        #region Validate Methods: ----------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureMuzzlePointsConsistency();
        }

        private void EnsureMuzzlePointsConsistency()
        {
            m_muzzlePoint ??= new Point[1];

            int desiredLength = Mathf.Max(1, m_extractorPoint?.Length ?? 0);
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

        #region Members: -------------------------------------------------------------------------------

        private GunState m_currentState = GunState.Idle;

        private bool m_slidesLocked = false;

        private float m_currentCooldown = 0f;
        private float m_nextShotTime = 0f;

        private int m_currentShootingModeIndex = 0;
        private INewShootingMode m_currentShootingMode = null;
        private int m_currentMagazineIndex = 0;

        #endregion

        #region Events: --------------------------------------------------------------------------------

        public event Action OnShot;
        public event Action OnShotEnd;
        public event Action OnReloadStart;
        public event Action OnReloadComplete;

        #endregion

        #region Initializers: --------------------------------------------------------------------------

        private void Awake()
        {
            foreach (AmmoMagazine ammoMagazine in this.m_ammoMagazines)
            {
                ammoMagazine.Init();
            }

            this.m_currentShootingMode = this.m_weaponData.ShootingModes[0];
            this.m_currentShootingMode.Init(this);
        }

        private void Start() { }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        #endregion

        #region Update Methods: ------------------------------------------------------------------------

        private void FixedUpdate()
        {
            this.m_currentShootingMode.OnUpdate();

            if (this.m_weaponData.HasCooldown && this.m_currentState != GunState.Firing)
            {
                float t = this.m_weaponData.MaxCooldown > 0 ? this.m_currentCooldown / this.m_weaponData.MaxCooldown : 0f;
                this.m_currentCooldown = Mathf.Max(0f, this.m_currentCooldown - this.m_weaponData.CooldownDecay.Evaluate(t) * Time.fixedDeltaTime);

                this.m_currentState = this.m_currentCooldown >= this.m_weaponData.MaxCooldown * 0.9f ? GunState.Cooldown : GunState.Idle;
            }

            if (!this.m_weaponData.HasCooldown && this.m_currentCooldown != 0f)
            {
                if(this.m_currentState == GunState.Cooldown)
                {
                    this.m_currentState = GunState.Idle;
                }

                this.m_currentCooldown = 0f;
            }
        }

        #endregion

        #region Input Methods: -------------------------------------------------------------------------

        public void OnFireInput()
        {
            if (!this.m_ammoMagazines[this.m_currentMagazineIndex].Empty())
            {
                this.m_currentShootingMode.TriggerPressed();
                OnShot?.Invoke();
            }
            else
            {
                AudioManager.TrySound(this.m_weaponData.EmptySound).transform.SetPositionAndRotation(transform.position, transform.rotation);
            }
        }

        public void OnFireRelease()
        {
            if (this.m_weaponData.AutoReload && this.m_ammoMagazines[this.m_currentMagazineIndex].CurrentAmmo <= 0)
            {
                TryReload();
            }

            this.m_currentShootingMode.TriggerReleased();
        }

        #endregion

        #region Logic Methods: -------------------------------------------------------------------------


        public void Feedback()
        {
            AudioManager.TrySound(this.m_weaponData.ShootSound).transform.SetPositionAndRotation(transform.position, transform.rotation);

            if (this.m_slides.Length <= 0)
            {
                Debug.LogWarning($"{this.m_weaponData.name} | {this.name} has no slide");
                return;
            }

            if (this.m_ammoMagazines[this.m_currentMagazineIndex].Empty() && this.m_asSlideLock)
            {
                foreach (Slide slide in this.m_slides)
                {
                    GunUtilities.AnimateSlide(
                        slide.SlideTransform,
                        slide.SlideEscursion,
                        0.1f
                    );
                }

                this.m_slidesLocked = true;
            }
            else
            {
                foreach (Slide slide in this.m_slides)
                {
                    GunUtilities.AnimateSlideFull(
                        slide.SlideTransform,
                        slide.SlideEscursion,
                        0.1f
                    );
                }
            }
        }

        public void ShootLogic()
        {
            OnShot?.Invoke();

            if (this.m_ammoMagazines[this.m_currentMagazineIndex].BulletPrefab == null)
            {
                Debug.LogError($"{this.m_weaponData.name} | {this.name} has no bullet prefab");
                return;
            }

            if (this.m_muzzlePoint.Length <= 0)
            {
                Debug.LogError($"{this.m_weaponData.name} | {this.name} has no muzzle flash points");
                return;
            }

            Debug.Log($"{this.m_weaponData.name} | {this.name} is shooting");

            if (!this.m_ammoMagazines[this.m_currentMagazineIndex].HasSpace(this.m_weaponData.BulletPerShot))
            {
                Debug.LogWarning($"{this.m_weaponData.name} | {this.name} is out of ammo");
                return;
            }

            this.m_currentState = GunState.Firing;
            this.m_nextShotTime = Time.time + this.m_weaponData.ShootRate;

            this.m_ammoMagazines[this.m_currentMagazineIndex].GetAmmo(this.m_weaponData.BulletPerShot);

            foreach (Point muzzlePoint in this.m_muzzlePoint)
            {
                if (this.m_weaponData.MuzzleFlashPrefab != null)
                {
                    GunUtilities.Flash(
                        this.transform,
                        this.m_weaponData.MuzzleFlashPrefab.name,
                        muzzlePoint.Position,
                        muzzlePoint.Rotation
                    );
                }

                GunUtilities.Shoot2(
                    this.transform,
                    this.m_ammoMagazines[this.m_currentMagazineIndex].BulletPrefab.name,
                    muzzlePoint.Position,
                    muzzlePoint.Rotation
                );
            }

            if (this.m_weaponData.CasingPrefab != null)
            {
                foreach (Point extractorPoint in this.m_extractorPoint)
                {
                    GunUtilities.EjectCasing(
                        this.transform,
                        this.m_weaponData.CasingPrefab.name,
                        extractorPoint.Position,
                        extractorPoint.Rotation
                    );
                }
            }

            if (m_weaponData.HasCooldown)
            {
                float t = this.m_weaponData.MaxCooldown > 0 ? this.m_currentCooldown / this.m_weaponData.MaxCooldown : 0f;
                float increaseAmount = this.m_weaponData.CooldownIncrease.Evaluate(t);
                this.m_currentCooldown = Mathf.Min(this.m_weaponData.MaxCooldown, this.m_currentCooldown + increaseAmount);
            }

            this.m_currentState = GunState.Idle;

            if (this.m_weaponData.AutoReload && this.CurrentAmmo <= 0)
            {
                TryReload();
            }
        }

        #endregion

        #region Public Methods: ------------------------------------------------------------------------

        public void TryReload()
        {
            if (this.m_ammoMagazines[this.m_currentMagazineIndex].Full() || this.m_currentState == GunState.Reloading)
            {
                return;
            }

            OnReloadStart?.Invoke();

            this.m_currentState = GunState.Reloading;

            AudioManager.TrySound(this.m_weaponData.ReloadSound).transform.SetPositionAndRotation(transform.position, transform.rotation);

            if (this.m_slidesLocked && this.m_asSlideLock)
            {
                foreach (Slide slide in this.m_slides)
                {
                    GunUtilities.AnimateSlide(
                       slide.SlideTransform,
                       -slide.SlideEscursion,
                       0.1f
                   );
                }

                this.m_slidesLocked = false;
            }

            float reloadDuration = this.m_weaponData.ReloadTime;

            DOVirtual.DelayedCall(reloadDuration, () =>
            {
                this.m_ammoMagazines[this.m_currentMagazineIndex].RefillAll();
                this.m_currentState = GunState.Idle;
                OnReloadComplete?.Invoke();
            }, false);
        }

        public void TryChangeShootingMode()
        {
            if (this.m_weaponData.ShootingModes.Length <= 1)
            {
                return;
            }

            this.m_currentShootingModeIndex = (this.m_currentShootingModeIndex + 1) % this.m_weaponData.ShootingModes.Length;

            this.m_currentShootingMode = this.m_weaponData.ShootingModes[this.m_currentShootingModeIndex];
            this.m_currentShootingMode.Init(this);
        }

        public void TryChangeMagazine()
        {
            if (this.m_ammoMagazines.Length <= 1)
            {
                return;
            }

            if (this.m_asSlideLock)
            {
                foreach (Slide slide in this.m_slides)
                {
                    GunUtilities.AnimateSlide(
                       slide.SlideTransform,
                       -slide.SlideEscursion,
                       0.1f
                    );
                }
            }

            this.m_currentMagazineIndex = (this.m_currentMagazineIndex + 1) % this.m_ammoMagazines.Length;
        }

        public bool CanShoot() => this.m_currentState == GunState.Idle && Time.time >= this.m_nextShotTime && !this.m_ammoMagazines[this.m_currentMagazineIndex].Empty();
        public float ShootRate => this.m_weaponData.ShootRate;
        public float NextShotTime => this.m_nextShotTime;

        public Point[] MuzzlePoint => m_muzzlePoint;
        public Point[] ExtractorPoint => m_extractorPoint;

        public bool EditMuzzle => m_editMuzzle;
        public bool EditExtractor => m_editExtractor;

        #endregion

    }

    [Serializable]
    public enum GunState
    {
        Idle,
        Firing,
        Reloading,
        Cooldown
    }
}