using UnityEngine;

namespace AF.TS.Characters
{
    public class HitBox
    {
        public Collider collider;
        public float Multiplier;

        public HitBox(Collider collider, float multiplier)
        {
            this.collider = collider;
            this.Multiplier = multiplier;

            this.collider.isTrigger = true;
        }
    }
}