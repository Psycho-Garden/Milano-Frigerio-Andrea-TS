using UnityEngine;

namespace AF.TS.Characters
{
    public interface IUnitDriver : IUnitCommon
    {
        Vector2 GetMovementInput();
        bool ShouldJump();
    }

}
