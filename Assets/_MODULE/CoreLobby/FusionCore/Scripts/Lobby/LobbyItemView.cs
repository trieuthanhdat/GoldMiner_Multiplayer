using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
//using DG.Tweening;

namespace CoreGame
{
    public class LobbyItemView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txtLobby = null;
        [SerializeField] TextMeshProUGUI txtPlayers = null;
        [SerializeField] Button buttonJoin = null;
        SessionInfo info = null;
        public void SetData(SessionInfo info)
        {
            gameObject.SetActive(true);
            this.info = info;
            this.txtLobby?.SetText($"RoomName:<color=green>{info.Name}</color>");
            this.txtPlayers?.SetText($"Players:{info.PlayerCount}/{info.MaxPlayers}");

            if (buttonJoin != null)
                buttonJoin.interactable = true;

            //DOVirtual.DelayedCall(0.5f, delegate {
            //    if (buttonJoin != null)
            //        buttonJoin.interactable = info.IsOpen;
            //});
            
        }

        public void OnClick_Join()
        {
            if (this.info != null)
            {
                FusionLauncher.Instance.JoinSession(info);
            }
        }

        //private void Update()
        //{
        //    if (this.info != null)
        //    {
        //        if (buttonJoin != null)
        //            buttonJoin.interactable = info.IsValid && info.IsOpen;
        //    }
        //}
    }
}

