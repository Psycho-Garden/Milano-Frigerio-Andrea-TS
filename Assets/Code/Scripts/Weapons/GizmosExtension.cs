using UnityEngine;

namespace AF.TS.Weapons
{
    public static class GizmosExtension
    {
        public static void DrawAxis(Transform transform, Point point, float size)
        {
            Vector3 position = point.GetWorldPosition(transform);
            Quaternion rotation = point.GetWorldRotation(transform);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + rotation * Vector3.right * size);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + rotation * Vector3.up * size);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + rotation * Vector3.forward * size);
        }
    }
}