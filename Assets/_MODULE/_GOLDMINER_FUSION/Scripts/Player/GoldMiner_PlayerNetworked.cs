using CoreGame;
using CoreLobby;
using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using UnityEngine;

public class GoldMiner_PlayerNetworked : PlayerNetworked, INetworkRunnerCallbacks
{
    [SerializeField] GoldMiner_PlayerGUI _playerGUI;
    [SerializeField] HookMovement _hookMovement;
    [SerializeField] HookScripts  hookPrefab;
    [SerializeField] Transform hookParent;
    [SerializeField] Vector3   hookPosition;
    public HookMovement HookMovement { get { return _hookMovement; } }
    //=== PlayerPf Color ===//
    #region _____PlayerPf Color_____
    [SerializeField]
    private Color[] _colors =
    {
        Color.black,
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
    };
    [Networked] private byte ColorIndex { get; set; }
    public Color Color => _colors[ColorIndex % (_colors.Length - 1) + 1];
    #endregion

    #region _____PlayerPf Info_____
    [Networked(OnChanged = nameof(OnPlayerChanged))]
    public NetworkString<_16> NickName { get; private set; }

    [Networked(OnChanged = nameof(OnPlayerChanged))]
    public int Score { get; protected set; }
    [Networked] private NetworkButtons _buttonsPrevious { get; set; }
    #endregion
    private GoldMiner_GameManagerFusion _gameManagerFusion;

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RpcSetNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName)) return;
        NickName = nickName;
    }
    public override void Spawned()
    {
        base.Spawned();
        GoldMiner_NetworkItem.OnItemCollected += GoldMiner_NetworkItem_OnItemCollected;
        if (ColorIndex == 0) ColorIndex = (byte)(Object.InputAuthority + 1);
        if (_playerGUI) _playerGUI.SetUpPlayer(this);
        /*SpawnHook();*/
    }

    private void SpawnHook()
    {
        Runner.Spawn(hookPrefab, hookPosition, Quaternion.identity, Runner.LocalPlayer,
        ((runner, obj) =>
        {
            var hookObj = obj.GetComponent<HookScripts>();
            if (hookObj != null)
            {
                hookObj.OnBeforeSpawned(()=>
                {
                    if (hookParent) hookObj.transform.SetParent(hookParent);
                    hookObj.transform.position   = hookPrefab.transform.position;
                    hookObj.transform.localScale = hookPrefab.transform.localScale;
                });
            }
        }));
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        GoldMiner_NetworkItem.OnItemCollected -= GoldMiner_NetworkItem_OnItemCollected;
    }
    private void GoldMiner_NetworkItem_OnItemCollected(int score)
    {
        AdToScore(score);
    }

    public override void OnBeforeSpawned(string displayName, bool isBot)
    {
        this.NickName = displayName;
        this.IsBotSynced = isBot;
        LocalPlayerData.NickName = displayName;
        InitScore();
    }
    protected async override UniTaskVoid SpawnedAsync()
    {
        await UniTask.WaitUntil(() => FusionLauncher.Session != null && GoldMiner_GameManagerFusion.Instance != null,
                                      cancellationToken: ctsDespawned.Token);
        FusionLauncher.Session.AddPlayer(this);
        FusionLauncher.Session.OnStartMatch += OnMatchStarted;
        _gameManagerFusion = GoldMiner_GameManagerFusion.Instance;
        _gameManagerFusion.State = GoldMiner_GameManagerFusion.GameState.Waiting;
    }

    protected override void OnMatchStarted()
    {
        MatchStartAsync().Forget();
    }
    protected async UniTaskVoid MatchStartAsync()
    {
        await UniTask.WaitUntil(() => _gameManagerFusion.State == GoldMiner_GameManagerFusion.GameState.Running);
        this.StateSynced = StateOfPlayer.Playing;
    }

    public void InitScore()
    {
        Score = 0;
    }
    public static void OnPlayerChanged(Changed<GoldMiner_PlayerNetworked> playerInfo)
    {
        playerInfo.Behaviour.HandlePlayerChanged(playerInfo.Behaviour);
    }
    private void HandlePlayerChanged(GoldMiner_PlayerNetworked playerInfo)
    {
        HandlePlayerChangedAsync(playerInfo).Forget();
    }
    protected async  UniTaskVoid HandlePlayerChangedAsync(GoldMiner_PlayerNetworked playerInfo)
    {
        await UniTask.WaitUntil(() => FusionLauncher.Session != null && GoldMiner_GameManagerFusion.Instance != null,
                                      cancellationToken: ctsDespawned.Token);
        GoldMiner_GameManagerFusion.Instance.HandleScoreChange(playerInfo);
        if (_playerGUI) _playerGUI.SetUpTxtScore(playerInfo);
    }
    #region ====HANDLE INPUTS====
    public override void FixedUpdateNetwork()
    {
        if (!IsMine)
            return;
        if(StateSynced == StateOfPlayer.Playing)
        {
            if (Object.IsValid && GetInput<GoldMinerInput>(out var input))
            {
                if (input.Buttons.WasPressed(_buttonsPrevious, GoldMinerButton.Fire))
                {
                    Hook();
                }
                _buttonsPrevious = input.Buttons;
            }
        }
        
    }
    #endregion
    private void Hook()
    {
        if (_hookMovement) _hookMovement.StartHook();
    }

    public void AdToScore(int score)
    {
        Score += score;
    }
}

enum GoldMinerButton
{
    Fire = 0
}
public struct GoldMinerInput : INetworkInput
{
    public NetworkButtons Buttons;
}

public class LocalPlayerData
{
    private static string _nickName = GetRandomNickName();

    public static string NickName
    {
        get => _nickName;
        set => _nickName = string.IsNullOrEmpty(value) ? GetRandomNickName() : value;
    }

    private static string GetRandomNickName()
    {
        var rngPlayerNumber = UnityEngine.Random.Range(0, 9999);
        return $"Player {rngPlayerNumber:0000}";
    }
}