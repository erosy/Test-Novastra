using UnityEngine;

namespace NovastraTest
{
    public class TurnManager : MonoBehaviour
    {
        public Unit CurrentUnit { get; private set; }

        public void SetCurrentUnit(Unit unit)
        {
            CurrentUnit = unit;

            if (CurrentUnit != null)
            {
                TurnStartEvent.Trigger(CurrentUnit);
            }
        }

        public void ClearCurrentUnit()
        {
            CurrentUnit = null;
        }
    }
}
