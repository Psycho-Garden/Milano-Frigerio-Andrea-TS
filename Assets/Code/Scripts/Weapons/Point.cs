using UnityEngine;

namespace AF.TS.Weapons
{
    [System.Serializable]
    public class Point
    {
        public Vector3 Position;
        public Vector3 Rotation;

        public Vector3 GetWorldPosition(Transform reference)
        {
            return reference.TransformPoint(Position);
        }

        public Quaternion GetWorldRotation(Transform reference)
        {
            return reference.rotation * Quaternion.Euler(Rotation);
        }
    }
}
