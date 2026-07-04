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

        [BoxGroup("Stats")]
        [SerializeField] List<StatProperty> properties = new();

        public IReadOnlyList<StatProperty> Properties => properties;

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