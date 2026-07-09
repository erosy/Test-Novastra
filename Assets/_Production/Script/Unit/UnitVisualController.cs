using System.Collections;
using Gamepangin;
using Lean.Pool;
using Live2D.Cubism.Rendering;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace NovastraTest
{
    public class UnitVisualController : MonoBehaviour,
        IEventListener<OnTakeDamage>,
        IEventListener<OnDeath>
    {
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int HitState = Animator.StringToHash("Hit");
        private static readonly int DeathState = Animator.StringToHash("Death");
        private const float StateTransitionDuration = 0.1f;

        [Title("Visual References")]
        [SerializeField] private Transform visualAnchor;
        [SerializeField] private GameObject fallbackVisual;

        private Unit ownerUnit;
        private UnitConfig config;
        private GameObject currentVisual;
        private Animator modelAnimator;
        private AnimationClip runtimeAnimatorClip;
        private PlayableGraph animatorGraph;
        private Coroutine skillCompletionRoutine;
        private int skillPlaybackVersion;
        private bool isSkillAnimationPlaying;
        private bool usesCubismWorldTransform;
        private bool isDead;

        private Transform VisualParent => visualAnchor != null ? visualAnchor : transform;

        public void Initialize(Unit unit, UnitConfig unitConfig)
        {
            ownerUnit = unit;
            config = unitConfig;
            isDead = false;

            ClearCurrentVisual();

            if (config == null || config.VisualPrefab == null)
            {
                SetFallbackActive(true);
                return;
            }

            usesCubismWorldTransform =
                config.VisualPrefab.GetComponentInChildren<CubismRenderController>(true) != null;

            if (usesCubismWorldTransform)
            {
                var stagingRoot = new GameObject($"{config.VisualPrefab.name} Spawn Staging");
                stagingRoot.SetActive(false);

                currentVisual = LeanPool.Spawn(config.VisualPrefab, stagingRoot.transform);
                SyncCubismWorldTransform();
                currentVisual.transform.SetParent(null, true);

                Destroy(stagingRoot);
            }
            else
            {
                currentVisual = LeanPool.Spawn(config.VisualPrefab, VisualParent);
                currentVisual.transform.localPosition = config.VisualLocalPosition;
                currentVisual.transform.localRotation = Quaternion.identity;
                currentVisual.transform.localScale = config.VisualLocalScale;
            }

            modelAnimator = currentVisual.GetComponentInChildren<Animator>(true);
            if (modelAnimator == null)
            {
                Debug.LogWarning($"Visual prefab '{config.VisualPrefab.name}' has no Animator.");
            }
            else if (modelAnimator.runtimeAnimatorController == null)
            {
                Debug.LogWarning(
                    $"Visual prefab '{config.VisualPrefab.name}' has no Animator Controller assigned.");
            }
            else
            {
                modelAnimator.Play(IdleState, 0, 0f);
            }

            SetFallbackActive(false);
        }

        private void OnEnable()
        {
            this.EventStartListening<OnTakeDamage>();
            this.EventStartListening<OnDeath>();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnTakeDamage>();
            this.EventStopListening<OnDeath>();

            ClearCurrentVisual();
        }

        private void LateUpdate()
        {
            if (usesCubismWorldTransform)
            {
                SyncCubismWorldTransform();
            }
        }

        public void OnEvent(OnTakeDamage e)
        {
            if (e.Target != ownerUnit || isDead) return;

            InterruptSkillAnimation();
            CrossFadeState(HitState);
        }

        public void OnEvent(OnDeath e)
        {
            if (e.DeadUnit != ownerUnit) return;

            isDead = true;
            InterruptSkillAnimation();
            CrossFadeState(DeathState);
        }

        public IEnumerator PlaySkillAnimation(AnimationClip clip, bool waitUntilFinish)
        {
            if (clip == null || modelAnimator == null || isDead)
            {
                yield break;
            }

            var playbackVersion = StartSkillAnimation(clip);
            if (!waitUntilFinish)
            {
                yield break;
            }

            while (isSkillAnimationPlaying &&
                   skillPlaybackVersion == playbackVersion &&
                   gameObject.activeInHierarchy)
            {
                yield return null;
            }
        }

        private void ClearCurrentVisual()
        {
            InterruptSkillAnimation(restoreIdle: false);
            modelAnimator = null;
            usesCubismWorldTransform = false;

            if (currentVisual == null) return;

            LeanPool.Despawn(currentVisual);
            currentVisual = null;
        }

        private int StartSkillAnimation(AnimationClip clip)
        {
            InterruptSkillAnimation(restoreIdle: false);

            skillPlaybackVersion++;
            isSkillAnimationPlaying = true;

            runtimeAnimatorClip = Instantiate(clip);
            runtimeAnimatorClip.wrapMode = WrapMode.ClampForever;

            animatorGraph = PlayableGraph.Create($"{ownerUnit.name} Skill Animation");
            animatorGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(animatorGraph, "Skill Animation", modelAnimator);
            var playable = AnimationClipPlayable.Create(animatorGraph, runtimeAnimatorClip);

            output.SetSourcePlayable(playable);
            animatorGraph.Play();

            var playbackVersion = skillPlaybackVersion;
            skillCompletionRoutine = StartCoroutine(
                CompleteSkillAnimationAfter(clip.length, playbackVersion));
            return playbackVersion;
        }

        private IEnumerator CompleteSkillAnimationAfter(float duration, int playbackVersion)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            if (playbackVersion != skillPlaybackVersion)
            {
                yield break;
            }

            skillCompletionRoutine = null;
            StopSkillPlayable();
            isSkillAnimationPlaying = false;

            if (!isDead)
            {
                CrossFadeState(IdleState);
            }
        }

        private void InterruptSkillAnimation(bool restoreIdle = false)
        {
            skillPlaybackVersion++;
            isSkillAnimationPlaying = false;

            if (skillCompletionRoutine != null)
            {
                StopCoroutine(skillCompletionRoutine);
                skillCompletionRoutine = null;
            }

            StopSkillPlayable();

            if (restoreIdle && !isDead)
            {
                CrossFadeState(IdleState);
            }
        }

        private void StopSkillPlayable()
        {
            if (animatorGraph.IsValid())
            {
                animatorGraph.Destroy();
            }

            if (runtimeAnimatorClip == null) return;

            Destroy(runtimeAnimatorClip);
            runtimeAnimatorClip = null;
        }

        private void CrossFadeState(int stateHash)
        {
            if (modelAnimator == null || modelAnimator.runtimeAnimatorController == null) return;

            modelAnimator.CrossFadeInFixedTime(stateHash, StateTransitionDuration, 0);
        }

        private void SyncCubismWorldTransform()
        {
            if (currentVisual == null || config == null) return;

            var anchor = VisualParent;
            var visualTransform = currentVisual.transform;

            visualTransform.SetPositionAndRotation(
                anchor.TransformPoint(config.VisualLocalPosition),
                anchor.rotation);
            visualTransform.localScale = Vector3.Scale(anchor.lossyScale, config.VisualLocalScale);
        }

        private void SetFallbackActive(bool active)
        {
            if (fallbackVisual == null) return;

            fallbackVisual.SetActive(active);
        }
    }
}
