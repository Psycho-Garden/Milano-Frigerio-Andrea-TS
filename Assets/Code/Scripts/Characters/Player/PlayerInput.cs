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
        bool ShootPressed { get; }
    }

    [DefaultExecutionOrder(-99)]
    [DisallowMultipleComponent]
    [HideMonoScript]
    public class PlayerInput : MonoBehaviour, IPlayerInputService
    {
        [BoxGroup("Input")]
        [Tooltip("The input actions asset")]
        [SerializeField, AssetsOnly, Required]
        private InputActionAsset m_inputActions;

        [BoxGroup("Input")]
        [Tooltip("The name of the action map")]
        [SerializeField, ValueDropdown(nameof(GetAvailableMaps))]
        private string m_actionMapName = "Player";

        private IEnumerable<string> GetAvailableMaps()
        {
            return m_inputActions != null ? m_inputActions.actionMaps.Select(map => map.name) : new List<string>();
        }

#if UNITY_EDITOR
        [BoxGroup("Input")]
        [Tooltip("The current action map")]
        [ShowInInspector, ReadOnly]
        private InputActionMap CurrentActionMap => m_inputActions.FindActionMap(m_actionMapName);
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && m_inputActions != null && !string.IsNullOrEmpty(m_actionMapName))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        private void Awake()
        {
            var map = m_inputActions.FindActionMap(m_actionMapName);

            m_moveAction = map.FindAction("Move");
            m_lookAction = map.FindAction("Look");
            m_runAction = map.FindAction("Run");
            m_crouchAction = map.FindAction("Crouch");
            m_jumpAction = map.FindAction("Jump");
            m_interactAction = map.FindAction("Interaction");
            m_menuAction = map.FindAction("Menu");
            m_aimAction = map.FindAction("Aim");
            m_shootAction = map.FindAction("Shoot");

            m_moveAction.Enable();
            m_lookAction.Enable();
            m_runAction.Enable();
            m_crouchAction.Enable();
            m_jumpAction.Enable();
            m_interactAction.Enable();
            m_menuAction.Enable();
            m_aimAction.Enable();
            m_shootAction.Enable();
        }

        private void OnEnable() => ServiceLocator.Register<PlayerInput>(this);

        private void OnDisable() => ServiceLocator.Unregister<PlayerInput>();

        public Vector2 MoveInput => m_moveAction.ReadValue<Vector2>();
        public Vector2 LookInput => m_lookAction.ReadValue<Vector2>();
        public bool IsRunning => m_runAction.IsPressed();
        public bool IsCrouching => m_crouchAction.IsPressed();
        public bool JumpPressed => m_jumpAction.WasPressedThisFrame();
        public bool InteractPressed => m_interactAction.WasPressedThisFrame();
        public bool MenuPressed => m_menuAction.WasPressedThisFrame();
        public bool IsAiming => m_aimAction.IsPressed();
        public bool ShootPressed => m_shootAction.WasPressedThisFrame();
    }

    [HideMonoScript]
    [DisallowMultipleComponent]
    public class Character : MonoBehaviour
    {
        //[SerializeField, Required] private CharacterMovement m_movement;

        //[SerializeField, Required] private CharacterInventory m_inventory;

        //[SerializeField, Required] private CharacterStats m_stats;
    }

}
