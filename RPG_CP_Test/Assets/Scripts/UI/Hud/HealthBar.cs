using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG.UI
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Gradient gradient;

        private void Awake()
        {
            // Находим компоненты если не назначены
            if (slider == null)
                slider = GetComponent<Slider>();

            if (healthText == null)
                healthText = GetComponentInChildren<TextMeshProUGUI>();

            if (fillImage == null && slider != null && slider.fillRect != null)
                fillImage = slider.fillRect.GetComponent<Image>();
        }

        /// <summary>
        /// Устанавливает максимальное здоровье
        /// </summary>
        public void SetMaxHealth(int maxHealth)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
            UpdateText(maxHealth, maxHealth);

            if (fillImage != null && gradient != null)
                fillImage.color = gradient.Evaluate(1f);
        }

        /// <summary>
        /// Устанавливает текущее здоровье
        /// </summary>
        public void SetHealth(int currentHealth, int maxHealth)
        {
            slider.value = currentHealth;
            UpdateText(currentHealth, maxHealth);

            if (fillImage != null && gradient != null)
                fillImage.color = gradient.Evaluate(slider.normalizedValue);
        }

        /// <summary>
        /// Обновляет текстовое отображение
        /// </summary>
        private void UpdateText(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"{current}/{max}";
        }

        /// <summary>
        /// Анимация получения урона
        /// </summary>
        public void FlashDamage()
        {
            StartCoroutine(DamageFlash());
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                fillImage.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                fillImage.color = originalColor;
            }
        }
    }
}