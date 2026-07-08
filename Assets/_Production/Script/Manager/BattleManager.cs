using System;
using System.Collections.Generic;
using System.Linq;
using Gamepangin;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NovastraTest
{
    public class BattleManager : Singleton<BattleManager>, IEventListener<OnDeath>
    {
        public BattleTeam PlayerTeam { get; private set; } = new BattleTeam(UnitFactionType.Player);
        public BattleTeam EnemyTeam { get; private set; } = new BattleTeam(UnitFactionType.Enemy);
        public BattleState CurrentState { get; private set; } = BattleState.Setup;

        private void OnEnable()
        {
            this.EventStartListening<OnDeath>();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnDeath>();
        }
        public void SetState(BattleState state)
        {
            CurrentState = state;
            Debug.Log("Change state into " + CurrentState.ToString());
            OnChangeBattleState.Trigger(CurrentState);
        }

        public void RegisterUnit(Unit unit)
        {
            if (unit == null) return;

            var team = GetTeam(unit.Faction);
            if (!team.Units.Contains(unit))
            {
                team.Units.Add(unit);
            }
        }

        public void UnregisterUnit(Unit unit)
        {
            if (unit == null) return;

            PlayerTeam.Units.Remove(unit);
            EnemyTeam.Units.Remove(unit);
        }

        public IReadOnlyList<Unit> GetOpposingLivingUnits(Unit unit)
        {
            if (unit == null) return new List<Unit>();

            return unit.Faction == UnitFactionType.Player
                ? EnemyTeam.LivingUnits.ToList()
                : PlayerTeam.LivingUnits.ToList();
        }

        public IReadOnlyList<Unit> GetFriendlyLivingUnits(Unit unit)
        {
            if (unit == null) return new List<Unit>();

            return unit.Faction == UnitFactionType.Player
                ? PlayerTeam.LivingUnits.ToList()
                : EnemyTeam.LivingUnits.ToList();
        }

        public IReadOnlyList<Unit> GetValidTargets(Unit caster, SkillConfig skill)
        {
            List<Unit> candidates = new();

            if (caster == null || skill == null) return candidates;


            switch (skill.TargetingType)
            {
                case TargetingType.Self:
                    candidates.Add(caster);
                    break;
                default:
                    switch (skill.FactionTarget)
                    {
                        case FactionTargetType.Player:
                            candidates = GetFriendlyLivingUnits(caster).ToList();
                            break;

                        case FactionTargetType.Enemy:
                            candidates = GetOpposingLivingUnits(caster).ToList();
                            break;

                        default:
                            candidates = GetAllLivingUnits().ToList();
                            break;
                    }
                    break;
            }

            return candidates;
        }

        public List<Unit> GetAllLivingUnits()
        {
            List<Unit> allUnits = new();
            allUnits.AddRange(PlayerTeam.LivingUnits);
            allUnits.AddRange(EnemyTeam.LivingUnits);
            return allUnits;
        }

        private BattleTeam GetTeam(UnitFactionType faction)
        {
            return faction == UnitFactionType.Player ? PlayerTeam : EnemyTeam;
        }

        public void OnEvent(OnDeath e)
        {
            switch (e.DeadUnit.Faction)
            {
                case UnitFactionType.Enemy:

                    if (!UnitExists(EnemyTeam))
                        SetState(BattleState.Victory);
                    break;
                case UnitFactionType.Player:
                    if (!UnitExists(PlayerTeam))
                        SetState(BattleState.Defeat);
                    break;
            }
        }

        private bool UnitExists(BattleTeam team)
        {
            return team.HasLivingUnits;
        }

    }
}
