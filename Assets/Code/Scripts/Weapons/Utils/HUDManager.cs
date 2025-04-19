using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace AD.TS.UI.HUD
{
    public class HUDManager : MonoBehaviour
    {
        [FoldoutGroup("Character Stats")]
        [Tooltip("Health bar")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_healthBar;

        [FoldoutGroup("Character Stats")]
        [Tooltip("Stamina bar")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_staminaBar;

        [FoldoutGroup("Gun Stats")]
        [Tooltip("Gun Icon")]
        [SerializeField, Required, SceneObjectsOnly]
        private Image m_gunIcon;

        [FoldoutGroup("Gun Stats")]
        [Tooltip("Gun Ammo")]
        [SerializeField, Required, SceneObjectsOnly]
        private TMP_Text m_currentAmmo;

        [FoldoutGroup("Gun Stats")]
        [Tooltip("Gun Ammo")]
        [SerializeField, Required, SceneObjectsOnly]
        private TMP_Text m_currentAmmoMagazineCapacity;

        private void Start()
        {
            m_healthBar.value = 1f;
            m_staminaBar.value = 1f;
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        #region Private Functions --------------------------------------------------------

        private void UpdateBar(Slider bar, float value)
        {
            bar.value = value;
        }

        private void UpdateHealth(float value) => UpdateBar(this.m_healthBar, value);

        private void UpdateStamina(float value) => UpdateBar(this.m_staminaBar, value);

        private void UpdateAmmo(int value) => this.m_currentAmmo.text = value.ToString();

        private void UpdateAmmoMagazineCapacity(int value) => this.m_currentAmmoMagazineCapacity.text = value.ToString();

        private void UpdateGunIcon(Sprite sprite) => this.m_gunIcon.sprite = sprite;

        #endregion
    }
}
