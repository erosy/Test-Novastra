using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NovastraTest
{
    [Serializable]
    public class SkillAction_AnimateOverride : SkillAction
    {
        [AssetsOnly]
        [SerializeField] private AnimationClip animationClip;

        [SerializeField] private bool waitUntilFinish;

        public override IEnumerator Execute(SkillExecutionContext context)
        {
            if (animationClip == null)
            {
                Debug.LogWarning($"Skill '{context.skill.name}' has an animation action with no clip.");
                yield break;
            }

            var visualController = context.Caster != null
                ? context.Caster.VisualController
                : null;

            if (visualController == null)
            {
                Debug.LogWarning($"Skill '{context.skill.name}' cannot animate because its caster has no visual controller.");
                yield break;
            }

            yield return visualController.PlaySkillAnimation(animationClip, waitUntilFinish);
        }
    }
}
