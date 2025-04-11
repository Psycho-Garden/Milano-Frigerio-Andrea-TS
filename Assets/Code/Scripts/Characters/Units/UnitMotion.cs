using System;
using UnityEngine;

namespace AF.TS.Characters
{
    [Serializable]
    public class UnitMotion : TUnitMotion
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform cameraTarget; // Camera follow target (es. head)

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private Transform characterTransform => Character.transform;

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);
            controller = character.GetComponent<CharacterController>();

            if (controller == null)
                Debug.LogError("Character requires a CharacterController.");

            if (cameraTarget == null)
                Debug.LogWarning("Camera target not set for mouse-based rotation.");
        }

        public override void OnFixedUpdate()
        {
            if (Character.Driver == null || controller == null) return;

            // Ground check
            Vector3 groundCheckPos = characterTransform.position + Vector3.down * (controller.height / 2f - controller.radius + 0.1f);
            isGrounded = Physics.CheckSphere(groundCheckPos, groundCheckDistance, groundMask);

            // Movement input
            Vector2 input = Character.Driver.GetMovementInput();
            Vector3 inputDir = new Vector3(input.x, 0f, input.y);

            // Mouse rotation
            if (cameraTarget != null)
            {
                Vector3 flatForward = cameraTarget.forward;
                flatForward.y = 0f;

                if (flatForward.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(flatForward);
                    characterTransform.rotation = Quaternion.Slerp(
                        characterTransform.rotation,
                        targetRotation,
                        10f * Time.fixedDeltaTime
                    );
                }
            }

            // World movement relative to character forward
            Vector3 move = characterTransform.TransformDirection(inputDir.normalized) * moveSpeed;
            controller.Move(move * Time.fixedDeltaTime);

            // Jump
            if (isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            if (Character.Driver.ShouldJump() && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity
            velocity.y += gravity * Time.fixedDeltaTime;
            controller.Move(velocity * Time.fixedDeltaTime);
        }

        public override void Move(Vector2 direction) { }

        public override void Jump()
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        public override void OnDrawGizmos(Character character)
        {
#if UNITY_EDITOR
            if (controller == null) return;
            Vector3 groundCheckPos = character.transform.position + Vector3.down * (controller.height / 2f - controller.radius + 0.1f);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(groundCheckPos, groundCheckDistance);
#endif
        }
    }
}
