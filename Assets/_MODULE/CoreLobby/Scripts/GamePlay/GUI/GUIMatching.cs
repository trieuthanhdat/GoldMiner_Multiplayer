using CoreGame;
using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIMatching : MonoBehaviour    //, INetworkUpdated
{
    [SerializeField]
    private TextMeshProUGUI txtTitle = null;
    [SerializeField]
    private TextMeshProUGUI txtPlayerCount = null;
    [SerializeField]
    private Button btnCancel = null;
    [SerializeField]
    private Button btnCheatPlay = null;
    [SerializeField]
    private TextMeshProUGUI txtTimeFindMatch = null;

    private void Start()
    {
        if (btnCancel != null)
            btnCancel.onClick.AddListener(ClickedCancelMatching);
        if (btnCheatPlay != null)
            btnCheatPlay.onClick.AddListener(ClickedCheatPlay);

        StartCoroutine(FxMatching());
        InitiateAsync().Forget();
    }

    private async UniTask InitiateAsync()
    {
        btnCancel.interactable = false;
        await UniTask.WaitUntil(() => FusionLauncher.Session != null);
        btnCancel.interactable = true;
        btnCheatPlay.gameObject.SetActive(FusionLauncher.Session.IsServer);

        //FusionLauncher.Session.AddUpdateNetworked(this);
    }

    private IEnumerator FxMatching()
    {
        while (true)
        {
            txtTitle?.SetText("Waiting player");
            yield return new WaitForSeconds(0.2f);
            txtTitle?.SetText("Waiting player.");
            yield return new WaitForSeconds(0.2f);
            txtTitle?.SetText("Waiting player..");
            yield return new WaitForSeconds(0.2f);
            txtTitle?.SetText("Waiting player...");
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void ClickedCancelMatching()
    {
        btnCancel.interactable = false;
        FusionLauncher.Instance.LeaveMatch();
    }
    
    private void ClickedCheatPlay()
    {
        FusionLauncher.Session.StartMatch(true);
    }
    public void SetActive(bool isActived)
    {
        gameObject.SetActive(isActived);
    }

    public void SetTimer(float tickTimer)
    {
        this.txtTimeFindMatch?.SetText(string.Format("{0:00}:{1:00}", (int)(tickTimer / 60f), ((int)tickTimer % 60)));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        //FusionLauncher.Session?.RemoveUpdateNetworked(this);
    }
    private void Update()
    {
        NetworkUpdate(Time.deltaTime);
    }
    public void NetworkUpdate(float deltatime)
    {
        if (FusionLauncher.Session != null)
        {
            this.txtPlayerCount?.SetText(FusionLauncher.Session.GetMatchingProgress());
            SetTimer(FusionLauncher.Session.FindMatchTime);
        }
    }
}
