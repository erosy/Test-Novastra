using System;
using Gamepangin;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NovastraTest
{
    [CreateAssetMenu(order = 0, fileName = "New Stat Property", menuName = "ScriptableObjects/Stat Property")]
    public class PropertyDefinition : DataDefinition<PropertyDefinition>
    {
        [InlineButton("UpdateFilename"), OnValueChanged(nameof(OnNameChanged))]
        public string propertyName;
        public string description;
        public ItemPropertyType propertyType;

#if UNITY_EDITOR
        private void UpdateFilename()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            string newAssetPath = assetPath.Replace(name + ".asset", propertyName + ".asset");
            AssetDatabase.RenameAsset(assetPath, propertyName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
        
        private void OnNameChanged()
        {
            var formattedString = propertyName.ToLower().Replace(" ", "-");
            id = $"stat-{formattedString}";
        }
    }

    [Serializable]
    public class StatProperty
    {
        [SerializeField] private PropertyDefinition definition;
        [SerializeField] private float propertyValue;
        [SerializeField] private string propertyString;
        
        public string Id => definition.Id;
        public ItemPropertyType PropertyType => definition.propertyType;

        public StatProperty()
        {
            definition = null;
            propertyValue = 100f;
            propertyString = string.Empty;
        }

        public StatProperty(string definitionId, float value)
        {
            definition = PropertyDefinition.GetWithId(definitionId);
            propertyValue = value;
        }
        
        public StatProperty GetClone() => (StatProperty)MemberwiseClone();

        public bool Boolean
        {
            get => propertyValue > 0f;
            set
            {
                if (PropertyType == ItemPropertyType.Boolean)
                {
                    SetIntervalValue(value ? 1 : 0);
                }
            }
        }

        public int Integer
        {
            get => (int)propertyValue;
            set
            {
                if (PropertyType == ItemPropertyType.Integer)
                {
                    SetIntervalValue(value);
                }
            }
        }

        public float Float
        {
            get => (float)Math.Round(propertyValue, 2);
            set
            {
                if (PropertyType == ItemPropertyType.Float)
                {
                    SetIntervalValue(value);
                }
            }
        }
        
        public string String
        {
            get => propertyString;
            set
            {
                if (PropertyType == ItemPropertyType.String)
                {
                    SetStringValue(value);
                }
            }
        }

        public UnityAction<StatProperty> PropertyChanged;

        private void SetIntervalValue(float value)
        {
            var oldValue = propertyValue;
            propertyValue = value;

            if (Math.Abs(oldValue - propertyValue) > 0.01f)
            {
                PropertyChanged?.Invoke(this);
            }
        }
        
        private void SetStringValue(string value)
        {
            var oldValue = propertyString;
            propertyString = value;

            if (!string.Equals(oldValue, propertyString, StringComparison.Ordinal))
            {
                PropertyChanged?.Invoke(this);
            }
        }
    }
}
