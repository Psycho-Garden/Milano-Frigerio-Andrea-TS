using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AF.TS.Utils
{
    public static class TriggerEventExtensions
    {
        private const bool HARD_BLOCK_SELF_REFERENCE = true;
        private const float SPHERE_RADIUS = 0.05f;

        public static void TriggerEventCheckAuto(this TriggerEvent triggerEvent, Object callingInstance)
        {
#if UNITY_EDITOR
            if (HARD_BLOCK_SELF_REFERENCE)
            {
                TriggerEventCheckAndRemoveSelfReference(triggerEvent, callingInstance);
            }
            else
            {
                TriggerEventCheck(triggerEvent, callingInstance);
            }
#endif
        }

#if UNITY_EDITOR
        public static void TriggerEventCheck(TriggerEvent triggerEvent, Object callingInstance)
        {
            var targets = triggerEvent.GetPersistentTargetObjects();

            if (targets.Contains(callingInstance))
            {
                Debug.LogWarning(
                    $"[<color=green>TriggerEvent</color>] '{callingInstance.name}' has a <color=red>self-reference in {triggerEvent.GetType().Name} — this may cause recursion!</color>",
                    callingInstance
                );
            }
        }

        public static void TriggerEventCheckAndRemoveSelfReference(TriggerEvent triggerEvent, Object callingInstance)
        {
            var indexedTargets = triggerEvent.GetPersistentTargetEntries();
            var indicesToRemove = new List<int>();

            foreach (var (index, target) in indexedTargets)
            {
                if (target == callingInstance)
                {
                    indicesToRemove.Add(index);
                }
            }

            if (indicesToRemove.Count > 0)
            {
                Debug.LogWarning(
                    $"<color=yellow>Removed {indicesToRemove.Count} self-referencing listener(s) to prevent recursion.</color>\n[This only affects the editor and is ignored at runtime]",
                    callingInstance
                );

                // Remove from last to first to avoid shifting
                indicesToRemove.Sort((a, b) => b.CompareTo(a));
                foreach (int index in indicesToRemove)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(triggerEvent, index);
                }

                EditorUtility.SetDirty(callingInstance);
            }
        }
#endif

        // Returns a HashSet of all unique UnityEngine.Object targets (GameObject or Component)
        public static HashSet<Object> GetPersistentTargetObjects(this UnityEventBase unityEvent)
        {
            var hashSet = new HashSet<Object>();
            int count = unityEvent.GetPersistentEventCount();

            for (int i = 0; i < count; i++)
            {
                var target = unityEvent.GetPersistentTarget(i);
                if (target != null)
                {
                    hashSet.Add(target);
                }
            }

            return hashSet;
        }

        // Returns index-target pairs for all persistent targets
        public static List<(int index, Object target)> GetPersistentTargetEntries(this UnityEventBase unityEvent)
        {
            var entries = new List<(int, Object)>();
            int count = unityEvent.GetPersistentEventCount();

            for (int i = 0; i < count; i++)
            {
                var target = unityEvent.GetPersistentTarget(i);
                if (target != null)
                {
                    entries.Add((i, target));
                }
            }

            return entries;
        }

#if UNITY_EDITOR

        public static void DrawConnectionGizmo(this TriggerEvent triggerEvent, Transform origin, Color color)
        {
            if (triggerEvent == null || origin == null)
                return;

            Vector3 originPos = origin.position;
            int listenerCount = triggerEvent.GetPersistentEventCount();

            if (listenerCount == 0)
                return;

            Gizmos.color = color;

            for (int i = 0; i < listenerCount; i++)
            {
                Object targetObj = triggerEvent.GetPersistentTarget(i);

                if (targetObj == null)
                    continue;

                Transform targetTransform = targetObj as Transform;
                if (targetTransform != null)
                {
                    Gizmos.DrawLine(originPos, targetTransform.position);
                    Gizmos.DrawSphere(targetTransform.position, SPHERE_RADIUS);
                }
                else if (targetObj is Component component && component.transform != null)
                {
                    Gizmos.DrawLine(originPos, component.transform.position);
                    Gizmos.DrawSphere(component.transform.position, SPHERE_RADIUS);
                }
            }
        }

#endif
    }
}
