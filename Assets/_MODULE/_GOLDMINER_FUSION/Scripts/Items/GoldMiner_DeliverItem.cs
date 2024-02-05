using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMiner_DeliverItem : NetworkBehaviour
{
    public void DeliverItem(GoldMiner_NetworkItem item, bool recycle = false)
    {
        if (recycle)
        {
            item.OnCollected();
            return;
        }
        Runner.Despawn(item.Object);
    }
}
