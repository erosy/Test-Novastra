using Gamepangin;
using UnityEngine;
using Naninovel;
using Naninovel.UI;
using Naninovel.Commands;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace NovastraTest
{
    public class VNHandler : Singleton<VNHandler>, IEventListener<OnChangeBattleState>
    {
        private ICameraManager _cameraManager;
        private IStateManager _stateManager;
        private IUIManager _uiManager;
        private ICustomVariableManager _customVariableManager;
        private IScriptPlayer _scriptPlayer;
        private IScriptManager _scriptManager;
        private CameraConfiguration _cameraConfiguration;
        private IScriptLoader _scriptLoader;

        [SerializeField] private Script startingScript;
        [SerializeField] private Camera vnCamera;

        void OnEnable()
        {
            this.EventStartListening<OnChangeBattleState>();
        }

        void ODisable()
        {
            this.EventStopListening<OnChangeBattleState>();
        }

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

        //TO DO:: only activate VN when enter new state (VN Pause)

        private void StartVisualNovel(Script vnScript)
        {
            StartVisualNovelAsync(vnScript).Forget();
        }

        private async UniTaskVoid StartVisualNovelAsync(Script requestedScript)
        {
            if (requestedScript == null) return;

            if (!Engine.Initialized)
            {
                await RuntimeInitializer.Initialize();
            }

            CheckDependencies();
            SetupCameraState();
            SetupUIState();

            await _scriptPlayer.LoadAndPlay(requestedScript.Path);
        }

        public async UniTask EndVisualNovelAsync(AsyncToken asyncToken = default, bool resetState = true, bool additive = true)
        {
            CheckDependencies();

            _scriptPlayer.Stop();

            // hide visual novel dialogue printer
            var hidePrinter = new HidePrinter();
            hidePrinter.Execute(asyncToken).Forget();

            // clear visual novel backlog
            var clearBacklog = new ClearBacklog();
            clearBacklog.Execute(asyncToken).Forget();

            // resets engine services and unloads unused assets
            if (resetState)
            {
                await _stateManager.ResetState();
                ResetCameraState();
                ResetUIState();
            }

            if (BattleManager.Instance.CurrentState == BattleState.VisualNovelPause)
            {
                BattleManager.Instance.SetState(BattleState.TurnStart);
            }
        }

        private void CheckDependencies()
        {
            if (_cameraManager == null)
            {
                _cameraManager = Engine.GetService<ICameraManager>();
            }

            if (_cameraConfiguration == null)
            {
                _cameraConfiguration = Engine.GetConfiguration<CameraConfiguration>();
            }

            if (_stateManager == null)
            {
                _stateManager = Engine.GetService<IStateManager>();
            }

            if (_uiManager == null)
            {
                _uiManager = Engine.GetService<IUIManager>();
            }

            if (_customVariableManager == null)
            {
                _customVariableManager = Engine.GetService<ICustomVariableManager>();
            }

            if (_scriptPlayer == null)
            {
                _scriptPlayer = Engine.GetService<IScriptPlayer>();
            }

            if (_scriptLoader == null)
            {
                _scriptLoader = Engine.GetService<IScriptLoader>();
            }
        }

        #region  UI Settings

        private void SetupUIState()
        {
            _uiManager.GetUI<ContinueInputUI>()?.Show();
        }


        private void ResetUIState()
        {
            _uiManager.GetUI<ContinueInputUI>()?.Hide();
        }

        #endregion

        #region  VN Camera Settings

        private void SetupVisualNovelCamera()
        {
            SetEnableVisualNovelCamera(false);
            SetVisualNovelCameraToOverlay();
        }

        private void SetupCameraState()
        {
            SetEnableVisualNovelCamera(true);

            var mainCameraStack = GetMainCameraData().cameraStack;

            if (!mainCameraStack.Contains(_cameraManager.Camera))
            {
                mainCameraStack.Add(_cameraManager.Camera);
            }

            if (_cameraConfiguration.UseUICamera && !mainCameraStack.Contains(_cameraManager.UICamera))
            {
                mainCameraStack.Add(_cameraManager.UICamera);
            }
        }

        private UniversalAdditionalCameraData GetMainCameraData()
        {
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Main Camera is null. Ensure a camera is tagged 'MainCamera' and active in the scene.");
                return null;
            }

            return CameraExtensions.GetUniversalAdditionalCameraData(mainCam);
        }

        private void SetEnableVisualNovelCamera(bool isEnabled)
        {
            if (_cameraManager.Camera != null)
            {
                _cameraManager.Camera.enabled = isEnabled;
            }

            if (_cameraConfiguration.UseUICamera && _cameraManager.UICamera != null)
            {
                _cameraManager.UICamera.enabled = isEnabled;
            }
        }

        private void SetVisualNovelCameraToOverlay()
        {
            var vnCameraData = CameraExtensions.GetUniversalAdditionalCameraData(_cameraManager.Camera);
            vnCameraData.renderType = CameraRenderType.Overlay;

            if (_cameraConfiguration.UseUICamera)
            {
                var vnUICameraData = CameraExtensions.GetUniversalAdditionalCameraData(_cameraManager.UICamera);
                vnUICameraData.renderType = CameraRenderType.Overlay;
            }
        }

        private void ResetCameraState()
        {
            SetEnableVisualNovelCamera(false);

            var mainCameraStack = GetMainCameraData().cameraStack;
            if (mainCameraStack.Contains(_cameraManager.Camera))
            {
                mainCameraStack.Remove(_cameraManager.Camera);
            }

            if (_cameraConfiguration.UseUICamera && mainCameraStack.Contains(_cameraManager.UICamera))
            {
                mainCameraStack.Remove(_cameraManager.UICamera);
            }
        }

        #endregion

        private void OnInitializeFinished()
        {
            CheckDependencies();
            SetupVisualNovelCamera();

            _stateManager?.ResetState();
        }

        public void OnEvent(OnChangeBattleState e)
        {
            if (e.battleState == BattleState.VisualNovelPause)
            {
                startingScript = BattleManager.Instance.BattleStartingScript;
                StartVisualNovel(startingScript);
            }
        }
    }

}
