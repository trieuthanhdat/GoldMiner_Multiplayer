using CoreGame;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using TMPro;

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
    [Header("UGUI HANDLER")]
    [SerializeField] private GUIMatching GUIMatching = null;
    [SerializeField] private GUIMain     GUIMain     = null;
    [SerializeField] private LevelBoundaries levelBoundaries = null;
    #endregion
    //===== UIs =====//
    #region _____ GAME COMMON UI _____
    [Header("GAME COMMON UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreFillUI;
    #endregion

    #region _____ NetWork _____
    [Networked] protected TickTimer _timer { get; set; }
    [Networked] private GameState _gameState { get; set; }
    [Networked] private GoldMiner_PlayerNetworked _winner { get; set; }
    #endregion
    public LevelBoundaries LevelBoundaries { get => levelBoundaries; set => levelBoundaries = value; }
    public bool IsServer => Runner.IsSharedModeMasterClient || Runner.IsServer;
    public float GameSessionTime => _timer.RemainingTime(Runner).GetValueOrDefault();
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

    

    private void EndGame()
    {
        if (_winner != null)
            return;

        //Update winner score here

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
    private void OnMatchStarted()
    {
        GUIMatching?.SetActive(false);

        if (GUIMain != null)
        {
            UniTask.Create(async () =>
            {
                await UniTask.Delay(1000);
            });

            GUIMain.OnMatchPrepareAsync(false).Forget();
        }
        OnStartCountdown();
    }
    #region _____ TIMER _____
    public void OnStartCountdown()
    {
        if (!Object.HasStateAuthority)
            return;

        Runner.SessionInfo.IsOpen = false;
        _gameState = GameState.Starting;
        float timer = _startDelay + ((Constant.MILISECOND_DELAY_SEEK_START -100) * 1.0f / 1000f);
        _timer = TickTimer.CreateFromSeconds(Runner, timer);

    }

    public void NetworkUpdate(float deltatime)
    {
        if (FusionLauncher.Session != null)
        {
            SetTimer(GameSessionTime);
        }
    }
    public void SetTimer(float tickTimer)
    {
        this.countdownText?.SetText(string.Format("{0:00}:{1:00}", (int)(tickTimer / 60f), ((int)tickTimer % 60)));
    }
    private void Update()
    {
        NetworkUpdate(Time.deltaTime);
    }
    
    public override void FixedUpdateNetwork()
    {
        switch (_gameState)
        {
            case GameState.Waiting:
                int count = 0;
                foreach (PlayerRef _ in Runner.ActivePlayers)
                    count++;
                break;
            case GameState.Starting:
                if (Object.HasStateAuthority && _timer.Expired(Runner))
                {
                    _gameState = GameState.Running;
                    _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
                }
                break;
            case GameState.Running:
                /*UI?.SetGameTimer(_timer.RemainingTime(Runner));*/
                Debug.Log($"{nameof(GoldMiner_GameManagerFusion).ToUpper()}: Game state Running");
                if (_timer.Expired(Runner))
                {
                    EndGame();
                }
                break;
            case GameState.Ending:
                /*UI?.SetWinner(_winner, _timer.RemainingTime(Runner));*/

                if (_timer.Expired(Runner))
                    FusionLauncher.Session.EndMatchAsync().Forget();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
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
        scoreCount = score;
        Debug.Log($"{nameof(GoldMiner_GameManagerFusion)}: new score "+ scoreCount);
    }
    public void DisplayScore()
    {
        if (scoreText) scoreText.text = "$ " + scoreCount;
        if (scoreFillUI) scoreFillUI.fillAmount = (float)scoreCount / 100f;

    }
    #endregion

}
