using UnityEngine;

namespace AF.TS.Characters
{
    public interface IUnitMotion : IUnitCommon
    {
        void Move(Vector2 direction);
        void Jump();
    }

}
