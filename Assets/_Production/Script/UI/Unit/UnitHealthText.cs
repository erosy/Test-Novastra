using UnityEngine;
using UnityEngine.UI;

namespace NovastraTest
{
    public class UnitHealthText : MonoBehaviour
    {
        [SerializeField] private Text healthText;

        public void UpdateHealth(float currentHealth)
        {
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}";
            }
        }
    }
}
