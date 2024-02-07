using System.Collections.Generic;
using System.Threading;
using CoreGame;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace CoreLobby
{
    public enum StateOfPlayer : byte
    {
        InMatching = 0,
        Playing,
        //More...
        Finished,
    }

    public class PlayerNetworked : NetworkBehaviour, INetworkRunnerCallbacks
    {
        public static PlayerNetworked LocalPlayer = null;
        protected CancellationTokenSource ctsDespawned = new CancellationTokenSource();
        public uint PlayerId => Object.Id.Raw;
        public bool IsMine => Object.HasInputAuthority;
        public bool IsMineNotBot => !IsBotSynced && IsMine;

        #region Networked

        [Capacity(50)]
        [Networked]
        public NetworkString<_16> DisplayNameSynced { get; set; }
        [Networked]
        public NetworkBool IsBotSynced { get; set; }
        [Networked(OnChanged = nameof(OnStateSyncedChanged))]
        public StateOfPlayer StateSynced { get; protected set; }

        protected static void OnStateSyncedChanged(Changed<PlayerNetworked> changed)
        {
            changed.Behaviour.HandleStateChanged();
        }
        #endregion

        /// <summary>
        /// #Local
        /// </summary>
        /// 
        protected virtual void HandleStateChanged(){}
        protected virtual void OnMatchStarted(){}
        protected virtual Vector3 GetRandomPoint(Vector3 center, float maxDistance)
        {
            return new Vector3(Random.value * 5, 0, Random.value * 5);
        }
        /// <summary>
        /// #Local
        /// </summary>
        public virtual void OnBeforeSpawned(string displayName, bool isBot)
        {
            this.DisplayNameSynced = displayName;
            this.IsBotSynced = isBot;
            this.StateSynced = StateOfPlayer.InMatching;
        }
        /// <summary>
        /// #Local
        /// </summary>
        public override void Spawned()
        {
            if (IsMineNotBot)
            {
                LocalPlayer = this;
            }
            SpawnedAsync().Forget();
        }
        /// <summary>
        /// #Local
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (StateSynced == StateOfPlayer.Playing)
            {
                //Todo: something game logic
            }
        }
        /// <summary>
        /// #Local
        /// </summary>
        protected async virtual UniTaskVoid SpawnedAsync()
        {
            await UniTask.WaitUntil(() => FusionLauncher.Session != null, cancellationToken: ctsDespawned.Token);
            FusionLauncher.Session.AddPlayer(this);
            FusionLauncher.Session.OnStartMatch += OnMatchStarted;

            if (IsMineNotBot)
            {
                CameraManager.Default.SetTarget(this.transform, true);
            }
        }
        /// <summary>
        /// #Local
        /// </summary>
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            CancelInvoke();
        }

        /// <summary>
        /// #Local
        /// </summary>
        public virtual void OnGameEnd()
        {
            if (StateSynced == StateOfPlayer.Playing)
                StateSynced = StateOfPlayer.Finished;
        }
        /// <summary>
        /// #Local
        /// </summary>
        public virtual bool CanMove()
        {
            if (StateSynced == StateOfPlayer.Playing || StateSynced == StateOfPlayer.InMatching)
            {
                // Check more skill, bufff....
                return true;
            }
            return false;
        }

        protected virtual void OnValidate(){}
        public virtual void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public virtual void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public virtual void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        
        public virtual void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public virtual void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public virtual void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public virtual void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public virtual void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public virtual void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public virtual void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public virtual void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public virtual void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public virtual void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public virtual void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data)
        {
        }

        public virtual void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public virtual void OnSceneLoadStart(NetworkRunner runner)
        {
        }

    }
}