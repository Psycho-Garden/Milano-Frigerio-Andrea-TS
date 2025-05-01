using UnityEngine;
using AF.TS.Utils;
using AF.TS.Weapons;

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