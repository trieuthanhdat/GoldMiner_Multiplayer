using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using CoreLobby;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

namespace CoreGame
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Failed,
        EnteringLobby,
        InLobby,
        Starting,
        Started,
        EnteringGame,
        InGame,
    }

    [RequireComponent(typeof(SceneLoader))]
    public class FusionLauncher : MonoSingleton<FusionLauncher>, INetworkRunnerCallbacks
    {
        [SerializeField] bool PlayMiniGameGoldMiner = true;

        public const string PREF_SESSION_NAME = "PREF_SESSION_NAME";
        public static SessionManager Session = null;

        private NetworkRunner runner;
        private NetworkSceneManagerDefault loader;
        private Action<List<SessionInfo>> onSessionListUpdated;

        [Space(5)]
        public SessionManager SessionPf;
        public SerializableDictionary<NetworkCharacterType, NetworkCharacterConfig> networkCharacterConfigTable = new SerializableDictionary<NetworkCharacterType, NetworkCharacterConfig>();
        [Space(5)]
        [ReadOnly][SerializeField] GameConfigs GameConfigs = null;
        [Header("[Dev]Lobby")]
        [SerializeField] FusionLobby lobbyManager = null;
        [Header("[Common]UI-Matching")]

        [SerializeField] UIMatching UIMatching = null;
        [Space(10)]
        [SerializeField] ErrorBox disconnectBox = null;
        //[Space(5)]
        //[SerializeField] GameObject loadingScreenObj = null;
        [Space(10)]
        [SerializeField] bool joinLobbyWhenAwake = true;
        [SerializeField] bool autoStartMatchWhenFull = true;
        [SerializeField] bool autoReconnnect = true;

        //public InputNetworkEvent<NetworkRunner, NetworkInput> OnInputEvent = new InputNetworkEvent<NetworkRunner, NetworkInput>();

        public ConnectionStatus ConnectionStatus { get; private set; }
        public bool IsServer => Runner.IsSinglePlayer || (Runner.IsServer || Runner.IsSharedModeMasterClient);

        public NetworkRunner Runner { get => runner; }
        
        public UnityButton OnSessionStarted = new UnityButton();
        public UnityButton OnJoinedLobby = new UnityButton();
        public Action<List<SessionInfo>> OnUpdateSessionList { get => onSessionListUpdated; set => onSessionListUpdated = value; }
        public string SessionConnect { get => PlayerPrefs.GetString(PREF_SESSION_NAME, string.Empty); set => PlayerPrefs.SetString(PREF_SESSION_NAME, value); }

        private bool _isSessionForMiniGame = false;
        public SerializableDictionary<NetworkCharacterType, List<Vector3>> _spawnedLocationTable = new SerializableDictionary<NetworkCharacterType, List<Vector3>>(); 
        protected override void OnInitiate()
        {
            DontDestroyOnLoad(this.gameObject);
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            InitTableData();
            this.GameConfigs?.OnEnable();

            if (loader == null)
            {
                loader = GetComponent<NetworkSceneManagerDefault>();
            }

            this.UIMatching?.IsVisible(false);

            if (joinLobbyWhenAwake)
                FusionInitiate();
        }

        private void InitTableData()
        {
            if (networkCharacterConfigTable == null) return;

            foreach (var characterConfig in networkCharacterConfigTable)
            {
                var characterKey = characterConfig.Key;
                var spawnLocations = characterConfig.Value.spawnLocations.spawnLocationList;

                if (_spawnedLocationTable.ContainsKey(characterKey))
                {
                    _spawnedLocationTable[characterKey] = spawnLocations;
                }
                else
                {
                    _spawnedLocationTable.Add(characterKey, spawnLocations);
                }
            }
        }


        async void FusionInitiate()
        {
            string lobbyId = GameConfigs.LobbyId;
            lobbyManager?.Show(lobbyId);
            await EnterLobby(lobbyId);
            if (autoReconnnect)
                Reconnect().Forget();
        }

#if UNITY_EDITOR
        //GUIStyle style = new GUIStyle();
        private void OnGUI()
        {
            //style = new GUIStyle();
            //style.fontSize = 10;
            //style.normal.textColor = Color.cyan;
        }
        private void OnValidate()
        {
            if (GameConfigs == null)
                GameConfigs = Resources.Load<GameConfigs>("GameConfigs");
        }
#endif

        //private void OnApplicationQuit()
        //{
        //    PlayerPrefs.DeleteKey(PREF_SESSION_NAME);
        //}

        public virtual void LeaveMatch()
        {
            DoDisconnect(ShutdownReason.Ok);
        }
        //[Button]
        //public void OnClick_CancelMatching()
        //{
        //    DoDisconnect(ShutdownReason.GameClosed);
        //    GameConfigs.Instance.LoadScene(SceneType.Lobby);
        //}
        public virtual void JoinRandomRoom()
        {
            StartSession(new SessionProps
            {
                LobbyId = GameConfigs.LobbyId,
                SessionName = string.Empty,
            }, GameConfigs.PlayingMode != GameMode.Shared, PlayMiniGameGoldMiner);
        }
        public virtual void Connect()
        {
            if (Runner == null)
            {
                SetConnectionStatus(ConnectionStatus.Connecting);
                GameObject go = new GameObject("Runner-Clone");
                go.transform.SetParent(transform);

                runner = go.AddComponent<NetworkRunner>();
                Runner.AddCallbacks(this);
            }
        }
        protected virtual void DoDisconnect(ShutdownReason reason = ShutdownReason.Ok)
        {
            if (Runner != null)
            {
                SetConnectionStatus(ConnectionStatus.Disconnected);
                Debug.LogError($"Do-Shutdown Server with reason :{reason}");
                Runner.Shutdown(true, reason);
            }
        }
        public async virtual Task EnterLobby(string lobbyId)
        {
            Debug.Log($"Enter Lobby :{lobbyId}");

            Connect();
            SetConnectionStatus(ConnectionStatus.EnteringLobby);

            var result = await Runner.JoinSessionLobby(SessionLobby.Custom, lobbyId);
            if (!result.Ok)
            {
                this.OnUpdateSessionList = null;
                Debug.Log("Joined-Lobby-Failed");
                SetConnectionStatus(ConnectionStatus.Failed);
            }
            else
            {
                this.ShowLog("Joined-Lobby-Success");
                OnJoinedLobby?.Invoke();
            }
        }
        public virtual void JoinSession(SessionInfo info)
        {
            try
            {
                StartSession(new SessionProps(info.Properties));
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
        public virtual void CreateSession(SessionProps props)
        {
            this.ShowLog($"Create Session:{props.SessionName}");
            StartSession(props, GameConfigs.PlayingMode != GameMode.Shared, PlayMiniGameGoldMiner);
        }
        /// <summary>
        /// Start Session!!!
        /// </summary>
       protected async virtual void StartSession(SessionProps props, bool disableClientSessionCreation = false, bool isSessionForMiniGame = false)
        {
            Connect();
            SetConnectionStatus(ConnectionStatus.Starting);

            GameMode mode = GameConfigs.PlayingMode;
            Runner.ProvideInput = (mode != GameMode.Server);
            _isSessionForMiniGame = isSessionForMiniGame;
            LoadingScreen.Instance.Show("Connecting");

            
            var sceneIndex = !_isSessionForMiniGame ? GameConfigs.GetSceneReference(SceneType.GameScene).SceneIndex :
                                                      GameConfigs.GetMiniGameSceneReference(SceneType.MiniGameScene, 
                                                                                NetworkCharacterType.GOLD_MINER).SceneIndex;
            Debug.Log($"{nameof(FusionLauncher)}: scene index {sceneIndex}");
            //IsVisibleLoadingScr(true);
            await Runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SceneManager = loader,

                PlayerCount = (int)this.GameConfigs.MaxPlayer,
                SessionName = props.SessionName, // == Null If Random Session

                IsOpen = true,
                IsVisible = true,

                CustomLobbyName = props.LobbyId,

                SessionProperties = props.Properties,
                DisableClientSessionCreation = disableClientSessionCreation,
                ObjectPool = FindObjectOfType<FusionPrebabPools>(),
                //ObjectProvider = FindObjectOfType<NetworkObjectProviderDefault>(), //*Fusion 2
                //SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = sceneIndex
            });

            Debug.Log("Runner.StartGame succss!!");
            //IsVisibleLoadingScr(false);
            lobbyManager?.gameObject.SetActive(false);
            OnSessionStarted?.Invoke();

            this.SessionConnect = props.SessionName;
        }

        public virtual async UniTaskVoid Reconnect()
        {
            if (string.IsNullOrEmpty(SessionConnect))
            {
                this.LogError("Oop, can't reconnect session is null");
                return;
            }
            Connect();
            this.ShowLog($"*****Fusion-Reconnect with session name: {SessionConnect}");
            GameMode mode = this.GameConfigs.PlayingMode;
            Runner.ProvideInput = mode != GameMode.Server;
            string lobbyId = this.GameConfigs.LobbyId;

            StartGameResult result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SceneManager = loader,
                SessionName = SessionConnect,
                CustomLobbyName = lobbyId,
                ObjectPool = FindObjectOfType<FusionPrebabPools>(),
                Scene = !_isSessionForMiniGame ? GameConfigs.GetSceneReference(SceneType.GameScene).SceneIndex :
                                                 GameConfigs.GetMiniGameSceneReference(SceneType.MiniGameScene,
                                                                               NetworkCharacterType.GOLD_MINER).SceneIndex,
                DisableClientSessionCreation = true
            });
            
            if (!result.Ok)
            {
                this.LogError("Reconnect result:" + result.ShutdownReason);
                // error handle
                if (result.ShutdownReason == ShutdownReason.GameNotFound)
                {
                    //disconnectBox?.Show(ConnectionStatus.Disconnected, "Oop,The battle is closed !!!", delegate
                    //{
                    //    //GameConfigs.LoadScene(SceneType.Lobby);
                    //});
                    SessionConnect = string.Empty;
                    GameConfigs.LoadScene(SceneType.Lobby);
                }
                else
                {
                    await UniTask.DelayFrame(1);
                    Reconnect().Forget();
                }
            }
        }
        private void UpdateRoomInfo()
        {
            if (Runner != null && Session != null)
            {
                //this.txtPlayerMatchingCount?.SetText($"{this.players.Count}/{this.GameConfigs.MaxPlayer}");
                //if (this.players.Count >= this.GameConfigs.MaxPlayer && AutoStartMatchWhenFull)
                //{
                //    StartMatch();
                //    CancelInvoke(nameof(UpdateRoomInfo));
                //}

                //if (buttonPlayNow != null)
                //{
                //    buttonPlayNow.gameObject.SetActive(IsServer);
                //}
            }
        }

        //#Go-to-game 
        public virtual void StartSession()
        {
            if (IsServer)
            {
                Session.HideSession();
            }

            this.CancelInvoke();
        }

        public virtual void DestroySession()
        {
            if (IsServer)
            {
                //Runner.SetActiveScene(GameConfigs.GetSceneReference(SceneType.GameOver).ScenePath);
                Runner.Shutdown(true, ShutdownReason.GameClosed, true);
            }
            //Destroy(this.gameObject);
        }
        //#Map is loaded
        public virtual void OnMapLoaded() { }

        public virtual void SpawnPlayerNetworked(PlayerRef playerRef)
        {
            if (runner != null)
            {
                Debug.Log($"{typeof(FusionLauncher)}: Is session for mini Game {_isSessionForMiniGame}");
                var charConfig    = _isSessionForMiniGame ? networkCharacterConfigTable[NetworkCharacterType.GOLD_MINER] :
                                                            networkCharacterConfigTable[NetworkCharacterType.MAIN_GAME ] ;

                var spawnPosition = _isSessionForMiniGame ? charConfig.useAvailableSpawnLocation ? GetRandomPositionFromConfig(NetworkCharacterType.GOLD_MINER) :
                                                            new Vector3(UnityEngine.Random.value * 5, 0, -Constant.ROOM_POS_Z) :
                                                            new Vector3(UnityEngine.Random.value * 5, 0, -Constant.ROOM_POS_Z) ;
                var spawnRotation = _isSessionForMiniGame ? Quaternion.identity :
                                                            Quaternion.Euler(0, UnityEngine.Random.value * 360f, 0);
                Debug.Log($"{nameof(FusionLauncher)}: charConfig {charConfig.networkCharacterType} spawn location: {spawnPosition} rotaion: {spawnRotation}");
                runner.Spawn(charConfig.playerNetworkPf, 
                             spawnPosition, 
                             spawnRotation, 
                             playerRef, 
                             ((runner, obj) =>
                             {
                                string displayName = UserData.Local.DisplayName;
                                PlayerNetworked player = obj.GetComponent<PlayerNetworked>();
                                player.OnBeforeSpawned(displayName, false);
                             }));
            }
            else
            {
                Debug.LogError("*Important: SpawnPlayerNetworked is Error, MISSING RUNNER!!");
            }
        }
        //NOT YET DONE
        private Vector3 GetRandomPositionFromConfig(NetworkCharacterType networkCharacterType)
        {
            if (networkCharacterConfigTable == null || !_spawnedLocationTable.ContainsKey(networkCharacterType))
            {
                return Vector3.zero;
            }
            List<Vector3> positionList = _spawnedLocationTable[networkCharacterType];

            if (positionList.Count == 0)
            {
                // No positions left for the specified character type.
                return Vector3.zero;
            }

            int randomIndex = UnityEngine.Random.Range(0, positionList.Count);
            Vector3 randomPosition = positionList[randomIndex];

            // Remove the selected position from the list.
            positionList.RemoveAt(randomIndex);

            return randomPosition;
        }


        protected virtual void SetConnectionStatus(ConnectionStatus status, string reason = "")
        {
            if (ConnectionStatus == status)
                return;
            ConnectionStatus = status;

            Debug.Log($"[FUSION]ConnectionStatus={status} {reason}<color>");
        }
        public void Jump()
        {
            InputData.ButtonFlags |= ButtonFlag.MOUSE_DOWN;/*true ? ButtonFlag.MOUSE_DOWN : 0;*/
        }
        public virtual void ExecuteInput(ButtonFlag buttonFlag)
        {
            InputData.ButtonFlags |= buttonFlag;
        }
        #region FUSION-OVERRIDE
        public virtual void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("<color=cyan>Connected to server</color>");
            SetConnectionStatus(ConnectionStatus.Connected);
        }

        public virtual void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.LogError("Disconnected from server");
            //DoDisconnect();
        }

        public virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log($"Connect failed {reason}");
            //DoDisconnect();
            SetConnectionStatus(ConnectionStatus.Failed, reason.ToString());
        }

        public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            this.ShowLog($"Player {playerRef} had Joined!");
            if (Session == null && IsServer)
            {
                Session = runner.Spawn(this.SessionPf, Vector3.zero, Quaternion.identity, Runner.LocalPlayer);
                Session.transform.SetParent(this.transform);
            }

            //if (runner.IsServer || runner.Topology == SimulationConfig.Topologies.Shared && playerRef == runner.LocalPlayer)
            //{
            //    if (playerRef == runner.LocalPlayer)
            //    {
            //        SpawnPlayerNetworked(playerRef);
            //    }
            //    else
            //    {
            //        Debug.LogError($"Local-Player:{runner.LocalPlayer} # playerSpawn:{playerRef}");
            //    }
            //}

            SetConnectionStatus(ConnectionStatus.Started);
        }

        public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Player {playerRef} Left!");
            //DespawnPlayer(playerRef);
        }

        public virtual void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            Debug.LogError($"Shutdown with reason **{reason}**");
            SetConnectionStatus(ConnectionStatus.Disconnected, reason.ToString());

            this.runner = null;
            Session = null;

            if (reason == ShutdownReason.Ok)
            {
                GameConfigs.LoadScene(SceneType.Lobby);
            }
            //if (reason == ShutdownReason.GameClosed)
            //{
            //    Destroy(this.gameObject);
            //}

            Destroy(this.gameObject);
        }

        public virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            request.Accept();
        }

        public InputData InputData = new InputData();
        public virtual void SetInputDirection(Vector3 direction3D)
        {
            if (Constant.IS_MOBILE_BUILD)
            {
                InputData.Direction = direction3D;
            }
        }
        public virtual void OnInput(NetworkRunner runner, NetworkInput input)
        {
            //inputData.ButtonFlags |= Input.GetKey(KeyCode.W) ? ButtonFlag.FORWARD : 0;
            //inputData.ButtonFlags |= Input.GetKey(KeyCode.A) ? ButtonFlag.LEFT : 0;
            //inputData.ButtonFlags |= Input.GetKey(KeyCode.S) ? ButtonFlag.BACKWARD : 0;
            //inputData.ButtonFlags |= Input.GetKey(KeyCode.D) ? ButtonFlag.RIGHT : 0;

            //_data.ButtonFlags |= Input.GetMouseButtonDown(0) ? ButtonFlag.MOUSE_DOWN : 0;
            //_data.ButtonFlags |= Input.GetMouseButtonUp(0) ? ButtonFlag.MOUSE_UP : 0;

            if (!Constant.IS_MOBILE_BUILD)
            {
                Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                InputData.Direction = direction.normalized;
            }

            input.Set(InputData);

            //// Clear the flags so they don't spill over into the next tick unless they're still valid input.
            InputData.ButtonFlags = 0;
        }

        public virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            SetConnectionStatus(ConnectionStatus.InLobby);
            OnUpdateSessionList?.Invoke(sessionList);
            Debug.Log($"OnSessionListUpdated: <color=cyan>{sessionList.Count}</color>");
        }
        public virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public virtual void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public virtual void OnSceneLoadDone(NetworkRunner runner)
        {
            if (ConnectionStatus == ConnectionStatus.Started ||
                ConnectionStatus == ConnectionStatus.EnteringGame
            )
            {
                //SetConnectionStatus(ConnectionStatus.InGame);
                //objUIMatching?.IsVisible(false);
                //lobbyManager.Hide();
            }
        }
        public virtual void OnSceneLoadStart(NetworkRunner runner)
        {
            if (ConnectionStatus == ConnectionStatus.Starting ||
                ConnectionStatus == ConnectionStatus.Started    )
            {
                //SetConnectionStatus(ConnectionStatus.EnteringGame);
                //objUIMatching?.IsVisible(false);
            }
        }
        #endregion
    }
}
