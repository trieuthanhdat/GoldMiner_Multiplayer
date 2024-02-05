using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_DeliverItem : NetworkBehaviour
{
    public void DeliverItem(GoldMiner_NetworkItem item)
    {
        Runner.Despawn(item.Object);
    }
}
