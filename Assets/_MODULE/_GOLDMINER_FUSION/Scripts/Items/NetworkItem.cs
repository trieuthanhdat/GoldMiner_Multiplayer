using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkItem: NetworkBehaviour
{
    public override void Spawned()
    {
        Debug.Log("GOLEMINER_NETWORKITEM: on spawned");
        base.Spawned();
    }
}
