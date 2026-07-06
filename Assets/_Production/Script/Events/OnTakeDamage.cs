using Gamepangin;

namespace NovastraTest
{
    public struct OnTakeDamage
    {
        public Unit Target;
        public float DamageAmount;

        public static OnTakeDamage onTakeDamageEvent;

        public OnTakeDamage(Unit target, float damageAmount)
        {
            Target = target;
            DamageAmount = damageAmount;
        }

        public static void Trigger(Unit target, float damageAmount)
        {
            onTakeDamageEvent = new OnTakeDamage(target, damageAmount);
            EventManager.TriggerEvent(onTakeDamageEvent);
        }
    }
}
