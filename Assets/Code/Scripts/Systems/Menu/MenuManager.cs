using UnityEngine;
using UnityEngine.Audio;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEditor;

namespace AF.TS.UI
{
    [HideMonoScript]
    public class MenuManager : MonoBehaviour
    {
        #region Exposed Members: -----------------------------------------------------------------------

        [Title("Reference")]
        [FoldoutGroup("Audio")]
        [SerializeField, Required] private AudioMixer m_audioMixer;

        [FoldoutGroup("Audio")]
        [SerializeField] private TMP_Dropdown m_dropdownAudioMode;

        [FoldoutGroup("Audio")]
        [SerializeField] private Slider m_sliderMaster;

        [FoldoutGroup("Audio")]
        [SerializeField] private Slider m_sliderMusic;

        [FoldoutGroup("Audio")]
        [SerializeField] private Slider m_sliderSFX;

        [FoldoutGroup("Graphics")]
        [SerializeField] private TMP_Dropdown m_dropdownResolution;

        [FoldoutGroup("Graphics")]
        [SerializeField] private Toggle m_toggleFullscreen;

        [FoldoutGroup("Graphics")]
        [SerializeField] private Slider m_sliderBrightness;

        [FoldoutGroup("Graphics")]
        [SerializeField] private Slider m_sliderMouseSensitivity;

        [FoldoutGroup("Accessibility")]
        [SerializeField] private TMP_Dropdown m_dropdownBlindness;

        private Resolution[] resolutions;

        #endregion

        #region Unity Callbacks: -----------------------------------------------------------------------

        private void Start()
        {
            // Load audio settings
            if (m_dropdownAudioMode != null) m_dropdownAudioMode.value = PlayerPrefs.GetInt("AudioMode", 0);
            if (m_sliderMaster != null) m_sliderMaster.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            m_sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            m_sliderSFX.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

            // Load graphics settings
            resolutions = Screen.resolutions;
            m_dropdownResolution.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            m_dropdownResolution.AddOptions(options);
            m_dropdownResolution.value = PlayerPrefs.GetInt("Resolution", currentResolutionIndex);
            m_dropdownResolution.RefreshShownValue();

            m_toggleFullscreen.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            m_sliderBrightness.value = PlayerPrefs.GetFloat("Brightness", 1.0f);

            // Load accessibility
            m_dropdownBlindness.value = PlayerPrefs.GetInt("Blindness", 0);
        }

        private void OnEnable()
        {
            m_dropdownAudioMode?.onValueChanged.AddListener(OnAudioModeChanged);
            m_sliderMaster?.onValueChanged.AddListener(OnMasterVolumeChanged);
            m_sliderMusic?.onValueChanged.AddListener(OnMusicVolumeChanged);
            m_sliderSFX?.onValueChanged.AddListener(OnSFXVolumeChanged);
            m_dropdownResolution?.onValueChanged.AddListener(OnResolutionChanged);
            m_toggleFullscreen?.onValueChanged.AddListener(OnFullscreenChanged);
            m_sliderBrightness?.onValueChanged.AddListener(OnBrightnessChanged);
            m_dropdownBlindness?.onValueChanged.AddListener(OnBlindnessChanged);
        }

        private void OnDisable()
        {
            m_dropdownAudioMode?.onValueChanged.RemoveListener(OnAudioModeChanged);
            m_sliderMaster?.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            m_sliderMusic?.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            m_sliderSFX?.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            m_dropdownResolution?.onValueChanged.RemoveListener(OnResolutionChanged);
            m_toggleFullscreen?.onValueChanged.RemoveListener(OnFullscreenChanged);
            m_sliderBrightness?.onValueChanged.RemoveListener(OnBrightnessChanged);
            m_dropdownBlindness?.onValueChanged.RemoveListener(OnBlindnessChanged);
        }

        #endregion

        #region Methods: -------------------------------------------------------------------------------

        private void OnBlindnessChanged(int value)
        {
            PlayerPrefs.SetInt("Blindness", value);
            // Implementa ajustes visuales para accesibilidad aquí si aplica
        }

        private void OnBrightnessChanged(float value)
        {
            PlayerPrefs.SetFloat("Brightness", value);
            // Aquí podrías ajustar el brillo usando post-procesado o un shader overlay
        }

        private void OnResolutionChanged(int index)
        {
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt("Resolution", index);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }

        private void OnSFXVolumeChanged(float volume)
        {
            m_audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        private void OnMusicVolumeChanged(float volume)
        {
            m_audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        private void OnMasterVolumeChanged(float volume)
        {
            m_audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        private void OnAudioModeChanged(int mode)
        {
            PlayerPrefs.SetInt("AudioMode", mode);
            // Aplica el cambio de modo si es necesario (por ejemplo, mono, estéreo, surround, etc.)
        }

        #endregion

        #region Public Methods: ------------------------------------------------------------------------

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Private Methods: -----------------------------------------------------------------------

        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }

        #endregion
    }
}
