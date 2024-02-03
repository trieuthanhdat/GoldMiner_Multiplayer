//using Fusion;
//using System.Collections.Generic;
//using UnityEngine;
//using Game;

//public class PlayerManagers : NetworkBehaviour
//{
//    [SerializeField]
//    private PlayerNetworked PlayerPf = null;

//    public static ICollection<PlayerNetworked> AllPlayers => dictPlayers.Values;
//    private static readonly Dictionary<uint, PlayerNetworked> dictPlayers = new Dictionary<uint, PlayerNetworked>();

//    public void SpawnBot()
//    {
//        this.Runner.Spawn(this.PlayerPf, Vector3.zero, Quaternion.identity, this.Object.InputAuthority, ((runner, obj) =>
//        {
//            string displayName = $"Bot {this.Object.InputAuthority}";
//            PlayerNetworked player = obj.GetComponent<PlayerNetworked>();
//            player.OnBeforeSpawned(displayName, true);
//        }));
//    }
    
//    public static void AddPlayer(PlayerNetworked player)
//    {
//        Debug.Log($"Added player: {player.Object.InputAuthority} - playerId:{player.PlayerId}");
//        uint playerId = player.PlayerId;
//        if (dictPlayers.ContainsKey(playerId))
//        {
//            Debug.LogError($"Available player:{playerId} in dic");
//            return;
//        }
//        dictPlayers.Add(playerId, player);
//    }
//    public static void RemovePlayer(PlayerNetworked player)
//    {
//        uint playerId = player.PlayerId;
//        if (!dictPlayers.ContainsKey(playerId))
//        {
//            Debug.LogError($"Not available player:{playerId}");
//            return;
//        }
//        dictPlayers.Remove(playerId);
//    }
//    public static T GetPlayer<T>(uint playerId) where T : AnimationUnitViewNetworked
//    {
//        if (dictPlayers.ContainsKey(playerId))
//            return dictPlayers[playerId] as T;

//        Debug.LogError($"Missing player staff:{playerId}");
//        return null;
//    }
//    public static bool HasPlayer(uint playerId)
//    {
//        return dictPlayers.ContainsKey(playerId);
//    }

//}
