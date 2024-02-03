using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using CoreLobby;

namespace CoreGame
{
    [System.Serializable]
    public struct PrisonerNetwork : INetworkStruct
    {
        [Networked]
        public uint PrisonerId { get; set; }// Victim Id
        [Networked]
        public uint CatcherId { get; set; } // Player Catch Id
    }

    public class GameManager : NetworkBehaviour
    {
        #region Singleton
        private static GameManager instance = null;
        public static GameManager Instance
        {
            set => instance = value;
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                }
                return instance;
            }
        }
        #endregion

        [SerializeField]
        private GUIMatching GUIMatching = null;
        [SerializeField]
        private GameObject objHost = null;
        [SerializeField]
        private GUIMain mainGUI = null;
        [SerializeField]
        private GUIGameEndPreview endGameGUI = null;

        public SessionProps SessionProps = new SessionProps();
        public bool IsServer => Runner.IsSharedModeMasterClient || Runner.IsServer;

        #region ____Networked_______
        [Networked]
        public TickTimer TimeSynced { get; private set; }
        #endregion
        
        protected virtual void Awake()
        {
            if (FindObjectOfType<FusionLauncher>() == null)
            {
                SceneManager.LoadScene(0, LoadSceneMode.Single);
                return;
            }
        }
        public override void Spawned()
        {
            LoadingScreen.Instance.Hide();
            SpawnedAsync().Forget();
        }
        public override void FixedUpdateNetwork()
        {
            this.objHost?.SetActive(IsServer);

            if (TimeSynced.IsRunning)
            {
                this.mainGUI?.SetTimer(TimeSynced.RemainingTime(Runner).GetValueOrDefault());
                if (TimeSynced.Expired(Runner))
                {
                    if (IsServer)
                    {
                        TimeSynced = new TickTimer();
                    }
                }
            }
        }

        /// <summary>
        /// #All-Client
        /// </summary>
        protected async virtual UniTaskVoid SpawnedAsync()
        {
            await UniTask.WaitUntil(() => FusionLauncher.Session != null);

            FusionLauncher.Session.OnStartMatch += OnMatchStarted;
            FusionLauncher.Session.OnSceneLoaded().Forget();

            GUIMatching?.SetActive(FusionLauncher.Session.StateSyned == SessionState.MATCHING);
        }

        /// <summary>
        /// #All-Client
        /// </summary>
        protected virtual void OnMatchStarted()
        {
            GUIMatching?.SetActive(false);

            if (mainGUI != null)
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(1000);
                });

                mainGUI.OnMatchPrepareAsync().Forget();
            }
            if (IsServer)
            {
                float timeBattle = UserData.Local != null ? UserData.Local.TimeBattle : GameConfigs.Default.GameTime;
                float timer = timeBattle + (Constant.MILISECOND_DELAY_SEEK_START * 1.0f / 1000f);
                TimeSynced = TickTimer.CreateFromSeconds(Runner, timer);
            }
        }

        /// <summary>
        /// #All-Client
        /// </summary>
        public virtual void ShowPrevGameEnd()
        {
            endGameGUI?.Show();
        }
#if UNITY_EDITOR

        public virtual void OnValidate()
        {
            if (this.GUIMatching == null)
                this.GUIMatching = FindObjectOfType<GUIMatching>(true);
        }

#endif
    }

}
