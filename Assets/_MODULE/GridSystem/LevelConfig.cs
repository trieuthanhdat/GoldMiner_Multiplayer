using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Gameplay/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Grid Configuration")]
    public int gridSizeX;
    public int gridSizeY;
    public float cellSize;

    [Header("Item Density")]
    [Range(0, 100)]public int goldDensity;
    [Range(0, 100)]public int diamondDensity;
    [Range(0, 100)]public int rockDensity;

    [Header("Total Amount")]
    public int totalAmount;

    // Getters for percentages, if needed
    public float GoldPercentage => goldDensity / 100f;
    public float DiamondPercentage => diamondDensity / 100f;
    public float RockPercentage => rockDensity / 100f;
}
