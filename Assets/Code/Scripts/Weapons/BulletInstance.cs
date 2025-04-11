using UnityEngine;

namespace AF.TS.Weapons
{
    public class BulletInstance
    {
        public GameObject BulletObject;
        public Vector3 StartPosition;
        public float DistanceTraveled;

        public BulletInstance(GameObject bullet, Vector3 start)
        {
            BulletObject = bullet;
            StartPosition = start;
            DistanceTraveled = 0f;
        }
    }
}
