using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "Gameplay/ItemConfig")]
public class ItemConfig : ScriptableObject
{
    [SerializeField] private List<ItemInfo> itemInfos = new List<ItemInfo>();

    public List<ItemInfo> ItemInfos => itemInfos;

    public List<GameObject> GetPrefabList(ItemInfo.ItemType itemType)
    {
        // Use LINQ to find the correct ItemInfo based on the provided itemType
        ItemInfo foundItemInfo = itemInfos.Find(itemInfo => itemInfo.itemType == itemType);

        if (foundItemInfo != null)
        {
            // Return the prefab list from the found ItemInfo
            return foundItemInfo.PrefabList;
        }
        else
        {
            Debug.LogError($"{nameof(ItemConfig)}: Prefab list for {itemType} is not found.");
            return new List<GameObject>();
        }
    }
}


