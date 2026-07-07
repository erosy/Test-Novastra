
using UnityEngine;

namespace NovastraTest
{
    public class TargetingManager : MonoBehaviour
    {
        public TargetingType CurrentTargetingType{get; private set;}

        private void SetTargetingType(TargetingType targetingType)
        {
            CurrentTargetingType = targetingType;
            SetDefaultTargeting();
        }

        private void SetDefaultTargeting()
        {
            // switch (CurrentTargetingType)
            // {
                
            // }
        }
    }
}