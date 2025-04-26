using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using AF.TS.Utils;
using System.Collections.Generic;
using System.Linq;

namespace AF.TS.Characters
{
    public interface IPlayerInputService
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsRunning { get; }
        bool IsCrouching { get; }
        bool JumpPressed { get; }
        bool InteractPressed { get; }
        bool MenuPressed { get; }
        bool IsAiming { get; }
        bool ShootHeld { get; }
    }

    [DefaultExecutionOrder(-99)]
    [DisallowMultipleComponent]
    [HideMonoScript]
    public class CharacterInput : MonoBehaviour, IPlayerInputService
    {
        [BoxGroup("Input")]
        [Tooltip("The input actions asset")]
        [SerializeField, AssetsOnly, Required]
        private InputActionAsset m_inputActions;

        [BoxGroup("Input/Player Control")]
        [Tooltip("The name of the action map")]
        [SerializeField, ValueDropdown(nameof(GetAvailableMaps))]
        private string m_actionMapNamePlayerControl = "Controls";

        [BoxGroup("Input/Menu")]
        [Tooltip("The name of the action map")]
        [SerializeField, ValueDropdown(nameof(GetAvailableMaps))]
        private string m_actionMapNameMenu = "Menu";

        [BoxGroup("Input/Weapon")]
        [Tooltip("The name of the action map")]
        [SerializeField, ValueDropdown(nameof(GetAvailableMaps))]
        private string m_actionMapNameWeapon = "weapon";

        private IEnumerable<string> GetAvailableMaps()
        {
            return m_inputActions != null ? m_inputActions.actionMaps.Select(map => map.name) : new List<string>();
        }

#if UNITY_EDITOR
        [BoxGroup("Input/Player Control")]
        [Tooltip("The current action map")]
        [ShowInInspector, ReadOnly]
        private InputActionMap CurrentActionMapPlayerControl => m_inputActions.FindActionMap(m_actionMapNamePlayerControl);

        [BoxGroup("Input/Menu")]
        [Tooltip("The current action map")]
        [ShowInInspector, ReadOnly]
        private InputActionMap CurrentActionMapMenu => m_inputActions.FindActionMap(m_actionMapNameMenu);

        [BoxGroup("Input/Weapon")]
        [Tooltip("The current action map")]
        [ShowInInspector, ReadOnly]
        private InputActionMap CurrentActionMapWeapon => m_inputActions.FindActionMap(m_actionMapNameWeapon);
#endif

        private InputAction m_moveAction;
        private InputAction m_lookAction;
        private InputAction m_runAction;
        private InputAction m_crouchAction;
        private InputAction m_jumpAction;
        private InputAction m_interactAction;

        private InputAction m_menuAction;

        private InputAction m_aimAction;
        private InputAction m_shootAction;
        private InputAction m_reloadAction;
        private InputAction m_primaryGunAction;
        private InputAction m_secondaryGunAction;
        private InputAction m_tertiaryGunAction;
        private InputAction m_switchActiveWeaponAction;
        private InputAction m_switchShootingModeAction;
        private InputAction m_switchAmmoMagazineAction;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && m_inputActions != null && !string.IsNullOrEmpty(m_actionMapNamePlayerControl))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (!Application.isPlaying && m_inputActions != null && !string.IsNullOrEmpty(m_actionMapNameMenu))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (!Application.isPlaying && m_inputActions != null && !string.IsNullOrEmpty(m_actionMapNameWeapon))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        private void Awake()
        {
            var mapPlayer = m_inputActions.FindActionMap(m_actionMapNamePlayerControl);
            var mapMenu = m_inputActions.FindActionMap(m_actionMapNameMenu);
            var mapWeapon = m_inputActions.FindActionMap(m_actionMapNameWeapon);

            m_moveAction = mapPlayer.FindAction("Move");
            m_lookAction = mapPlayer.FindAction("Look");
            m_runAction = mapPlayer.FindAction("Run");
            m_crouchAction = mapPlayer.FindAction("Crouch");
            m_jumpAction = mapPlayer.FindAction("Jump");
            m_interactAction = mapPlayer.FindAction("Interaction");

            m_menuAction = mapMenu.FindAction("Menu");

            m_aimAction = mapWeapon.FindAction("Aim");
            m_shootAction = mapWeapon.FindAction("Shoot");
            m_reloadAction = mapWeapon.FindAction("Reload");
            m_primaryGunAction = mapWeapon.FindAction("PrimaryGun");
            m_secondaryGunAction = mapWeapon.FindAction("SecondaryGun");
            m_tertiaryGunAction = mapWeapon.FindAction("TertiaryGun");
            m_switchActiveWeaponAction = mapWeapon.FindAction("SwitchWeapon");
            m_switchShootingModeAction = mapWeapon.FindAction("SwitchShootingMode");
            m_switchAmmoMagazineAction = mapWeapon.FindAction("SwitchAmmoMagazine");

            m_moveAction.Enable();
            m_lookAction.Enable();
            m_runAction.Enable();
            m_crouchAction.Enable();
            m_jumpAction.Enable();
            m_interactAction.Enable();

            m_menuAction.Enable();

            m_aimAction.Enable();
            m_shootAction.Enable();
            m_reloadAction.Enable();
            m_primaryGunAction.Enable();
            m_secondaryGunAction.Enable();
            m_tertiaryGunAction.Enable();
            m_switchActiveWeaponAction.Enable();
            m_switchShootingModeAction.Enable();
            m_switchAmmoMagazineAction.Enable();
        }

        private void OnEnable() => ServiceLocator.Register<CharacterInput>(this);

        private void OnDisable() => ServiceLocator.Unregister<CharacterInput>();

        private bool IsGamePaused => Time.timeScale == 0;

        private bool AllowGameplayInput => !this.IsGamePaused;

        public Vector2 MoveInput => this.AllowGameplayInput ? this.m_moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 LookInput => this.AllowGameplayInput ? this.m_lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsRunning => this.AllowGameplayInput && this.m_runAction.IsPressed();
        public bool IsCrouching => this.AllowGameplayInput && this.m_crouchAction.IsPressed();
        public bool JumpPressed => this.AllowGameplayInput && this.m_jumpAction.WasPressedThisFrame();
        public bool InteractPressed => this.AllowGameplayInput && this.m_interactAction.WasPressedThisFrame();

        public bool MenuPressed => this.m_menuAction.WasPressedThisFrame();

        public bool IsAiming => this.AllowGameplayInput && this.m_aimAction.IsPressed();
        public bool ShootHeld => this.AllowGameplayInput && this.m_shootAction.IsPressed();
        public bool ReloadPressed => this.AllowGameplayInput && this.m_reloadAction.WasPressedThisFrame();
        public bool PrimaryGunPressed => this.AllowGameplayInput && this.m_primaryGunAction.WasPressedThisFrame();
        public bool SecondaryGunPressed => this.AllowGameplayInput && this.m_secondaryGunAction.WasPressedThisFrame();
        public bool TertiaryGunPressed => this.AllowGameplayInput && this.m_tertiaryGunAction.WasPressedThisFrame();
        public bool SwitchActiveWeaponPressed => this.AllowGameplayInput && this.m_switchActiveWeaponAction.WasPressedThisFrame();
        public bool SwitchShootingModePressed => this.AllowGameplayInput && this.m_switchShootingModeAction.WasPressedThisFrame();
        public bool SwitchAmmoMagazinePressed => this.AllowGameplayInput && this.m_switchAmmoMagazineAction.WasPressedThisFrame();

    }
}