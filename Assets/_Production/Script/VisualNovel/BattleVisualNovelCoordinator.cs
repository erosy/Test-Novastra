using System;
using Gamepangin;
using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NovastraTest
{
    public class BattleVisualNovelCoordinator : MonoBehaviour, IEventListener<OnChangeBattleState>
    {
        private const string VisualNovelSceneName = "VN Handler Scene";
        private const int HandlerLookupFrameLimit = 60;

        private bool isRunning;
        private bool loadedScene;

        private void OnEnable()
        {
            this.EventStartListening<OnChangeBattleState>();
        }

        private void OnDisable()
        {
            this.EventStopListening<OnChangeBattleState>();
        }

        public void OnEvent(OnChangeBattleState e)
        {
            if (e.battleState != BattleState.VisualNovelPause)
            {
                return;
            }

            RunVisualNovelPauseAsync().Forget();
        }

        private async UniTaskVoid RunVisualNovelPauseAsync()
        {
            if (isRunning)
            {
                Debug.LogWarning("Visual novel pause is already running. Ignoring duplicate state event.");
                return;
            }

            isRunning = true;

            var battleManager = BattleManager.Instance;
            var battleCamera = Camera.main;
            var script = battleManager.PendingVisualNovelScript;
            var resumeState = battleManager.VisualNovelResumeState;

            try
            {
                if (script == null)
                {
                    Debug.LogWarning("Battle entered VisualNovelPause without a pending Naninovel script.");
                    return;
                }

                await LoadVisualNovelSceneAsync();

                var handler = await FindHandlerAsync();
                if (handler == null)
                {
                    Debug.LogError($"Unable to find {nameof(VNHandler)} after loading {VisualNovelSceneName}.");
                    return;
                }

                await handler.PlayAsync(script, battleCamera);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                await UnloadVisualNovelSceneAsync();

                battleManager.ClearVisualNovelPauseRequest();

                if (battleManager.CurrentState == BattleState.VisualNovelPause)
                {
                    battleManager.SetState(resumeState);
                }

                isRunning = false;
            }
        }

        private async UniTask LoadVisualNovelSceneAsync()
        {
            var scene = SceneManager.GetSceneByName(VisualNovelSceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                loadedScene = false;
                return;
            }

            var operation = SceneManager.LoadSceneAsync(VisualNovelSceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to load additive scene '{VisualNovelSceneName}'. Ensure it is included in build settings.");
            }

            loadedScene = true;
            await operation.ToUniTask();
        }

        private async UniTask<VNHandler> FindHandlerAsync()
        {
            for (var frame = 0; frame < HandlerLookupFrameLimit; frame++)
            {
                var handler = FindFirstObjectByType<VNHandler>();
                if (handler != null)
                {
                    return handler;
                }

                await UniTask.DelayFrame(1);
            }

            return null;
        }

        private async UniTask UnloadVisualNovelSceneAsync()
        {
            if (!loadedScene)
            {
                return;
            }

            var scene = SceneManager.GetSceneByName(VisualNovelSceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation != null)
                {
                    await operation.ToUniTask();
                }
            }

            loadedScene = false;
        }
    }
}
