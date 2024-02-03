using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    public GameConfigs GameConfigs = null;
    public ImageConfigs ImageConfigs = null;
    
    public string CacheEmail
    {
        get => PlayerPrefs.GetString("CacheEmail", string.Empty);
        set
        {
            PlayerPrefs.SetString("CacheEmail", value);
            PlayerPrefs.Save();
        }
    }
    public string CachePassword
    {
        get => PlayerPrefs.GetString("CachePassword", string.Empty);
        set
        {
            PlayerPrefs.SetString("CachePassword", value);
            PlayerPrefs.Save();
        }
    }

    public GameResultData GameResultCache = new GameResultData();

    protected override void OnInitiate()
    {
        DontDestroyOnLoad(this.gameObject);
        GameConfigs?.OnEnable();
        ImageConfigs?.InitiateDic();
        UserData.Local = new UserData();
    }

    public void SubmitBattleEndAsync(bool isVictory, bool isVictim, bool isYourCatched, int numPrisoner)
    {
        GameResultCache = new GameResultData();
        GameResultCache.IsVictory = isVictory;

        if (!isVictory)
        {
            GameResultCache.IsClaimed = true;
            GameResultCache.IsSynced = true;
        }
        else
        {
            GameResultCache.IsClaimed = false;
            GameResultCache.IsSynced = false;
            this.ShowLog($"SubmitBattleEndAsync: isVictory:{isVictory} -isVictim:{isVictim} - numPrisoner:{numPrisoner} -isYourCatched:{isYourCatched} ");
            //CloudScripts.Instance.banSubmitBattleEnd(isVictory, isVictim, numPrisoner, isYourCatched, responseData =>
            //{
            //    GameResultCache.IsSynced = true;
            //    if (responseData.IsSuccess)
            //    {
            //        GameResultCache.GoidCollected = responseData.GoldCollected;
            //    }
            //    else
            //    {
            //        Debug.LogError("SubmitBattleEnd has error: " + responseData.ErrorCode);
            //    }
            //});
        }
    }
}
