using System;
using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Utils;
using UnityEngine.UIElements;
using AF.TS.Weapons;
using Unity.Cinemachine;
using Unity.VisualScripting;
using AF.TS.Characters;

namespace AF.TS.Characters
{
    [DefaultExecutionOrder(-95)]
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class Character : MonoBehaviour
    {
        #region Exposed Members --------------------------------------------------------------------

        [BoxGroup("Settings")]
        [Tooltip("The movement module of the character")]
        [SerializeReference, Required, InlineEditor(InlineEditorObjectFieldModes.Boxed), HideLabel]
        protected IModuleMovement m_movement;

        [BoxGroup("Settings")]
        [Tooltip("")]
        [SerializeReference, Required, InlineEditor(InlineEditorObjectFieldModes.Boxed), HideLabel]
        protected IModuleStats m_stats;

        [BoxGroup("Settings")]
        [Tooltip("")]
        [SerializeReference, Required, InlineEditor(InlineEditorObjectFieldModes.Boxed), HideLabel]
        protected IModuleInventory m_inventory;

        [FoldoutGroup("References")]
        [Tooltip("The main camera of the character")]
        [SerializeField, Required, SceneObjectsOnly]
        protected Camera m_mainCamera;

        #endregion

        #region Private Members --------------------------------------------------------------------

        [FoldoutGroup("Debug")]
        [Tooltip("The input module of the character")]
        [ShowInInspector, ReadOnly]
        protected CharacterInput m_input;

        #endregion

        #region Events -----------------------------------------------------------------------------

        public static event Action OnCharacterDied;

        #endregion

        #region Initialization ---------------------------------------------------------------------

        protected void Awake() { }

        protected void Start()
        {
            this.m_input = ServiceLocator.Get<CharacterInput>();

            this.m_movement?.Init(this);
            this.m_inventory?.Init(this);
            this.m_stats?.Init(this);

            this.m_movement?.OnStart();
            this.m_inventory?.OnStart();
            this.m_stats?.OnStart();
        }

        protected void OnEnable()
        {
            ServiceLocator.Register<Character>(this);
        }

        protected void OnDisable()
        {
            ServiceLocator.Unregister<Character>();
        }

        protected void OnDestroy() { }

        #endregion

        #region Update -----------------------------------------------------------------------------

        protected void Update()
        {
            this.m_movement?.OnUpdate();
            this.m_inventory?.OnUpdate();
            this.m_stats?.OnUpdate();
        }

        protected void LateUpdate()
        {
            this.m_movement?.OnLateUpdate();
            this.m_inventory?.OnLateUpdate();
            this.m_stats?.OnLateUpdate();
        }

        protected void FixedUpdate()
        {
            this.m_movement?.OnFixedUpdate();
            this.m_inventory?.OnFixedUpdate();
            this.m_stats?.OnFixedUpdate();
        }

        #endregion

        #region Private Methods --------------------------------------------------------------------
        #endregion

        #region Public Methods ---------------------------------------------------------------------

        public CharacterInput Input => this.m_input;
        public Camera Camera => this.m_mainCamera;

        public CharacterMovement Movement => this.m_movement as CharacterMovement;
        public CharacterInventory Inventory => this.m_inventory as CharacterInventory;
        public CharacterStats Stats => this.m_stats as CharacterStats;

        #endregion

        #region Gizmos -----------------------------------------------------------------------------

        protected void OnDrawGizmosSelected()
        {
            this.m_movement?.OnDrawGizmosSelected();
            this.m_inventory?.OnDrawGizmosSelected();
            this.m_stats?.OnDrawGizmosSelected();
        }

        #endregion

    }


    [Serializable]
    public abstract class TModule : IModule
    {
        protected Character Character { get; private set; }

        public virtual void Init(Character character)
        {
            Character = character;
        }

        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnDrawGizmosSelected() { }
    }

