using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Naninovel;


namespace NovastraTest
{
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "ScriptableObjects/BattleConfig", order = 1)]
    public class BattleConfig: SerializedScriptableObject
    {
        [RequiredListLength(0,3)]
        [SerializeField]
        private List<UnitConfig> enemyConfigs;

        [Title("Naninovel Script")]
        [SerializeField] 
        private bool scriptExists;
        [ShowIf("scriptExists")]
        [SerializeField]
        private Script startingScript;


        public IReadOnlyList<UnitConfig> EnemyConfigs => enemyConfigs;

        public bool ScriptExists => scriptExists;

        public Script StartingScript => startingScript;
        

    }
}