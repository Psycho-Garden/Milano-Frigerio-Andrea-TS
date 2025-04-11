using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AF.TS.Characters
{
    [Serializable]
    public class InputActionDriver : TUnitDriver
    {
        [SerializeField] private InputActionAsset inputAsset;
        [SerializeField] private string actionMapName = "Gameplay";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";

        private InputActionMap actionMap;
        private InputAction moveAction;
        private InputAction jumpAction;

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);

            if (inputAsset == null)
            {
                Debug.LogError("InputActionAsset is not assigned in InputActionDriver.");
                return;
            }

            actionMap = inputAsset.FindActionMap(actionMapName, true);
            moveAction = actionMap.FindAction(moveActionName, true);
            jumpAction = actionMap.FindAction(jumpActionName, true);

            actionMap.Enable();
        }

        public override void OnDispose(Character character)
        {
            base.OnDispose(character);
            actionMap?.Disable();
        }

        public override Vector2 GetMovementInput()
        {
            return moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public override bool ShouldJump()
        {
            return jumpAction != null && jumpAction.triggered;
        }
    }
}
