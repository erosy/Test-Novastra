using System.Collections;
using Gamepangin;
using UnityEngine;
using UnityEngine.Rendering;

namespace NovastraTest
{
    public class BattlePostProcessController : MonoBehaviour,
        IEventListener<OnTakeDamage>,
        IEventListener<OnHeal>,
        IEventListener<OnDeath>,
        IEventListener<OnChangeBattleState>
    {
        private const float DangerHealthThreshold = 0.3f;
        private const float DamageFadeInDuration = 0.02f;
        private const float DamageFadeOutDuration = 0.18f;
        private const float HealFadeInDuration = 0.1f;
        private const float HealFadeOutDuration = 0.35f;
        private const float DangerFadeDuration = 0.25f;
        private const float EndStateFadeDuration = 0.6f;

        [Header("Battle")]
        [SerializeField] private BattleManager battleManager;

        [Header("Volumes")]
        [SerializeField] private Volume baselineVolume;
        [SerializeField] private Volume damageVolume;
        [SerializeField] private Volume healVolume;
        [SerializeField] private Volume dangerVolume;
        [SerializeField] private Volume victoryVolume;
        [SerializeField] private Volume defeatVolume;

        private Coroutine damageRoutine;
        private Coroutine healRoutine;
        private Coroutine dangerRoutine;
        private Coroutine endStateRoutine;
        private bool dangerActive;
        private bool battleEnded;

        private void Awake()
        {
            battleManager ??= FindAnyObjectByType<BattleManager>();
        }

        private void OnEnable()
        {
            this.EventStartListening<OnTakeDamage>();
            this.EventStartListening<OnHeal>();
            this.EventStartListening<OnDeath>();
            this.EventStartListening<OnChangeBattleState>();

            battleEnded = false;
            dangerActive = false;
            ResetVolumeWeights();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnTakeDamage>();
            this.EventStopListening<OnHeal>();
            this.EventStopListening<OnDeath>();
            this.EventStopListening<OnChangeBattleState>();

            StopAllCoroutines();
            damageRoutine = null;
            healRoutine = null;
            dangerRoutine = null;
            endStateRoutine = null;
            ResetVolumeWeights(includeBaseline: true);
        }

        private void LateUpdate()
        {
            if (!battleEnded)
            {
                RefreshDangerState();
            }
        }

        public void OnEvent(OnTakeDamage e)
        {
            if (battleEnded || e.Target == null ||
                e.Target.Faction != UnitFactionType.Player)
            {
                return;
            }

            RestartPulse(damageVolume, ref damageRoutine,
                DamageFadeInDuration, DamageFadeOutDuration);
        }

        public void OnEvent(OnHeal e)
        {
            if (battleEnded || e.Target == null) return;

            RestartPulse(healVolume, ref healRoutine,
                HealFadeInDuration, HealFadeOutDuration);
        }

        public void OnEvent(OnDeath e)
        {
            if (!battleEnded)
            {
                RefreshDangerState();
            }
        }

        public void OnEvent(OnChangeBattleState e)
        {
            switch (e.battleState)
            {
                case BattleState.Victory:
                    EnterEndState(victoryVolume);
                    break;
                case BattleState.Defeat:
                    EnterEndState(defeatVolume);
                    break;
            }
        }

        private void RestartPulse(
            Volume volume,
            ref Coroutine routine,
            float fadeInDuration,
            float fadeOutDuration)
        {
            if (volume == null) return;

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(
                PulseVolume(volume, fadeInDuration, fadeOutDuration));
        }

        private IEnumerator PulseVolume(
            Volume volume,
            float fadeInDuration,
            float fadeOutDuration)
        {
            yield return FadeVolume(volume, 1f, fadeInDuration);
            yield return FadeVolume(volume, 0f, fadeOutDuration);

            if (volume == damageVolume)
            {
                damageRoutine = null;
            }
            else if (volume == healVolume)
            {
                healRoutine = null;
            }
        }

        private void RefreshDangerState()
        {
            if (dangerVolume == null || battleManager == null) return;

            var livingPlayerCount = 0;
            Unit survivingPlayer = null;
            foreach (var unit in battleManager.PlayerTeam.Units)
            {
                if (unit == null || unit.Health == null || !unit.IsAlive) continue;

                livingPlayerCount++;
                survivingPlayer = unit;
                if (livingPlayerCount > 1) break;
            }

            var shouldBeActive =
                livingPlayerCount == 1 &&
                survivingPlayer.Health != null &&
                survivingPlayer.Health.NormalizedHealth < DangerHealthThreshold;

            if (shouldBeActive == dangerActive) return;

            dangerActive = shouldBeActive;
            if (dangerRoutine != null)
            {
                StopCoroutine(dangerRoutine);
            }

            dangerRoutine = StartCoroutine(
                FadeDangerVolume(shouldBeActive ? 1f : 0f));
        }

        private IEnumerator FadeDangerVolume(float targetWeight)
        {
            yield return FadeVolume(dangerVolume, targetWeight, DangerFadeDuration);
            dangerRoutine = null;
        }

        private void EnterEndState(Volume endStateVolume)
        {
            if (battleEnded || endStateVolume == null) return;

            battleEnded = true;
            dangerActive = false;

            StopTransientRoutines();
            SetWeight(damageVolume, 0f);
            SetWeight(healVolume, 0f);
            SetWeight(dangerVolume, 0f);

            if (endStateRoutine != null)
            {
                StopCoroutine(endStateRoutine);
            }

            endStateRoutine = StartCoroutine(
                FadeEndStateVolume(endStateVolume));
        }

        private IEnumerator FadeEndStateVolume(Volume volume)
        {
            yield return FadeVolume(volume, 1f, EndStateFadeDuration);
            endStateRoutine = null;
        }

        private IEnumerator FadeVolume(
            Volume volume,
            float targetWeight,
            float duration)
        {
            if (volume == null) yield break;

            var startWeight = volume.weight;
            if (duration <= 0f)
            {
                volume.weight = targetWeight;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                volume.weight = Mathf.Lerp(
                    startWeight,
                    targetWeight,
                    Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            volume.weight = targetWeight;
        }

        private void StopTransientRoutines()
        {
            if (damageRoutine != null) StopCoroutine(damageRoutine);
            if (healRoutine != null) StopCoroutine(healRoutine);
            if (dangerRoutine != null) StopCoroutine(dangerRoutine);

            damageRoutine = null;
            healRoutine = null;
            dangerRoutine = null;
        }

        private void ResetVolumeWeights(bool includeBaseline = false)
        {
            SetWeight(baselineVolume, includeBaseline ? 0f : 1f);
            SetWeight(damageVolume, 0f);
            SetWeight(healVolume, 0f);
            SetWeight(dangerVolume, 0f);
            SetWeight(victoryVolume, 0f);
            SetWeight(defeatVolume, 0f);
        }

        private static void SetWeight(Volume volume, float weight)
        {
            if (volume != null)
            {
                volume.weight = weight;
            }
        }
    }
}
