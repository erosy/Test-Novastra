using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace NovastraTest
{
    public class Unit : MonoBehaviour
    {
        public GameObject outline;
        public UnitConfig Config { get; private set; }
        public UnitFactionType Faction { get; private set; }
        public UnitHealth Health { get; private set; }
        public IReadOnlyList<SkillConfig> Skills => Config.Skills;

        [Title("Stat Properties")]
        [ShowInInspector, ReadOnly]
        public float Attack { get; private set; }
        [ShowInInspector, ReadOnly]
        public float Defense { get; private set; }
        [ShowInInspector, ReadOnly]
        public float Speed { get; private set; }
        public bool IsAlive => Health.CurrentHealth > 0;

        [Title("Debug")]
        public bool IsTestingDummy;

        [ShowIf("IsTestingDummy")]
        public UnitConfig TestingDummyConfig;
        public UnitFactionType TestingDummyFaction;

        public void Initialize(UnitConfig config, UnitFactionType faction)
        {
            Config = config;
            Faction = faction;
            Health = GetComponent<UnitHealth>();

            // read HP/ATK/DEF/SPD from config.Properties
            // Health.InitHealth(hp);
            Attack = config.Properties.FirstOrDefault(p => p.Definition.statType == StatType.Attack)?.Float ?? 0f;
            Defense = config.Properties.FirstOrDefault(p => p.Definition.statType == StatType.Defense)?.Float ?? 0f;
            Speed = config.Properties.FirstOrDefault(p => p.Definition.statType == StatType.Speed)?.Integer ?? 0f;

            var hp = config.Properties.FirstOrDefault(p => p.Definition.statType == StatType.Health)?.Float ?? 100f;
            Health.InitHealth(hp);

            outline.SetActive(false);
        }

        [Button("Init Dummy")]
        public void InitDummy()
        {
            if (!IsTestingDummy) return;

            Initialize(TestingDummyConfig, TestingDummyFaction);
        }

        public void SetOutlineActive(bool active)
        {
            outline.SetActive(active);
        }

    }
}