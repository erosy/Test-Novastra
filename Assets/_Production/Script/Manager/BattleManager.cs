using System;
using System.Collections.Generic;
using System.Linq;
using Gamepangin;
using Lean.Pool;
using Naninovel;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NovastraTest
{
    public class BattleManager : Singleton<BattleManager>, IEventListener<OnDeath>
    {
        [Title("Dummy Player Unit System")]
        [SerializeField] private List<UnitConfig> playerUnits;

        [Title("Battle Config")]
        public BattleConfig battleConfig;

        [Title("Positioning Hooks")]
        [SerializeField] private List<GameObjectHookData> playerPositions;
        [SerializeField] private List<GameObjectHookData> enemyPositions;
        protected override bool IsPersistBetweenScenes => false;

        public BattleTeam PlayerTeam { get; private set; } = new BattleTeam(UnitFactionType.Player);
        public BattleTeam EnemyTeam { get; private set; } = new BattleTeam(UnitFactionType.Enemy);
        public BattleState CurrentState { get; private set; } = BattleState.Setup;

        public bool HasVNScriptBeforePlay => battleConfig != null && battleConfig.ScriptExists && battleConfig.StartingScript != null;
        public Script BattleStartingScript => battleConfig != null ? battleConfig.StartingScript : null;
        public Script PendingVisualNovelScript { get; private set; }
        public BattleState VisualNovelResumeState { get; private set; } = BattleState.TurnStart;

        protected override void Awake()
        {
            base.Awake();

            if (GetComponent<BattleVisualNovelCoordinator>() == null)
            {
                gameObject.AddComponent<BattleVisualNovelCoordinator>();
            }
        }

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

        public void RequestVisualNovelPause(Script script, BattleState resumeState = BattleState.TurnStart)
        {
            if (script == null)
            {
                Debug.LogWarning("Visual novel pause requested without a script. Resuming battle flow.");
                SetState(resumeState);
                return;
            }

            PendingVisualNovelScript = script;
            VisualNovelResumeState = resumeState;
            SetState(BattleState.VisualNovelPause);
        }

        public void ClearVisualNovelPauseRequest()
        {
            PendingVisualNovelScript = null;
            VisualNovelResumeState = BattleState.TurnStart;
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

        [Button]

        public void SetupBattle()
        {
            for (int i = 0; i < playerUnits.Count; i++)
            {
                if (i >= playerPositions.Count) continue;

                var unit = LeanPool.Spawn(playerUnits[i].UnitPrefab, playerPositions[i].Reference.transform.position, Quaternion.identity);

                var unitComponent = unit.GetComponent<Unit>();

                unitComponent.Initialize(playerUnits[i], UnitFactionType.Player);

                RegisterUnit(unitComponent);
            }

            for (int i = 0; i < battleConfig.EnemyConfigs.Count; i++)
            {
                if (i >= enemyPositions.Count) continue;

                var unit = LeanPool.Spawn(battleConfig.EnemyConfigs[i].UnitPrefab, enemyPositions[i].Reference.transform.position, Quaternion.identity);

                var unitComponent = unit.GetComponent<Unit>();

                unitComponent.Initialize(battleConfig.EnemyConfigs[i], UnitFactionType.Enemy);

                RegisterUnit(unitComponent);
            }

            SetState(BattleState.Setup);
        }

    }
}
