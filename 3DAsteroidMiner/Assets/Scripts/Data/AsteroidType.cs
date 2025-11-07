using UnityEngine;

namespace AsteroidMiner.Data
{
    /// <summary>
    /// ScriptableObject defining asteroid properties (rarity, value, health, visuals).
    /// Based on the 16 asteroid types from the original web game.
    /// </summary>
    [CreateAssetMenu(fileName = "AsteroidType", menuName = "Asteroid Miner/Asteroid Type")]
    public class AsteroidType : ScriptableObject
    {
        [Header("Identification")]
        public string resourceName;
        public AsteroidRarity rarity;
        
        [Header("Gameplay Properties")]
        [Tooltip("Credits awarded per asteroid")]
        public int value;
        
        [Tooltip("Health points (mining hits required)")]
        public float health;
        
        [Tooltip("Spawn weight (higher = more common within rarity tier)")]
        [Range(0f, 1f)]
        public float spawnChance;
        
        [Header("Visual Properties")]
        public Color asteroidColor = Color.gray;
        public Material asteroidMaterial;
        
        [Tooltip("Optional: New visual system data (swiss cheese shader)")]
        public AsteroidTypeVisualData visualData;
        
        [Tooltip("Size variation range")]
        public Vector2 sizeRange = new Vector2(3f, 8f);
        
        [Tooltip("Rotation speed range (degrees/second)")]
        public Vector2 rotationSpeedRange = new Vector2(10f, 30f);
    }
    
    /// <summary>
    /// Asteroid rarity tiers matching the original game's distribution.
    /// </summary>
    public enum AsteroidRarity
    {
        Common,      // 60% total (Iron Ore, Copper)
        Uncommon,    // 20% total (Nickel, Silver, Titanium)
        Rare,        // 12% total (Gold, Emerald, Platinum)
        Epic,        // 6% total (Ruby, Sapphire, Obsidian)
        Legendary    // 2% total (Quantum Crystal, Nebulite, Dark Matter)
    }
}
