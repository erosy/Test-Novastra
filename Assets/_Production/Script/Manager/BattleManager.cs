using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NovastraTest
{
    public class BattleManager : MonoBehaviour
    {
        public BattleTeam PlayerTeam { get; private set; } = new BattleTeam(UnitFactionType.Player);
        public BattleTeam EnemyTeam { get; private set; } = new BattleTeam(UnitFactionType.Enemy);
        public BattleState CurrentState { get; private set; } = BattleState.Setup;

        public void SetState(BattleState state)
        {
            CurrentState = state;
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

        public IReadOnlyList<Unit> GetValidTargets(Unit caster, SkillConfig skill)
        {
            if (caster == null || skill == null) return new List<Unit>();

            return skill.TargetingType == TargetingType.Self
                ? new List<Unit> { caster }
                : GetOpposingLivingUnits(caster);
        }

        private BattleTeam GetTeam(UnitFactionType faction)
        {
            return faction == UnitFactionType.Player ? PlayerTeam : EnemyTeam;
        }
    }
}
