using UnityEngine;
using System;
using RPG.Core;
using RPG.DI;

namespace RPG.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 50;
        [SerializeField] private int experienceReward = 20;

        [Header("Visual Effects")]
        [SerializeField] private GameObject deathEffect;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private Material hitMaterial;

        private int currentHealth;
        private Material originalMaterial;
        private Renderer enemyRenderer;
        private bool isDead = false;

        public bool IsDead => isDead;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public float HealthPercentage => (float)currentHealth / maxHealth;

        public event Action OnDeath;
        public event Action<int, int> OnHealthChanged;

        private void Start()
        {
            currentHealth = maxHealth;
            enemyRenderer = GetComponentInChildren<Renderer>();

            if (enemyRenderer != null)
            {
                originalMaterial = enemyRenderer.material;
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // ¬изуальный эффект получени€ урона
            StartCoroutine(DamageFlash());

            // Ёффект попадани€
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
            }

            // ¬оспроизводим звук
            var audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            audioManager?.PlaySFX("EnemyHit");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (enemyRenderer != null && hitMaterial != null)
            {
                enemyRenderer.material = hitMaterial;
                yield return new WaitForSeconds(0.1f);
                enemyRenderer.material = originalMaterial;
            }
        }

        private void Die()
        {
            isDead = true;

            // Ёффект смерти
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            // ”ведомл€ем о смерти
            OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}