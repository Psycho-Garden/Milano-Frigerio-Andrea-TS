using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using AF.TS.Utils;
using AF.TS.Weapons;

namespace AF.TS.Items
{
    [HideMonoScript]
    public class Interactable : MonoBehaviour, IInteractable
    {
        const bool HARD_BLOCK_SELF_REFERENCE = true;

        [Serializable]
        public class TriggerEnterEvent : UnityEvent<Transform> { }

        [HorizontalGroup("Events/count", width: 16f), PropertyOrder(1)]
        [Tooltip("")]
        [SerializeField, HideLabel]
        protected bool m_isInteractableOnce = false;

        [HorizontalGroup("Events/count"), PropertyOrder(1)]
        [Tooltip("Max number of ricochets allowed.")]
        [SerializeField, EnableIf("m_isInteractableOnce"), MinValue(0)]
        private int m_maxPerformances = 1;

        [FoldoutGroup("Events", Expanded = true), PropertyOrder(1)]
        [Tooltip("Event triggered when the interactable is interacted")]
        [SerializeField]
        protected TriggerEnterEvent m_onInteracted = new();

        [FoldoutGroup("Events"), PropertyOrder(1)]
        [Tooltip("Color of the connections")]
        [SerializeField, ColorPalette]
        protected Color m_colorConnection = Color.red;

        private int m_interactedCount = 0;

        [FoldoutGroup("Events"), PropertyOrder(1)]
        [Tooltip("Test interactable")]
        [Button("Interact", ButtonSizes.Medium)]
        public virtual void Interact()
        {
            if (m_interactedCount++ >= m_maxPerformances && m_isInteractableOnce)
            {
                return;
            }

            this.m_onInteracted.Invoke(this.transform);
        }


        private void OnValidate()
        {
#if UNITY_EDITOR
            for (int i = 0; i < m_onInteracted.GetPersistentEventCount(); i++)
            {
                var target = m_onInteracted.GetPersistentTarget(i);
                if (target == this)
                {
                    Debug.LogWarning($"[<color=green>Interactable</color>] '{name}' has a <color=red>self-reference in m_onInteracted — this may cause recursion!</color>", this);
                }
            }

#endif
#if UNITY_EDITOR && HARD_BLOCK_SELF_REFERENCE
            for (int i = m_onInteracted.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (m_onInteracted.GetPersistentTarget(i) == this)
                {
                    Debug.LogWarning("<color=yellow>Removed self-referencing listener to prevent recursion.</color>\n[This is disabled in the code]");
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(m_onInteracted, i);
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
#endif
        }


        public virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = this.m_colorConnection;

#if UNITY_EDITOR
            if (this.m_onInteracted == null) return;

            int listenerCount = this.m_onInteracted.GetPersistentEventCount();

            for (int i = 0; i < listenerCount; i++)
            {
                UnityEngine.Object targetObj = this.m_onInteracted.GetPersistentTarget(i);
                if (targetObj is Transform targetTransform)
                {
                    Gizmos.DrawLine(this.transform.position, targetTransform.position);
                }
                else if (targetObj is Component component)
                {
                    Gizmos.DrawLine(this.transform.position, component.transform.position);
                }
            }
#endif
        }
    }

    public interface IInteractable
    {
        void Interact();
    }

}

namespace AF.TS.Audio
{
    public static class AudioManager
    {
        const string AUDIO_SOURCE_NAME = "AudioSource";

        const float DEFAULT_EXTRA_TIME = 0.1f;
        const float DEFAULT_SPATIAL_BLEND = 1f;

        public static AudioSource TrySound(AudioClip clip)
        {
            return TrySound(clip, 1f);
        }

        public static AudioSource TrySound(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                Debug.LogWarning("Audio clip is null");
                return null;
            }

            if (volume <= 0)
            {
                Debug.LogWarning("Volume is <= 0");
                return null;
            }

            ServiceLocator.Get<ObjectPooler>().Get(AUDIO_SOURCE_NAME, clip.length + DEFAULT_EXTRA_TIME).TryGetComponent(out AudioSource audioSource);

            audioSource.spatialBlend = DEFAULT_SPATIAL_BLEND;

            audioSource.PlayOneShot(clip, volume);

            return audioSource;
        }
    }
}