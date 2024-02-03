using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PathologicalGames;
using UnityEngine.UI;

namespace CoreGame
{
    public class FusionLobby : MonoBehaviour
    {
        [SerializeField] SpawnPool lobbyItemPools;

        [SerializeField] LobbyItemView lobbyItemPf = null;
        [SerializeField] TextMeshProUGUI txtLobbyId = null;
        [SerializeField] CanvasGroup canvasGroup = null;

        [SerializeField] TMP_InputField input_DisplayName;
        [SerializeField] TMP_InputField input_SessionName;
        [SerializeField]
        private Button btnJoinRandomRoom = null;
        [SerializeField]
        private Button btnCreateRoom = null;
        
        private void Start()
        {
            string roomName = Random.Range(1000, 99999).ToString();
            this.input_SessionName.text = roomName;

            string displayName = UserData.Local.DisplayName;
            
            if (input_DisplayName != null)
            {
                input_DisplayName.text = displayName;
                input_DisplayName.onEndEdit.AddListener(newName =>
                {
                    UserData.Local.DisplayName = newName.Trim();
                    //GameConfigs.Instance.DisplayName = newName.Trim();
                });
            }

            //GameConfigs.Instance.DisplayName = displayName;// Caching

            if (btnJoinRandomRoom != null)
                btnJoinRandomRoom.onClick.AddListener(delegate
                {
                    FusionLauncher.Instance.JoinRandomRoom();
                });
            if (btnCreateRoom != null)
                btnCreateRoom.onClick.AddListener(delegate
                {
                    FusionLauncher.Instance.CreateSession(new SessionProps
                    {
                        SessionName = input_SessionName.text,
                    });
                });
        }

        private void OnEnable()
        {
            FusionLauncher.Instance.OnUpdateSessionList += OnSessionListUpdated;
            FusionLauncher.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);

        }
        private void OnDisable()
        {
            try
            {
                FusionLauncher.Instance.OnUpdateSessionList -= OnSessionListUpdated;
                FusionLauncher.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
            }
            catch (System.Exception ex)
            {
            }
        }
        void OnJoinedLobby()
        {
            this.canvasGroup?.SetVisible(true);
        }
        void CreateNewSession()
        {
            SessionProps props = new SessionProps();

            string roomName = input_SessionName.text.Trim();
            props.SessionName = roomName;
            FusionLauncher.Instance.CreateSession(props);
        }

        public void Show(string lobbyId)
        {
            txtLobbyId?.SetText(lobbyId);
            gameObject.SetActive(true);
            this.canvasGroup?.SetVisible(false, 0.7f);
        }

        public void OnSessionListUpdated(List<SessionInfo> sessions)
        {
            lobbyItemPools.DespawnAll();
            if (sessions != null)
            {
                foreach (SessionInfo info in sessions)
                {
                    Debug.Log(info.ToString());
                    lobbyItemPools.Spawn<LobbyItemView>()?.SetData(info);
                }
                this.canvasGroup?.SetVisible(true);
            }
            else
            {
                this.canvasGroup?.SetVisible(false, 0.7f);
            }
        }

        public void OnClick_CreateSession()
        {
            CreateNewSession();
            Hide();
        }

        public void OnClick_JoinRandomSession()
        {
            FusionLauncher.Instance.JoinRandomRoom();
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }

}
