using System.Collections;
using Gamepangin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NovastraTest
{
    public class BattleInputManager : MonoBehaviour, IEventListener<OnChangeBattleState>, IEventListener<OnTurnStart>
    {
        [SerializeField] private TargetingManager targetingManager;
        [Header("Battle Input Actions")]
        [SerializeField] private InputActionReference selectSkill1Action;
        [SerializeField] private InputActionReference selectSkill2Action;
        [SerializeField] private InputActionReference previousTargetAction;
        [SerializeField] private InputActionReference nextTargetAction;
        [SerializeField] private InputActionReference confirmAction;
        [Header("Enemy Timing")]
        [SerializeField, Min(0f)] private float enemyTurnDelay = 0.75f;
        [SerializeField, Min(0f)] private float afterActionDelay = 0.25f;

        private Unit currentUnitInTurn;
        private SkillConfig selectedSkill;
        private BattleState currentBattleState;
        private Coroutine enemyTurnRoutine;
        private Coroutine skillExecutionRoutine;
        private Coroutine afterActionRoutine;

        private InputAction fallbackSelectSkill1Action;
        private InputAction fallbackSelectSkill2Action;
        private InputAction fallbackPreviousTargetAction;
        private InputAction fallbackNextTargetAction;
        private InputAction fallbackConfirmAction;

        private InputAction SelectSkill1Input => selectSkill1Action != null && selectSkill1Action.action != null ? selectSkill1Action.action : fallbackSelectSkill1Action;
        private InputAction SelectSkill2Input => selectSkill2Action != null && selectSkill2Action.action != null ? selectSkill2Action.action : fallbackSelectSkill2Action;
        private InputAction PreviousTargetInput => previousTargetAction != null && previousTargetAction.action != null ? previousTargetAction.action : fallbackPreviousTargetAction;
        private InputAction NextTargetInput => nextTargetAction != null && nextTargetAction.action != null ? nextTargetAction.action : fallbackNextTargetAction;
        private InputAction ConfirmInput => confirmAction != null && confirmAction.action != null ? confirmAction.action : fallbackConfirmAction;

        private void OnEnable()
        {
            this.EventStartListening<OnChangeBattleState>();
            this.EventStartListening<OnTurnStart>();

            EnsureFallbackActions();
            SubscribeInputActions();
            EnableInputActions();

            Debug.Log($"Battle input enabled. Actions enabled: {AreBattleActionsEnabled()}");
        }

        private void OnDisable()
        {
            StopEnemyTurnRoutine();
            StopSkillExecutionRoutine();
            StopAfterActionRoutine();

            UnsubscribeInputActions();
            DisableInputActions();

            this.EventStopListening<OnChangeBattleState>();
            this.EventStopListening<OnTurnStart>();
        }

        private void OnDestroy()
        {
            DisposeFallbackActions();
        }

        private void EnsureFallbackActions()
        {
            fallbackSelectSkill1Action ??= CreateButtonAction("SelectSkill1", "<Keyboard>/l", "<Keyboard>/1");
            fallbackSelectSkill2Action ??= CreateButtonAction("SelectSkill2", "<Keyboard>/k", "<Keyboard>/2");
            fallbackPreviousTargetAction ??= CreateButtonAction("PreviousTarget", "<Keyboard>/leftArrow", "<Keyboard>/a");
            fallbackNextTargetAction ??= CreateButtonAction("NextTarget", "<Keyboard>/rightArrow", "<Keyboard>/d");
            fallbackConfirmAction ??= CreateButtonAction("Confirm", "<Keyboard>/enter", "<Keyboard>/space");
        }

        private InputAction CreateButtonAction(string actionName, params string[] bindings)
        {
            var action = new InputAction(actionName, InputActionType.Button);

            foreach (var binding in bindings)
            {
                action.AddBinding(binding);
            }

            return action;
        }

        private void SubscribeInputActions()
        {
            SelectSkill1Input.performed += OnSelectSkill1;
            SelectSkill2Input.performed += OnSelectSkill2;
            PreviousTargetInput.performed += OnPreviousTarget;
            NextTargetInput.performed += OnNextTarget;
            ConfirmInput.performed += OnConfirm;
        }

        private void UnsubscribeInputActions()
        {
            SelectSkill1Input.performed -= OnSelectSkill1;
            SelectSkill2Input.performed -= OnSelectSkill2;
            PreviousTargetInput.performed -= OnPreviousTarget;
            NextTargetInput.performed -= OnNextTarget;
            ConfirmInput.performed -= OnConfirm;
        }

        private void EnableInputActions()
        {
            SelectSkill1Input.Enable();
            SelectSkill2Input.Enable();
            PreviousTargetInput.Enable();
            NextTargetInput.Enable();
            ConfirmInput.Enable();
        }

        private void DisableInputActions()
        {
            SelectSkill1Input.Disable();
            SelectSkill2Input.Disable();
            PreviousTargetInput.Disable();
            NextTargetInput.Disable();
            ConfirmInput.Disable();
        }

        private void DisposeFallbackActions()
        {
            fallbackSelectSkill1Action?.Dispose();
            fallbackSelectSkill2Action?.Dispose();
            fallbackPreviousTargetAction?.Dispose();
            fallbackNextTargetAction?.Dispose();
            fallbackConfirmAction?.Dispose();
        }

        private bool AreBattleActionsEnabled()
        {
            return SelectSkill1Input.enabled &&
                   SelectSkill2Input.enabled &&
                   PreviousTargetInput.enabled &&
                   NextTargetInput.enabled &&
                   ConfirmInput.enabled;
        }

        private void OnSelectSkill1(InputAction.CallbackContext context)
        {
            if (!CanAcceptPlayerInput(nameof(OnSelectSkill1))) return;

            TrySelectSkill(0);
        }

        private void OnSelectSkill2(InputAction.CallbackContext context)
        {
            if (!CanAcceptPlayerInput(nameof(OnSelectSkill2))) return;

            TrySelectSkill(1);
        }

        private void OnPreviousTarget(InputAction.CallbackContext context)
        {
            if (!CanAcceptPlayerInput(nameof(OnPreviousTarget))) return;
            if (selectedSkill == null) return;

            targetingManager.SelectPrevious();
        }

        private void OnNextTarget(InputAction.CallbackContext context)
        {
            if (!CanAcceptPlayerInput(nameof(OnNextTarget))) return;
            if (selectedSkill == null) return;

            targetingManager.SelectNext();
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            if (!CanAcceptPlayerInput(nameof(OnConfirm))) return;

            ConfirmTargeting();
        }

        private bool CanAcceptPlayerInput(string inputName)
        {
            if (currentBattleState != BattleState.WaitingForInput)
            {
                Debug.Log($"{inputName} ignored. State: {currentBattleState}, actions enabled: {AreBattleActionsEnabled()}");
                return false;
            }

            if (currentUnitInTurn == null)
            {
                Debug.Log($"{inputName} ignored. Current unit is null, actions enabled: {AreBattleActionsEnabled()}");
                return false;
            }

            if (currentUnitInTurn.Faction == UnitFactionType.Enemy)
            {
                Debug.Log($"{inputName} ignored. Current unit is enemy: {currentUnitInTurn.name}");
                return false;
            }

            return true;
        }

        private void TrySelectSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= currentUnitInTurn.Skills.Count)
            {
                Debug.LogWarning($"{currentUnitInTurn.name} does not have skill slot {skillIndex}.");
                return;
            }

            SelectSkill(currentUnitInTurn.Skills[skillIndex]);
        }

        private void SelectSkill(SkillConfig skill)
        {
            if (skill == null)
            {
                Debug.LogWarning("Cannot select a null skill.");
                return;
            }

            selectedSkill = skill;

            var targetCandidates = BattleManager.Instance.GetValidTargets(currentUnitInTurn, selectedSkill);

            Debug.Log($"Selected skill {selectedSkill.name}. State: {currentBattleState}, unit: {currentUnitInTurn.name}, faction: {currentUnitInTurn.Faction}, targets: {targetCandidates.Count}");

            targetingManager.BeginTargeting(currentUnitInTurn, selectedSkill, targetCandidates);
        }

        private void ConfirmTargeting()
        {
            if (selectedSkill == null || !targetingManager.HasValidSelection)

                return;

            BattleManager.Instance.SetState(BattleState.ResolvingActions);
        }

        private void ResolveEnemyTurn()
        {
            Unit caster = currentUnitInTurn;

            if (caster.Skills.Count == 0)
            {
                enemyTurnRoutine = null;
                BattleManager.Instance.SetState(BattleState.CheckingBattleEnd);
                return;
            }

            selectedSkill = caster.Skills[0];
            var targetCandidates = BattleManager.Instance.GetValidTargets(caster, selectedSkill);

            targetingManager.BeginTargeting(caster, selectedSkill, targetCandidates);

            if (!targetingManager.HasValidSelection)
            {
                targetingManager.ClearTargeting();
                selectedSkill = null;
                enemyTurnRoutine = null;
                BattleManager.Instance.SetState(BattleState.CheckingBattleEnd);
                return;
            }

            enemyTurnRoutine = null;
            BattleManager.Instance.SetState(BattleState.ResolvingActions);
        }

        private void StartEnemyTurnRoutine()
        {
            if (enemyTurnRoutine != null) return;

            enemyTurnRoutine = StartCoroutine(EnemyTurnRoutine());
        }

        private IEnumerator EnemyTurnRoutine()
        {
            Debug.Log($"Enemy turn delay started for {currentUnitInTurn.name}.");

            if (enemyTurnDelay > 0f)
            {
                yield return new WaitForSeconds(enemyTurnDelay);
            }

            if (currentBattleState != BattleState.WaitingForInput ||
                BattleManager.Instance.CurrentState != BattleState.WaitingForInput ||
                currentUnitInTurn == null ||
                currentUnitInTurn.Faction != UnitFactionType.Enemy ||
                !currentUnitInTurn.IsAlive)
            {
                Debug.Log("Enemy turn delay cancelled because the battle state or current unit changed.");
                enemyTurnRoutine = null;
                yield break;
            }

            ResolveEnemyTurn();
        }

        private void StopEnemyTurnRoutine()
        {
            if (enemyTurnRoutine == null) return;

            StopCoroutine(enemyTurnRoutine);
            enemyTurnRoutine = null;
        }

        private void StopAfterActionRoutine()
        {
            if (afterActionRoutine == null) return;

            StopCoroutine(afterActionRoutine);
            afterActionRoutine = null;
        }

        private void StopSkillExecutionRoutine()
        {
            if (skillExecutionRoutine == null) return;

            StopCoroutine(skillExecutionRoutine);
            skillExecutionRoutine = null;
        }

        private void ResolveTargeting()
        {
            if (selectedSkill == null)
            {
                Debug.LogWarning("Cannot resolve targeting because no skill is selected.");
                BattleManager.Instance.SetState(BattleState.CheckingBattleEnd);
                return;
            }

            Unit caster = currentUnitInTurn;
            var targets = targetingManager.GetSelectedTargets();
            var isEnemyAction = caster != null && caster.Faction == UnitFactionType.Enemy;
            var skillToExecute = selectedSkill;

            targetingManager.ClearTargeting();
            selectedSkill = null;

            skillExecutionRoutine = StartCoroutine(
                ExecuteSkillSequence(skillToExecute, caster, targets, isEnemyAction));
        }

        private IEnumerator ExecuteSkillSequence(
            SkillConfig skill,
            Unit caster,
            System.Collections.Generic.List<Unit> targets,
            bool isEnemyAction)
        {
            yield return skill.Execute(caster, targets);
            skillExecutionRoutine = null;

            if (BattleManager.Instance.CurrentState != BattleState.Victory &&
                BattleManager.Instance.CurrentState != BattleState.Defeat)
            {
                if (isEnemyAction && afterActionDelay > 0f)
                {
                    afterActionRoutine = StartCoroutine(EndTurnAfterActionDelay());
                }
                else
                {
                    BattleManager.Instance.SetState(BattleState.CheckingBattleEnd);
                }
            }
        }

        private IEnumerator EndTurnAfterActionDelay()
        {
            yield return new WaitForSeconds(afterActionDelay);
            afterActionRoutine = null;

            if (BattleManager.Instance.CurrentState == BattleState.ResolvingActions)
            {
                BattleManager.Instance.SetState(BattleState.CheckingBattleEnd);
            }
        }

        public void OnEvent(OnChangeBattleState e)
        {
            if (BattleManager.Instance.CurrentState != e.battleState)
            {
                Debug.Log($"Stale battle input state event ignored. Event state: {e.battleState}, current state: {BattleManager.Instance.CurrentState}");
                return;
            }

            currentBattleState = BattleManager.Instance.CurrentState;
            Debug.Log($"Battle input state changed. State: {currentBattleState}, unit: {(currentUnitInTurn != null ? currentUnitInTurn.name : "null")}, faction: {(currentUnitInTurn != null ? currentUnitInTurn.Faction.ToString() : "none")}, actions enabled: {AreBattleActionsEnabled()}");

            switch (currentBattleState)
            {
                case BattleState.ResolvingActions:
                ResolveTargeting();
                break;

                case BattleState.WaitingForInput:
                selectedSkill = null;
                targetingManager.ClearTargeting();

                if (currentUnitInTurn != null && currentUnitInTurn.Faction == UnitFactionType.Enemy)
                {
                    StartEnemyTurnRoutine();
                }
                break;

                case BattleState.Victory:
                case BattleState.Defeat:
                StopEnemyTurnRoutine();
                StopSkillExecutionRoutine();
                StopAfterActionRoutine();
                break;

                default:
                break;
            }
        }

        public void OnEvent(OnTurnStart e)
        {
            currentUnitInTurn = e.Unit;
            Debug.Log($"Turn started. Unit: {(currentUnitInTurn != null ? currentUnitInTurn.name : "null")}, faction: {(currentUnitInTurn != null ? currentUnitInTurn.Faction.ToString() : "none")}");
        }
    }
}
