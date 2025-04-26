using UnityEngine;
using UnityEngine.Audio;
using Sirenix.OdinInspector;
using TMPro;
using System;
using System.Collections.Generic;
using AF.TS.Utils;

namespace AF.TS.UI
{
    [Serializable]
    public class AudioController : MenuModule
    {
        #region Fields

        [FoldoutGroup("References")]
        [Tooltip("Audio mixer")]
        [SerializeField]
        private AudioMixer m_audioMixer;

        [FoldoutGroup("References")]
        [Tooltip("List of audio sliders")]
        [SerializeField]
        private List<AudioSlider> m_audioSliders = new();

        [FoldoutGroup("References")]
        [Tooltip("Dropdown to select audio mode")]
        [SerializeField]
        private TMP_Dropdown m_audioModeDropdown;

        private static readonly Dictionary<AudioGroupType, string> MixerParameterMap = new()
        {
            { AudioGroupType.Master, "Master" },
            { AudioGroupType.Music, "Music" },
            { AudioGroupType.SFX, "SFX" }
        };

        private const string SAVE_KEY = "AudioSettingsData";

        private AudioSpeakerMode m_audioMode = AudioSpeakerMode.Stereo;

        #endregion

        #region Properties

        public AudioMixer Mixer => this.m_audioMixer;
        public List<AudioSlider> Sliders => this.m_audioSliders;

        #endregion

        #region SaveData

        [Serializable]
        private class SaveData
        {
            public List<AudioVolumeData> Volumes = new();
            public List<AudioMuteData> Mutes = new();
            public AudioSpeakerMode AudioMode;
        }

        [Serializable]
        private class AudioVolumeData
        {
            public AudioGroupType Group;
            public float Volume;
        }

        [Serializable]
        private class AudioMuteData
        {
            public AudioGroupType Group;
            public bool Muted;
        }

        #endregion

        #region Initialization

        public override void Initialize()
        {
            if (this.m_audioSliders == null || this.m_audioMixer == null) return;

            // First: initialize all sliders (attach listeners)
            foreach (var audioSlider in this.m_audioSliders)
            {
                audioSlider.Initialize(this);
            }

            // Then: Load the saved settings (this will also update sliders/toggles)
            this.LoadSettings();

            // Populate and apply dropdown for Audio Mode
            DropdownHelper.PopulateAudioSpeakerModeDropdown(this.m_audioModeDropdown);
            this.m_audioModeDropdown.SetValueWithoutNotify(DropdownHelper.GetSpeakerModeIndex(this.m_audioMode));
            this.m_audioModeDropdown.onValueChanged.AddListener(this.OnAudioModeDropdownChanged);

            this.ApplyAudioMode();
        }

        #endregion

        #region Audio Handling

        public void SetVolume(AudioGroupType groupType, float value)
        {
            if (!AudioController.MixerParameterMap.TryGetValue(groupType, out var parameter)) return;

            float volumeDb = Mathf.Lerp(-80f, 0f, value);
            this.m_audioMixer.SetFloat(parameter, volumeDb);
        }

        public void Mute(AudioGroupType groupType, bool mute, float lastVolume)
        {
            if (!AudioController.MixerParameterMap.TryGetValue(groupType, out var parameter)) return;

            if (mute)
            {
                this.m_audioMixer.SetFloat(parameter, -80f);
            }
            else
            {
                float volumeDb = Mathf.Lerp(-80f, 0f, lastVolume);
                this.m_audioMixer.SetFloat(parameter, volumeDb);
            }
        }

        private void ApplyAudioMode()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.speakerMode = this.m_audioMode;
            AudioSettings.Reset(config);

            Audio.AudioResetEvent.Raise();
        }

        #endregion

        #region Settings Save/Load

        public void SaveSettings()
        {
            SaveData saveData = new();

            foreach (var slider in this.m_audioSliders)
            {
                saveData.Volumes.Add(new AudioVolumeData { Group = slider.GroupType, Volume = slider.LastVolume });
                saveData.Mutes.Add(new AudioMuteData { Group = slider.GroupType, Muted = slider.IsMuted });
            }

            saveData.AudioMode = this.m_audioMode;

            SaveSystem.Save(saveData, SAVE_KEY);
        }

        private void LoadSettings()
        {
            if (!SaveSystem.Exists(SAVE_KEY)) return;

            SaveData saveData = SaveSystem.Load<SaveData>(SAVE_KEY);
            if (saveData == null) return;

            foreach (var volumeData in saveData.Volumes)
            {
                var slider = this.m_audioSliders.Find(s => s.GroupType == volumeData.Group);
                if (slider != null)
                {
                    slider.LastVolume = volumeData.Volume;
                    slider.SetSliderWithoutNotify(volumeData.Volume);
                    this.SetVolume(slider.GroupType, slider.LastVolume);
                }
            }

            foreach (var muteData in saveData.Mutes)
            {
                var slider = this.m_audioSliders.Find(s => s.GroupType == muteData.Group);
                if (slider != null)
                {
                    slider.SetToggleWithoutNotify(muteData.Muted);
                    slider.ApplyMute();
                    this.Mute(slider.GroupType, slider.IsMuted, slider.LastVolume);
                }
            }

            this.m_audioMode = saveData.AudioMode;
            this.m_audioModeDropdown.SetValueWithoutNotify(DropdownHelper.GetSpeakerModeIndex(this.m_audioMode));
            this.ApplyAudioMode();
        }

        #endregion

        #region Dropdown Callbacks

        private void OnAudioModeDropdownChanged(int selectedIndex)
        {
            this.m_audioMode = DropdownHelper.GetSpeakerModeByIndex(selectedIndex);
            this.ApplyAudioMode();
            this.SaveSettings();
        }

        #endregion
    }
}
