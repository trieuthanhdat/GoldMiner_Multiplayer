using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridLevelSystem : MonoSingleton<GridLevelSystem>
{
    [SerializeField] private LevelConfig levelConfig;
    public List<Vector3> listGridPosition = new List<Vector3>();
    public List<Vector3> listPosition = new List<Vector3>();
    public static event Action OnCompleteBuildGrid;

    void Start()
    {
        CreateGridPositions();
        CreateSpawnPosition();
    }
    private void CreateGridPositions()
    {
        listGridPosition.Clear();
        Vector3 gridPosition = transform.position;
        Vector3 gridScale = transform.localScale;

        float halfCellSizeX = 0.5f * levelConfig.cellSize * gridScale.x;
        float halfCellSizeY = 0.5f * levelConfig.cellSize * gridScale.y;

        for (int x = 0; x < levelConfig.gridSizeX - 1; x++)
        {
            for (int y = 0; y < levelConfig.gridSizeY - 1; y++)
            {
                Vector3 position = new Vector3(
                    x * levelConfig.cellSize * gridScale.x + gridPosition.x,
                    y * levelConfig.cellSize  * gridScale.y + gridPosition.y,
                    0f
                );

                Debug.Log($"{nameof(GridLevelSystem)}: add position " + position);
                listGridPosition.Add(position);
            }
        }
    }
    private void CreateSpawnPosition()
    {
        listPosition.Clear();
        Vector3 gridPosition = transform.position;
        Vector3 gridScale = transform.localScale;

        float halfCellSizeX = 0.5f * levelConfig.cellSize * gridScale.x;
        float halfCellSizeY = 0.5f * levelConfig.cellSize * gridScale.y;

        for (int x = 0; x < levelConfig.gridSizeX - 1; x++)
        {
            for (int y = 0; y < levelConfig.gridSizeY - 1; y++)
            {
                Vector3 position = new Vector3(
                    x * levelConfig.cellSize * gridScale.x + halfCellSizeX + gridPosition.x,
                    y * levelConfig.cellSize * gridScale.y + halfCellSizeY + gridPosition.y,
                    0f
                );

                Debug.Log($"{nameof(GridLevelSystem)}: add spawn position " + position);
                listPosition.Add(position);
            }
        }
        OnCompleteBuildGrid?.Invoke();
    }
    public void OnDrawGizmos()
    {
        SetupPositionAndCreateGrid();
    }

    private void SetupPositionAndCreateGrid()
    {
        Gizmos.color = Color.white;

        Vector3 gridPosition = transform.position;
        Vector3 gridScale = transform.localScale;

        for (int x = 0; x <= levelConfig.gridSizeX; x++)
        {
            Gizmos.DrawLine(
                new Vector3(x * levelConfig.cellSize * gridScale.x + gridPosition.x, gridPosition.y, gridPosition.z),
                new Vector3(x * levelConfig.cellSize * gridScale.x + gridPosition.x, levelConfig.gridSizeY * levelConfig.cellSize * gridScale.y + gridPosition.y, gridPosition.z)
            );
        }

        for (int y = 0; y <= levelConfig.gridSizeY; y++)
        {
            Gizmos.DrawLine(
                new Vector3(gridPosition.x, y * levelConfig.cellSize * gridScale.y + gridPosition.y, gridPosition.z),
                new Vector3(levelConfig.gridSizeX * levelConfig.cellSize * gridScale.x + gridPosition.x, y * levelConfig.cellSize * gridScale.y + gridPosition.y, gridPosition.z)
            );
        }
    }
    public void RemovePosition(Vector3 pos)
    {
        if (listPosition.Count == 0)
        {
            Debug.LogError($"{nameof(GridLevelSystem)}: no position to remove");
            return ;
        }
        listPosition.Remove(pos);
    }
    public Vector3 GetRandomGridPosition()
    {
        if (listPosition.Count == 0)
        {
            Debug.LogError($"{nameof(GridLevelSystem)}: no position to use");
            return Vector3.zero;
        }
        return listPosition[UnityEngine.Random.Range(0, listPosition.Count)];
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"{nameof(GridLevelSystem)}: OnPlayerJoined on completeBuildGrid invoke");
    }
}
