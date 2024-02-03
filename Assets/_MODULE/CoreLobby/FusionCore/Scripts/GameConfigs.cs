using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

[CreateAssetMenu()]
public class GameConfigs : ScriptableObject
{
    public static GameConfigs Default = null;

    [Header("Lobby-Configs")]
    public GameMode PlayingMode = GameMode.Shared;
    public string LobbyId = "Lobby_Dev_01";

    public uint MaxPlayer = 12;
    public float GameTime = 60;
    public float LimitTimeFindMatch = 120;
    
    [Space(5)]
    public SceneParam[] SceneArray = new SceneParam[0];
    
    public void OnEnable()
    {
        if (Default == null)
            Default = this;
    }
    
    public void LoadSceneNetwork(NetworkRunner runner, SceneType sceneType)
    {
        foreach (var item in SceneArray)
        {
            if (item.SceneType == sceneType)
            {
                runner.SetActiveScene(item.SceneRef.ScenePath);
                return;
            }
        }
        Debug.LogError($"Missing scene :{sceneType}");
    }

    public void LoadScene(SceneType sceneType, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        foreach (var item in this.SceneArray)
        {
            if (item.SceneType == sceneType)
            {
                SceneManager.LoadScene(item.SceneRef.ScenePath, loadSceneMode);
                return;
            }
        }
        Debug.LogError($"Missing scene :{sceneType}");
    }

    public SceneReference GetSceneReference(SceneType sceneType)
    {
        foreach (var item in this.SceneArray)
        {
            if (item.SceneType == sceneType)
                return item.SceneRef;
        }
        Debug.LogError($"Missing scene :{sceneType}");
        return null;
    }
    public SceneReference GetMiniGameSceneReference(SceneType sceneType, NetworkCharacterType characterType)
    {
        foreach (var item in this.SceneArray)
        {
            if (item.SceneType == sceneType && item.characterType == characterType)
                return item.SceneRef;
        }
        Debug.LogError($"Missing scene :{sceneType}");
        return null;
    }

    [System.Serializable]
    public struct SceneParam
    {
        public SceneType SceneType;
        public NetworkCharacterType characterType;
        public SceneReference SceneRef;
    }
}

[System.Serializable]
public class SkinConfig
{
    public int SkinId;
    public Texture2D Texture;
    public Color SkinColor = Color.white;
    public int Cost = 300;
}
public enum SceneType : byte
{
    Lobby = 0,
    Menu = 1,
    GameScene = 2,
    GameOver = 3,
    Login = 4,
    MiniGameScene = 5
}

[System.Serializable]
public struct MinMax
{
    public float min;
    public float max;

    public MinMax(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

[System.Serializable]
public class GameResultData
{
    public bool IsVictory;
    public int GoidCollected;
    public bool IsClaimed;

    public bool IsSynced;
    public GameResultData()
    {
        IsClaimed = true;
        GoidCollected = 0;
        IsVictory = false;
        IsSynced = false;
    }
}
