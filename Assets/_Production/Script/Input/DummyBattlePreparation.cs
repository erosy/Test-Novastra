using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Lean.Pool;
using Unity.Mathematics;

namespace NovastraTest
{
    public class DummyBattlePreparation : MonoBehaviour
    {
        [SerializeField] private List<UnitConfig> playerUnits;
        [SerializeField] private List<UnitConfig> enemyUnits;
        [SerializeField] private List<Transform> playerPositions;
        [SerializeField] private List<Transform> enemyPositions;

        [Button]
        public void SetupBattle()
        {
            for (int i = 0; i < playerUnits.Count; i++)
            {
                if (i >= playerPositions.Count) continue;

                var unit = LeanPool.Spawn(playerUnits[i].UnitPrefab, playerPositions[i].position, Quaternion.identity);

                var unitComponent = unit.GetComponent<Unit>();

                unitComponent.Initialize(playerUnits[i], UnitFactionType.Player);

                BattleManager.Instance.RegisterUnit(unitComponent);
            }

            for (int i = 0; i < enemyUnits.Count; i++)
            {
                if (i >= enemyPositions.Count) continue;

                var unit = LeanPool.Spawn(enemyUnits[i].UnitPrefab, enemyPositions[i].position, Quaternion.identity);

                var unitComponent = unit.GetComponent<Unit>();

                unitComponent.Initialize(enemyUnits[i], UnitFactionType.Enemy);

                BattleManager.Instance.RegisterUnit(unitComponent);
            }


            BattleManager.Instance.SetState(BattleState.Setup);
        }

    }
}


//Since I have limited time to create character select that requires save data and blackboard class, I decide to generate current character setup using dummy battle preparation.