    [Serializable]
    public class CharacterMovement : TModule, IModuleMovement
    {
        #region Exposed Members --------------------------------------------------------------------

        [FoldoutGroup("Movement Settings")]
        [Tooltip("The speed of the character"), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_walkSpeed = 2f;

        [HorizontalGroup("Movement Settings/running", width: 16f)]
        [Tooltip("Enable running")]
        [SerializeField, HideLabel]
        private bool m_canRun = false;

        [HorizontalGroup("Movement Settings/running")]
        [Tooltip("The speed of the character when running"), Unit(Units.MetersPerSecond)]
        [SerializeField, EnableIf("m_canRun"), MinValue(0f)]
        private float m_runSpeed = 0f;

        [HorizontalGroup("Movement Settings/run crouch data", width: 16f, VisibleIf = "@(m_canRun && m_canCrouch)")]
        [Tooltip("Enable running while crouching")]
        [SerializeField, HideLabel]
        private bool m_canRunWhileCrouching = false;

        [HorizontalGroup("Movement Settings/run crouch data")]
        [Tooltip("The speed of the character when crouching and running"), Unit(Units.MetersPerSecond)]
        [SerializeField, EnableIf("m_canRunWhileCrouching"), MinValue(0f)]
        private float m_crouchRunSpeed = 0f;

        [HorizontalGroup("Movement Settings/run jump data", width: 16f, VisibleIf = "@(m_canRun && m_canJump)")]
        [Tooltip("Enable running while jumping")]
        [SerializeField, HideLabel]
        private bool m_canRunWhileJumping = false;

        [HorizontalGroup("Movement Settings/run jump data")]
        [Tooltip("The speed of the character when jumping and running"), Unit(Units.MetersPerSecond)]
        [SerializeField, EnableIf("m_canRunWhileJumping"), MinValue(0f)]
        private float m_jumpRunSpeed = 0f;

        [HorizontalGroup("Movement Settings/crounch", width: 16f)]
        [Tooltip("Enable crouching")]
        [SerializeField, HideLabel]
        private bool m_canCrouch = false;

        [HorizontalGroup("Movement Settings/crounch")]
        [Tooltip("The speed of the character when crouching"), Unit(Units.MetersPerSecond)]
        [SerializeField, EnableIf("m_canCrouch"), MinValue(0f)]
        private float m_crouchSpeed = 0f;

        [BoxGroup("Movement Settings/crounch data", showLabel: false, VisibleIf = "m_canCrouch")]
        [Tooltip("The height of the character when standing"), Unit(Units.Meter)]
        [SerializeField]
        private float m_standingHeight = 2f;

        [BoxGroup("Movement Settings/crounch data", showLabel: false, VisibleIf = "m_canCrouch")]
        [Tooltip("The height of the character when crouching"), Unit(Units.Meter)]
        [SerializeField]
        private float m_crouchHeight = 1f;

        [HorizontalGroup("Movement Settings/jump", width: 16f)]
        [Tooltip("Enable jumping")]
        [SerializeField, HideLabel]
        private bool m_canJump = false;

        [HorizontalGroup("Movement Settings/jump")]
        [Tooltip("The height of the jump"), Unit(Units.Meter)]
        [SerializeField, EnableIf("m_canJump"), MinValue(0f)]
        private float m_jumpHeight = 0f;

        [BoxGroup("Movement Settings/jump data", showLabel: false, VisibleIf = "m_canJump")]
        [Tooltip("The speed of the character when jump"), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_jumpSpeed = 0f;

        [HorizontalGroup("Movement Settings/jump data/air jump", width: 16f)]
        [Tooltip("Enable air jumping")]
        [SerializeField, HideLabel]
        private bool m_canAirJump = false;

        [HorizontalGroup("Movement Settings/jump data/air jump")]
        [Tooltip("The maximum amount of jumps in the air")]
        [SerializeField, EnableIf("m_canAirJump"), MinValue(0)]
        private int m_maxAirJumps = 0;

        [FoldoutGroup("Movement Settings")]
        [Tooltip("The gravity of the character"), Unit(Units.MetersPerSecondSquared)]
        [SerializeField]
        private float m_gravity = -9.81f;

        [FoldoutGroup("Movement Settings")]
        [Tooltip("The character controller")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private CharacterController m_controller;

        #endregion

        #region Members ----------------------------------------------------------------------------

        private Vector3 m_velocity;
        private float m_currentHeight;

        [FoldoutGroup("Debug")]
        [Tooltip("The current jump count of the character")]
        [ShowInInspector, ReadOnly]
        private int m_jumpCount = 0;

        [FoldoutGroup("Debug")]
        [Tooltip("The current movement state of the character")]
        [ShowInInspector, ReadOnly]
        private MovementState m_state = MovementState.Idle;

        #endregion

        #region Initialization ---------------------------------------------------------------------

        public override void Init(Character character)
        {
            base.Init(character);

            this.m_currentHeight = this.m_controller.height;
        }

        #endregion

        #region Update -----------------------------------------------------------------------------

        public override void OnUpdate()
        {
            HandleInput();
            HandleRotation();
            UpdateMovementState();
        }

        public override void OnFixedUpdate()
        {
            HandleMovement();
            HandleGravity();
        }

        #endregion

        #region Private Methods --------------------------------------------------------------------

        private void HandleInput()
        {
            if (Character.Input.IsCrouching && m_canCrouch)
            {
                this.m_controller.height = Mathf.Lerp(this.m_controller.height, this.m_crouchHeight, Time.deltaTime * 10f);
            }
            else
            {
                this.m_controller.height = Mathf.Lerp(this.m_controller.height, this.m_standingHeight, Time.deltaTime * 10f);
            }

            if (
                Character.Input.JumpPressed
                && this.m_canJump
                && (this.m_controller.isGrounded || this.m_jumpCount < this.m_maxAirJumps - 1)
                && Character.Stats.HasEnoughStamina(10f)
                )
            {
                this.m_velocity.y = Mathf.Sqrt(this.m_jumpHeight * -2f * this.m_gravity);
                this.m_jumpCount++;
                this.m_state |= MovementState.Jumping;
            }

            bool isMoving = Character.Input.MoveInput.magnitude > 0.1f;

            if (isMoving && Character.Input.IsRunning && m_state.HasFlag(MovementState.Running))
            {
                Character.Stats.ConsumeStamina(5f * Time.deltaTime);

                if (!Character.Stats.HasEnoughStamina(0.1f))
                {
                    m_state &= ~MovementState.Running;
                }
            }
        }

        private void HandleMovement()
        {
            Vector2 input = Character.Input.MoveInput;
            Vector3 forward = Vector3.Scale(Character.Camera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = Character.Camera.transform.right;
            Vector3 move = (forward * input.y + right * input.x).normalized;

            float targetSpeed = GetCurrentSpeed();

            Vector3 motion = move * targetSpeed;
            this.m_velocity.x = motion.x;
            this.m_velocity.z = motion.z;

            this.m_controller.Move(this.m_velocity * Time.fixedDeltaTime);
        }

        private void HandleRotation()
        {
            //Vector2 look = Character.Input.LookInput;
            //if (look.sqrMagnitude > 0.01f)
            //{
            //    float yaw = look.x * 5f;
            //    Character.transform.Rotate(0f, yaw, 0f);
            //}

            Character.transform.rotation = Quaternion.Euler(0f, Character.Camera.transform.eulerAngles.y, 0f);
        }

        private void HandleGravity()
        {
            if (this.m_controller.isGrounded)
            {
                this.m_velocity.y = -2f;
                this.m_jumpCount = 0;
                this.m_state &= ~MovementState.Jumping;
            }
            else
            {
                this.m_velocity.y += this.m_gravity * Time.fixedDeltaTime;
            }
        }

        private float GetCurrentSpeed()
        {
            if (this.m_state.HasFlag(MovementState.Running))
            {
                if (this.m_state.HasFlag(MovementState.Crouching))
                {
                    return this.m_crouchRunSpeed;
                }

                if (this.m_state.HasFlag(MovementState.Jumping))
                {
                    return this.m_jumpRunSpeed;
                }

                return this.m_runSpeed;
            }

            if (this.m_state.HasFlag(MovementState.Jumping))
            {
                return this.m_jumpSpeed;
            }

            if (this.m_state.HasFlag(MovementState.Crouching))
            {
                return this.m_crouchSpeed;
            }

            return this.m_walkSpeed;
        }

        private void UpdateMovementState()
        {
            this.m_state = MovementState.Idle;

            if (Character.Input.MoveInput.magnitude > 0.1f)
            {
                this.m_state |= MovementState.Walking;
            }

            if (Character.Input.IsRunning && this.m_canRun && Character.Stats.HasEnoughStamina(1f))
            {
                this.m_state |= MovementState.Running;
            }

            if (Character.Input.IsCrouching && this.m_canCrouch)
            {
                this.m_state |= MovementState.Crouching;
            }

            if (!m_controller.isGrounded)
            {
                this.m_state |= MovementState.Jumping;
            }
        }

        #endregion

        #region Public Methods ---------------------------------------------------------------------

        public bool IsRunning => this.m_state.HasFlag(MovementState.Running);

        public bool IsJumping => this.m_state.HasFlag(MovementState.Jumping);

        public bool IsCrouching => this.m_state.HasFlag(MovementState.Crouching);

        public bool IsWalking => this.m_state.HasFlag(MovementState.Walking);

        public bool IsIdle => this.m_state.HasFlag(MovementState.Idle);

        #endregion

        #region Gizmos -----------------------------------------------------------------------------

        public override void OnDrawGizmosSelected()
        {
            if (Character == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Character.transform.position, Character.transform.forward * 2f);
        }

        #endregion

        [Flags]
        [Serializable]
        private enum MovementState
        {
            Idle = 0,
            Walking = 1 << 0,
            Running = 1 << 1,
            Crouching = 1 << 2,
            Jumping = 1 << 3
        }

    }

    [Serializable]
    public class CharacterStats : TModule, IModuleStats
    {
        #region Exposed Members --------------------------------------------------------------------

        [BoxGroup("Stats")]
        [Tooltip("Maximum Health")]
        [SerializeField, MinValue(0)]
        private float m_maxHealth = 100f;

        [BoxGroup("Stats")]
        [Tooltip("Maximum Stamina")]
        [SerializeField, MinValue(0)]
        private float m_maxStamina = 100f;

        [FoldoutGroup("Stats/Stamina")]
        [Tooltip("The rate at which stamina regenerates")]
        [SerializeField, MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_regenStamina;

        [FoldoutGroup("Stats/Stamina")]
        [Tooltip("The rate at which stamina drains")]
        [SerializeField, MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_drainStamina;

        [FoldoutGroup("Debug")]
        [Tooltip("Current Health")]
        [ShowInInspector, ReadOnly, ProgressBar(0, "m_maxHealth", ColorGetter = "red")]
        public float CurrentHealth
        {
            get => m_currentHealth;
            private set
            {
                if (Mathf.Approximately(m_currentHealth, value)) return;

                m_currentHealth = value;
                OnHealthChanged?.Invoke(HealthNormalized);
            }
        }

        [FoldoutGroup("Debug")]
        [Tooltip("Current Stamina")]
        [ShowInInspector, ReadOnly, ProgressBar(0, "m_maxStamina", ColorGetter = "yellow")]
        public float CurrentStamina
        {
            get => m_currentStamina;
            private set
            {
                if (Mathf.Approximately(m_currentStamina, value)) return;

                m_currentStamina = value;
                OnStaminaChanged?.Invoke(StaminaNormalized);
            }
        }

        #endregion

        #region Runtime Members --------------------------------------------------------------------

        private float m_currentHealth = 0f;
        private float m_currentStamina = 0f;

        #endregion

        #region Events -----------------------------------------------------------------------------

        public event Action<float> OnHealthChanged;
        public event Action<float> OnStaminaChanged;

        #endregion

        #region Initialization ---------------------------------------------------------------------

        public override void Init(Character character)
        {
            base.Init(character);

            m_currentHealth = m_maxHealth;
            m_currentStamina = m_maxStamina;
        }

        #endregion

        #region Update -----------------------------------------------------------------------------

        public override void OnUpdate()
        {
            RegenerateStamina();
        }

        #endregion

        #region Private Methods --------------------------------------------------------------------

        private void RegenerateStamina()
        {
            if (CurrentStamina < m_maxStamina)
            {
                float regenAmount = m_regenStamina.Evaluate(CurrentStamina) * Time.deltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina + regenAmount, m_maxStamina);
            }
        }

        public void ConsumeStamina(float amount)
        {
            float drainAmount = amount * m_drainStamina.Evaluate(CurrentStamina);
            CurrentStamina = Mathf.Max(CurrentStamina - drainAmount, 0f);
        }

        public void RestoreHealth(float amount)
        {
            this.m_currentHealth = Mathf.Min(this.m_currentHealth + amount, this.m_maxHealth);
        }

        public void TakeDamage(float amount)
        {
            this.m_currentHealth -= amount;
            this.m_currentHealth = Mathf.Max(this.m_currentHealth, 0f);
        }

        public bool HasEnoughStamina(float required) => this.m_currentStamina >= required;
        public float StaminaNormalized => this.m_currentStamina / this.m_maxStamina;
        public float HealthNormalized => this.m_currentHealth / this.m_maxHealth;

        #endregion
    }

    [Serializable]
    public class CharacterInventory : TModule, IModuleInventory
    {
        #region Exposed Members --------------------------------------------------------------------

        [BoxGroup("Inventory")]
        [Tooltip("The weapon holders or references to weapon instances")]
        [SerializeField, Required, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true), RequiredListLength(3)]
        private WeaponDataInventory[] m_weapons = new WeaponDataInventory[3];

        [BoxGroup("Camera")]
        [Tooltip("Aimed FOV")]
        [SerializeField]
        private float m_aimFOV = 40f;

        [BoxGroup("Camera")]
        [Tooltip("Camera noise amplitude while aiming")]
        [SerializeField, Range(0f, 1f)]
        private float m_aimPerlinAmplitude = 0.5f;

        [BoxGroup("Weapon Aiming")]
        [Tooltip("Speed of the transition when aiming")]
        [SerializeField]
        private float m_aimSmoothSpeed = 10f;

        [BoxGroup("Weapon Aiming")]
        [Tooltip("Allow aiming while running or jumping")]
        [SerializeField]
        private bool m_allowAimWhileRunningOrJumping = false;

        [ShowInInspector, ReadOnly]
        private string CurrentWeapon => m_weapons != null && m_weapons.Length > 0 ? m_weapons[m_currentWeaponIndex].GunController.name : "None";

        #endregion

        #region Private Members --------------------------------------------------------------------

        private int m_currentWeaponIndex = 0;
        private float m_defaultFOV;
        private float m_defaultAmplitudeGain;
        private CinemachineCamera m_camera;
        private CinemachineBasicMultiChannelPerlin m_perlinNoise;
        private CharacterMovement m_movement;

        #endregion

        #region Events -----------------------------------------------------------------------------

        public event Action<int, int, Sprite> OnWeaponChanged;
        public event Action<int, int> OnMagazineChanged;
        public event Action<int> OnAmmoChanged;
        
        #endregion

        #region Initialization ---------------------------------------------------------------------

        public override void Init(Character character)
        {
            base.Init(character);

            this.m_movement = Character.Movement;

            if (Character.Camera.TryGetComponent<CinemachineBrain>(out CinemachineBrain cameraBrain))
            {
                this.m_camera = cameraBrain.ActiveVirtualCamera as CinemachineCamera;
                this.m_defaultFOV = this.m_camera.Lens.FieldOfView;

                this.m_perlinNoise = this.m_camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                this.m_defaultAmplitudeGain = this.m_perlinNoise.AmplitudeGain;
            }
            
            if (this.m_weapons.Length > 0)
            {
                foreach (WeaponDataInventory weapon in this.m_weapons)
                {
                    weapon.GunController.gameObject.SetActive(false);
                    weapon.GunController.CameraPerlinNoise(this.m_perlinNoise);
                }

                EquipWeapon(0);
            }
        }

        #endregion

        #region Update -----------------------------------------------------------------------------

        public override void OnUpdate()
        {
            HandleAiming();
            HandleInput();
            HandleWeaponRotation();
        }

        #endregion

        #region Private Methods --------------------------------------------------------------------

        private void HandleInput()
        {
            if (Character.Input.SwitchActiveWeaponPressed)
            {
                CycleWeapon();
            }

            if (Character.Input.SwitchShootingModePressed)
            {
                this.m_weapons[m_currentWeaponIndex].GunController?.OnNextShootingMode();
            }

            if (Character.Input.SwitchAmmoMagazinePressed)
            {
                this.m_weapons[m_currentWeaponIndex].GunController?.OnNextMagazine();

                OnMagazineChanged?.Invoke(
                    this.m_weapons[m_currentWeaponIndex].GunController.CurrentAmmo, 
                    this.m_weapons[m_currentWeaponIndex].GunController.CurrentMagazineCapacity
                    );
            }

            if (Character.Input.ShootPressed)
            {
                this.m_weapons[m_currentWeaponIndex].GunController?.OnFireInput();
            }

            if (!Character.Input.ShootPressed)
            {
                this.m_weapons[m_currentWeaponIndex].GunController?.OnFireRelease();
            }

            if (Character.Input.ReloadPressed)
            {
                this.m_weapons[m_currentWeaponIndex].GunController?.OnReloadInput();
            }

            if (Character.Input.PrimaryGunPressed)
            {
                EquipWeapon(0);
            }

            if (Character.Input.SecondaryGunPressed)
            {
                EquipWeapon(1);
            }

            if (Character.Input.TertiaryGunPressed)
            {
                EquipWeapon(2);
            }
        }

        private void HandleAiming()
        {
            bool isAiming = Character.Input.IsAiming;
            bool canAim = m_allowAimWhileRunningOrJumping || (!m_movement.IsRunning && !m_movement.IsJumping);

            Vector3 targetPosition = isAiming && canAim ? m_weapons[m_currentWeaponIndex].AimOffset.Position : m_weapons[m_currentWeaponIndex].IdleOffset.Position;
            Vector3 currentPosition = m_weapons[m_currentWeaponIndex].GunController.transform.localPosition;
            m_weapons[m_currentWeaponIndex].GunController.transform.localPosition = Vector3.MoveTowards(currentPosition, targetPosition, m_aimSmoothSpeed * Time.deltaTime);

            Vector3 targetRotation = isAiming && canAim ? m_weapons[m_currentWeaponIndex].AimOffset.Rotation : m_weapons[m_currentWeaponIndex].IdleOffset.Rotation;
            Quaternion targetRotationQuaternion = Quaternion.Euler(targetRotation);
            Quaternion currentRotation = m_weapons[m_currentWeaponIndex].GunController.transform.localRotation;
            m_weapons[m_currentWeaponIndex].GunController.transform.localRotation = Quaternion.RotateTowards(currentRotation, targetRotationQuaternion, m_aimSmoothSpeed * Time.deltaTime);

            if (this.m_camera != null)
            {
                float targetFOV = isAiming ? this.m_aimFOV : this.m_defaultFOV;
                this.m_camera.Lens.FieldOfView = Mathf.Lerp(
                    this.m_camera.Lens.FieldOfView,
                    targetFOV,
                    Time.deltaTime * this.m_aimSmoothSpeed
                );

                if (this.m_perlinNoise != null)
                {
                    this.m_perlinNoise.AmplitudeGain = (isAiming && canAim) ? this.m_aimPerlinAmplitude : this.m_defaultAmplitudeGain;
                }
            }
        }

        private void CycleWeapon()
        {
            int nextIndex = (this.m_currentWeaponIndex + 1) % this.m_weapons.Length;
            EquipWeapon(nextIndex);
        }

        private void EquipWeapon(int index)
        {
            if (index < 0 || index >= this.m_weapons.Length || this.m_weapons[index] == null) return;

            this.m_weapons[this.m_currentWeaponIndex].GunController.gameObject.SetActive(false);

            this.m_weapons[m_currentWeaponIndex].GunController.OnShot -= OnAmmo;
            this.m_weapons[m_currentWeaponIndex].GunController.OnReloadComplete -= OnAmmo;

            this.m_currentWeaponIndex = index;
            this.m_weapons[this.m_currentWeaponIndex].GunController.gameObject.SetActive(true);
            this.m_weapons[this.m_currentWeaponIndex].GunController.transform.SetLocalPositionAndRotation(
                this.m_weapons[this.m_currentWeaponIndex].IdleOffset.Position,
                Quaternion.Euler(this.m_weapons[this.m_currentWeaponIndex].IdleOffset.Rotation)
                );

            OnWeaponChanged?.Invoke(
                this.m_weapons[this.m_currentWeaponIndex].GunController.CurrentAmmo, 
                this.m_weapons[this.m_currentWeaponIndex].GunController.CurrentMagazineCapacity, 
                this.m_weapons[this.m_currentWeaponIndex].GunController.Icon
                );

            this.m_weapons[m_currentWeaponIndex].GunController.OnShot += OnAmmo;
            this.m_weapons[m_currentWeaponIndex].GunController.OnReloadComplete += OnAmmo;
        }

        private void OnAmmo()
        {
            OnAmmoChanged?.Invoke(this.m_weapons[this.m_currentWeaponIndex].GunController.CurrentAmmo);
        }

        private void HandleWeaponRotation()
        {
            Vector3 oldRotation = this.m_weapons[this.m_currentWeaponIndex].GunController.transform.rotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(Character.Camera.transform.eulerAngles.x, oldRotation.y, oldRotation.z);
            this.m_weapons[this.m_currentWeaponIndex].GunController.transform.rotation = targetRotation;
        }

        #endregion

    }

    [Serializable]
    public class WeaponDataInventory
    {
        [Tooltip("The gun controller")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private NewGunController m_gunController;

        [FoldoutGroup("Idle Offset")]
        [Tooltip("The position of the weapon when not aiming")]
        [SerializeField, InlineProperty, HideLabel]
        private Point m_idleOffset;

        [FoldoutGroup("Aim Offset")]
        [Tooltip("The position of the weapon when aiming")]
        [SerializeField, InlineProperty, HideLabel]
        private Point m_aimOffset;

        public NewGunController GunController => m_gunController;
        public Point IdleOffset => m_idleOffset;
        public Point AimOffset => m_aimOffset;
    }

    public interface IModule
    {
        public void Init(Character character);
        public void OnStart();
        public void OnUpdate();
        public void OnLateUpdate();
        public void OnFixedUpdate();
        public void OnDrawGizmosSelected();
    }

    public interface IModuleMovement : IModule { }

    public interface IModuleStats : IModule { }

    public interface IModuleInventory : IModule { }


}