using CoreGame;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class GUIGameEnd : MonoBehaviour
{
    [SerializeField]
    private GameObject objVictory = null;
    [SerializeField]
    private GameObject objDefeat = null;
    [SerializeField]
    private TextMeshProUGUI txtYourCoin = null;
    [SerializeField]
    private TextMeshProUGUI txtCoinCollected = null;
    [SerializeField]
    private Button btnMenu = null;
    [SerializeField]
    private Button btnAds = null;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private void Start()
    {
        if (btnMenu != null)
            btnMenu.onClick.AddListener(ClickedMenu);
        this.txtYourCoin?.SetText(UserData.Local.Gold.ToString("N0"));

        FusionLauncher.Instance.DestroySession();

        cts.CancelAfterSlim(TimeSpan.FromSeconds(30)); // 30 sec timeout.
        SetUpAsync().Forget();
    }
    private void ClickedMenu()
    {
        FusionLauncher fusionLauncher = FindObjectOfType<FusionLauncher>();
        if (fusionLauncher != null)
        {
            Destroy(fusionLauncher.gameObject);
        }
        GameConfigs.Default.LoadScene(SceneType.Lobby);
    }
    private async UniTaskVoid SetUpAsync()
    {
        try
        {
            await UniTask.WaitUntil(() => DataManager.Instance.GameResultCache.IsSynced, cancellationToken: cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            ex.Log();
            if (ex.CancellationToken == cts.Token)
            {
                //Timeout
                DataManager.Instance.GameResultCache.IsSynced = true;
                DataManager.Instance.GameResultCache.GoidCollected = 0;
            }
        }


        GameResultData result = DataManager.Instance.GameResultCache;

        objVictory?.SetVisible(result.IsVictory);
        objDefeat?.SetVisible(!result.IsVictory);

        float goldCollected = result.GoidCollected;

        this.txtCoinCollected?.SetText("0");
        DOVirtual.Float(0, goldCollected, 0.4f, value =>
        {
            this.txtCoinCollected?.SetText(Mathf.RoundToInt(value).ToString());
        }).SetDelay(0.2f).OnComplete(delegate
        {
            this.txtCoinCollected?.SetText(result.GoidCollected.ToString());
        });
    }

}
