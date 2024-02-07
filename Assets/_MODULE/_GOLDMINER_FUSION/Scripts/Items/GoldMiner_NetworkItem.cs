using Fusion;
using FusionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;

public class GoldMiner_NetworkItem : NetworkBehaviour, ISparseVisual<ItemState, GoldMiner_NetworkItem>
{
    #region _____ SERIALIZED FIELDS _____
    [SerializeField] ItemDetailType itemType;
    [SerializeField] int itemScore = 20;
    [SerializeField] SpriteRenderer spriteRenderer;
    [Space(50)]
    [Header("NETWORK SYNC")]
    [SerializeField] ItemSpriteConfig itemSpriteConfig;
    #endregion  

    #region _____ GROUP ITEM'S ATTRIBUTES _____
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }
    public ItemDetailType ItemType { get => itemType; }
    public int ItemScore { get => itemScore; }
    private float _moveSpeed = 3;
    #endregion

    #region _____ NETWORKED _____
    [Networked(OnChanged = nameof(OnScoreChange))]
    public int ItemScoreSync { get; set; }
    [Networked(OnChanged = nameof(OnSpriteChange))]
    public int CurrentItemIndexSync { get; set; }
    [Networked(OnChanged = nameof(OnStatusChange))]
    public NetworkBool ActiveStatusSync { get; set; }
    [Networked(OnChanged = nameof(OnPickupItemSync))]
    public bool canPickupItemSync { get; set; }
    #endregion
    private bool _canPickupItem = false;
    //EVENTS
    public static event Action<int>       OnItemCollected;
    public static event Action<Transform> OnItemPickup;

    public static void OnPickupItemSync(Changed<GoldMiner_NetworkItem> changeInfo)
    {
        changeInfo.Behaviour.RPC_HandlePickupItem(changeInfo.Behaviour.canPickupItemSync);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_HandlePickupItem(bool canPickup, RpcInfo info = default)
    {
        _canPickupItem = canPickup;
        //OnItemPickup?.Invoke(this.transform);
        Debug.Log($"GOLDMINER_NETWORKITEM: on canPickUpItem synce => can pick up {_canPickupItem}");
    }

    public static void OnStatusChange(Changed<GoldMiner_NetworkItem> changeInfo)
    {
        changeInfo.Behaviour.ActiveStatusSync = changeInfo.Behaviour.gameObject.activeInHierarchy;
        Debug.Log($"GOLDMINER_NETWORKITEM: on status synce => is enable {changeInfo.Behaviour.gameObject.activeInHierarchy}");
    }
    public static void OnSpriteChange(Changed<GoldMiner_NetworkItem> changeInfo)
    {
        changeInfo.Behaviour.HandleItemSpriteSync(changeInfo.Behaviour);
        Debug.Log("GOLDMINER_NETWORKITEM: On Sprite synce => new sprite " + changeInfo.Behaviour.SpriteRenderer.sprite.name);
    }

    public static void OnScoreChange(Changed<GoldMiner_NetworkItem> changeInfo)
    {
        changeInfo.Behaviour.itemScore = changeInfo.Behaviour.ItemScoreSync;
        Debug.Log($"GOLDMINER_NETWORKITEM: on score synce => new score {changeInfo.Behaviour.itemScore}");
    }
    public void HandleItemSpriteSync(GoldMiner_NetworkItem changeInfo)
    {
        int index = GetSpriteIndexFromConfig(changeInfo.CurrentItemIndexSync);
        spriteRenderer.sprite = itemSpriteConfig.listItemSprite[index].itemSprite;
    }
    public int GetSpriteIndexFromConfig(int type)
    {
        //If null => return first sprite
        if (itemSpriteConfig == null) return 0;
        foreach(var i in itemSpriteConfig.listItemSprite)
        {
            if (i.itemType == (ItemDetailType)type) return itemSpriteConfig.listItemSprite.IndexOf(i);
        }
        return 0;
    }
    //CONSTRUCTOR
    public GoldMiner_NetworkItem(SpriteRenderer spriteRenderer, int score)
    {
        this.spriteRenderer = spriteRenderer;
        this.itemScore = score;
    }
    public float Speed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }
    
    public override void Spawned()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        this.ItemScoreSync = itemScore;
        this.CurrentItemIndexSync = (int)itemType;
        base.Spawned();
        ActiveStatusSync = true;
        canPickupItemSync = _canPickupItem;
        Debug.Log($"GOLEMINER_NETWORKITEM: on spawned {name} isActive {gameObject.activeInHierarchy}");
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        ActiveStatusSync = false;
    }
    public void UpdateItemPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    public void ApplyNewAttributes(ItemDetailType newType, int newScore, bool copyScale = false, GameObject source = null)
    {
        itemScore = newScore;
        this.ItemScoreSync = newScore;
        if (copyScale && source)
        {
            transform.localScale = source.transform.localScale;
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        this.CurrentItemIndexSync = (int)newType;
        Debug.Log("GOLDMINER_NETWORKITEM: new score "+ itemScore + " new Sprite " + spriteRenderer.sprite);
    }
    
    public void OnPickUp(HookMovement hookMovement)
    {
        if (hookMovement) Speed = hookMovement.MoveSpeed;
        _canPickupItem = true;
    }
    public void OnCollected()
    {
        OnItemCollected?.Invoke(ItemScore);
        Debug.Log($"{nameof(GoldMiner_NetworkItem).ToUpper()}: on item collected: {name} - {itemScore}");
    }
   
    public void ApplyStateToVisual(NetworkBehaviour owner, ItemState state, float t, bool isFirstRender, bool isLastRender)
    {
        transform.rotation = Quaternion.Euler(0, 0, (float)state.Direction);
        transform.position = state.Position;
    }
}

public struct ItemState : ISparseState<GoldMiner_NetworkItem>
{
    /// <summary>
    /// Required sparse state properties
    /// </summary>
    public int StartTick { get; set; }
    public int EndTick { get; set; }

    /// <summary>
    /// Bullet specific sparse properties
    /// </summary>
    public Vector2 Position;
    public Angle Direction;

    /// <summary>
    /// Extrapolated local values
    /// </summary>
    public Vector2 Forward
    {
        get
        {
            float a = Mathf.Deg2Rad * -(float)Direction;
            return new Vector2(Mathf.Sin(a), Mathf.Cos(a));
        }
    }

    public ItemState(Vector2 startPosition, Vector2 direction)
    {
        StartTick = 0;
        EndTick = 0;
        Position = startPosition;
        Direction = Vector2.SignedAngle(Vector2.up, direction);
    }

    public void Extrapolate(float t, GoldMiner_NetworkItem prefab)
    {
        Position += t * prefab.Speed * Forward;
    }

}
