using UnityEngine;
using RPG.Core;
using RPG.UI.Menus;

namespace RPG.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int baseDamage = 20;
        [SerializeField] private int baseDefense = 10;

        [Header("Level")]
        [SerializeField] private int level = 1;
        [SerializeField] private int experience = 0;

        [Header("Level Up Settings")]
        [SerializeField] private int healthPerLevel = 20;
        [SerializeField] private int manaPerLevel = 10;
        [SerializeField] private int damagePerLevel = 5;
        [SerializeField] private int defensePerLevel = 2;
        [SerializeField] private int expPerLevel = 100;

        private int currentHealth;
        private int currentMana;

        // События для UI
        public event System.Action<int, int> OnHealthChanged;
        public event System.Action<int, int> OnManaChanged;
        public event System.Action<int> OnLevelUp;
        public event System.Action<int, int> OnExperienceChanged;
        public event System.Action OnPlayerDeath;

        // Свойства
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int Damage => baseDamage;
        public int Defense => baseDefense;
        public int Level => level;
        public int Experience => experience;

        private void Start()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;

            // Вызываем события для инициализации UI
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log($"PlayerStats initialized: Health={currentHealth}/{maxHealth}, Level={level}, Exp={experience}/{GetExpNeededForNextLevel()}");
        }

        /// <summary>
        /// Получить опыт необходимый для следующего уровня
        /// </summary>
        private int GetExpNeededForNextLevel()
        {
            return level * expPerLevel;
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;

            int actualDamage = Mathf.Max(1, damage - baseDefense);
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);

            Debug.Log($"Player took {actualDamage} damage! Health: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Лечение
        /// </summary>
        public void Heal(int amount)
        {
            if (currentHealth <= 0) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

            Debug.Log($"Player healed for {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Использовать ману
        /// </summary>
        public void UseMana(int amount)
        {
            if (amount <= 0) return;

            currentMana = Mathf.Max(0, currentMana - amount);
            OnManaChanged?.Invoke(currentMana, maxMana);

            Debug.Log($"Used {amount} mana. Remaining: {currentMana}/{maxMana}");
        }

        /// <summary>
        /// Восстановить ману
        /// </summary>
        public void RestoreMana(int amount)
        {
            if (amount <= 0) return;

            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana, maxMana);

            Debug.Log($"Restored {amount} mana. Current: {currentMana}/{maxMana}");
        }

        /// <summary>
        /// Добавить опыт
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            experience += amount;
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log($"Gained {amount} XP! Total: {experience}/{GetExpNeededForNextLevel()}");

            // Проверяем повышение уровня
            while (experience >= GetExpNeededForNextLevel())
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Установить опыт (для загрузки сохранения)
        /// </summary>
        public void SetExperience(int newExp)
        {
            if (newExp < 0) newExp = 0;

            experience = newExp;
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log($"Experience set to: {experience}/{GetExpNeededForNextLevel()}");

            // Проверяем повышение уровня (если опыт больше чем нужно)
            while (experience >= GetExpNeededForNextLevel() && level < 100)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Повышение уровня
        /// </summary>
        private void LevelUp()
        {
            // Вычитаем опыт за текущий уровень
            experience -= GetExpNeededForNextLevel();
            level++;

            // Увеличиваем статы
            maxHealth += healthPerLevel;
            maxMana += manaPerLevel;
            baseDamage += damagePerLevel;
            baseDefense += defensePerLevel;

            // Полное восстановление
            currentHealth = maxHealth;
            currentMana = maxMana;

            // Вызываем события
            OnLevelUp?.Invoke(level);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log($"🎉 LEVEL UP! Now level {level}!");
            Debug.Log($"   Max Health: {maxHealth}");
            Debug.Log($"   Max Mana: {maxMana}");
            Debug.Log($"   Damage: {baseDamage}");
            Debug.Log($"   Defense: {baseDefense}");
            Debug.Log($"   Next level: {experience}/{GetExpNeededForNextLevel()} XP");
        }

        /// <summary>
        /// Смерть игрока
        /// </summary>
        private void Die()
        {
            Debug.Log("💀 Player died!");
            OnPlayerDeath?.Invoke();

            // Ищем меню Game Over
            GameOverMenu gameOverMenu = FindAnyObjectByType<GameOverMenu>();
            if (gameOverMenu != null)
            {
                gameOverMenu.ShowGameOver();
            }
            else
            {
                // Если нет меню, загружаем главное меню
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// Полное восстановление (при респавне)
        /// </summary>
        public void FullRestore()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);

            Debug.Log("Player fully restored!");
        }

        /// <summary>
        /// Сохранить статы
        /// </summary>
        public void SaveStats()
        {
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            PlayerPrefs.SetInt("PlayerMana", currentMana);
            PlayerPrefs.SetInt("PlayerLevel", level);
            PlayerPrefs.SetInt("PlayerExp", experience);
            PlayerPrefs.SetInt("PlayerMaxHealth", maxHealth);
            PlayerPrefs.SetInt("PlayerMaxMana", maxMana);
            PlayerPrefs.SetInt("PlayerDamage", baseDamage);
            PlayerPrefs.SetInt("PlayerDefense", baseDefense);
            PlayerPrefs.Save();

            Debug.Log("Player stats saved");
        }

        /// <summary>
        /// Загрузить статы
        /// </summary>
        public void LoadStats()
        {
            maxHealth = PlayerPrefs.GetInt("PlayerMaxHealth", maxHealth);
            maxMana = PlayerPrefs.GetInt("PlayerMaxMana", maxMana);
            baseDamage = PlayerPrefs.GetInt("PlayerDamage", baseDamage);
            baseDefense = PlayerPrefs.GetInt("PlayerDefense", baseDefense);

            currentHealth = PlayerPrefs.GetInt("PlayerHealth", maxHealth);
            currentMana = PlayerPrefs.GetInt("PlayerMana", maxMana);
            level = PlayerPrefs.GetInt("PlayerLevel", 1);
            experience = PlayerPrefs.GetInt("PlayerExp", 0);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log($"Player stats loaded: Level={level}, Health={currentHealth}/{maxHealth}, Exp={experience}/{GetExpNeededForNextLevel()}");
        }

        /// <summary>
        /// Сброс статов (новая игра)
        /// </summary>
        public void ResetStats()
        {
            level = 1;
            experience = 0;
            maxHealth = 100;
            maxMana = 100;
            baseDamage = 20;
            baseDefense = 10;
            currentHealth = maxHealth;
            currentMana = maxMana;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnLevelUp?.Invoke(level);
            OnExperienceChanged?.Invoke(experience, GetExpNeededForNextLevel());

            Debug.Log("Player stats reset to default");
        }
    }
}