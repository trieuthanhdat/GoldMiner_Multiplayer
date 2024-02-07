using Fusion;
using FusionHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;

public class HookScripts : NetworkBehaviour
{
    #region _____ REFERENCES _____
    [SerializeField] private Transform             itemHolder;
    [SerializeField] private PlayerAnimation       playerAnim;
    [SerializeField] private HookMovement          hookMovement;
    [SerializeField] private Transform             hookTransform;
    [SerializeField] private GoldMiner_NetworkItem originItem;
    [SerializeField] private Hitbox                hookHitBox;
    [SerializeField] private LayerMask             collisionMask;
    #endregion
    public bool IsMine => Object.HasStateAuthority || Object.HasInputAuthority;
    public bool IsMasterClient => Runner.IsSharedModeMasterClient;

    [Networked, Capacity(50)] private NetworkArray<ItemState> _itemStates => default;

    private List<LagCompensatedHit> _lagCompensatedHits = new List<LagCompensatedHit>();

    protected SparseCollection<ItemState, GoldMiner_NetworkItem> _items;
    private bool itemAttached;
    private Vector3 _hookStartPosition;
    private bool _hasGetItem;
    private bool _hasCollectedItem;
    private Action spawnCallback;
    private GoldMiner_NetworkItem _currentOrigin;
    private GoldMiner_PlayerNetworked _playerReference;

    public GoldMiner_NetworkItem CurrentOrigin { get => _currentOrigin; set { _currentOrigin = value; } }

    public override void Spawned()
    {
        base.Spawned();
        if(TryGetComponent<NetworkRigidbody2D>(out NetworkRigidbody2D networkRigd))
        {
            networkRigd.enabled = true;
        }
        CheckHookMovementAndPlayerAnimation();

        _items = new SparseCollection<ItemState, GoldMiner_NetworkItem>(_itemStates, originItem);
        _hookStartPosition = hookTransform.position;
#if UNITY_EDITOR
        name += GetInstanceID();
#endif
        if(_playerReference == null)
            _playerReference = GetComponentInParent<GoldMiner_PlayerNetworked>();
        spawnCallback?.Invoke();
        Debug.Log("HOOK SCRIPTS: on hook spawned");
    }


    public void OnBeforeSpawned(Action spawnCallback)
    {
        CheckHookMovementAndPlayerAnimation();
        this.spawnCallback = spawnCallback;
    }

    private void CheckHookMovementAndPlayerAnimation()
    {
        if (hookMovement == null) hookMovement = GetComponentInParent<HookMovement>();
        if (playerAnim == null) playerAnim = GetComponentInParent<PlayerAnimation>();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _items.Clear();
        base.Despawned(runner, hasState);
    }
    public override void Render()
    {
        _items.Render(this);
    }
    public override void FixedUpdateNetwork()
    {
        CheckCollision();
    }
    private NetworkBehaviour WillHitItem(float delta, int layer, out NetworkBehaviour itemGet)
    {
        if (hookMovement.CanRotate)
        {
            itemGet = null;
            return null;
        }

        _lagCompensatedHits.Clear();
        bool isMoveDown = hookMovement.MoveDown;
        Vector3 sphereCenter = hookTransform.position;
        float sphereRadius = hookHitBox.SphereRadius;

        var count = Runner.LagCompensation.OverlapSphere(sphereCenter, sphereRadius, Object.InputAuthority, _lagCompensatedHits, layer);
        _lagCompensatedHits.SortDistance();

#if UNITY_EDITOR
        // Visualize the sphere
        DebugDrawSphere(sphereCenter, sphereRadius, Color.red);
#endif

        Debug.Log($"{nameof(HookScripts).ToUpper()}: Overlapping sphere over network => is forward {hookMovement.MoveDown} count {count}");

        for (int i = 0; i < count; i++)
        {
            GameObject hit = _lagCompensatedHits[i].GameObject;
            if(_currentOrigin != null)
                if (hit == _currentOrigin.gameObject)
                    continue;

            if(isMoveDown)
            {
                if (hit.TryGetComponent<GoldMiner_NetworkItem>(out var itemNetwork))
                {
                    itemGet = itemNetwork;
                    return itemNetwork;
                }
            }
            else
            {
                if (hit.TryGetComponent<GoldMiner_DeliverItem>(out var itemNetwork))
                {
                    itemGet = itemNetwork;
                    return itemNetwork;
                }
            }
            
        }

        itemGet = null;
        return null;
    }

    private void DebugDrawSphere(Vector3 center, float radius, Color color)
    {
        // Draw the wire sphere for visualization purposes
        Debug.DrawRay(center + Vector3.up      * radius, Vector3.forward * radius, color);
        Debug.DrawRay(center + Vector3.right   * radius, Vector3.back    * radius, color);
        Debug.DrawRay(center + Vector3.down    * radius, Vector3.right   * radius, color);
        Debug.DrawRay(center + Vector3.left    * radius, Vector3.up      * radius, color);
        Debug.DrawRay(center + Vector3.forward * radius, Vector3.left    * radius, color);
        Debug.DrawRay(center + Vector3.back    * radius, Vector3.down    * radius, color);
    }

