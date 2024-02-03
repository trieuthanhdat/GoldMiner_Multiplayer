using CoreGame;
using TMPro;
using UnityEngine;

public class GUIGameEndPreview : MonoBehaviour
{
    [SerializeField]
    private GameObject objMainGUI = null;
    [SerializeField]
    private GameObject objVictory = null;
    [SerializeField]
    private GameObject objDefeat = null;
    
    public void Show()
    {
        gameObject.SetActive(true);
        objMainGUI?.SetVisible(false);

        GameResultData result = DataManager.Instance.GameResultCache;

        objVictory?.SetVisible(result.IsVictory);
        objDefeat?.SetVisible(!result.IsVictory);
    }

}
