using UnityEngine;

namespace NovastraTest
{
    public class UnitHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100;
        private float currentHealth;

        public float CurrentHealth => currentHealth;

        public void InitHealth(float health)
        {
            maxHealth = health;
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            Debug.Log($"{gameObject.name} healed {amount}. Current health: {currentHealth}");
        }
    }
}