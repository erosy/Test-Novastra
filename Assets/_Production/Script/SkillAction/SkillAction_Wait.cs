using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NovastraTest
{
    [Serializable]
    public class SkillAction_Wait : SkillAction
    {
        [MinValue(0f)]
        [SerializeField] private float duration;

        public override IEnumerator Execute(SkillExecutionContext context)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
        }
    }
}
