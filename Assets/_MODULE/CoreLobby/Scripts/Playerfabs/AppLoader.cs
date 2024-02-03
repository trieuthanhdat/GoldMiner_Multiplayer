using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLoader : MonoBehaviour
{
    private void Start()
    {
        
    }
    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
        DataManager.Instance.CachePassword = string.Empty;
        GameConfigs.Default.LoadScene(SceneType.Login);
    }
}
