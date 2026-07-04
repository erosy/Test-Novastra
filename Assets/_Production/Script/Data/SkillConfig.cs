using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Gamepangin;

namespace NovastraTest
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ScriptableObjects/SkillConfig", order = 1)]
    public class SkillConfig : DataDefinition<SkillConfig>
    {
        [OnValueChanged(nameof(OnNameChanged))]
        [SerializeField] private string skillName;

        [Title("Skill Action Sequence")]
        [SerializeReference]
        private List<SkillAction> actionSequence = new List<SkillAction>();

        public IReadOnlyList<SkillAction> ActionSequence => actionSequence;

        private void OnNameChanged()
        {
            var formattedString = skillName.ToLower().Replace(" ", "-");
            id = $"skill-{formattedString}";
        }
    }

    public enum SkillTargetType
    {
        Caster,
        Target
    }
}