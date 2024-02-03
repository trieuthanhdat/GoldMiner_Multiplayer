using CoreGame;
using CoreLobby;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_PlayerNetworked : PlayerNetworked
{
    [SerializeField] GoldMiner_PlayerGUI _playerGUI;
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
    public int Score { get; private set; }
    #endregion
    [Networked] private NetworkButtons _buttonsPrevious { get; set; }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RpcSetNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName)) return;
        NickName = nickName;
    }
    public override void Spawned()
    {
        base.Spawned();
        RpcSetNickName(LocalPlayerData.NickName);
        if (ColorIndex == 0) ColorIndex = (byte)(Object.InputAuthority + 1);
        if (_playerGUI) _playerGUI.SetUpPlayer(this);
    }
    public override void OnBeforeSpawned(string displayName, bool isBot)
    {
        this.NickName = displayName;
        this.IsBotSynced = isBot;
        LocalPlayerData.NickName = displayName;
        GoldMiner_GameManagerFusion.Instance.State = GoldMiner_GameManagerFusion.GameState.Waiting;
        InitScore();
    }
    protected async override UniTaskVoid SpawnedAsync()
    {
        await UniTask.WaitUntil(() => FusionLauncher.Session != null, cancellationToken: ctsDespawned.Token);
        FusionLauncher.Session.AddPlayer(this);
        FusionLauncher.Session.OnStartMatch += OnMatchStarted;
    }

    protected override void OnMatchStarted()
    {
    }

    public void InitScore()
    {
        Score = 0;
    }
    public static void OnPlayerChanged(Changed<GoldMiner_PlayerNetworked> playerInfo)
    {
        GoldMiner_GameManagerFusion.Instance.HandleScoreChange(playerInfo.Behaviour);
        playerInfo.Behaviour.HandlePlayerChanged(playerInfo.Behaviour);
    }

    private void HandlePlayerChanged(GoldMiner_PlayerNetworked playerInfo)
    {
        if (_playerGUI) _playerGUI.SetUpTxtScore(playerInfo);
    }

    #region ====HANDLE INPUTS====
    private const string BUTTON_FIRE1 = "Fire1";
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        GoldMinerInput localInput = new GoldMinerInput();

        localInput.Buttons.Set(GoldMinerButton.Fire, Input.GetButton(BUTTON_FIRE1));

        input.Set(localInput);
    }
    public override void FixedUpdateNetwork()
    {
        if (!(Object.HasStateAuthority || Object.HasInputAuthority))
            return;

        if(GetInput<GoldMinerInput>(out var input))
        {
            if(input.Buttons.WasPressed(_buttonsPrevious, GoldMinerButton.Fire))
            {
                Hook();
                _buttonsPrevious = input.Buttons;
            }
        }
    }
    #endregion
    private void Hook()
    {

    }

    public void AdToScore(int score)
    {
        Score += score;
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    
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