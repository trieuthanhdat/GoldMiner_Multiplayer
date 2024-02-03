using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class UIButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [ReadOnly][SerializeField] Button button = null;
    [Range(0.5f, 1.5f)][SerializeField] float sizeDown = 0.97f;
    //[SerializeField]
    //private AudioType audioType = AudioType.ui_click_button;
    //private void Start()
    //{
    //    button.onClick.AddListener(PlaySoundClick);
    //}
    private void PlaySoundClick()
    {
        //AudioManager.Instance.PlayOnceShoot(audioType);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable == false)
            return;
        transform.DOKill();
        transform.localScale = new Vector3(sizeDown, sizeDown, 1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.interactable == false)
            return;
        transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack);
        PlaySoundClick();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
        //button.transition = Selectable.Transition.ColorTint;
    }
#endif
}
