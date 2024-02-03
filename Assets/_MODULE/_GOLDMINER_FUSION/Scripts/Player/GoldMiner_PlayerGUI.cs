using CoreLobby;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldMiner_PlayerGUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtNickName;
    [SerializeField] private TextMeshProUGUI _txtMineScore;
    [SerializeField] private Color _mineColor = Color.white;

    public void SetUpPlayer(GoldMiner_PlayerNetworked player)
    {
        SetUpTxtNickName(player);
        SetUpTxtScore(player);
    }
    private void SetUpTxtNickName(GoldMiner_PlayerNetworked player)
    {
        if (_txtNickName != null)
        {
            _txtNickName.text = player.NickName.ToString();
            _mineColor = player.Color;
            _txtNickName.color = player.Color;
        }
    }
    public void SetUpTxtScore(GoldMiner_PlayerNetworked player)
    {
        if (_txtNickName) _txtMineScore.text = player.Score.ToString();

    }
}
