using UnityEngine;
using Gamepangin;

namespace NovastraTest
{
    public class UnitHealth : MonoBehaviour, IEventListener<OnTakeDamage>, IEventListener<OnHeal>
    {
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private UnitHealthText healthText;

        private float currentHealth;

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
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth < 0)
            {
                currentHealth = 0;
                OnDeath.Trigger(GetComponent<Unit>());
            }
            healthText.UpdateHealth(currentHealth);
            Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            healthText.UpdateHealth(currentHealth);
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
