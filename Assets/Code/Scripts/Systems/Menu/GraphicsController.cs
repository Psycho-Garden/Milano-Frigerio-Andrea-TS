using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using TMPro;
using AF.TS.Utils;

namespace AF.TS.UI
{
    [Serializable]
    public class GraphicsController : MenuModule
    {
        #region Fields

        [FoldoutGroup("References")]
        [Tooltip("The resolution dropdown")]
        [SerializeField]
        private TMP_Dropdown m_dropdownResolution;

        [FoldoutGroup("References")]
        [Tooltip("The screen mode dropdown")]
        [SerializeField]
        private TMP_Dropdown m_dropdownScreenMode;

        [FoldoutGroup("References")]
        [Tooltip("The quality dropdown")]
        [SerializeField]
        private TMP_Dropdown m_dropdownQuality;

        [FoldoutGroup("References")]
        [Tooltip("The frame rate dropdown")]
        [SerializeField]
        private TMP_Dropdown m_dropdownFrameRate;

        [FoldoutGroup("References")]
        [Tooltip("The brightness slider")]
        [SerializeField]
        private Slider m_sliderBrightness;

        [FoldoutGroup("References")]
        [Tooltip("The volume")]
        [SerializeField]
        private Volume m_volume;

        private const string SAVE_KEY = "GraphicsSettingsData";

        private int m_selectedResolutionIndex;
        private FullScreenMode m_screenMode = FullScreenMode.FullScreenWindow;
        private int m_selectedQualityIndex;
        private int m_targetFrameRate = 60;
        private float m_brightness = 0.5f;

        #endregion

        #region SaveData

        [Serializable]
        private class SaveData
        {
            public int ResolutionIndex;
            public FullScreenMode ScreenMode;
            public int QualityLevel;
            public int FrameRate;
            public float Brightness;
        }

        #endregion

        #region Initialization

        public override void Initialize()
        {
            // Load saved settings first
            this.LoadSettings();

            // Populate dropdowns
            int resolutionIndex;
            ResolutionHelper.PopulateResolutionDropdown(this.m_dropdownResolution, out resolutionIndex);
            this.m_dropdownResolution.SetValueWithoutNotify(this.m_selectedResolutionIndex);
            this.m_dropdownResolution.onValueChanged.AddListener(this.OnResolutionChanged);

            this.m_dropdownScreenMode.ClearOptions();
            this.m_dropdownScreenMode.AddOptions(new List<string> { "Fullscreen", "Windowed", "Borderless" });
            this.m_dropdownScreenMode.SetValueWithoutNotify((int)this.m_screenMode);
            this.m_dropdownScreenMode.onValueChanged.AddListener(this.OnScreenModeChanged);

            this.m_dropdownQuality.ClearOptions();
            this.m_dropdownQuality.AddOptions(new List<string>(QualitySettings.names));
            this.m_dropdownQuality.SetValueWithoutNotify(this.m_selectedQualityIndex);
            this.m_dropdownQuality.onValueChanged.AddListener(this.OnQualityChanged);

            this.m_dropdownFrameRate.ClearOptions();
            this.m_dropdownFrameRate.AddOptions(new List<string> { "30", "60", "120", "144", "Unlimited" });
            this.m_dropdownFrameRate.SetValueWithoutNotify(this.FrameRateToIndex(this.m_targetFrameRate));
            this.m_dropdownFrameRate.onValueChanged.AddListener(this.OnFrameRateChanged);

            if (this.m_sliderBrightness != null)
            {
                this.m_sliderBrightness.SetValueWithoutNotify(this.m_brightness);
                this.m_sliderBrightness.onValueChanged.AddListener(this.OnBrightnessChanged);
            }

            // Apply current settings
            this.ApplyGraphicsSettings();
        }

        #endregion

        #region Apply Settings

        private void ApplyGraphicsSettings()
        {
            this.ApplyResolution();
            this.ApplyScreenMode();
            this.ApplyQuality();
            this.ApplyFrameRate();
            this.ApplyBrightness();
        }

        private void ApplyResolution()
        {
            Resolution res = ResolutionHelper.GetResolutionByIndex(this.m_selectedResolutionIndex);
            Screen.SetResolution(res.width, res.height, this.m_screenMode);
        }

        private void ApplyScreenMode()
        {
            Screen.fullScreenMode = this.m_screenMode;
        }

        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(this.m_selectedQualityIndex);
        }

        private void ApplyFrameRate()
        {
            Application.targetFrameRate = this.m_targetFrameRate;
        }

        private void ApplyBrightness()
        {
            if (this.m_volume != null && this.m_volume.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 1f, this.m_brightness);
            }
        }

        #endregion

        #region Save/Load

        private void SaveSettings()
        {
            SaveData saveData = new()
            {
                ResolutionIndex = this.m_selectedResolutionIndex,
                ScreenMode = this.m_screenMode,
                QualityLevel = this.m_selectedQualityIndex,
                FrameRate = this.m_targetFrameRate,
                Brightness = this.m_brightness
            };

            SaveSystem.Save(saveData, SAVE_KEY);
        }

        private void LoadSettings()
        {
            if (!SaveSystem.Exists(SAVE_KEY)) return;

            SaveData saveData = SaveSystem.Load<SaveData>(SAVE_KEY);
            if (saveData == null) return;

            this.m_selectedResolutionIndex = saveData.ResolutionIndex;
            this.m_screenMode = saveData.ScreenMode;
            this.m_selectedQualityIndex = saveData.QualityLevel;
            this.m_targetFrameRate = saveData.FrameRate;
            this.m_brightness = saveData.Brightness;
        }

        #endregion

        #region Dropdown/Slider Callbacks

        private void OnResolutionChanged(int index)
        {
            this.m_selectedResolutionIndex = index;
            this.ApplyResolution();
            this.SaveSettings();
        }

        private void OnScreenModeChanged(int index)
        {
            this.m_screenMode = (FullScreenMode)index;
            this.ApplyScreenMode();
            this.SaveSettings();
        }

        private void OnQualityChanged(int index)
        {
            this.m_selectedQualityIndex = index;
            this.ApplyQuality();
            this.SaveSettings();
        }

        private void OnFrameRateChanged(int index)
        {
            this.m_targetFrameRate = this.IndexToFrameRate(index);
            this.ApplyFrameRate();
            this.SaveSettings();
        }

        private void OnBrightnessChanged(float value)
        {
            this.m_brightness = value;
            this.ApplyBrightness();
            this.SaveSettings();
        }

        #endregion

        #region Helpers

        private int FrameRateToIndex(int frameRate)
        {
            return frameRate switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                144 => 3,
                _ => 4 // Unlimited
            };
        }

        private int IndexToFrameRate(int index)
        {
            return index switch
            {
                0 => 30,
                1 => 60,
                2 => 120,
                3 => 144,
                _ => -1 // Unlimited
            };
        }

        #endregion
    }
}
