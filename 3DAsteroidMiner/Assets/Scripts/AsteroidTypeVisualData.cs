using UnityEngine;
using AsteroidMiner.Data;

/// <summary>
/// ScriptableObject that defines the visual properties for each asteroid type.
/// Create instances via: Assets > Create > Game > Asteroid Type Visual Data
/// </summary>
[CreateAssetMenu(fileName = "AsteroidTypeVisual_", menuName = "Game/Asteroid Type Visual Data", order = 1)]
public class AsteroidTypeVisualData : ScriptableObject
{
    [Header("Asteroid Type Info")]
    [Tooltip("Name of the asteroid type (e.g., Iron Ore, Gold, Quantum Crystal)")]
    public string typeName;
    
    [Tooltip("Resource type from the original game")]
    public string resourceName;
    
    [Header("Color Configuration")]
    [Tooltip("The color revealed in the holes (represents the asteroid's mineral type)")]
    public Color typeColor = new Color(1f, 0.5f, 0f, 1f);
    
    [Range(0f, 3f)]
    [Tooltip("Brightness multiplier for the type color")]
    public float typeColorIntensity = 1.5f;
    
    [Range(0f, 2f)]
    [Tooltip("Emission/glow intensity in the holes")]
    public float typeColorEmission = 0.3f;
    
    [Header("Rarity Settings")]
    [Tooltip("Rarity tier: Common, Uncommon, Rare, Epic, Legendary")]
    public AsteroidRarity rarity = AsteroidRarity.Common;
    
    [Tooltip("Value multiplier (affects visual fanciness)")]
    public int baseValue = 2;
    
    /// <summary>
    /// Creates a preset for a specific asteroid type.
    /// </summary>
    public static AsteroidTypeVisualData CreatePreset(string name, Color typeCol, AsteroidRarity rare, int value)
    {
        var data = CreateInstance<AsteroidTypeVisualData>();
        data.typeName = name;
        data.resourceName = name;
        data.typeColor = typeCol;
        data.rarity = rare;
        data.baseValue = value;
        
        // Adjust color intensity and emission based on rarity
        // (Hole configuration comes from the material, not per-type)
        switch (rare)
        {
            case AsteroidRarity.Common:
                data.typeColorIntensity = 1.0f;
                data.typeColorEmission = 0.1f;
                break;
                
            case AsteroidRarity.Uncommon:
                data.typeColorIntensity = 1.3f;
                data.typeColorEmission = 0.2f;
                break;
                
            case AsteroidRarity.Rare:
                data.typeColorIntensity = 1.6f;
                data.typeColorEmission = 0.35f;
                break;
                
            case AsteroidRarity.Epic:
                data.typeColorIntensity = 2.0f;
                data.typeColorEmission = 0.5f;
                break;
                
            case AsteroidRarity.Legendary:
                data.typeColorIntensity = 2.5f;
                data.typeColorEmission = 0.8f;
                break;
        }
        
        return data;
    }
}
