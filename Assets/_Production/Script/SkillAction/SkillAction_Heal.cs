using System;


namespace NovastraTest
{
    public class SkillAction_Heal : SkillAction
    {

        public SkillTargetType targetType;

        public int healAmount;

        public int HealAmount => healAmount;

        public override void Execute()
        {
            //do heal here.
        }
    }
}