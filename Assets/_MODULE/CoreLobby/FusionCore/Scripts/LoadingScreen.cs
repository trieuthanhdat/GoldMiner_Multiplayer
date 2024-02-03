using TMPro;
using UnityEngine;

public class LoadingScreen : MonoSingleton<LoadingScreen>
{
    [SerializeField]
    private GameObject objLoadingScreen = null;

    [SerializeField]
    private GameObject objContent = null;
    [SerializeField]
    private TextMeshProUGUI txtContent = null;

    protected override void OnInitiate()
    {
        base.OnInitiate();
        DontDestroyOnLoad(gameObject);
        objLoadingScreen?.SetVisible(false);
    }

    private void SetVisible(bool isVisible)
    {
        objLoadingScreen?.SetVisible(isVisible);
    }
    public void Show()
    {
        SetVisible(true);
        objContent?.SetVisible(false);
    }
    public void Show(string content)
    {
        SetVisible(true);
        objContent?.SetVisible(true);
        txtContent?.SetText(content);
    }
    public void Hide()
    {
        SetVisible(false);
    }
}
