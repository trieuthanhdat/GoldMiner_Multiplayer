using CoreGame;
using CoreLobby;

using UnityEngine;

public class GoldMiner_SessionManager : SessionManager
{
    [SerializeField] private GoldMiner_GameManagerFusion managerPf;

    public override void SpawnLocalPlayerAvatar()
    {
        base.SpawnLocalPlayerAvatar();
    }
    public override void Spawned()
    {
        base.Spawned();
        Runner.Spawn(managerPf, Vector3.zero, Quaternion.identity);
    }

}
