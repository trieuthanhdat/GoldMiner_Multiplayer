using Fusion;
using FusionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SocialPlatforms.Impl;

public class GoldMiner_NetworkItem : NetworkBehaviour, ISparseVisual<ItemState, GoldMiner_NetworkItem>
{
    [SerializeField] int itemScore = 20;
    [SerializeField] SpriteRenderer spriteRenderer;
    #region _____ GROUP ITEM'S ATTRIBUTES _____
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }
    private int    _currentItemScore = 20;
    private Sprite _currentItemSprite;
    public  int   CurrentItemScore  { get => _currentItemScore;  set => _currentItemScore = value ; }
    public Sprite CurrentItemSprite { get => _currentItemSprite; set => _currentItemSprite = value; }
    #endregion
    public static event Action<int> OnItemCollected;

    private bool _hasSetup = false;
    private float _moveSpeed = 3;

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
        base.Spawned();
        if(spriteRenderer) _currentItemSprite = spriteRenderer.sprite;
        _currentItemScore = itemScore;
        _hasSetup = true;
    }
    public void ApplyNewAttributes(Sprite newSprite, int newScore)
    {
        _currentItemSprite = newSprite;
        if(spriteRenderer) spriteRenderer.sprite = _currentItemSprite;
        _currentItemScore = newScore;
    }
    public void OnPickUp(HookMovement hookMovement)
    {
        if (hookMovement) Speed = hookMovement.MoveSpeed;
    }
    public void OnCollected()
    {
        if (!_hasSetup) return;
        OnItemCollected?.Invoke(itemScore);
        Debug.Log($"{nameof(GoldMiner_NetworkItem).ToUpper()}: on item collected: {name} - {itemScore}");
    }
    private void OnDisable()
    {
        OnCollected();
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
