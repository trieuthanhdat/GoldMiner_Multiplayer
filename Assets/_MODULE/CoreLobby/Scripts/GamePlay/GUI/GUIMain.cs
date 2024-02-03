using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIMain : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI txtTimer = null;
    [SerializeField]
    private TextMeshProUGUI txtGoldDisplay = null;
    [SerializeField]
    private CanvasGroup canvasGroup = null;
    [SerializeField]
    private Transform tfPlayerStatusPf = null;
    [SerializeField]
    private GameObject objGameHub = null;
    [SerializeField]
    private GameObject objPrepareHub = null;
    [SerializeField]
    private TextMeshProUGUI txtTimeToHide = null;


    private void Awake()
    {
        objGameHub?.SetVisible(false);
        objPrepareHub?.SetVisible(false);

        SetVisible(false);
    }
    private void Start()
    {
        if (UserData.Local != null)
            txtGoldDisplay?.SetText(UserData.Local.Gold.ToString());
        else
            txtGoldDisplay?.SetText("N/A");
    }
    public async UniTaskVoid OnMatchPrepareAsync()
    {
        SetVisible(true);
        objPrepareHub?.SetVisible(true);

        await UniTask.Delay(100);

        int delay = Constant.MILISECOND_DELAY_SEEK_START - 100;
        while (delay >= 0)
        {
            if (txtTimeToHide != null)
            {
                txtTimeToHide.alpha = 0f;
                txtTimeToHide.DOFade(1.0f, 0.3f);

                txtTimeToHide.SetText($"{Mathf.CeilToInt(delay * 1f / 1000f)}");
            }

            await UniTask.Delay(1000);
            delay -= 1000;
        }

        objGameHub?.SetVisible(true);
        objPrepareHub?.SetVisible(false);
    }

    public void SetVisible(bool isVisible)
    {
        canvasGroup?.SetVisible(isVisible);
        //gameObject.SetVisible(isVisible);
    }

    public void SetTimer(float tickTimer)
    {
        this.txtTimer?.SetText(string.Format("{0:00}:{1:00}", (int)(tickTimer / 60f), ((int)tickTimer % 60)));
    }
}
