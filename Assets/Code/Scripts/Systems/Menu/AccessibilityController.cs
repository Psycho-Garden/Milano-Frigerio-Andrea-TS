using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using TMPro;
using AF.TS.Utils;

namespace AF.TS.UI
{
    [Serializable]
    public class AccessibilityController : MenuModule
    {
        #region Fields

        [FoldoutGroup("References")]
        [Tooltip("The dropdown where the colorblind mode is selected")]
        [SerializeField]
        private TMP_Dropdown m_dropdownColorBlindMode;

        [FoldoutGroup("References")]
        [Tooltip("The volume where the colorblind mode is applied")]
        [SerializeField]
        private Volume m_postProcessingVolume;

        private ChannelMixer m_channelMixer;

        private ColorBlindMode m_currentMode = ColorBlindMode.Normal;

        private const string SAVE_KEY = "AccessibilitySettingsData";

        #endregion

        #region SaveData

        [Serializable]
        private class SaveData
        {
            public ColorBlindMode Mode;
        }

        #endregion

        #region Initialization

        public override void Initialize()
        {
            if (this.m_postProcessingVolume == null)
            {
                Debug.LogError("PostProcessingVolume is missing!");
                return;
            }

            this.m_postProcessingVolume.profile.TryGet<ChannelMixer>(out this.m_channelMixer);

            if (this.m_channelMixer == null)
            {
                Debug.LogError("ChannelMixer effect not found in the volume profile!");
                return;
            }

            this.LoadSettings();

            this.PopulateDropdown();
            this.m_dropdownColorBlindMode.SetValueWithoutNotify((int)this.m_currentMode);
            this.m_dropdownColorBlindMode.onValueChanged.AddListener(this.OnColorBlindModeChanged);

            this.ApplyColorBlindMode();
        }

        #endregion

        #region Dropdown Methods

        private void PopulateDropdown()
        {
            this.m_dropdownColorBlindMode.ClearOptions();
            List<string> options = new();

            foreach (var mode in Enum.GetValues(typeof(ColorBlindMode)))
            {
                options.Add(mode.ToString());
            }

            this.m_dropdownColorBlindMode.AddOptions(options);
        }

        private void OnColorBlindModeChanged(int selectedIndex)
        {
            this.m_currentMode = (ColorBlindMode)selectedIndex;
            this.ApplyColorBlindMode();
            this.SaveSettings();
        }

        #endregion

        #region Apply Settings

        private void ApplyColorBlindMode()
        {
            var preset = ColorBlindPresets.GetPreset(this.m_currentMode);

            this.m_channelMixer.redOutRedIn.value = preset.red.x;
            this.m_channelMixer.redOutGreenIn.value = preset.red.y;
            this.m_channelMixer.redOutBlueIn.value = preset.red.z;

            this.m_channelMixer.greenOutRedIn.value = preset.green.x;
            this.m_channelMixer.greenOutGreenIn.value = preset.green.y;
            this.m_channelMixer.greenOutBlueIn.value = preset.green.z;

            this.m_channelMixer.blueOutRedIn.value = preset.blue.x;
            this.m_channelMixer.blueOutGreenIn.value = preset.blue.y;
            this.m_channelMixer.blueOutBlueIn.value = preset.blue.z;
        }

        #endregion

        #region Save/Load

        private void SaveSettings()
        {
            SaveData saveData = new()
            {
                Mode = this.m_currentMode
            };

            SaveSystem.Save(saveData, SAVE_KEY);
        }

        private void LoadSettings()
        {
            if (!SaveSystem.Exists(SAVE_KEY)) return;

            SaveData saveData = SaveSystem.Load<SaveData>(SAVE_KEY);
            if (saveData == null) return;

            this.m_currentMode = saveData.Mode;
        }

        #endregion
    }
}
