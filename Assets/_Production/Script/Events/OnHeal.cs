using Gamepangin;

namespace NovastraTest
{
    public struct OnHeal
    {
        public Unit Target;
        public float HealAmount;

        public static OnHeal onHealEvent;

        public OnHeal(Unit target, float healAmount)
        {
            Target = target;
            HealAmount = healAmount;
        }

        public static void Trigger(Unit target, float healAmount)
        {
            onHealEvent = new OnHeal(target, healAmount);
            EventManager.TriggerEvent(onHealEvent);
        }
    }
}
