using System;


namespace NovastraTest
{
    public class SkillAction_Damage : SkillAction
    {

        public SkillTargetType targetType;

        public int damageAmount;

        public int DamageAmount => damageAmount;

        public override void Execute()
        {
            //do damage here.
        }
    }
}