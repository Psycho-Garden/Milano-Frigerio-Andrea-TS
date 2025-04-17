using UnityEngine;

namespace AF.TS.Weapons
{
    [System.Serializable]
    public class Point
    {
        [Tooltip("Local position relative to the weapon transform")]
        public Vector3 Position;

        [Tooltip("Local rotation in Euler angles")]
        public Vector3 Rotation;

        public Vector3 GetWorldPosition(Transform reference) => reference.TransformPoint(Position);
        public Quaternion GetWorldRotation(Transform reference) => reference.rotation * Quaternion.Euler(Rotation);
    }
}
