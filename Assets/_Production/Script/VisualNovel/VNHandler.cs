using System;
using Gamepangin;
using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NovastraTest
{
    public class VNHandler : Singleton<VNHandler>
    {
        private ICameraManager cameraManager;
        private CameraConfiguration cameraConfiguration;
        private IStateManager stateManager;
        private IUIManager uiManager;
        private IScriptPlayer scriptPlayer;
        private Camera battleCamera;

        protected override void Awake()
        {
            base.Awake();

            Engine.OnInitializationFinished += OnInitializeFinished;
        }

        protected override void OnDestroy()
        {
            Engine.OnInitializationFinished -= OnInitializeFinished;
            base.OnDestroy();
        }

        public async UniTask PlayAsync(Script requestedScript, Camera baseCamera)
        {
            if (requestedScript == null)
            {
                throw new ArgumentNullException(nameof(requestedScript));
            }

            if (!Engine.Initialized)
            {
                await RuntimeInitializer.Initialize();
            }

            CheckDependencies();
            battleCamera = baseCamera != null ? baseCamera : Camera.main;
            SetupCameraState();
            SetupUIState();

            var scriptPath = requestedScript.Path;
            var playbackStopped = new UniTaskCompletionSource();

            void HandleStop(Script stoppedScript)
            {
                if (stoppedScript == null || stoppedScript.Path == scriptPath)
                {
                    playbackStopped.TrySetResult();
                }
            }

            scriptPlayer.OnStop += HandleStop;

            try
            {
                await scriptPlayer.LoadAndPlay(scriptPath);

                if (scriptPlayer.Playing)
                {
                    await playbackStopped.Task;
                }
            }
            finally
            {
                scriptPlayer.OnStop -= HandleStop;
                await EndVisualNovelAsync();
            }
        }

        public async UniTask EndVisualNovelAsync(AsyncToken asyncToken = default, bool resetState = true)
        {
            CheckDependencies();

            if (scriptPlayer.Playing)
            {
                scriptPlayer.Stop();
            }

            try
            {
                var hidePrinter = new HidePrinter();
                await hidePrinter.Execute(asyncToken);

                var clearBacklog = new ClearBacklog();
                await clearBacklog.Execute(asyncToken);

            }
            finally
            {
                ResetCameraState();
                ResetUIState();
                battleCamera = null;
            }

            if (resetState)
            {
                await stateManager.ResetState();
            }
        }

        private void CheckDependencies()
        {
            cameraManager ??= Engine.GetService<ICameraManager>();
            cameraConfiguration ??= Engine.GetConfiguration<CameraConfiguration>();
            stateManager ??= Engine.GetService<IStateManager>();
            uiManager ??= Engine.GetService<IUIManager>();
            scriptPlayer ??= Engine.GetService<IScriptPlayer>();
        }

        private void SetupUIState()
        {
            uiManager.GetUI<ContinueInputUI>()?.Show();
        }

        private void ResetUIState()
        {
            uiManager.GetUI<ContinueInputUI>()?.Hide();
        }

        private void SetupVisualNovelCamera()
        {
            SetEnableVisualNovelCamera(false);
            SetVisualNovelCameraToOverlay();
        }

        private void SetupCameraState()
        {
            SetEnableVisualNovelCamera(true);

            var mainCameraData = GetBattleCameraData();
            if (mainCameraData == null)
            {
                return;
            }

            var mainCameraStack = mainCameraData.cameraStack;
            RemoveInvalidStackCameras(mainCameraStack);

            if (cameraManager.Camera != null &&
                cameraManager.Camera != battleCamera &&
                !mainCameraStack.Contains(cameraManager.Camera))
            {
                mainCameraStack.Add(cameraManager.Camera);
            }

            if (cameraConfiguration.UseUICamera &&
                cameraManager.UICamera != null &&
                cameraManager.UICamera != battleCamera &&
                !mainCameraStack.Contains(cameraManager.UICamera))
            {
                mainCameraStack.Add(cameraManager.UICamera);
            }
        }

        private UniversalAdditionalCameraData GetBattleCameraData()
        {
            var baseCamera = battleCamera != null ? battleCamera : Camera.main;
            if (baseCamera == null)
            {
                Debug.LogError("Main Camera is null. Ensure a camera is tagged 'MainCamera' and active in the battle scene.");
                return null;
            }

            return CameraExtensions.GetUniversalAdditionalCameraData(baseCamera);
        }

        private void SetEnableVisualNovelCamera(bool isEnabled)
        {
            if (cameraManager.Camera != null)
            {
                cameraManager.Camera.enabled = isEnabled;
            }

            if (cameraConfiguration.UseUICamera && cameraManager.UICamera != null)
            {
                cameraManager.UICamera.enabled = isEnabled;
            }
        }

        private void SetVisualNovelCameraToOverlay()
        {
            if (cameraManager.Camera == null)
            {
                return;
            }

            var vnCameraData = CameraExtensions.GetUniversalAdditionalCameraData(cameraManager.Camera);
            vnCameraData.renderType = CameraRenderType.Overlay;

            if (cameraConfiguration.UseUICamera && cameraManager.UICamera != null)
            {
                var vnUICameraData = CameraExtensions.GetUniversalAdditionalCameraData(cameraManager.UICamera);
                vnUICameraData.renderType = CameraRenderType.Overlay;
            }
        }

        private void ResetCameraState()
        {
            SetEnableVisualNovelCamera(false);

            var mainCameraData = GetBattleCameraData();
            if (mainCameraData == null)
            {
                return;
            }

            var mainCameraStack = mainCameraData.cameraStack;
            RemoveInvalidStackCameras(mainCameraStack);

            if (cameraManager.Camera != null && mainCameraStack.Contains(cameraManager.Camera))
            {
                mainCameraStack.Remove(cameraManager.Camera);
            }

            if (cameraConfiguration.UseUICamera &&
                cameraManager.UICamera != null &&
                mainCameraStack.Contains(cameraManager.UICamera))
            {
                mainCameraStack.Remove(cameraManager.UICamera);
            }

            RemoveInvalidStackCameras(mainCameraStack);
        }

        private void RemoveInvalidStackCameras(System.Collections.Generic.List<Camera> cameraStack)
        {
            cameraStack.RemoveAll(camera => camera == null);
        }

        private void OnInitializeFinished()
        {
            CheckDependencies();
            SetupVisualNovelCamera();

            stateManager.ResetState().Forget();
        }
    }
}
