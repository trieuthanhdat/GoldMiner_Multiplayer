using CoreGame;
using CoreLobby;
using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class MainGame_PlayerNetwork : PlayerNetworked
{
    [SerializeField, ReadOnly]
    private PlayerGUI playerGUI = null;
    [SerializeField]
    private Moverment3D moverment = null;
    #region Networked
    [Networked(OnChanged = nameof(OnHealthSyncedChanged))]
    public int HealthSynced { get; set; } = 0;
   
    private static void OnHealthSyncedChanged(Changed<MainGame_PlayerNetwork> changed)
    {
        changed.Behaviour.HandleHealthChanged();
    }
    #endregion
   
    protected void HandleHealthChanged()
    {
        playerGUI?.SetHp(this.HealthSynced);
    }
    /// <summary>
    /// #Local
    /// </summary>
    protected override void OnMatchStarted()
    {
        if (IsMineNotBot)
        {
            Vector3 startMatchWithPosition = GetRandomPoint(Vector3.zero, 8);
            startMatchWithPosition.y = 0;
            moverment?.TeleportTo(startMatchWithPosition);
        }
    }
    protected override Vector3 GetRandomPoint(Vector3 center, float maxDistance)
    {
        return new Vector3(Random.value * 5, 0, Random.value * 5);
    }
   
    public override void OnBeforeSpawned(string displayName, bool isBot)
    {
        base.OnBeforeSpawned(displayName, isBot);
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
    protected async override UniTaskVoid SpawnedAsync()
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
    public override void OnGameEnd()
    {
        if (StateSynced == StateOfPlayer.Playing)
            StateSynced = StateOfPlayer.Finished;
    }
    /// <summary>
    /// #Local
    /// </summary>
    public override bool CanMove()
    {
        if (StateSynced == StateOfPlayer.Playing || StateSynced == StateOfPlayer.InMatching)
        {
            // Check more skill, bufff....
            return true;
        }
        return false;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (moverment == null)
            moverment = GetComponent<Moverment3D>();
    }
#endif
}
