using System;
using UnityEngine;


namespace NovastraTest
{
    public class SkillAction_Damage : SkillAction
    {

        public SkillTargetType targetType;

        public int damageAmount;

        public int DamageAmount => damageAmount;

        public override void Execute(SkillExecutionContext context)
        {
            //do damage here.
            foreach (var target in context.Targets)
            {
                float finalDamage = Mathf.Max(1f, damageAmount + (damageAmount * context.Caster.Attack/100f));
                OnTakeDamage.Trigger(target, finalDamage);
            }
        }
    }
}