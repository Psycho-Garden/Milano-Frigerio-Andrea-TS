using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using AF.TS.Characters;
using AF.TS.Utils;
using DG.Tweening;
using UnityEngine.Windows;

namespace AD.TS.UI.HUD
{
    [HideMonoScript]
    public class HUDManager : MonoBehaviour
    {
        [FoldoutGroup("Player")]
        [BoxGroup("Player/Stats")]
        [Tooltip("Health bar")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_healthBar;

        [BoxGroup("Player/Stats")]
        [Tooltip("Stamina bar")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_staminaBar;

        [BoxGroup("Player/Stats")]
        [Tooltip("Damage screen")]
        [SerializeField, Required, SceneObjectsOnly]
        private Image m_damageScreen;

        [BoxGroup("Player/Gun Stats")]
        [Tooltip("Gun Icon")]
        [SerializeField, Required, SceneObjectsOnly]
        private Image m_gunIcon;

        [BoxGroup("Player/Gun Stats")]
        [Tooltip("Gun Ammo")]
        [SerializeField, Required, SceneObjectsOnly]
        private TMP_Text m_currentAmmo;

        [BoxGroup("Player/Gun Stats")]
        [Tooltip("Gun Ammo")]
        [SerializeField, Required, SceneObjectsOnly]
        private TMP_Text m_currentAmmoMagazineCapacity;

        [FoldoutGroup("Boss")]
        [BoxGroup("Boss/Stats")]
        [Tooltip("Health bar")]
        [SerializeField, Required, SceneObjectsOnly]
        private Slider m_healthBarBoss;

        [FoldoutGroup("Debug")]
        [Tooltip("Target character")]
        [ShowInInspector, ReadOnly]
        private Character m_targetPlayer;

        [FoldoutGroup("Debug")]
        [Tooltip("Target character")]
        [ShowInInspector, ReadOnly]
        private Boss m_targetBoss;

        private CharacterInput m_input;

        private void Awake()
        {
            m_targetPlayer = ServiceLocator.Get<Character>();
            m_targetBoss = ServiceLocator.Get<Boss>();

            m_input = ServiceLocator.Get<CharacterInput>();
        }

        private void Start()
        {
            m_healthBar.value = 1f;
            m_staminaBar.value = 1f;

            m_healthBarBoss.value = 1f;
        }

        private void OnEnable()
        {
            m_targetPlayer.Stats.OnStaminaChanged += UpdateStamina;
            m_targetPlayer.HealthSystem.OnHealthChanged += UpdateHealth;
            m_targetPlayer.HealthSystem.OnTakeDamage += TakeDamage;

            m_targetPlayer.Inventory.OnWeaponChanged += UpdateGun;
            m_targetPlayer.Inventory.OnMagazineChanged += UpdateMagazine;
            m_targetPlayer.Inventory.OnAmmoChanged += UpdateAmmo;

            m_targetBoss.HealthSystem.OnHealthChanged += UpdateHealthBoss;
        }

        private void OnDisable()
        {
            m_targetPlayer.Stats.OnStaminaChanged -= UpdateStamina;
            m_targetPlayer.HealthSystem.OnHealthChanged -= UpdateHealth;
            m_targetPlayer.HealthSystem.OnTakeDamage -= TakeDamage;

            m_targetPlayer.Inventory.OnWeaponChanged -= UpdateGun;
            m_targetPlayer.Inventory.OnMagazineChanged -= UpdateMagazine;
            m_targetPlayer.Inventory.OnAmmoChanged -= UpdateAmmo;

            m_targetBoss.HealthSystem.OnHealthChanged -= UpdateHealthBoss;
        }

        #region Private Functions --------------------------------------------------------

        private void UpdateBar(Slider bar, float value)
        {
            bar.value = value;
        }

        private void UpdateHealth(float value) => UpdateBar(this.m_healthBar, value);

        private void UpdateStamina(float value) => UpdateBar(this.m_staminaBar, value);

        private void UpdateHealthBoss(float value) => UpdateBar(this.m_healthBarBoss, value);

        private void UpdateMagazine(int value, int capacity)
        {
            UpdateAmmoMagazineCapacity(capacity);
            UpdateAmmo(value);
        }

        private void UpdateGun(int value, int capacity, Sprite sprite)
        {
            UpdateAmmoMagazineCapacity(capacity);
            UpdateAmmo(value);
            UpdateGunIcon(sprite);
        }

        private void TakeDamage()
        {
            m_damageScreen.DOKill();

            m_damageScreen
                .DOFade(1f, 0.1f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    m_damageScreen.DOFade(0f, 0.1f).SetEase(Ease.Linear);
                });
        }

        private void UpdateAmmo(int value) => this.m_currentAmmo.text = value.ToString();

        private void UpdateAmmoMagazineCapacity(int value) => this.m_currentAmmoMagazineCapacity.text = value.ToString();

        private void UpdateGunIcon(Sprite sprite) => this.m_gunIcon.sprite = sprite;

        #endregion
    }

}
