using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamepangin;
using UnityEngine;

namespace NovastraTest
{
    public class TurnManager : MonoBehaviour, IEventListener<OnChangeBattleState>
    {

        private void OnEnable()
        {
            this.EventStartListening<OnChangeBattleState>();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnChangeBattleState>();
        }

        public Unit CurrentUnit { get; private set; }

        private readonly List<TurnEntry> timeline = new();
        public IReadOnlyList<TurnEntry> Timeline => timeline;

        public IReadOnlyList<Unit> GetTurnPreview(int count)
        {
            if (count <= 0)
            {
                return Array.Empty<Unit>();
            }

            var simulatedTimeline = timeline
                .Where(entry => entry.Unit != null && entry.Unit.IsAlive)
                .Select(entry => new SimulatedTurnEntry(entry.Unit, entry.CurrentActionValue, entry.BaseActionValue))
                .ToList();

            if (simulatedTimeline.Count == 0)
            {
                return Array.Empty<Unit>();
            }

            List<Unit> preview = new(count);

            for (int turnIndex = 0; turnIndex < count; turnIndex++)
            {
                var nextEntry = simulatedTimeline[0];

                for (int entryIndex = 1; entryIndex < simulatedTimeline.Count; entryIndex++)
                {
                    if (simulatedTimeline[entryIndex].CurrentActionValue < nextEntry.CurrentActionValue)
                    {
                        nextEntry = simulatedTimeline[entryIndex];
                    }
                }

                float elapsedActionValue = nextEntry.CurrentActionValue;

                foreach (var entry in simulatedTimeline)
                {
                    entry.CurrentActionValue = Math.Max(0f, entry.CurrentActionValue - elapsedActionValue);
                }

                preview.Add(nextEntry.Unit);
                nextEntry.CurrentActionValue = nextEntry.BaseActionValue;
            }

            return preview;
        }

        public void SetCurrentUnit(Unit unit)
        {
            CurrentUnit = unit;
        }

        public void ClearCurrentUnit()
        {
            CurrentUnit = null;
        }

        public void StartNextTurn()
        {
            timeline.RemoveAll(entry => entry.Unit == null || !entry.Unit.IsAlive);

            var nextEntry = timeline
                .OrderBy(entry => entry.CurrentActionValue)
                .FirstOrDefault();

            if (nextEntry == null)
            {
                ClearCurrentUnit();
                return;
            }

            float elapsedActionValue = nextEntry.CurrentActionValue;

            foreach (var entry in timeline)
            {
                entry.CurrentActionValue = Math.Max(0f, entry.CurrentActionValue - elapsedActionValue);
            }

            CurrentUnit = nextEntry.Unit;
            OnTurnStart.Trigger(CurrentUnit);

            Debug.Log($"current unit is {CurrentUnit.Config.name} from faction {CurrentUnit.Faction}");

            BattleManager.Instance.SetState(BattleState.WaitingForInput);
        }

        public void Initialize(List<Unit> units)
        {
            timeline.Clear();

            foreach (var unit in units)
            {
                if (unit == null || !unit.IsAlive) continue;

                var unitTimeline = new TurnEntry()
                {
                    Unit = unit
                };

                unitTimeline.CurrentActionValue = unitTimeline.BaseActionValue;

                timeline.Add(unitTimeline);
            }

            if (BattleManager.Instance.HasVNScriptBeforePlay)
            {
                BattleManager.Instance.RequestVisualNovelPause(BattleManager.Instance.BattleStartingScript);
            }

            else
            {
                BattleManager.Instance.SetState(BattleState.TurnStart);
            }
        }

        public void EndCurrentTurn()
        {
            if (CurrentUnit != null)
            {
                var currentEntry = timeline.FirstOrDefault(entry => entry.Unit == CurrentUnit);

                if (currentEntry != null && CurrentUnit.IsAlive)
                {
                    currentEntry.CurrentActionValue = currentEntry.BaseActionValue;
                }
            }

            ClearCurrentUnit();
            BattleManager.Instance.SetState(BattleState.TurnStart);
        }

        public void OnEvent(OnChangeBattleState e)
        {
            switch (e.battleState)
            {
                case BattleState.TurnStart:
                    StartNextTurn();
                    break;

                case BattleState.Setup:
                    Initialize(BattleManager.Instance.GetAllLivingUnits());
                    break;

                case BattleState.CheckingBattleEnd:
                    EndCurrentTurn();
                    break;

                default:
                    break;
            }
        }

    }

    public class TurnEntry
    {
        public Unit Unit;
        public float CurrentActionValue;

        public float BaseActionValue => 10000f / Math.Max(1f, Unit.Speed);
    }

    internal sealed class SimulatedTurnEntry
    {
        public Unit Unit { get; }
        public float CurrentActionValue { get; set; }
        public float BaseActionValue { get; }

        public SimulatedTurnEntry(Unit unit, float currentActionValue, float baseActionValue)
        {
            Unit = unit;
            CurrentActionValue = currentActionValue;
            BaseActionValue = baseActionValue;
        }
    }
}
