using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_NetworkItem : NetworkBehaviour
{
    [SerializeField] int itemScore = 20;

    public static event Action<int> OnItemCollected;
    public override void Spawned()
    {
        base.Spawned();
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }
    private void OnDisable()
    {
        OnItemCollected?.Invoke(itemScore);
    }
}
