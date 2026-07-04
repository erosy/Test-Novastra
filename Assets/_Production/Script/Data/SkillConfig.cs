using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Gamepangin;

namespace NovastraTest
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ScriptableObjects/SkillConfig", order = 1)]
    public class SkillConfig : DataDefinition<SkillConfig>
    {
        [SerializeField] private string skillName;

        [Title("Skill Action Sequence")]
        [SerializeReference]
        private List<SkillAction> actionSequence = new List<SkillAction>();
    }

    public enum SkillTargetType
    {
        Caster,
        Target
    }
}