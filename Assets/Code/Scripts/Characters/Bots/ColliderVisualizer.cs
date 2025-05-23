﻿using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Utils
{
    [DisallowMultipleComponent]
    public class ColliderVisualizer : MonoBehaviour
    {
#if UNITY_EDITOR

        #region Fields ----------------------------------------------------------------------------

        [FoldoutGroup("Debug"), PropertyOrder(999)]
        [Tooltip("Choose when to show collider gizmos in the editor")]
        [SerializeField]
        private ColliderGizmoDisplayMode m_displayMode = ColliderGizmoDisplayMode.OnlyWhenSelected;

        [FoldoutGroup("Debug"), PropertyOrder(999)]
        [Tooltip("Only show enabled colliders")]
        [SerializeField, ToggleLeft] private bool m_showColliders = true;

        [FoldoutGroup("Debug"), PropertyOrder(999)]
        [Tooltip("Only show enabled colliders")]
        [SerializeField, ToggleLeft] private bool m_showCollidesrOnlyEnabled = true;

        [FoldoutGroup("Debug"), PropertyOrder(999)]
        [Tooltip("The color of the gizmos")]
        [SerializeField, ColorPalette, HideLabel] private Color m_gizmoColor = Color.green;

        #endregion

        #region Unity Methods ---------------------------------------------------------------------

        protected virtual void OnDrawGizmos()
        {
            if (m_displayMode == ColliderGizmoDisplayMode.Always && m_showColliders)
            {
                DrawCollidersGizmo();
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (m_displayMode == ColliderGizmoDisplayMode.OnlyWhenSelected && m_showColliders)
            {
                DrawCollidersGizmo();
            }
        }

        #endregion

        #region Private Methods -------------------------------------------------------------------

        private void DrawCollidersGizmo()
        {
            if (!enabled) return;

            Collider[] colliders = GetComponents<Collider>();

            // Get the Collider component
            if (colliders.Length <= 0)
            {
                return;
            }

            Gizmos.color = m_gizmoColor;

            foreach (Collider collider in colliders)
            {
                if (!collider.enabled && m_showCollidesrOnlyEnabled) continue;

                // Draw the Collider based on its type
                if (collider is BoxCollider boxCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                }
                else if (collider is SphereCollider sphereCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
                    Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
                }
                else if (collider is CapsuleCollider capsuleCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    DrawCapsuleGizmos(capsuleCollider);
                }
                else if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
                {
                    Gizmos.DrawMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
                }

                Gizmos.matrix = Matrix4x4.identity; // Reset matrix
            }
        }

        // Helper to draw a Capsule Collider
        private void DrawCapsuleGizmos(CapsuleCollider capsule)
        {
            Vector3 center = capsule.center;
            float radius = capsule.radius;
            float height = Mathf.Max(0, capsule.height / 2 - radius);

            // Draw main capsule body
            Gizmos.DrawWireSphere(center + Vector3.up * height, radius);
            Gizmos.DrawWireSphere(center - Vector3.up * height, radius);
            Gizmos.DrawLine(center + Vector3.up * height + Vector3.right * radius, center - Vector3.up * height + Vector3.right * radius);
            Gizmos.DrawLine(center + Vector3.up * height - Vector3.right * radius, center - Vector3.up * height - Vector3.right * radius);
            Gizmos.DrawLine(center + Vector3.up * height + Vector3.forward * radius, center - Vector3.up * height + Vector3.forward * radius);
            Gizmos.DrawLine(center + Vector3.up * height - Vector3.forward * radius, center - Vector3.up * height - Vector3.forward * radius);
        }

        #endregion

        public enum ColliderGizmoDisplayMode
        {
            Always,
            OnlyWhenSelected
        }

#endif
    }
}