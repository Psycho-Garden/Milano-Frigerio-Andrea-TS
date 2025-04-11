using UnityEngine;

namespace AF.TS.Characters
{
    [System.Serializable]
    public class KeyboardDriver : TUnitDriver
    {
        public override Vector2 GetMovementInput()
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        public override bool ShouldJump()
        {
            return Input.GetButtonDown("Jump");
        }
    }
}
