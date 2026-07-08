using System;


namespace NovastraTest
{
    [Serializable]
    public class SkillAction_Heal : SkillAction
    {

        public SkillTargetType targetType;

        public int healAmount;

        public int HealAmount => healAmount;

        public override void Execute(SkillExecutionContext context)
        {
            //do heal here.
            foreach (var target in context.Targets)
            {
                OnHeal.Trigger(target, healAmount);
            }
        }
    }
}