//using Cysharp.Threading.Tasks;
//using Fusion;

//namespace CoreGame
//{
//    public class PlayerNetworked : NetworkBehaviour
//    {
//        public static PlayerNetworked Local = null;
        
//        public uint PlayerId => Object.Id.Raw;
//        public bool IsMine => Object.HasInputAuthority;
//        public bool IsMineNotBot => !IsBotSynced && IsMine;


//        #region Networked
//        [Capacity(50)]
//        [Networked] public NetworkString<_16> DisplayNameSynced { get; set; }
//        [Networked] public NetworkBool IsBotSynced { get; set; }

//        private static void ON_STATE_CHANGE(Changed<PlayerNetworked> changed)
//        {
//        }

//        #endregion

        
//        public void OnBeforeSpawned(string displayName, bool isBot)
//        {
//            this.DisplayNameSynced = displayName;
//            this.IsBotSynced = isBot;
//        }

//        public override void Spawned()
//        {
//            if (IsMineNotBot)
//            {
//                Local = this;
//                CameraManager.Target = this.transform;
//            }
//            PlayerManagers.AddPlayer(this);
//        }

//        public override void Despawned(NetworkRunner runner, bool hasState)
//        {
//            if (hasState)
//            {

//            }
//        }

//        public override void FixedUpdateNetwork()
//        {
//        }
//    }
//}