using System;
using System.Collections.Generic;
using Gamepangin;
using UnityEngine;
using UnityEngine.UI;

namespace NovastraTest
{
    public class TurnOrderUI : MonoBehaviour, IEventListener<OnTurnStart>, IEventListener<OnChangeBattleState>
    {
        private const int SlotCount = 5;

        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Text[] slotTexts = new Text[SlotCount];
        [SerializeField] private Image[] slotBackgrounds = new Image[SlotCount];
        [SerializeField] private Color playerColor = new(0.15f, 0.45f, 0.85f, 0.85f);
        [SerializeField] private Color enemyColor = new(0.8f, 0.2f, 0.2f, 0.85f);

        private void OnEnable()
        {
            this.EventStartListening<OnTurnStart>();
            this.EventStartListening<OnChangeBattleState>();
            Refresh();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnTurnStart>();
            this.EventStopListening<OnChangeBattleState>();
        }

        public void OnEvent(OnTurnStart e)
        {
            Refresh();
        }

        public void OnEvent(OnChangeBattleState e)
        {
            if (e.battleState == BattleState.Victory || e.battleState == BattleState.Defeat)
            {
                ClearSlots();
            }
        }

        private void Refresh()
        {
            if (turnManager == null)
            {
                ClearSlots();
                return;
            }

            IReadOnlyList<Unit> preview = turnManager.GetTurnPreview(SlotCount);

            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                int previewIndex = SlotCount - 1 - slotIndex;
                Unit unit = previewIndex < preview.Count ? preview[previewIndex] : null;
                SetSlot(slotIndex, unit);
            }
        }

        private void SetSlot(int slotIndex, Unit unit)
        {
            Text slotText = GetElement(slotTexts, slotIndex);
            Image slotBackground = GetElement(slotBackgrounds, slotIndex);
            bool hasUnit = unit != null && unit.Config != null;

            if (slotText != null)
            {
                slotText.text = hasUnit
                    ? $"{unit.Config.UnitName} — {unit.Faction.ToString().ToUpperInvariant()}"
                    : string.Empty;
                slotText.gameObject.SetActive(hasUnit);
            }

            if (slotBackground != null)
            {
                slotBackground.color = unit != null && unit.Faction == UnitFactionType.Enemy
                    ? enemyColor
                    : playerColor;
                slotBackground.gameObject.SetActive(hasUnit);
            }
        }

        private void ClearSlots()
        {
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                SetSlot(slotIndex, null);
            }
        }

        private static T GetElement<T>(IReadOnlyList<T> elements, int index) where T : class
        {
            return elements != null && index >= 0 && index < elements.Count ? elements[index] : null;
        }

        private void OnValidate()
        {
            if (slotTexts == null || slotTexts.Length != SlotCount)
            {
                Array.Resize(ref slotTexts, SlotCount);
            }

            if (slotBackgrounds == null || slotBackgrounds.Length != SlotCount)
            {
                Array.Resize(ref slotBackgrounds, SlotCount);
            }
        }
    }
}
