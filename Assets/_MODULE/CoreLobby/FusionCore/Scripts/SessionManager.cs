using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using CoreLobby;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreGame
{
    public enum SessionState : byte
    {
        MATCHING = 0,
        PLAYING = 1,
        End = 2,
    }
    public interface IMapLoader
    {
        void SpawnAvatar(PlayerNetworked player, bool lateJoin);
        void DespawnAvatar(PlayerNetworked player, bool earlyLeave);
    }

    [System.Serializable]
    public struct PlayerRoomInfo : INetworkStruct
    {
        [Networked]
        public uint PlayerId { get; set; }
        //
    }
    public class SessionManager : NetworkBehaviour
    {
        [SerializeField]
        private SessionProps props = null;
        public SessionProps Props
        {
            get
            {
                if (props == null)
                    props = new SessionProps(Runner.SessionInfo.Properties);
                return props;
            }
            set => props = value;
        }


        #region NETWORKED
        [Networked(OnChanged = nameof(OnStateChanged))]
        public SessionState StateSyned { get; set; }
        [Networked]
        public TickTimer LimitTimeFindMatchSynced { get; set; }

        private static void OnStateChanged(Changed<SessionManager> changed)
        {
            changed.Behaviour.GameStateChanged();
        }
        #endregion ============

        private readonly Dictionary<uint, PlayerNetworked> dictPlayers = new Dictionary<uint, PlayerNetworked>();
        public ICollection<PlayerNetworked> AllPlayers => dictPlayers.Values;
        public bool IsServer { get => Runner.IsServer || Runner.IsSharedModeMasterClient; }
        public System.Action OnStartMatch { get; set; }
        [ShowInInspector]
        public uint MaxPlayer { get; private set; }
        public float FindMatchTime => LimitTimeFindMatchSynced.RemainingTime(Runner).GetValueOrDefault();
        public override void Spawned()
        {
            FusionLauncher.Session = this;
            MaxPlayer = GameConfigs.Default.MaxPlayer;
            if (IsServer)
            {
                float timeFindMatch = UserData.Local != null ? UserData.Local.TimeFindMatch : GameConfigs.Default.LimitTimeFindMatch;
                LimitTimeFindMatchSynced = TickTimer.CreateFromSeconds(Runner, timeFindMatch);
            }
        }

        public override void FixedUpdateNetwork()
        {
            float deltaTime = Runner.DeltaTime;
            if (IsServer)
            {
                if (LimitTimeFindMatchSynced.IsRunning)
                {
                    if (LimitTimeFindMatchSynced.Expired(Runner))
                    {
                        LimitTimeFindMatchSynced = new TickTimer();

                        //Todo: Custome follow game
                        if (dictPlayers.Count > 1)
                        {
                            StartMatch(true);
                        }
                        else
                        {
                            float timeFindMatch = UserData.Local != null ? UserData.Local.TimeFindMatch : GameConfigs.Default.LimitTimeFindMatch;
                            LimitTimeFindMatchSynced = TickTimer.CreateFromSeconds(Runner, timeFindMatch);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #All-Client
        /// </summary>
        private void GameStateChanged()
        {
            switch (this.StateSyned)
            {
                case SessionState.MATCHING:
                    break;
                case SessionState.PLAYING:
                    OnStartMatch?.Invoke();
                    OnStartMatch = null;

                    break;
            }
        }

        /// <summary>
        /// #Local
        /// </summary>
        private void OnGameEnd(bool isVictory)
        {
            PlayerNetworked.LocalPlayer.OnGameEnd();
            GameManager.Instance.ShowPrevGameEnd();
            EndMatchAsync().Forget();
        }

        /// <summary>
        /// #Server
        /// </summary>
        private void OnMatchStarted()
        {
            if (IsServer)
            {
            }
        }
        private bool HasLocalAvatar()
        {
            foreach (var item in this.AllPlayers)
            {
                if (item.Object.InputAuthority == Runner.LocalPlayer)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// #All-Client
        /// </summary>
        public async UniTaskVoid OnSceneLoaded()
        {
            if (StateSyned == SessionState.PLAYING)
            {
                int playerCount = Runner.ActivePlayers.Count();
                //todo: Waiting all player networked reconnected !
                await UniTask.WaitUntil(() => dictPlayers.Count == playerCount);
            }
            else if (StateSyned == SessionState.MATCHING)
            {
                if (!HasLocalAvatar())
                {
                    SpawnLocalPlayerAvatar();
                }
            }
        }
        public virtual void SpawnLocalPlayerAvatar()
        {
            if (Runner.Topology == SimulationConfig.Topologies.Shared)
            {
                Debug.Log($"{nameof(SessionManager)}: start to Spawn local player avatar");
                FusionLauncher.Instance.SpawnPlayerNetworked(Runner.LocalPlayer);
            }
        }
        /// <summary>
        /// #Server
        /// </summary>
        public virtual void StartMatch(bool isCheating = false)
        {
            this.ShowLog("F-Start Match!!!");
            LimitTimeFindMatchSynced = new TickTimer();
            if (dictPlayers.Count >= MaxPlayer || isCheating)
            {
                this.StateSyned = SessionState.PLAYING;

                OnMatchStarted();
                HideSession();
            }
        }
        /// <summary>
        /// #Server
        /// </summary>
        public async UniTaskVoid EndMatchAsync()
        {
            await UniTask.Delay(1000);

            if (IsServer)
                Runner.SetActiveScene(GameConfigs.Default.GetSceneReference(SceneType.GameOver).ScenePath);
        }

        public void OpenSession()
        {
            Runner.SessionInfo.IsOpen = true;
            Runner.SessionInfo.IsVisible = true;
        }
        public void HideSession()
        {
            Runner.SessionInfo.IsVisible = false;
        }

        //private void OnGUI()
        //{
        //    GUILayout.Space(40);
        //    GUILayout.Label($"State:{StateSyned}");
        //    GUILayout.Label($"PlayerParam:{ListPlayerParams.Count}");
        //}

        #region PLAYER MANAGER
        public void AddPlayer(PlayerNetworked player)
        {
            Debug.Log($"Added player: {player.Object.InputAuthority} - playerId:{player.PlayerId}");
            uint playerId = player.PlayerId;
            if (dictPlayers.ContainsKey(playerId))
            {
                Debug.LogError($"Available player:{playerId} in dic");
                return;
            }
            dictPlayers.Add(playerId, player);
            // Check matching...
            if (dictPlayers.Count >= MaxPlayer)
            {
                this.LimitTimeFindMatchSynced = new TickTimer();
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(500);
                    if (IsServer)
                        StartMatch();
                });
            }
        }
        public void RemovePlayer(PlayerNetworked player)
        {
            uint playerId = player.PlayerId;
            if (!dictPlayers.ContainsKey(playerId))
            {
                Debug.LogError($"Not available player:{playerId}");
                return;
            }
            dictPlayers.Remove(playerId);
        }
        public PlayerNetworked GetPlayer(uint playerId)
        {
            if (dictPlayers.ContainsKey(playerId))
                return dictPlayers[playerId];
            Debug.LogError($"Missing player staff:{playerId}");
            return null;
        }
        public bool HasPlayer(uint playerId)
        {
            return dictPlayers.ContainsKey(playerId);
        }
        
        #endregion

        public string GetMatchingProgress()
        {
            return string.Format("Players {0}/{1}", dictPlayers.Count, MaxPlayer);
        }

    }
}
