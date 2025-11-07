using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example integration of the asteroid visual system with the game's asteroid spawning.
/// This shows how to connect the visual data with your existing asteroid system.
/// </summary>
public class AsteroidSpawnerWithVisuals : MonoBehaviour
{
    [System.Serializable]
    public class AsteroidTypeConfig
    {
        [Header("Game Data")]
        public string typeName;                    // e.g., "Iron Ore", "Gold"
        public int baseValue;                      // Credits earned
        public float health;                       // Health to destroy
        [Range(0f, 1f)] public float spawnChance;  // Spawn probability
        
        [Header("Visual Data")]
        public AsteroidTypeVisualData visualData;  // Shader configuration
    }
    
    [Header("Prefab Setup")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform asteroidContainer;
    
    [Header("Asteroid Types (16 Total)")]
    [SerializeField] private AsteroidTypeConfig[] asteroidTypes;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAsteroids = 100;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(1000, 500, 1000);
    
    private List<GameObject> activeAsteroids = new List<GameObject>();
    private float spawnTimer;
    
    private void Start()
    {
        // Validate configuration
        if (asteroidTypes == null || asteroidTypes.Length == 0)
        {
            Debug.LogError("No asteroid types configured! Please assign visual data assets.");
            return;
        }
        
        if (asteroidPrefab == null)
        {
            Debug.LogError("Asteroid prefab not assigned!");
            return;
        }
        
        // Ensure prefab has required components
        ValidateAsteroidPrefab();
        
        Debug.Log($"Asteroid spawner initialized with {asteroidTypes.Length} types");
    }
    
    private void Update()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval && activeAsteroids.Count < maxAsteroids)
        {
            SpawnRandomAsteroid();
            spawnTimer = 0f;
        }
    }
    
    /// <summary>
    /// Spawns a random asteroid based on spawn chances.
    /// </summary>
    public void SpawnRandomAsteroid()
    {
        // Select asteroid type based on weighted random
        AsteroidTypeConfig selectedType = SelectRandomAsteroidType();
        
        if (selectedType == null)
        {
            Debug.LogWarning("Failed to select asteroid type!");
            return;
        }
        
        SpawnAsteroid(selectedType);
    }
    
    /// <summary>
    /// Spawns a specific asteroid type at a random position.
    /// </summary>
    public GameObject SpawnAsteroid(AsteroidTypeConfig typeConfig)
    {
        // Generate random position
        Vector3 spawnPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        return SpawnAsteroid(typeConfig, spawnPosition, Random.rotation);
    }
    
    /// <summary>
    /// Spawns a specific asteroid type at a specific position.
    /// </summary>
    public GameObject SpawnAsteroid(AsteroidTypeConfig typeConfig, Vector3 position, Quaternion rotation)
    {
        // Instantiate asteroid
        GameObject asteroid = Instantiate(asteroidPrefab, position, rotation, asteroidContainer);
        
        // Configure visual appearance
        AsteroidVisualController visualController = asteroid.GetComponent<AsteroidVisualController>();
        if (visualController != null && typeConfig.visualData != null)
        {
            // Set the type visuals
            visualController.SetAsteroidType(typeConfig.visualData);
            
            // Add slight variation to each asteroid
            visualController.RandomizeHolePattern(0.15f);
        }
        else
        {
            Debug.LogWarning($"AsteroidVisualController or visual data missing for {typeConfig.typeName}");
        }
        
        // Configure game properties (if you have an Asteroid component)
        // Example:
        // Asteroid asteroidComponent = asteroid.GetComponent<Asteroid>();
        // if (asteroidComponent != null)
        // {
        //     asteroidComponent.SetProperties(typeConfig.typeName, typeConfig.baseValue, typeConfig.health);
        // }
        
        // Track active asteroids
        activeAsteroids.Add(asteroid);
        
        // Subscribe to destruction event (if applicable)
        // asteroidComponent.OnDestroyed += () => OnAsteroidDestroyed(asteroid);
        
        return asteroid;
    }
    
    /// <summary>
    /// Spawns a specific asteroid type by name.
    /// </summary>
    public GameObject SpawnAsteroidByName(string typeName)
    {
        var typeConfig = System.Array.Find(asteroidTypes, t => t.typeName == typeName);
        
        if (typeConfig == null)
        {
            Debug.LogError($"Asteroid type '{typeName}' not found!");
            return null;
        }
        
        return SpawnAsteroid(typeConfig);
    }
    
    /// <summary>
    /// Selects a random asteroid type based on weighted spawn chances.
    /// </summary>
    private AsteroidTypeConfig SelectRandomAsteroidType()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var type in asteroidTypes)
        {
            totalWeight += type.spawnChance;
        }
        
        if (totalWeight <= 0f)
        {
            Debug.LogError("Total spawn chance is 0! Check asteroid type configurations.");
            return asteroidTypes[0];
        }
        
        // Random selection
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (var type in asteroidTypes)
        {
            cumulativeWeight += type.spawnChance;
            if (randomValue <= cumulativeWeight)
            {
                return type;
            }
        }
        
        // Fallback (should never reach here)
        return asteroidTypes[asteroidTypes.Length - 1];
    }
    
    /// <summary>
    /// Called when an asteroid is destroyed (cleanup).
    /// </summary>
    private void OnAsteroidDestroyed(GameObject asteroid)
    {
        if (activeAsteroids.Contains(asteroid))
        {
            activeAsteroids.Remove(asteroid);
        }
    }
    
    /// <summary>
    /// Validates that the asteroid prefab has required components.
    /// </summary>
    private void ValidateAsteroidPrefab()
    {
        if (asteroidPrefab.GetComponent<MeshRenderer>() == null)
        {
            Debug.LogWarning("Asteroid prefab missing MeshRenderer component!");
        }
        
        if (asteroidPrefab.GetComponent<AsteroidVisualController>() == null)
        {
            Debug.LogWarning("Asteroid prefab missing AsteroidVisualController component! Adding it now...");
            // Note: This won't persist to the prefab, just for this session
        }
    }
    
    /// <summary>
    /// Gets asteroid type config by name.
    /// </summary>
    public AsteroidTypeConfig GetAsteroidType(string typeName)
    {
        return System.Array.Find(asteroidTypes, t => t.typeName == typeName);
    }
    
    /// <summary>
    /// Clears all spawned asteroids.
    /// </summary>
    public void ClearAllAsteroids()
    {
        foreach (var asteroid in activeAsteroids)
        {
            if (asteroid != null)
            {
                Destroy(asteroid);
            }
        }
        activeAsteroids.Clear();
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor gizmo to visualize spawn area.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
    
    /// <summary>
    /// Context menu option to spawn test asteroids.
    /// </summary>
    [ContextMenu("Spawn Test Asteroids (All Types)")]
    private void SpawnTestAsteroids()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Must be in Play Mode to spawn test asteroids!");
            return;
        }
        
        float radius = 100f;
        int typesCount = asteroidTypes.Length;
        
        for (int i = 0; i < typesCount; i++)
        {
            float angle = (360f / typesCount) * i * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
            
            SpawnAsteroid(asteroidTypes[i], position, Quaternion.identity);
        }
        
        Debug.Log($"Spawned {typesCount} test asteroids in a circle");
    }
#endif
}
