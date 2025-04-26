using UnityEngine;
using Sirenix.OdinInspector;
using AF.TS.Characters;
using AF.TS.Utils;

namespace AD.TS.UI.HUD
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        [FoldoutGroup("Settings")]
        [Tooltip("")]
        [SerializeField]
        private TriggerEvent m_gameOverEvent;

        [FoldoutGroup("Menu")]
        [Tooltip("Menu")]
        [SerializeField, Required, SceneObjectsOnly]
        private GameObject m_menu;

        private CharacterInput m_input;

        private void OnValidate()
        {
            this.m_gameOverEvent.TriggerEventCheckAuto(this);
        }

        private void Awake()
        {
            ServiceLocator.Register<GameManager>(this);

            this.m_input = ServiceLocator.Get<CharacterInput>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            ServiceLocator.Get<Character>().HealthSystem.OnDie += GameOver;
        }

        private void OnDisable()
        {
            ServiceLocator.Get<Character>().HealthSystem.OnDie -= GameOver;
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameManager>();
        }

        private void Update()
        {
            if (this.m_input.MenuPressed)
            {
                this.m_menu.SetActive(!this.m_menu.activeSelf);
                Cursor.visible = !Cursor.visible;
                Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
                Time.timeScale = Cursor.visible ? 0f : 1f;
            }
        }

        public void GameOver()
        {
            this.m_gameOverEvent?.Invoke(this.transform);
        }

        public void Resume()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }

}
