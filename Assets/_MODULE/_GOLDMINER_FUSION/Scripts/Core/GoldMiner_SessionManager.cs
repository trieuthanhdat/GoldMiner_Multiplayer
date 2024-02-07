using CoreGame;
using CoreLobby;

using UnityEngine;

public class GoldMiner_SessionManager : SessionManager
{
    public static GoldMiner_SessionManager instance;
    [SerializeField] private GoldMiner_GameManagerFusion managerPf;

    public override void SpawnLocalPlayerAvatar()
    {
        base.SpawnLocalPlayerAvatar();
    }
    public override void Spawned()
    {
        instance = this;
        base.Spawned();
        //Runner.Spawn(managerPf, Vector3.zero, Quaternion.identity);
    }

}
