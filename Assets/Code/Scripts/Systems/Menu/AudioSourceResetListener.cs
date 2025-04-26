using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Audio
{
    [HideMonoScript]
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceResetListener : MonoBehaviour
    {
        private AudioSource m_audioSource;

        private void Awake()
        {
            this.m_audioSource = this.GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            AudioResetEvent.OnAudioReset += this.HandleAudioReset;
        }

        private void OnDisable()
        {
            AudioResetEvent.OnAudioReset -= this.HandleAudioReset;
        }

        private void HandleAudioReset()
        {
            this.m_audioSource.enabled = false;
            this.m_audioSource.enabled = true;
        }
    }
}