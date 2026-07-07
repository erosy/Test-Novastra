using UnityEngine;
using UnityEngine.InputSystem;

namespace NovastraTest
{
    public class BattleInputManager : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TargetingManager targetingManager;
        [SerializeField] private TurnManager turnManager;

        private SkillConfig selectedSkill;

        private void Update()
        {
            if (battleManager.CurrentState != BattleState.WaitingForInput)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.rightArrowKey.wasPressedThisFrame)
                targetingManager.SelectNext();

            if (keyboard.leftArrowKey.wasPressedThisFrame)
                targetingManager.SelectPrevious();

            if (keyboard.enterKey.wasPressedThisFrame)
                ConfirmTargeting();
        }

        private void SelectSkill(SkillConfig skill)
        {
            selectedSkill = skill;
            Unit caster = turnManager.CurrentUnit;

            var targetCandidates = battleManager.GetValidTargets(caster, selectedSkill);

            targetingManager.BeginTargeting(caster, selectedSkill, targetCandidates);
        }

        private void ConfirmTargeting()
        {
            if (selectedSkill == null || !targetingManager.HasValidSelection)

            return;

            Unit caster = turnManager.CurrentUnit;
            var targets = targetingManager.GetSelectedTargets();

            selectedSkill.Execute(caster, targets);

            targetingManager.ClearTargeting();
            selectedSkill= null;

            battleManager.SetState(BattleState.ResolvingActions);
        }
    }


}
