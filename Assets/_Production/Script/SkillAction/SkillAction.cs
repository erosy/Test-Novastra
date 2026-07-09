using System;
using System.Collections;

namespace NovastraTest
{
    [Serializable]
    public abstract class SkillAction
    {
        public abstract IEnumerator Execute(SkillExecutionContext context);
    }
}
