using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using AF.TS.Utils;
using AF.TS.Weapons;

namespace AF.TS.Items
{
    [HideMonoScript]
    public class Interactable : MonoBehaviour, IInteractable
    {
        [Serializable]
        public class TriggerEnterEvent : UnityEvent<Transform> { }

        [FoldoutGroup("Events", Expanded = true)]
        [Tooltip("Event triggered when the interactable is interacted"), PropertyOrder(1)]
        [SerializeField]
        protected TriggerEnterEvent m_onInteracted = new();

        [FoldoutGroup("Events")]
        [Tooltip("Color of the connections"), PropertyOrder(1)]
        [SerializeField, ColorPalette]
        protected Color m_colorConnection = Color.red;

        [FoldoutGroup("Events")]
        [Tooltip("Test interactable"), PropertyOrder(1)]
        [Button("Interact", ButtonSizes.Medium)]
        public virtual void Interact()
        {
            this.m_onInteracted.Invoke(this.transform);
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
                Debug.LogError("Audio clip is null");
                return null;
            }

            if (volume <= 0)
            {
                Debug.LogError("Volume is <= 0");
                return null;
            }

            ServiceLocator.Get<ObjectPooler>().Get(AUDIO_SOURCE_NAME, clip.length + DEFAULT_EXTRA_TIME).TryGetComponent(out AudioSource audioSource);

            audioSource.spatialBlend = DEFAULT_SPATIAL_BLEND;

            audioSource.PlayOneShot(clip, volume);

            return audioSource;
        }
    }
}