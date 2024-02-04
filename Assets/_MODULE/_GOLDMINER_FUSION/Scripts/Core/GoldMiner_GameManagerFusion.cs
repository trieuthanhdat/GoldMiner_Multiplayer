using CoreGame;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;

public class GoldMiner_GameManagerFusion : NetworkBehaviour
{
    public enum GameState
    {
        Waiting,
        Starting,
        Running,
        Ending
    }
    #region Singleton
    private static GoldMiner_GameManagerFusion instance = null;
    public static GoldMiner_GameManagerFusion Instance
    {
        set => instance = value;
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GoldMiner_GameManagerFusion>();
            }
            return instance;
        }
    }
    #endregion
    [Header("TIMER")]
    [SerializeField] private float _startDelay = 4.0f;
    [SerializeField] private float _endDelay = 4.0f;
    [SerializeField] private float _gameSessionLength = 120.0f;
    #region _____ REFERENCES _____
    [Header("REFERENCES")]
    [SerializeField] private GUIMatching GUIMatching = null;
    #endregion
    //===== UIs =====//
    #region _____ GAME COMMON UI _____
    [Header("GAME COMMON UI")]
    [SerializeField] private Text countdownText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Image scoreFillUI;
    #endregion

    #region _____ NetWork _____
    [Networked] private TickTimer _timer { get; set; }
    [Networked] private GameState _gameState { get; set; }
    [Networked] private GoldMiner_PlayerNetworked _winner { get; set; }
    #endregion
    public bool IsServer => Runner.IsSharedModeMasterClient || Runner.IsServer;
    public GameState State { get => _gameState; set => _gameState = value; }
    private PlayerAnimation playerAnim;
    private int scoreCount { get; set; } = 0;
    public override void Spawned()
    {
        base.Spawned();
        Init();
        LoadingScreen.Instance.Hide();
        SpawnedAsync().Forget();
    }
    protected async virtual UniTaskVoid SpawnedAsync()
    {
        await UniTask.WaitUntil(() => FusionLauncher.Session != null);

        FusionLauncher.Session.OnStartMatch += OnMatchStarted;
        FusionLauncher.Session.OnSceneLoaded().Forget();

        GUIMatching?.SetActive(FusionLauncher.Session.StateSyned == SessionState.MATCHING);
    }

    private void OnMatchStarted()
    {
        GUIMatching?.SetActive(false);
        _gameState = GameState.Running;

        if (IsServer)
        {
            float timeBattle = UserData.Local != null ? UserData.Local.TimeBattle : GameConfigs.Default.GameTime;
            float timer = timeBattle + (Constant.MILISECOND_DELAY_SEEK_START * 1.0f / 1000f);
            _timer = TickTimer.CreateFromSeconds(Runner, timer);
        }
    }

    private void EndGame()
    {
        if (_winner != null)
            return;

        int maxScore = -1;
        /* foreach (var player in _players.Values)
         {
             if (player.Score >= maxScore)
             {
                 _winner = player;
                 maxScore = player.Score;
             }
         }*/
        _timer = TickTimer.CreateFromSeconds(Runner, _endDelay);
        _gameState = GameState.Ending;
    }
    public void OnExit()
    {
        Runner.Shutdown(false, ShutdownReason.GameIsFull);
    }
    public void Init()
    {
        scoreCount = 0;
        DisplayScore();
    }

    public void OnStartCountdown()
    {
        if (!Object.HasStateAuthority)
            return;

        Runner.SessionInfo.IsOpen = false;

        _gameState = GameState.Starting;
        _timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
    }
    public override void FixedUpdateNetwork()
    {
        // Update the game display with the information relevant to the current game state
        switch (_gameState)
        {
            case GameState.Waiting:
                int count = 0;
                foreach (PlayerRef _ in Runner.ActivePlayers)
                    count++;
                /*UI?.SetWaitingPlayers(count, HasStateAuthority);*/
                break;
            case GameState.Starting:
                /* UI?.SetCountdownTimer(_timer.RemainingTime(Runner));*/

                if (Object.HasStateAuthority && _timer.Expired(Runner))
                {
                    /*foreach (var player in Runner.ActivePlayers)
                        SpawnSpaceship(player);*/
                    _gameState = GameState.Running;
                    _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);

                    // Make sure we have an initial snapshot for host migration - otherwise
                    // the first X seconds will return to the StartGame screen again because no players have been spawned
                    Runner.PushHostMigrationSnapshot();
                }
                break;
            case GameState.Running:
                /*UI?.SetGameTimer(_timer.RemainingTime(Runner));*/
                if (_timer.Expired(Runner))
                {
                    EndGame();
                }
                break;
            case GameState.Ending:
                /*UI?.SetWinner(_winner, _timer.RemainingTime(Runner));*/

                if (_timer.Expired(Runner))
                    Runner.Shutdown();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #region _____ BUTTON'S EVENT _____
    public void OnClick_StartGame()
    {
        OnStartCountdown();
    }
    #endregion

    #region _____ SCORE _____
    public void HandleScoreChange(GoldMiner_PlayerNetworked info)
    {
        if(info.IsMine) AddToScore(info.Score);
        DisplayScore();
    }
    public void AddToScore(int score)
    {
        scoreCount += score;
        Debug.Log($"{nameof(GoldMiner_GameManagerFusion)}: new score "+ scoreCount);
    }
    public void DisplayScore()
    {
        if (scoreText) scoreText.text = "$ " + scoreCount;
        if (scoreFillUI) scoreFillUI.fillAmount = (float)scoreCount / 100f;

    }
    #endregion

}
