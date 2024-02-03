using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.SceneManagement;
using CoreLobby;

namespace CoreGame
{
    public class MapLoader : NetworkBehaviour, IMapLoader
    {
        #region Singleton
        private static MapLoader instance = null;
        public static MapLoader Instance
        {
            set => instance = value;
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MapLoader>();
                    instance?.OnInitiate();
                }
                return instance;
            }
        }
        #endregion

        [SerializeField] GameObject hostObj = null;
        public bool IsServer => Runner.IsServer || Runner.IsSharedModeMasterClient;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                OnInitiate();
            }
        }

        public virtual void OnInitiate() { }

        public override void Spawned()
        {
            FusionLauncher.Instance.OnMapLoaded();
            //Invoke(nameof(SpawnMineGameScene), 0.2f);
        }
        public override void FixedUpdateNetwork()
        {
            this.hostObj?.SetVisible(IsServer);
        }

        //void SpawnMineGameScene()
        //{
        //    foreach (PlayerNetwork player in FusionLauncher.Instance.Players)
        //    {
        //        player.SpawnAvatar(false);
        //    }
        //}
        public virtual void SpawnAvatar(PlayerNetworked playerNetwork, bool lateJoin = false)
        {

        }

        public virtual void DespawnAvatar(PlayerNetworked player, bool earlyLeave = true)
        {

        }

        
    }

}
