using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoreGame
{
	public class ErrorBox : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _status;
		[SerializeField] private TextMeshProUGUI _message;
		
		System.Action onClickOk = null;

		public void Show(ConnectionStatus stat, string message, System.Action onClickOk = null)
		{
			gameObject.SetActive(true);

			this.onClickOk = onClickOk;
			_status?.SetText(stat.ToString());
			_message?.SetText(message);
		}

		public void OnClose()
		{
			gameObject.SetActive(false);
		}

		public void OnClick_Reconnect()
        {
			onClickOk?.Invoke();
			gameObject.SetActive(false);
        }
	}
}