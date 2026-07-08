using System.Collections.Generic;
using Gamepangin;

namespace NovastraTest
{
    public struct OnDeath
    {
        public Unit DeadUnit;

        public OnDeath(Unit deadUnit)
        {
            DeadUnit = deadUnit;
        }

        public static OnDeath onDeathEvent;

        public static void Trigger(Unit deadUnit)
        {
            onDeathEvent = new OnDeath(deadUnit);
            EventManager.TriggerEvent(onDeathEvent);
        }
    }
}