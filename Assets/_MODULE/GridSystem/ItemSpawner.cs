using UnityEngine;
using System.Collections.Generic;
using Fusion;
using CoreGame;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private LevelConfig levelConfig;
    [SerializeField] private List<ItemConfig> itemConfigs;
    [SerializeField] private Transform itemParent;
    protected CancellationTokenSource ctsDespawned = new CancellationTokenSource();

    private bool _completeGridBuild = false;
    private bool _otherPlayerJoined = false;
    private bool _hasSpawnedItems   = false;
    public bool IsServer => Runner.IsSinglePlayer || (Runner.IsServer || Runner.IsSharedModeMasterClient);

    private void OnEnable()
    {
        GridLevelSystem.OnCompleteBuildGrid += GridLevelSystem_OnCompleteGridCreation;
    }
    private void FusionLauncher_OnStartMatch()
    {
        _otherPlayerJoined = true;
    }
    private void FusionLauncher_OnOtherPlayerJoined()
    {
        _otherPlayerJoined = true;
    }
    private void GridLevelSystem_OnCompleteGridCreation()
    {
        _completeGridBuild = true;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GridLevelSystem.OnCompleteBuildGrid -= GridLevelSystem_OnCompleteGridCreation;
        FusionLauncher.OnOtherPlayerJoined -= FusionLauncher_OnOtherPlayerJoined;
        if(FusionLauncher.Session) FusionLauncher.Session.OnStartMatch -= FusionLauncher_OnStartMatch;
        base.Despawned(runner, hasState);
    }
    public override void Spawned()
    {
        base.Spawned();
        SpawnedAsync().Forget();
    }
    protected async UniTaskVoid SpawnedAsync()
    {
        await UniTask.WaitUntil(() => FusionLauncher.Session != null && GoldMiner_GameManagerFusion.Instance != null,
                                      cancellationToken: ctsDespawned.Token);
        FusionLauncher.OnOtherPlayerJoined  += FusionLauncher_OnOtherPlayerJoined;
        FusionLauncher.Session.OnStartMatch += FusionLauncher_OnStartMatch;

        await UniTask.WaitUntil(() => _completeGridBuild == true && _otherPlayerJoined == true);
        Debug.Log($"{nameof(ItemSpawner)}: start to spawn items");
        SpawnItems();
    }
    void SpawnItems()
    {
        //Allow Only Server to Spawn Items
        if (!IsServer) return;
        if (_hasSpawnedItems) return;
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
        _hasSpawnedItems = true;
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
        Vector3 randomRotation = new Vector3(chosenPrefab.transform.position.x,
                                             chosenPrefab.transform.position.y,
                                             Random.Range(-360f, 360f));

        // Ensure that the chosen position is valid
        if(chosenPrefab!= null)
        {
            Debug.Log($"{nameof(ItemSpawner)}: start spawning item {chosenPrefab.name}");
            var item = Runner.Spawn(chosenPrefab, randomPosition, Quaternion.Euler(randomRotation));
            item.gameObject.SetActive(true);

            Debug.Log($"{nameof(ItemSpawner)}: spawned item {chosenPrefab.name}");
        }
        //OFFLINE
        /* var item = Instantiate(chosenPrefab, randomPosition, Quaternion.identity);*/

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
