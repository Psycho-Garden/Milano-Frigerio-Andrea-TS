using UnityEngine;
using Sirenix.OdinInspector.Editor;


using UnityEditor;

namespace AF.TS.Weapons
{
    [CustomEditor(typeof(NewGunController))]
    public class NewGunControllerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private void OnSceneGUI()
        {
            NewGunController gun = (NewGunController)target;
            Transform gunTransform = gun.transform;

            if (gun.EditMuzzle)
            {
                Handles.color = Color.red;
                DrawAndEditPoints(gun.MuzzlePoint, gunTransform, "Muzzle");
            }

            if (gun.EditExtractor)
            {
                Handles.color = Color.yellow;
                DrawAndEditPoints(gun.ExtractorPoint, gunTransform, "Extractor");
            }
        }

        private void DrawAndEditPoints(Point[] points, Transform reference, string labelPrefix)
        {
            if (points == null)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];

                Vector3 worldPos = point.GetWorldPosition(reference);
                Quaternion worldRot = point.GetWorldRotation(reference);

                EditorGUI.BeginChangeCheck();

                // Position handle
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, worldRot);
                // Rotation handle
                Quaternion newWorldRot = Handles.RotationHandle(worldRot, newWorldPos);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(reference, $"Move {labelPrefix} Point {i}");

                    point.Position = reference.InverseTransformPoint(newWorldPos);
                    point.Rotation = (Quaternion.Inverse(reference.rotation) * newWorldRot).eulerAngles;

                    EditorUtility.SetDirty(reference);
                }

                Handles.Label(newWorldPos + Vector3.up * 0.05f, $"{labelPrefix} {i}", EditorStyles.boldLabel);
                Handles.ArrowHandleCap(0, newWorldPos, newWorldRot, 0.15f, EventType.Repaint);
            }
        }
    }
}
