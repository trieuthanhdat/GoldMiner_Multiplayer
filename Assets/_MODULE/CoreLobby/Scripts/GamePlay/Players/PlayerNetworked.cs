using System.Threading;
using CoreGame;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace CoreLobby
{
    public enum StateOfPlayer : byte
    {
        InMatching = 0,
        Playing,
        //More...
        Finished,
    }

    public sealed class PlayerNetworked : NetworkBehaviour
    {
        public static PlayerNetworked LocalPlayer = null;

        [SerializeField, ReadOnly]
        private PlayerGUI playerGUI = null;
        [SerializeField]
        private Moverment3D moverment = null;

        private CancellationTokenSource ctsDespawned = new CancellationTokenSource();

        public uint PlayerId => Object.Id.Raw;
        public bool IsMine => Object.HasInputAuthority;
        public bool IsMineNotBot => !IsBotSynced && IsMine;

        #region Networked
        [Networked(OnChanged = nameof(OnHealthSyncedChanged))]
        public int HealthSynced { get; set; } = 0;

        [Capacity(50)]
        [Networked]
        public NetworkString<_16> DisplayNameSynced { get; set; }
        [Networked]
        public NetworkBool IsBotSynced { get; set; }
        [Networked(OnChanged = nameof(OnStateSyncedChanged))]
        public StateOfPlayer StateSynced { get; private set; }

        private static void OnStateSyncedChanged(Changed<PlayerNetworked> changed)
        {
            changed.Behaviour.HandleStateChanged();
        }
        private static void OnHealthSyncedChanged(Changed<PlayerNetworked> changed)
        {
            changed.Behaviour.HandleHealthChanged();
        }
        #endregion

        /// <summary>
        /// #Local
        /// </summary>
        private void OnMatchStarted()
        {
            if (IsMineNotBot)
            {
                Vector3 startMatchWithPosition = GetRandomPoint(Vector3.zero, 8);
                startMatchWithPosition.y = 0;
                moverment?.TeleportTo(startMatchWithPosition);
            }
        }
        private Vector3 GetRandomPoint(Vector3 center, float maxDistance)
        {
            return new Vector3(Random.value * 5, 0, Random.value * 5);
            // Get Random Point inside Sphere which position is center, radius is maxDistance
            Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;
            NavMeshHit hit; // NavMesh Sampling Info Container
            NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);
            return hit.position;
        }

        /// <summary>
        /// #Local
        /// </summary>
        private void HandleStateChanged()
        {

        }
        private void HandleHealthChanged()
        {
            playerGUI?.SetHp(this.HealthSynced);
        }
        /// <summary>
        /// #Local
        /// </summary>
        public void OnBeforeSpawned(string displayName, bool isBot)
        {
            this.DisplayNameSynced = displayName;
            this.IsBotSynced = isBot;
            this.StateSynced = StateOfPlayer.InMatching;
            this.HealthSynced = 1000;
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
            PlayerGUI playerGUIPf = Resources.Load<PlayerGUI>("PlayerGUIPf");

            playerGUI = Instantiate<PlayerGUI>(playerGUIPf, transform.position, Quaternion.identity);
            playerGUI?.SetPlayer(this);

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
        private async UniTaskVoid SpawnedAsync()
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
        public void OnGameEnd()
        {
            if (StateSynced == StateOfPlayer.Playing)
                StateSynced = StateOfPlayer.Finished;
        }
        /// <summary>
        /// #Local
        /// </summary>
        public bool CanMove()
        {
            if (StateSynced == StateOfPlayer.Playing || StateSynced == StateOfPlayer.InMatching)
            {
                // Check more skill, bufff....
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (moverment == null)
                moverment = GetComponent<Moverment3D>();
        }
#endif

    }
}