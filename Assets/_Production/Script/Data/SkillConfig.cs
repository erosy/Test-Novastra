using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Gamepangin;
using System;

namespace NovastraTest
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ScriptableObjects/SkillConfig", order = 1)]
    public class SkillConfig : DataDefinition<SkillConfig>
    {
        [OnValueChanged(nameof(OnNameChanged))]
        [SerializeField] private string skillName;

        [Title("Targeting Settings")]
        [SerializeField] private TargetingType targetingType;

        [ShowIf(nameof(IsMultipleTargets))]
        [MinValue(2)]
        [SerializeField] private int targetCount = 2;

        public TargetingType TargetingType => targetingType;
        public int TargetCount => Math.Max(2, targetCount);

        private bool IsMultipleTargets => targetingType == TargetingType.MultipleTargets;

        [Title("Skill Action Sequence")]
        [SerializeReference]
        private List<SkillAction> actionSequence = new List<SkillAction>();

        public IReadOnlyList<SkillAction> ActionSequence => actionSequence;

        private void OnNameChanged()
        {
            var formattedString = skillName.ToLower().Replace(" ", "-");
            id = $"skill-{formattedString}";
        }

        public void Execute(Unit caster, List<Unit> targets)
        {
            var context = new SkillExecutionContext(caster, targets, this);

            OnAttack.Trigger(caster, targets, this);

            foreach (var action in actionSequence)
            {
                action.Execute(context);
            }
        }
    }

    public enum SkillTargetType
    {
        Caster,
        Target
    }
}
