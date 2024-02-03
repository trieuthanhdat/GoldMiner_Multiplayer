using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using CoreLobby;

namespace CoreGame
{
    public class UIMatching : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txtPlayerMatchingCount = null;
        [SerializeField] TextMeshProUGUI txtRoomName = null;
        [SerializeField] TextMeshProUGUI txtMatching = null;
        [SerializeField] Button btnPlayNow = null;
        [SerializeField] GameObject objHostServer = null;

        [ReadOnly][SerializeField] NetworkRunner runner = null;
        bool IsServer => runner != null && runner.IsSharedModeMasterClient;

        private void Start()
        {
            if (btnPlayNow != null)
            {
                btnPlayNow.onClick.AddListener(delegate
                {
                    FusionLauncher.Instance.StartSession();
                });
            }
        }

        public void IsVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void Show(NetworkRunner runner)
        {
            this.runner = runner;
            IsVisible(true);
            if (btnPlayNow != null)
            {
                btnPlayNow.gameObject.SetActive(IsServer);
            }
            this.txtRoomName?.SetText($"Room: <color=yellow>{runner.SessionInfo.Name}</color>");

            objHostServer?.SetVisible(IsServer);
            StartCoroutine(nameof(IRunningTextEffect));
        }
        IEnumerator IRunningTextEffect()
        {
            while (true)
            {
                txtMatching?.SetText("Matching");
                yield return new WaitForSeconds(0.2f);
                txtMatching?.SetText("Matching.");
                yield return new WaitForSeconds(0.2f);
                txtMatching?.SetText("Matching..");
                yield return new WaitForSeconds(0.2f);
                txtMatching?.SetText("Matching...");
                yield return new WaitForSeconds(0.2f);
            }
        }
        public void UpdatePlayer(ICollection<PlayerNetworked> players)
        {
            this.txtPlayerMatchingCount?.SetText($"Player: <color=yellow>{players.Count}/{GameConfigs.Default.MaxPlayer}</color>");
        }
        private void Update()
        {
            objHostServer?.SetVisible(IsServer);
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void OnClick_PlayNow()
        {
            IsVisible(false);
        }
    }

}
