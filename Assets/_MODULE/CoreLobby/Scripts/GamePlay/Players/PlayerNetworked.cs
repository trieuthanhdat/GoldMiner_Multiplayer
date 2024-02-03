using System.Threading;
using CoreGame;
using Cysharp.Threading.Tasks;
using Fusion;
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

    public class PlayerNetworked : NetworkBehaviour
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

#if UNITY_EDITOR
        protected virtual void OnValidate(){}
#endif

    }
}