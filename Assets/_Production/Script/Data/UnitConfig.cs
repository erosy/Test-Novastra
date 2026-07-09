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

        [Title("Visual Settings")]
        [AssetsOnly]
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Vector3 visualLocalPosition;
        [SerializeField] private Vector3 visualLocalScale = Vector3.one;

        [Header("Stat Properties")]
        [SerializeField] List<StatProperty> properties = new();

        [Header("Skills")]
        [SerializeField] List<SkillConfig> skills = new();

        public IReadOnlyList<StatProperty> Properties => properties;
        public IReadOnlyList<SkillConfig> Skills => skills;

        public GameObject UnitPrefab => unitPrefab;
        public GameObject VisualPrefab => visualPrefab;
        public Vector3 VisualLocalPosition => visualLocalPosition;
        public Vector3 VisualLocalScale => visualLocalScale;

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
