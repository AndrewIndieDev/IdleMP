using AndrewDowsett.CommonObservers;
using AndrewDowsett.IDisposables;
using AndrewDowsett.Networking;
using AndrewDowsett.ObjectPooling;
using AndrewDowsett.SceneLoading;
using AndrewDowsett.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndrewDowsett.SingleEntryPoint
{
    public class GameInitiator : MonoBehaviour
    {
        [Header("Initiator Settings")]
        public EProgressBarType loadingBarType;

        [Header("Scene Post Initiation")]
        public string SceneToLoad;

        [Header("Prefabs to Instantiate")]
        // All of these can be deleted if you don't need them.
        // You can add as many as you want to add here.
        // Remember to also add or remove them in the BindObjects method.
        public EntryScreen _entryScreen;
        public Camera _camera;
        public UpdateManager _updateManager;
        public FixedUpdateManager _fixedUpdateManager;
        public LateUpdateManager _lateUpdateManager;
        public SceneLoader _sceneLoader;
        public IntroAnimation _introAnimation;
        public ObjectPool[] _objectPools;
        public CustomNetworkManager _networkManager;
        public SteamManager _steamManager;
        public PlayerPartyManager _playerPartyManager;
        public PopupManager _popupManager;
        public DevMode _devMode;
        public GameManager _gameManager;
        public RPCManager _rpcManager;

        public GameObject[] _instantiateOnStart;

        private async void Start()
        {
            BindObjects();

            // We can use an IDisposable here to simplify showing and hiding the loading screen:
            using (var loadingSceneDisposable = new DisposableShowEntryScreen(_entryScreen, loadingBarType))
            {
                loadingSceneDisposable.SetLoadingBarPercent(0f);
                await InitializeSteamNetwork(loadingSceneDisposable, 0.3f);
                await InitializeObjects(loadingSceneDisposable, 0.2f);
                await CreateObjects(loadingSceneDisposable, 0.1f);
                await PrepareGame(loadingSceneDisposable, 0.2f);
                await LoadStartingScenes(loadingSceneDisposable, 0.1f);

                loadingSceneDisposable.SetLoadingText("Done...");
                loadingSceneDisposable.SetLoadingBarPercent(1.00f);
            }

            await BeginGame();
        }

        private void BindObjects()
        {
            // Bind all objects
            _entryScreen = Instantiate(_entryScreen);
            _camera = Instantiate(_camera);
            _updateManager = Instantiate(_updateManager);
            _fixedUpdateManager = Instantiate(_fixedUpdateManager);
            _lateUpdateManager = Instantiate(_lateUpdateManager);
            _sceneLoader = Instantiate(_sceneLoader);
            _introAnimation = Instantiate(_introAnimation);
            for (int i = 0; i < _objectPools.Length; i++)
            {
                _objectPools[i] = Instantiate(_objectPools[i]);
            }
            _steamManager = Instantiate(_steamManager);
            _playerPartyManager = Instantiate(_playerPartyManager);
            _popupManager = Instantiate(_popupManager);
            _devMode = Instantiate(_devMode);
            _gameManager = Instantiate(_gameManager);
            _rpcManager = Instantiate(_rpcManager);

            foreach (var obj in _instantiateOnStart)
            {
                Instantiate(obj);
            }
        }

        private async UniTask InitializeSteamNetwork(DisposableShowEntryScreen loadingSceneDisposable, float percentageToUse)
        {
            loadingSceneDisposable.SetLoadingText("Starting the Network Manager...");
            _networkManager = Instantiate(_networkManager);
            _networkManager.Init();
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse / 3);
            await UniTask.Delay(1000);

            loadingSceneDisposable.SetLoadingText("Checking Steam Status...");
            await _steamManager.Init();
            if (_steamManager.steamSucceeded)
            {
                loadingSceneDisposable.SetLoadingText("Steam connected successfully...");
            }
            else
            {
                loadingSceneDisposable.SetLoadingText("Steam failed to connect, continuing...");
            }
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse / 3);
            await UniTask.Delay(1000);

            loadingSceneDisposable.SetLoadingText("Setting up the Party Manager...");
            _playerPartyManager.Init();
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse / 3);
            await UniTask.Delay(500);
        }

        private async UniTask InitializeObjects(DisposableShowEntryScreen loadingSceneDisposable, float percentageToUse)
        {
            // Perform initialization for analytics/steam etc.
            loadingSceneDisposable.SetLoadingText("Initializing...");

            _sceneLoader.Initialize();
            for (int i = 0; i < _objectPools.Length; i++)
            {
                _objectPools[i].Initialize();
            }

            await UniTask.Delay(500);
            loadingSceneDisposable.SetLoadingText("Finished Initializing...");
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse);
            await UniTask.Delay(500);
        }

        private async UniTask CreateObjects(DisposableShowEntryScreen loadingSceneDisposable, float percentageToUse)
        {
            // Instantiate all objects into the scene
            float initialPercent = loadingSceneDisposable.GetLoadingBarPercent();
            float percentPerPool = percentageToUse / _objectPools.Length;
            for (int i = 0; i < _objectPools.Length; i++)
            {
                await _objectPools[i].SpawnDefaultPool(loadingSceneDisposable, percentPerPool);
            }
            loadingSceneDisposable.SetLoadingBarPercent(initialPercent + percentageToUse);
            loadingSceneDisposable.SetLoadingText("Finished Pooling Objects...");

            await UniTask.Delay(500);
        }

        private async UniTask PrepareGame(DisposableShowEntryScreen loadingSceneDisposable, float percentageToUse)
        {
            // Prepare objects in the scene, if they need methods called before the game starts
            loadingSceneDisposable.SetLoadingText("Preparing Game...");
            await UniTask.Delay(500);
            loadingSceneDisposable.SetLoadingText("Finished Preparing Game...");
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse);
            await UniTask.Delay(500);
        }

        private async UniTask LoadStartingScenes(DisposableShowEntryScreen loadingSceneDisposable, float percentageToUse)
        {
            loadingSceneDisposable.SetLoadingText("Loading Scene...");
            // Load the main menu scene
            _sceneLoader.LoadScene(SceneToLoad);
            // Wait for the scenes to finish loading
            await UniTask.WaitUntil(() => !_sceneLoader.CurrentlyLoadingScene);
            loadingSceneDisposable.SetLoadingText("Finished Loading Scene...");
            loadingSceneDisposable.SetLoadingBarPercent(loadingSceneDisposable.GetLoadingBarPercent() + percentageToUse);
            
            await UniTask.Delay(500);
        }

        private async UniTask BeginGame()
        {
            // Wait for the intro animation
            await _introAnimation.Play();

            // Unload the entry scene
            _sceneLoader.UnloadScene(gameObject.scene.name);
        }
    }
}