using CoreGame;
using CoreLobby;
using Cysharp.Threading.Tasks;
using Fusion;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_SessionManager : SessionManager
{
    [SerializeField]
    private SessionProps props = null;
    public SessionProps Props
    {
        get
        {
            if (props == null)
                props = new SessionProps(Runner.SessionInfo.Properties);
            return props;
        }
        set => props = value;
    }

    private readonly Dictionary<uint, PlayerNetworked> dictPlayers = new Dictionary<uint, PlayerNetworked>();
    public override void FixedUpdateNetwork()
    {
        float deltaTime = Runner.DeltaTime;
        if (IsServer)
        {
            if (LimitTimeFindMatchSynced.IsRunning)
            {
                if (LimitTimeFindMatchSynced.Expired(Runner))
                {
                    LimitTimeFindMatchSynced = new TickTimer();

                    //Todo: Custome follow game
                    if (dictPlayers.Count > 1)
                    {
                        StartMatch(true);
                    }
                    else
                    {
                        float timeFindMatch = UserData.Local != null ? UserData.Local.TimeFindMatch : GameConfigs.Default.LimitTimeFindMatch;
                        LimitTimeFindMatchSynced = TickTimer.CreateFromSeconds(Runner, timeFindMatch);
                    }
                }
            }
        }
    }

    /// <summary>
    /// #All-Client
    /// </summary>
    private void GameStateChanged()
    {
        switch (this.StateSyned)
        {
            case SessionState.MATCHING:
                break;
            case SessionState.PLAYING:
                OnStartMatch?.Invoke();
                OnStartMatch = null;

                break;
        }
    }

    /// <summary>
    /// #Local
    /// </summary>
    private void OnGameEnd(bool isVictory)
    {
        PlayerNetworked.LocalPlayer.OnGameEnd();
        GameManager.Instance.ShowPrevGameEnd();
        EndMatchAsync().Forget();
    }

    /// <summary>
    /// #Server
    /// </summary>
    private void OnMatchStarted()
    {
        if (IsServer)
        {
        }
    }
    private bool HasLocalAvatar()
    {
        foreach (var item in this.AllPlayers)
        {
            if (item.Object.InputAuthority == Runner.LocalPlayer)
            {
                return true;
            }
        }
        return false;
    }
    
    private void SpawnLocalPlayerAvatar()
    {
        if (Runner.Topology == SimulationConfig.Topologies.Shared)
        {
            FusionLauncher.Instance.SpawnPlayerNetworked(Runner.LocalPlayer);
        }
    }
    /// <summary>
    /// #Server
    /// </summary>
    public void StartMatch(bool isCheating = false)
    {
        this.ShowLog("F-Start Match!!!");
        LimitTimeFindMatchSynced = new TickTimer();
        if (dictPlayers.Count >= MaxPlayer || isCheating)
        {
            this.StateSyned = SessionState.PLAYING;

            OnMatchStarted();
            HideSession();
        }
    }
    /// <summary>
    /// #Server
    /// </summary>
    public async UniTaskVoid EndMatchAsync()
    {
        await UniTask.Delay(1000);

        if (IsServer)
            Runner.SetActiveScene(GameConfigs.Default.GetSceneReference(SceneType.GameOver).ScenePath);
    }

    public void OpenSession()
    {
        Runner.SessionInfo.IsOpen = true;
        Runner.SessionInfo.IsVisible = true;
    }
    public void HideSession()
    {
        Runner.SessionInfo.IsVisible = false;
    }

    //private void OnGUI()
    //{
    //    GUILayout.Space(40);
    //    GUILayout.Label($"State:{StateSyned}");
    //    GUILayout.Label($"PlayerParam:{ListPlayerParams.Count}");
    //}

    #region PLAYER MANAGER
    public void AddPlayer(PlayerNetworked player)
    {
        Debug.Log($"Added player: {player.Object.InputAuthority} - playerId:{player.PlayerId}");
        uint playerId = player.PlayerId;
        if (dictPlayers.ContainsKey(playerId))
        {
            Debug.LogError($"Available player:{playerId} in dic");
            return;
        }
        dictPlayers.Add(playerId, player);
        // Check matching...
        if (dictPlayers.Count >= MaxPlayer)
        {
            this.LimitTimeFindMatchSynced = new TickTimer();
            UniTask.Create(async () =>
            {
                await UniTask.Delay(500);
                if (IsServer)
                    StartMatch();
            });
        }
    }
    public void RemovePlayer(PlayerNetworked player)
    {
        uint playerId = player.PlayerId;
        if (!dictPlayers.ContainsKey(playerId))
        {
            Debug.LogError($"Not available player:{playerId}");
            return;
        }
        dictPlayers.Remove(playerId);
    }
    public PlayerNetworked GetPlayer(uint playerId)
    {
        if (dictPlayers.ContainsKey(playerId))
            return dictPlayers[playerId];
        Debug.LogError($"Missing player staff:{playerId}");
        return null;
    }
    public bool HasPlayer(uint playerId)
    {
        return dictPlayers.ContainsKey(playerId);
    }

    #endregion

    public string GetMatchingProgress()
    {
        return string.Format("Players {0}/{1}", dictPlayers.Count, MaxPlayer);
    }

}
