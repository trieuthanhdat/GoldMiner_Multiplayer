using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundaries : MonoBehaviour
{
    [SerializeField] private int _levelBoundaryX = 0;
    [SerializeField] private int _levelBoundaryY = 0;

    public int LevelBoundaryX => _levelBoundaryX;
    public int LevelBoundaryY => _levelBoundaryY;

    [Range(0.0f, 10.0f)][SerializeField] private float _tolerance = 0.0f;

    [SerializeField] private Camera _levelCamera = null;

    // Utility Method for the LevelBoundariesEditor script
    // Sets the level boundaries based on the screen viewport of the orthographic camera
    public void SetBoundariesBasedOnCamera()
    {
        if (_levelCamera == null)
        {
            Debug.LogError("No Camera assigned");
            return;
        }

        _levelBoundaryX = Mathf.CeilToInt(_levelCamera.orthographicSize * _levelCamera.aspect);
        _levelBoundaryY = Mathf.CeilToInt(_levelCamera.orthographicSize);
    }

    // Utility Method for the LevelBoundariesEditor script
    // Sets the level boundaries based on the Bounds handle
    public void SetBoundariesBasedOnBounds(Bounds bounds)
    {
        _levelBoundaryX = Mathf.CeilToInt(bounds.size.x);
        _levelBoundaryY = Mathf.CeilToInt(bounds.size.y);
    }

    public bool IsInsideBoundaries(Vector2 position)
    {
        if (Mathf.Abs(position.x) > _levelBoundaryX + _tolerance) return false;
        if (Mathf.Abs(position.y) > _levelBoundaryY + _tolerance) return false;

        return true;
    }

    // Makes a position which exceeds the level boundaries loop back into view from the opposite side
    public Vector2 LoopPosition(Vector3 position)
    {
        if (Mathf.Abs(position.x) > _levelBoundaryX)
        {
            position = new Vector2(-Mathf.Sign(position.x) * _levelBoundaryX, position.y);
        }

        if (Mathf.Abs(position.y) > _levelBoundaryY)
        {
            position = new Vector2(position.x, -Mathf.Sign(position.y) * _levelBoundaryY);
        }

        return position;
    }
}