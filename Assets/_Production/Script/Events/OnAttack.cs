using System.Collections.Generic;
using Gamepangin;

namespace NovastraTest
{
    public struct OnAttack
    {
        public Unit Caster;
        public List<Unit> Targets;
        public SkillConfig Skill;

        public OnAttack(Unit caster, List<Unit> targets, SkillConfig skill)
        {
            Caster = caster;
            Targets = targets;
            Skill = skill;
        }

        public static OnAttack onAttackEvent;
        public static void Trigger(Unit caster, List<Unit> targets, SkillConfig skill)
        {
            onAttackEvent = new OnAttack(caster, targets, skill);
            EventManager.TriggerEvent(onAttackEvent);
        }
    }
}