using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GoldMiner_NetworkItem : NetworkBehaviour
{
    [SerializeField] int itemScore = 20;

    public static event Action<int> OnItemCollected;
    private bool _hasSetup = false;
    public override void Spawned()
    {
        base.Spawned();
        _hasSetup = true;
    }
    
    private void OnDisable()
    {
        if (!_hasSetup) return;
        OnItemCollected?.Invoke(itemScore);
        Debug.Log($"{nameof(GoldMiner_NetworkItem).ToUpper()}: on item collected: {name} - {itemScore}");
    }
}
