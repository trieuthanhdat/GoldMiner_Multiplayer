using CoreGame;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkCharacterConfig", menuName = "Network/NetworkCharacter")]
public class NetworkCharacterConfig : ScriptableObject
{
    public NetworkCharacterType networkCharacterType;
    public NetworkObject playerNetworkPf;
    public SessionManager sessionManager;
    [Space(50)]
    public bool useAvailableSpawnLocation = false;
    public GameSpawnLocationConfig spawnLocations;
}
[Serializable]
public class GameSpawnLocationConfig
{
    public List<Vector3> spawnLocationList;
}
public enum NetworkCharacterType
{
    MAIN_GAME,
    GOLD_MINER
}
