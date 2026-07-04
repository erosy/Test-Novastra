using UnityEngine;
using Sirenix.OdinInspector;
using Gamepangin;
using System.Collections.Generic;


namespace NovastraTest
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "ScriptableObjects/UnitConfig", order = 1)]
    public class UnitConfig : DataDefinition<UnitConfig>
    {
        [OnValueChanged(nameof(OnNameChanged))]
        [SerializeField] private string unitName;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Sprite unitSprite;

        [Header("Stat Properties")]
        [SerializeField] List<StatProperty> properties = new();

        [Header("Skills")]
        [SerializeField] List<SkillConfig> skills = new();

        public IReadOnlyList<StatProperty> Properties => properties;
        public IReadOnlyList<SkillConfig> Skills => skills;

        private void OnNameChanged()
        {
            var formattedString = unitName.ToLower().Replace(" ", "-");
            id = $"unit-{formattedString}";
        }
    }
    


    public enum UnitFactionType
    {
        Player,
        Enemy   
    }
}