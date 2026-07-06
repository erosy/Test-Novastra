

using Gamepangin;

namespace NovastraTest
{
    public struct TurnStartEvent
    {
        public Unit Unit;

        public TurnStartEvent(Unit unit)
        {
            Unit = unit;
        }

        public static TurnStartEvent turnStartEvent;

        public static void Trigger(Unit unit)
        {
            turnStartEvent = new TurnStartEvent(unit);
            EventManager.TriggerEvent(turnStartEvent);
        }
    }
}