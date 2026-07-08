

using Gamepangin;

namespace NovastraTest
{
    public struct OnTurnStart
    {
        public Unit Unit;

        public OnTurnStart(Unit unit)
        {
            Unit = unit;
        }

        public static OnTurnStart turnStartEvent;

        public static void Trigger(Unit unit)
        {
            turnStartEvent = new OnTurnStart(unit);
            EventManager.TriggerEvent(turnStartEvent);
        }
    }
}