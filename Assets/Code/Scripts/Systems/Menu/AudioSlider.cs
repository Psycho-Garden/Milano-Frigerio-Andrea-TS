using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System;

namespace AF.TS.UI
{
    #region AudioSlider

    [Serializable]
    public class AudioSlider
    {
        #region Fields

        [FoldoutGroup("@GroupName")]
        [SerializeField]
        private AudioGroupType m_groupType;

        [FoldoutGroup("@GroupName")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_slider;

        [FoldoutGroup("@GroupName")]
        [SerializeField, SceneObjectsOnly]
        private Toggle m_toggle;

        [HideInInspector]
        public float LastVolume = 0.75f;

        #endregion

        #region Properties

        public AudioGroupType GroupType => this.m_groupType;
        public bool IsMuted => this.m_toggle != null && this.m_toggle.isOn;
        public string GroupName => this.m_groupType.ToString();

        #endregion

        #region Methods

        public void Initialize(AudioController controller)
        {
            if (this.m_slider != null)
            {
                this.m_slider.onValueChanged.AddListener((value) =>
                {
                    this.LastVolume = value;
                    controller.SetVolume(this.m_groupType, value);
                    controller.SaveSettings();
                });
            }

            if (this.m_toggle != null)
            {
                this.m_toggle.onValueChanged.AddListener((isMuted) =>
                {
                    if (this.m_slider != null)
                    {
                        this.m_slider.interactable = !isMuted;
                    }

                    controller.Mute(this.m_groupType, isMuted, this.LastVolume);
                    controller.SaveSettings();
                });
            }
        }

        public void ApplyMute()
        {
            if (this.m_toggle != null && this.m_slider != null)
            {
                this.m_slider.interactable = !this.m_toggle.isOn;
            }
        }

        public void SetSliderWithoutNotify(float value)
        {
            if (this.m_slider != null)
            {
                this.m_slider.SetValueWithoutNotify(value);
            }
        }

        public void SetToggleWithoutNotify(bool value)
        {
            if (this.m_toggle != null)
            {
                this.m_toggle.SetIsOnWithoutNotify(value);
            }
        }

        #endregion
    }

    #endregion
    #region Enums

    #endregion
}