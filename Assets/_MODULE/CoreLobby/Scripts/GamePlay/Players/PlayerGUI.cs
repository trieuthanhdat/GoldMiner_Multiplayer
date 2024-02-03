using CoreLobby;
using TMPro;
using UnityEngine;

public class PlayerGUI : MonoBehaviour
{
    const float SPEED_LERP = 15f;
    [SerializeField]
    private TextMeshPro txtDisplayName;
    [SerializeField]
    private TextMeshPro txtHealth;
    [SerializeField]
    private Color mineColor = Color.green;
    [SerializeField]
    private Color otherColor = Color.white;
    [SerializeField]
    private Transform tfVisual = null;
    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private bool moveWithLerp = false;
    public void SetPlayer(PlayerNetworked player)
    {
        if (txtDisplayName != null)
        {
            txtDisplayName.SetText(player.DisplayNameSynced.ToString());
            txtDisplayName.color = player.IsMineNotBot ? mineColor : otherColor;
        }
        target = player.transform;

        if (tfVisual != null)
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = FindObjectOfType<Camera>();
            tfVisual.rotation = cam.transform.rotation;
        }
    }
    public void SetHp(int hp)
    {
        this.txtHealth?.SetText($"HP: {hp}");
    }
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
        if (target != null)
            transform.position = target.position;
    }

    private void Update()
    {
        if (target != null)
        {
            if (moveWithLerp)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * SPEED_LERP);
            }
            else
            {
                transform.position = target.position;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (tfVisual == null)
            tfVisual = transform.GetChild(0);

        if (target != null)
            transform.position = target.position;
    }
#endif
}
