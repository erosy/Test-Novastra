using System.Collections.Generic;
using Gamepangin;

namespace NovastraTest
{
    public struct OnDeath
    {
        public Unit Caster;

        public OnDeath(Unit caster)
        {
            Caster = caster;
        }

        public static OnDeath onDeathEvent;

        public static void Trigger(Unit caster)
        {
            onDeathEvent = new OnDeath(caster);
            EventManager.TriggerEvent(onDeathEvent);
        }
    }
}