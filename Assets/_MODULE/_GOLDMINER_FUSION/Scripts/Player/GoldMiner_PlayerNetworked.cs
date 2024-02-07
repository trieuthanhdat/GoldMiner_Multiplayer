using CoreGame;
using CoreLobby;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using FusionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_PlayerNetworked : PlayerNetworked
{
    [SerializeField] GoldMiner_PlayerGUI _playerGUI;
    [SerializeField] HookMovement _hookMovement;
    [SerializeField] HookScripts hookPrefab;
    [SerializeField] Transform hookParent;
    [SerializeField] Vector3 hookPosition;
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
    public int Score { get; private set; }
    [Networked] private NetworkButtons _buttonsPrevious { get; set; }
    #endregion
    private GoldMiner_GameManagerFusion _gameManagerFusion;
    private int currentLocalScore = 0;
    public  int CurrentLocalScore { get => currentLocalScore; set => currentLocalScore = value; }

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
        if (_playerGUI) _playerGUI.SetUpPlayerGUI(PlayerId,this);
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
    private void GoldMiner_NetworkItem_OnItemCollected(uint collectorID, int score)
    {
        AdToScore(score, collectorID);
    }

    public override void OnBeforeSpawned(string displayName, bool isBot)
    {
        this.NickName = displayName;
        this.IsBotSynced = isBot;
        LocalPlayerData.NickName = displayName;
#if UNITY_EDITOR
        name += PlayerId;
#endif
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
        currentLocalScore = 0;
        Score = 0;
    }
    public static void OnPlayerChanged(Changed<GoldMiner_PlayerNetworked> playerInfo)
    {
        playerInfo.Behaviour.HandlePlayerChanged(playerInfo.Behaviour);
    }
    private void HandlePlayerChanged(GoldMiner_PlayerNetworked playerInfo)
    {
        if (_gameManagerFusion == null) return;
        StartCoroutine(Co_HandleScoreChange());
        IEnumerator Co_HandleScoreChange()
        {
            yield return new WaitForSeconds(0.2f);
            if (IsMine) _gameManagerFusion.HandleScoreChange(playerInfo.Score);
            GoldMiner_SessionManager goldMiner_SessionManager = GoldMiner_SessionManager.instance;
            if (goldMiner_SessionManager)
            {
                var player = goldMiner_SessionManager.GetPlayer(playerInfo.PlayerId);
                if (player == null) yield break;

                if (_playerGUI)
                {
                    _playerGUI.SetUpTxtScore(player.PlayerId, player.GetComponent<GoldMiner_PlayerNetworked>().Score);
                }
                Debug.Log($"{name}: player {PlayerId} => new Score {player.GetComponent<GoldMiner_PlayerNetworked>().CurrentLocalScore}");
            }
        }
    }
    

    #region ====HANDLE INPUTS====
    private const string BUTTON_FIRE1 = "Fire1";
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        GoldMinerInput localInput = new GoldMinerInput();
        localInput.Buttons.Set(GoldMinerButton.Fire, Input.GetButton(BUTTON_FIRE1));

        Debug.Log($"{nameof(GoldMiner_PlayerNetworked).ToUpper()}: on input {localInput}");
        input.Set(localInput);
    }
    public override void FixedUpdateNetwork()
    {
        if (!IsMine)
            return;
        if(StateSynced == StateOfPlayer.Playing)
        {
            if (Object.IsValid && GetInput<GoldMinerInput>(out var input))
            {
                Debug.Log($"{nameof(GoldMiner_PlayerNetworked).ToUpper()}: getting input {input}");
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
    public void AddToScore(int score)
    {
        Score += score;
        //Local variable
        currentLocalScore = Score;
    }
    public void AdToScore(int score, uint collectorID)
    {
        if (collectorID != this.PlayerId) return;
        Score += score;
        //Local variable
        currentLocalScore = Score;
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