using UnityEngine;
using Gamepangin;

namespace NovastraTest
{
    public class UnitHealth : MonoBehaviour, IEventListener<OnTakeDamage>, IEventListener<OnHeal>
    {
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private UnitHealthText healthText;

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        void OnEnable()
        {
            this.EventStartListening<OnTakeDamage>();
            this.EventStartListening<OnHeal>();
        }

        void OnDisable()
        {
            this.EventStopListening<OnTakeDamage>();
            this.EventStopListening<OnHeal>();
        }

        public void InitHealth(float health)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            isDead = false;

            if (healthText != null)
            {
                healthText.UpdateHealth(currentHealth);
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                OnDeath.Trigger(GetComponent<Unit>());
            }

            if (healthText != null)
            {
                healthText.UpdateHealth(currentHealth);
            }

            Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");
        }

        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            if (healthText != null)
            {
                healthText.UpdateHealth(currentHealth);
            }

            Debug.Log($"{gameObject.name} healed {amount}. Current health: {currentHealth}");
        }

        public void OnEvent(OnTakeDamage e)
        {
            if (e.Target.Health == this)
            {
                TakeDamage(e.DamageAmount);
            }
        }

        public void OnEvent(OnHeal e)
        {
            if (e.Target.Health == this)
            {
                Heal(e.HealAmount);
            }
        }
    }
}
