using Gamepangin;

namespace NovastraTest
{
    public struct OnChangeBattleState
    {
        public BattleState battleState;

        public OnChangeBattleState(BattleState battleState)
        {
            this.battleState = battleState;
        }

        public static OnChangeBattleState changeBattleStateEvent;

        public static void Trigger(BattleState battleState)
        {
            changeBattleStateEvent = new OnChangeBattleState(battleState);

            EventManager.TriggerEvent(changeBattleStateEvent);
        }
    }
}