    private void CheckCollision()
    {

        if (!IsMine)
            return;

        if (WillHitItem(hookMovement.MoveSpeed * Runner.DeltaTime, collisionMask, out NetworkBehaviour target) != null)
        {
            if (target != null)
            {
                if (target is GoldMiner_NetworkItem)
                {
                    HandleCollectITem(target.GetComponent<GoldMiner_NetworkItem>());
                }
                else if (target is GoldMiner_DeliverItem)
                {
                    HandleDeliveryItem(target.GetComponent<GoldMiner_DeliverItem>());
                }
            }
        }
        _items.Process(this, (ref ItemState item, int tick) =>
        {
            if (_currentOrigin == null) return false;
            _currentOrigin.transform.position = hookMovement.HookPosition;
            if (itemAttached == false)
            {
                item.EndTick = Runner.Tick;
                _currentOrigin?.gameObject?.SetActive(false);
                _currentOrigin = null;
                return true;
            }
            return false;
        });
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsMine) return;
        if (collision == null) return;
        if (collision.TryGetComponent<GoldMiner_NetworkItem>(out GoldMiner_NetworkItem targ))
        {
            HandleCollectITem(targ);
        }
        else if (collision.TryGetComponent<GoldMiner_DeliverItem>(out GoldMiner_DeliverItem del))
        {
            HandleDeliveryItem(del);
        }
    }
    void HandleCollectITem(GoldMiner_NetworkItem target)
    {
        if (itemAttached) return;

        Rpc_HandleCollectItem(target);
    }
    [Rpc (RpcSources.All, RpcTargets.All)]
    private void Rpc_HandleCollectItem(GoldMiner_NetworkItem target)
    {
        _currentOrigin = target;
        //originItem = _currentOrigin;
        //_items = new SparseCollection<ItemState, GoldMiner_NetworkItem>(_itemStates, originItem);

        //float timeDelayOffset = 1f;
        //float distance = Vector3.Distance(target.transform.position, _hookStartPosition);
        //float timeToMove = CalculateTimeToMove(distance, hookMovement.MoveSpeed) + timeDelayOffset;
        //ItemState itemState = new ItemState(target.transform.position, (_hookStartPosition - target.transform.position).normalized);
        //_items.Add(Runner, itemState, timeToMove);
        //target.gameObject.SetActive(false);
        //var fakeItem = Runner.Spawn(originItem, target.transform.position, target.transform.rotation,Runner.LocalPlayer);
        //CopyTarget(target, fakeItem);
        _currentOrigin.transform.SetParent(_playerReference.GetComponentInChildren<HookScripts>().itemHolder);
        _currentOrigin.OnPickUp(hookMovement, _playerReference.PlayerId);
        //Runner.Despawn(target.Object);
        //target?.gameObject?.SetActive(false);

        itemAttached = true;
        _hasCollectedItem = false;
        _hasGetItem = true;

        // animate hook
        hookMovement.HookAttachedItem();
        // animate player
        playerAnim.PullingItemAnimation();

        if (target.tag == Tags.SMALL_GOLD || target.tag == Tags.MIDDLE_GOLD ||
            target.tag == Tags.LARGE_GOLD)
        {
            SoundManager.instance.Gold();
        }
        else if (target.tag == Tags.MIDDLE_STONE || target.tag == Tags.LARGE_STONE)
        {
            SoundManager.instance.Stone();
        }
        SoundManager.instance.PullSound(true);
    }

    private void HandleDeliveryItem(GoldMiner_DeliverItem delivery)
    {
        if (!itemAttached) return;
        Rpc_HandleDeliverItem(this);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_HandleDeliverItem(HookScripts hook)
    {
        if (hook == null) return;
        itemAttached = false;
        _hasGetItem = false;
        _hasCollectedItem = true;
        playerAnim.IdleAnimation();
        SoundManager.instance.PullSound(false);

        //Deliver item
        hook.CurrentOrigin.OnCollected();
        hook.CurrentOrigin?.gameObject?.SetActive(false);
        Debug.Log("HOOK SCRIPT: Delivery new collected item");
    }

    float CalculateTimeToMove(float distance, float speed)
    {
        // Avoid division by zero
        if (speed == 0f)
        {
            Debug.LogError("Speed cannot be zero. Please set a valid speed.");
            return Mathf.Infinity;
        }

        return distance / speed;
    }
    private void CopyTarget(GoldMiner_NetworkItem source, GoldMiner_NetworkItem clone)
    {
        ItemDetailType newType = source.ItemType;
        int    newScore  = source.ItemScore;
        clone.ApplyNewAttributes(newType, newScore, true, source.gameObject);
        clone.name = source.name;
    }
} 