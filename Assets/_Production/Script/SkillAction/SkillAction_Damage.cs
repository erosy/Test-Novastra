using System;
using System.Collections;
using UnityEngine;


namespace NovastraTest
{
    [Serializable]
    public class SkillAction_Damage : SkillAction
    {

        public SkillTargetType targetType;

        public int damageAmount;

        public int DamageAmount => damageAmount;

        public override IEnumerator Execute(SkillExecutionContext context)
        {
            foreach (var target in context.Targets)
            {
                float finalDamage = Mathf.Max(1f, damageAmount + (damageAmount * context.Caster.Attack/100f));
                OnTakeDamage.Trigger(target, finalDamage);
            }

            yield break;
        }
    }
}
