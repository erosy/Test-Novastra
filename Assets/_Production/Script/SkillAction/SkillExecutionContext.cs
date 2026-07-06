using System.Collections.Generic;

namespace NovastraTest
{
    public readonly struct SkillExecutionContext
    {
        public Unit Caster { get; }
        public List<Unit> Targets { get; }
        public SkillConfig skill {get;}

        public SkillExecutionContext(Unit caster, List<Unit> targets, SkillConfig skill)
        {
            Caster = caster;
            Targets = targets;
            this.skill = skill;
        }
    }
}