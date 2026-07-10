
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovastraTest
{
    public class TargetingManager : MonoBehaviour
    {
        private readonly List<Unit> candidates = new List<Unit>();
        private readonly List<Unit> selectedTargets = new List<Unit>();
        private int selectedIndex;

        public Unit Caster { get; private set; }
        public SkillConfig CurrentSkill { get; private set; }
        public IReadOnlyList<Unit> Candidates => candidates;
        public IReadOnlyList<Unit> SelectedTargets => selectedTargets;
        public bool HasValidSelection => selectedTargets.Count > 0;
        public TargetingType CurrentTargetingType => CurrentSkill != null ? CurrentSkill.TargetingType : default;

        public void BeginTargeting(Unit caster, SkillConfig skill, IReadOnlyList<Unit> targetCandidates)
        {
            OutlineTargets(false);

            Caster = caster;
            CurrentSkill = skill;
            selectedIndex = 0;

            candidates.Clear();
            selectedTargets.Clear();

            if (skill == null || caster == null)
            {
                return;
            }

            if (skill.TargetingType == TargetingType.Self)
            {
                candidates.Add(caster);
            }
            else if (targetCandidates != null)
            {
                candidates.AddRange(targetCandidates.Where(unit => unit != null && unit.IsAlive));
            }

            RefreshSelectedTargets();
        }

        public void SelectNext()
        {
            MoveSelection(1);
        }

        public void SelectPrevious()
        {
            MoveSelection(-1);
        }

        public void SelectRandom()
        {
            if (CurrentSkill == null || candidates.Count <= 1) return;
            if (CurrentSkill.TargetingType == TargetingType.AllTargets || CurrentSkill.TargetingType == TargetingType.Self) return;

            selectedIndex = Random.Range(0, candidates.Count);
            RefreshSelectedTargets();
        }

        public List<Unit> GetSelectedTargets()
        {
            return selectedTargets.ToList();
        }

        public void ClearTargeting()
        {
            OutlineTargets(false);

            Caster = null;
            CurrentSkill = null;
            selectedIndex = 0;
            candidates.Clear();
            selectedTargets.Clear();
        }

        private void MoveSelection(int direction)
        {
            if (CurrentSkill == null || candidates.Count == 0) return;
            if (CurrentSkill.TargetingType == TargetingType.AllTargets || CurrentSkill.TargetingType == TargetingType.Self) return;

            selectedIndex = WrapIndex(selectedIndex + direction, candidates.Count);
            RefreshSelectedTargets();
        }

        private void RefreshSelectedTargets()
        {
            OutlineTargets(false);

            selectedTargets.Clear();

            if (CurrentSkill == null || candidates.Count == 0) return;

            switch (CurrentSkill.TargetingType)
            {
                case TargetingType.SingleTarget:
                    selectedTargets.Add(candidates[selectedIndex]);
                    break;
                case TargetingType.AllTargets:
                    selectedTargets.AddRange(candidates);
                    break;
                case TargetingType.MultipleTargets:
                    AddMultipleTargets();
                    break;
                case TargetingType.Self:
                    selectedTargets.Add(Caster);
                    break;
            }

            OutlineTargets(true);
            
        }

        private void AddMultipleTargets()
        {
            var targetCount = Mathf.Clamp(CurrentSkill.TargetCount, 1, candidates.Count);

            for (var i = 0; i < targetCount; i++)
            {
                var targetIndex = WrapIndex(selectedIndex + i, candidates.Count);
                selectedTargets.Add(candidates[targetIndex]);
            }
        }

        private int WrapIndex(int index, int count)
        {
            return (index % count + count) % count;
        }

        private void OutlineTargets(bool active)
        {
            foreach (var target in selectedTargets)
            {
                if (target == null) continue;

                target.SetOutlineActive(active);
            }
        }
    }
}
