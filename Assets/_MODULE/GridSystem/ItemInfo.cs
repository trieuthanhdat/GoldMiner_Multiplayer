using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemInfo
{
    public enum ItemType
    {
        Gold,
        Diamond,
        Rock
    }
    public enum ItemDetailType
    {
        Gold_small,
        Gold_medium,
        Gold_large,
        Diamond_one,
        Diamond_two,
        Rock_small,
        Rock_medium,
        Rock_large,
    }
    public ItemType itemType;
    public string itemName;

    // Add a property to get the prefab list for this item
    public List<GameObject> PrefabList
    {
        get
        {
            return itemPrefab != null ? itemPrefab : new List<GameObject>();
        }
    }

    // Define your prefab lists for each item type
    public List<GameObject> itemPrefab;

}

