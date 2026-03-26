using RPG.Core;
using RPG.DI;
using UnityEngine;

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

        private int currentHealth;
        private int currentMana;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int Damage => baseDamage;
        public int Defense => baseDefense;
        public int Level => level;
        public int Experience => experience;

        public System.Action<int, int> OnHealthChanged;
        public System.Action<int, int> OnManaChanged;
        public System.Action<int> OnLevelUp;
        public System.Action OnPlayerDeath;

        private void Start()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - baseDefense);
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
                Die();
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void UseMana(int amount)
        {
            currentMana = Mathf.Max(0, currentMana - amount);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        public void AddExperience(int amount)
        {
            experience += amount;
            int expNeeded = level * 100;

            while (experience >= expNeeded)
            {
                LevelUp();
                expNeeded = level * 100;
            }
        }

        private void LevelUp()
        {
            experience -= level * 100;
            level++;
            maxHealth += 20;
            maxMana += 10;
            baseDamage += 5;
            baseDefense += 2;
            currentHealth = maxHealth;
            currentMana = maxMana;

            OnLevelUp?.Invoke(level);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        private void Die()
        {
            OnPlayerDeath?.Invoke();
            var gameManager = DIContainer.Instance.Resolve<IGameManager>();
            gameManager?.GameOver();
        }
    }
}