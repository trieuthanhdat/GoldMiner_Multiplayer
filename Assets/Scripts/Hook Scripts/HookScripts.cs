using Fusion;
using FusionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HookScripts : NetworkBehaviour
{
    #region _____ REFERENCES _____
    [SerializeField] private Transform itemHolder;
    [SerializeField] private PlayerAnimation playerAnim;
    [SerializeField] private HookMovement hookMovement;
    [SerializeField] private float collisionRadius = 0.19f;
    [SerializeField] private GoldMiner_NetworkItem _originItem;
    #endregion
    [Networked, Capacity(50)] private NetworkArray<ItemState> _itemStates => default;
    protected SparseCollection<ItemState, GoldMiner_NetworkItem> _items;
    public bool IsMine => Object.HasInputAuthority;
    private bool itemAttached;
    public override void Spawned()
    {
        if (hookMovement == null) hookMovement = GetComponentInParent<HookMovement>();
        if (playerAnim == null)   playerAnim   = GetComponentInParent<PlayerAnimation>();
        _items = new SparseCollection<ItemState, GoldMiner_NetworkItem>(_itemStates, _originItem);
        base.Spawned();
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
    private void CheckCollision()
    {
        if (!(Object.HasStateAuthority || Object.HasInputAuthority))
            return;
        if (_items == null) return;
        _items.Process(this, (ref ItemState item, int tick) =>
        {
            if (tick >= item.EndTick)
            {
                item.EndTick = Runner.Tick;
                playerAnim.IdleAnimation();
                itemAttached = false;
                SoundManager.instance.PullSound(false);
                _originItem.OnCollected();
                return true;
            }
            return false;
        });
    }
    
    void OnTriggerEnter2D(Collider2D target)
    {

        if (target.TryGetComponent<GoldMiner_NetworkItem>(out GoldMiner_NetworkItem item))
        {
            _originItem.OnPickUp(hookMovement);
            CopyTarget(item, ref _originItem);
            target.gameObject.SetActive(false);

            float distance = Vector3.Distance(itemHolder.position, item.transform.position);
            float timeToMove = CalculateTimeToMove(distance, hookMovement.MoveSpeed);

            ItemState itemState = new ItemState(item.transform.position, (itemHolder.position - item.transform.position).normalized);
            _items.Add(Runner, itemState, timeToMove);

            itemAttached = true;
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

        } // if target is an item
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
    private void CopyTarget(GoldMiner_NetworkItem source, ref GoldMiner_NetworkItem clone)
    {
        Sprite newSprite = source.CurrentItemSprite;
        int    newScore  = source.CurrentItemScore;
        clone.ApplyNewAttributes(newSprite, newScore);
    }
} 




















