using TMPro;
using UnityEngine;

public class GoldMiner_PlayerGUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtNickName;
    [SerializeField] private TextMeshProUGUI _txtMineScore;
    [SerializeField] private Color _mineColor = Color.white;
    private uint _guiIdentify;
    public void SetUpPlayerGUI(uint id, GoldMiner_PlayerNetworked player)
    {
        _guiIdentify = id;
        SetUpTxtNickName(id, player);
        SetUpTxtScore(id, player.Score);
    }
    private void SetUpTxtNickName(uint id, GoldMiner_PlayerNetworked player)
    {
        if (_guiIdentify != id) return;
        if (_txtNickName != null)
        {
            _txtNickName.text = player.NickName.ToString();
            _mineColor = player.Color;
            _txtNickName.color = player.Color;
        }
    }
    public void SetUpTxtScore(uint id,int score)
    {
        if (_guiIdentify != id) return;
        if (_txtNickName) _txtMineScore.text = score.ToString();
    }
}
