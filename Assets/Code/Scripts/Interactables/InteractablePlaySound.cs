using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Audio;

namespace AF.TS.Items
{
    public class InteractablePlaySound : Interactable
    {
        [FoldoutGroup("Sound")]
        [Tooltip("The sound to play")]
        [SerializeField]
        private AudioClip m_clip;

        [FoldoutGroup("Sound")]
        [Tooltip("If true, the sound will be played in 3D, otherwise it will be played in 2D")]
        [SerializeField]
        private bool m_spatial3D = true;

        public override void Interact()
        {
            base.Interact();

            if (m_clip == null)
            {
                return;
            }

            AudioSource audioSource = AudioManager.TrySound(m_clip);
            audioSource.transform.position = this.transform.position;

            if (audioSource != null && m_spatial3D)
            {
                audioSource.spatialBlend = m_spatial3D ? 1f : 0f;
            }

        }
    }
}
