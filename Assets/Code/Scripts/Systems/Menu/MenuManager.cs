using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.UI
{
    [HideMonoScript]
    public class MenuManager : MonoBehaviour
    {
        #region Exposed Members

        [FoldoutGroup("Modules")]
        [FoldoutGroup("Modules/Audio")]
        [Tooltip("")]
        [SerializeField, HideLabel]
        private AudioController m_audioSettings;

        [FoldoutGroup("Modules")]
        [FoldoutGroup("Modules/Graphics")]
        [Tooltip("")]
        [SerializeField, HideLabel]
        private GraphicsController m_graphicsSettings;

        [FoldoutGroup("Modules")]
        [FoldoutGroup("Modules/Accessibility")]
        [Tooltip("")]
        [SerializeField, HideLabel]
        private AccessibilityController m_accessibilitySettings;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            this.m_audioSettings?.Initialize();
            this.m_graphicsSettings?.Initialize();
            this.m_accessibilitySettings?.Initialize();
        }

        private void OnEnable()
        {
            this.m_audioSettings?.OnEnable();
            this.m_graphicsSettings?.OnEnable();
            this.m_accessibilitySettings?.OnEnable();
        }

        private void OnDisable()
        {
            this.m_audioSettings?.OnDispose();
            this.m_graphicsSettings?.OnDispose();
            this.m_accessibilitySettings?.OnDispose();
        }

        #endregion

        #region Public Methods

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        #endregion
    }

    #region MenuModule

    [Serializable]
    public class MenuModule
    {
        public virtual void Initialize() { }
        public virtual void OnEnable() { }
        public virtual void OnDispose() { }
    }

    #endregion
}
