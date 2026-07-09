using System;
using System.Collections;

namespace NovastraTest
{
    [Serializable]
    public class SkillAction_Heal : SkillAction
    {

        public SkillTargetType targetType;

        public int healAmount;

        public int HealAmount => healAmount;

        public override IEnumerator Execute(SkillExecutionContext context)
        {
            foreach (var target in context.Targets)
            {
                OnHeal.Trigger(target, healAmount);
            }

            yield break;
        }
    }
}
