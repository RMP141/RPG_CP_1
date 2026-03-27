using UnityEngine;
using RPG.Player;

namespace RPG.UI
{
    public class UIUpdater : MonoBehaviour
    {
        [SerializeField] private HealthBar healthBar;

        private PlayerStats playerStats;

        void Start()
        {
            Debug.Log("UIUpdater Start");

            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"Player found: {player != null}");

            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
                Debug.Log($"PlayerStats found: {playerStats != null}");

                if (playerStats != null)
                {
                    // Подписываемся на событие
                    playerStats.OnHealthChanged += UpdateHealth;
                    Debug.Log("Subscribed to OnHealthChanged");

                    // Устанавливаем начальные значения
                    UpdateHealth(playerStats.CurrentHealth, playerStats.MaxHealth);
                    Debug.Log($"Initial health: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
                }
            }

            // Проверяем healthBar
            Debug.Log($"HealthBar assigned: {healthBar != null}");
        }

        void UpdateHealth(int current, int max)
        {
            Debug.Log($"UpdateHealth called: {current}/{max}");

            if (healthBar != null)
            {
                healthBar.SetHealth(current, max);
                Debug.Log("HealthBar.SetHealth called");
            }
            else
            {
                Debug.LogError("HealthBar is NULL!");
            }
        }

        void OnDestroy()
        {
            if (playerStats != null)
                playerStats.OnHealthChanged -= UpdateHealth;
        }
    }
}