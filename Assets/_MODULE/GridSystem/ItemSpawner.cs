using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private LevelConfig levelConfig;
    [SerializeField] private List<ItemConfig> itemConfigs;
    [SerializeField] private Transform itemParent;

    private void OnEnable()
    {
        GridLevelSystem.OnCompleteBuildGrid += GridLevelSystem_OnCompleteGridCreation;
    }
    private void OnDisable()
    {
        GridLevelSystem.OnCompleteBuildGrid -= GridLevelSystem_OnCompleteGridCreation;
    }
    private void GridLevelSystem_OnCompleteGridCreation()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        if (levelConfig == null)
        {
            Debug.LogError($"{nameof(ItemSpawner)}: LevelConfig is not assigned.");
            return;
        }

        int totalAmount = levelConfig.totalAmount;

        foreach (ItemConfig itemConfig in itemConfigs)
        {
            if (itemConfig == null)
            {
                Debug.LogError($"{nameof(ItemSpawner)}: ItemConfig is not assigned.");
                continue;
            }

            foreach (ItemInfo itemInfo in itemConfig.ItemInfos)
            {
                int itemCount = Mathf.RoundToInt(totalAmount * GetItemPercentage(itemInfo.itemType));
                for (int i = 0; i < itemCount; i++)
                {
                    SpawnItem(itemInfo);
                }
            }
        }
    }

    void SpawnItem(ItemInfo itemInfo)
    {
        List<GameObject> prefabList = itemInfo.PrefabList;

        if (prefabList.Count == 0)
        {
            Debug.LogError($"{nameof(ItemSpawner)}: Prefab list for {itemInfo.itemType} is empty. Cannot spawn item.");
            return;
        }

        // Randomly choose a prefab from the list
        GameObject chosenPrefab = prefabList[Random.Range(0, prefabList.Count)];

        // Calculate random position using the static reference to GridLevelSystem
        Vector3 randomPosition = GridLevelSystem.Instance.GetRandomGridPosition();

        // Ensure that the chosen position is valid
        var item = Instantiate(chosenPrefab, randomPosition, Quaternion.identity);
        item.gameObject.SetActive(true);

        GridLevelSystem.Instance.RemovePosition(randomPosition);
        Debug.Log($"{nameof(ItemSpawner)}: Spawned {itemInfo.itemName} item at position {randomPosition}");
    }




    float GetItemPercentage(ItemInfo.ItemType itemType)
    {
        if (levelConfig == null)
        {
            Debug.LogError($"{nameof(ItemSpawner)}: LevelConfig is not assigned.");
            return 0f;
        }

        // Implement logic to get the percentage based on the ItemType
        // For example, you might have different percentages for each type
        switch (itemType)
        {
            case ItemInfo.ItemType.Gold:
                return levelConfig.GoldPercentage;
            case ItemInfo.ItemType.Diamond:
                return levelConfig.DiamondPercentage;
            case ItemInfo.ItemType.Rock:
                return levelConfig.RockPercentage;
            default:
                return 0f;
        }
    }
}
