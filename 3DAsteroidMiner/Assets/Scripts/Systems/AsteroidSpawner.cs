using System.Collections.Generic;
using UnityEngine;
using AsteroidMiner.Data;
using AsteroidMiner.Core;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Procedural asteroid field spawner matching the original web game's mechanics.
    /// - Spawns asteroids based on sector difficulty
    /// - Uses weighted random selection for 16 asteroid types
    /// - Implements spatial distribution to avoid clustering
    /// - Supports dynamic spawning/despawning based on player proximity
    /// </summary>
    public class AsteroidSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AsteroidPool asteroidPool;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameState gameState;
        
        [Header("Asteroid Types (16 Total)")]
        [Tooltip("Assign all 16 asteroid types in order of rarity")]
        [SerializeField] private List<AsteroidType> asteroidTypes = new List<AsteroidType>();
        
        [Header("Spawn Configuration")]
        [SerializeField] private float spawnCheckInterval = 0.1f; // Check 10x per second
        [SerializeField] private float minDistanceFromPlayer = 100f;
        [SerializeField] private float maxDistanceFromPlayer = 500f;
        [SerializeField] private float despawnDistance = 700f;
        [SerializeField] private float minDistanceBetweenAsteroids = 35f; // Space-scale distances
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // ===== Cached Data =====
        private List<GameObject> spawnedAsteroids = new List<GameObject>();
        private float spawnTimer = 0f;
        private int currentSector = 1;
        private float worldSize;
        private int maxAsteroids;
        private float spawnChancePerCheck;
        
        // ===== Weighted Spawn Tables =====
        private Dictionary<AsteroidRarity, List<AsteroidType>> asteroidsByRarity;
        private Dictionary<AsteroidRarity, float> rarityWeights = new Dictionary<AsteroidRarity, float>()
        {
            { AsteroidRarity.Common, 0.60f },      // 60%
            { AsteroidRarity.Uncommon, 0.20f },    // 20%
            { AsteroidRarity.Rare, 0.12f },        // 12%
            { AsteroidRarity.Epic, 0.06f },        // 6%
            { AsteroidRarity.Legendary, 0.02f }    // 2%
        };
        
        // ===== Lifecycle =====
        
        private void Awake()
        {
            // Organize asteroids by rarity for efficient weighted selection
            OrganizeAsteroidsByRarity();
        }
        
        private void Start()
        {
            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log($"AsteroidSpawner: Found player at {playerTransform.position}");
                }
                else
                {
                    Debug.LogWarning("AsteroidSpawner: No player found with 'Player' tag! Asteroids will spawn at world origin.");
                }
            }
            
            // Find asteroid pool if not assigned
            if (asteroidPool == null)
            {
                asteroidPool = GetComponent<AsteroidPool>();
                if (asteroidPool == null)
                {
                    Debug.LogError("AsteroidSpawner: No AsteroidPool component found!");
                }
            }
            
            UpdateSectorParameters();
            
            // Spawn initial asteroids to populate the field
            int initialSpawnCount = Mathf.Min(50, maxAsteroids / 3); // Spawn 1/3 of max or 50, whichever is less
            Debug.Log($"AsteroidSpawner: Spawning {initialSpawnCount} initial asteroids");
            ForceSpawnAsteroids(initialSpawnCount);
        }
        
        private void Update()
        {
            // Update sector parameters if changed
            if (gameState != null && gameState.sector != currentSector)
            {
                UpdateSectorParameters();
            }
            
            // Spawn asteroids at intervals
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnCheckInterval)
            {
                spawnTimer = 0f;
                TrySpawnAsteroid();
            }
            
            // Despawn distant asteroids
            DespawnDistantAsteroids();
        }
        
        // ===== Spawning Logic =====
        
        /// <summary>
        /// Update spawn parameters based on current sector.
        /// Formula from original game:
        /// - Max asteroids = 150 + (sector - 1) * 50
        /// - World size = 3000 + (sector - 1) * 250
        /// - Spawn chance = 0.05 * (1 + sector * 0.1)
        /// </summary>
        private void UpdateSectorParameters()
        {
            currentSector = gameState != null ? gameState.sector : 1;
            
            // Original game formulas
            maxAsteroids = 150 + (currentSector - 1) * 50;
            worldSize = 3000f + (currentSector - 1) * 250f;
            
            // Base spawn chance adjusted for frame-based checks
            float baseSpawnChance = 0.05f * (1f + currentSector * 0.1f);
            spawnChancePerCheck = baseSpawnChance * spawnCheckInterval;
            
#if UNITY_EDITOR
            if (showDebugInfo)
            {
                Debug.Log($"Sector {currentSector}: Max Asteroids={maxAsteroids}, World Size={worldSize}, Spawn Chance={spawnChancePerCheck:F4}");
            }
#endif
        }
        
        /// <summary>
        /// Attempt to spawn a new asteroid if conditions are met.
        /// </summary>
        private void TrySpawnAsteroid()
        {
            // Check if at max capacity
            if (spawnedAsteroids.Count >= maxAsteroids)
            {
#if UNITY_EDITOR
                if (showDebugInfo && Time.frameCount % 300 == 0) // Log every 5 seconds at 60fps
                {
                    Debug.Log($"At max asteroid capacity: {spawnedAsteroids.Count}/{maxAsteroids}");
                }
#endif
                return;
            }
            
            // Random spawn chance
            float roll = Random.value;
            if (roll > spawnChancePerCheck)
            {
#if UNITY_EDITOR
                if (showDebugInfo && Time.frameCount % 600 == 0) // Log occasionally
                {
                    Debug.Log($"Spawn chance failed: {roll:F4} > {spawnChancePerCheck:F4}");
                }
#endif
                return;
            }
            
            // Generate spawn position
            Vector3 spawnPosition = GenerateSpawnPosition();
            
            // Check minimum distance from other asteroids
            if (!IsValidSpawnPosition(spawnPosition))
            {
#if UNITY_EDITOR
                if (showDebugInfo && spawnedAsteroids.Count < 100)
                {
                    Debug.Log($"Spawn position {spawnPosition} rejected - too close to existing asteroids");
                }
#endif
                return;
            }
            
            // Select asteroid type using weighted random
            AsteroidType selectedType = SelectRandomAsteroidType();
            if (selectedType == null)
            {
                Debug.LogError("Failed to select asteroid type!");
                return;
            }
            
            // Spawn from pool
            GameObject asteroid = asteroidPool.GetAsteroid(selectedType, spawnPosition);
            if (asteroid != null)
            {
                spawnedAsteroids.Add(asteroid);
                
#if UNITY_EDITOR
                if (showDebugInfo)
                {
                    Debug.Log($"Successfully spawned asteroid! Total: {spawnedAsteroids.Count}/{maxAsteroids} - Type: {selectedType.resourceName}");
                }
#endif
            }
        }
        
        /// <summary>
        /// Generate a random spawn position around the player within valid range.
        /// Uses true 3D spherical distribution to utilize full space.
        /// </summary>
        private Vector3 GenerateSpawnPosition()
        {
            Vector3 centerPoint = playerTransform != null ? playerTransform.position : Vector3.zero;
            
            // Use true spherical coordinates for full 3D distribution
            float theta = Random.Range(0f, Mathf.PI * 2f); // Horizontal angle (0-360 degrees)
            
            // For uniform distribution on sphere surface, use acos of random value
            // This ensures equal probability across all vertical angles
            float u = Random.Range(-1f, 1f);
            float phi = Mathf.Acos(u); // Vertical angle (0-180 degrees)
            
            // Random distance within range (use square root for uniform distribution in volume)
            float minDistSq = minDistanceFromPlayer * minDistanceFromPlayer;
            float maxDistSq = maxDistanceFromPlayer * maxDistanceFromPlayer;
            float distSq = Random.Range(minDistSq, maxDistSq);
            float distance = Mathf.Sqrt(distSq);
            
            // Convert spherical to Cartesian coordinates
            float sinPhi = Mathf.Sin(phi);
            float cosPhi = Mathf.Cos(phi);
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            
            Vector3 direction = new Vector3(
                sinPhi * cosTheta,  // X
                cosPhi,             // Y (vertical)
                sinPhi * sinTheta   // Z
            );
            
            Vector3 spawnPosition = centerPoint + direction * distance;
            
#if UNITY_EDITOR
            if (showDebugInfo && spawnedAsteroids.Count < 5)
            {
                Debug.Log($"Spawning asteroid #{spawnedAsteroids.Count + 1} at {spawnPosition} (distance: {distance:F1}m, vertical angle: {phi * Mathf.Rad2Deg:F1}°)");
            }
#endif
            
            return spawnPosition;
        }
        
        /// <summary>
        /// Check if spawn position is valid (not too close to other asteroids).
        /// Uses squared distance for performance.
        /// </summary>
        private bool IsValidSpawnPosition(Vector3 position)
        {
            float minDistSq = minDistanceBetweenAsteroids * minDistanceBetweenAsteroids;
            
            foreach (GameObject asteroid in spawnedAsteroids)
            {
                if (asteroid == null || !asteroid.activeInHierarchy)
                    continue;
                
                float distSq = (asteroid.transform.position - position).sqrMagnitude;
                if (distSq < minDistSq)
                {
#if UNITY_EDITOR
                    if (showDebugInfo && spawnedAsteroids.Count < 10)
                    {
                        Debug.Log($"Position {position} too close to existing asteroid (distance: {Mathf.Sqrt(distSq):F1}m < {minDistanceBetweenAsteroids}m)");
                    }
#endif
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Select a random asteroid type using weighted rarity system.
        /// </summary>
        private AsteroidType SelectRandomAsteroidType()
        {
            // Step 1: Select rarity tier
            float rarityRoll = Random.value;
            float cumulativeWeight = 0f;
            AsteroidRarity selectedRarity = AsteroidRarity.Common;
            
            foreach (var kvp in rarityWeights)
            {
                cumulativeWeight += kvp.Value;
                if (rarityRoll <= cumulativeWeight)
                {
                    selectedRarity = kvp.Key;
                    break;
                }
            }
            
            // Step 2: Select specific asteroid within rarity tier
            if (!asteroidsByRarity.ContainsKey(selectedRarity))
            {
                Debug.LogWarning($"No asteroids found for rarity {selectedRarity}");
                return null;
            }
            
            List<AsteroidType> possibleAsteroids = asteroidsByRarity[selectedRarity];
            if (possibleAsteroids.Count == 0)
                return null;
            
            // Weighted selection within tier
            float totalWeight = 0f;
            foreach (AsteroidType type in possibleAsteroids)
                totalWeight += type.spawnChance;
            
            float typeRoll = Random.value * totalWeight;
            float currentWeight = 0f;
            
            foreach (AsteroidType type in possibleAsteroids)
            {
                currentWeight += type.spawnChance;
                if (typeRoll <= currentWeight)
                    return type;
            }
            
            // Fallback to first in list
            return possibleAsteroids[0];
        }
        
        /// <summary>
        /// Despawn asteroids that are too far from the player.
        /// </summary>
        private void DespawnDistantAsteroids()
        {
            if (playerTransform == null)
                return;
            
            float despawnDistSq = despawnDistance * despawnDistance;
            int despawnedCount = 0;
            
            for (int i = spawnedAsteroids.Count - 1; i >= 0; i--)
            {
                GameObject asteroid = spawnedAsteroids[i];
                
                if (asteroid == null || !asteroid.activeInHierarchy)
                {
                    spawnedAsteroids.RemoveAt(i);
                    continue;
                }
                
                float distSq = (asteroid.transform.position - playerTransform.position).sqrMagnitude;
                if (distSq > despawnDistSq)
                {
                    asteroidPool.ReturnAsteroid(asteroid);
                    spawnedAsteroids.RemoveAt(i);
                    despawnedCount++;
                }
            }
            
#if UNITY_EDITOR
            if (showDebugInfo && despawnedCount > 0)
            {
                Debug.Log($"Despawned {despawnedCount} distant asteroids. Remaining: {spawnedAsteroids.Count}/{maxAsteroids}");
            }
#endif
        }
        
        /// <summary>
        /// Organize asteroid types into dictionary by rarity for efficient lookup.
        /// </summary>
        private void OrganizeAsteroidsByRarity()
        {
            asteroidsByRarity = new Dictionary<AsteroidRarity, List<AsteroidType>>();
            
            foreach (AsteroidRarity rarity in System.Enum.GetValues(typeof(AsteroidRarity)))
            {
                asteroidsByRarity[rarity] = new List<AsteroidType>();
            }
            
            foreach (AsteroidType type in asteroidTypes)
            {
                if (type != null)
                {
                    asteroidsByRarity[type.rarity].Add(type);
                }
            }
        }
        
        // ===== Public Methods =====
        
        /// <summary>
        /// Clear all spawned asteroids (useful for sector transitions).
        /// </summary>
        public void ClearAllAsteroids()
        {
            foreach (GameObject asteroid in spawnedAsteroids)
            {
                if (asteroid != null)
                    asteroidPool.ReturnAsteroid(asteroid);
            }
            
            spawnedAsteroids.Clear();
        }
        
        /// <summary>
        /// Force spawn a specific number of asteroids immediately.
        /// Useful for initial sector population.
        /// Uses grid-based distribution for initial spawn to ensure good spacing.
        /// </summary>
        public void ForceSpawnAsteroids(int count)
        {
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = count * 10; // Allow up to 10 attempts per asteroid
            
            // For initial spawn, use a more distributed approach
            // Divide the spawn area into angular segments to ensure coverage
            int segmentCount = Mathf.Min(count, 24); // Up to 24 angular segments (15 degrees each)
            float anglePerSegment = 360f / segmentCount;
            int asteroidsPerSegment = Mathf.CeilToInt((float)count / segmentCount);
            
            for (int segment = 0; segment < segmentCount && spawned < count; segment++)
            {
                for (int i = 0; i < asteroidsPerSegment && spawned < count && attempts < maxAttempts; i++)
                {
                    attempts++;
                    
                    // Generate position in this angular segment for better distribution
                    Vector3 spawnPosition = GenerateDistributedSpawnPosition(segment, segmentCount);
                    
                    // Check if position is valid (not too close to other asteroids)
                    if (spawnedAsteroids.Count > 0 && !IsValidSpawnPosition(spawnPosition))
                        continue;
                    
                    AsteroidType selectedType = SelectRandomAsteroidType();
                    
                    if (selectedType != null)
                    {
                        GameObject asteroid = asteroidPool.GetAsteroid(selectedType, spawnPosition);
                        if (asteroid != null)
                        {
                            spawnedAsteroids.Add(asteroid);
                            spawned++;
                        }
                    }
                }
            }
            
            Debug.Log($"Force spawned {spawned}/{count} asteroids in {attempts} attempts (success rate: {(float)spawned/attempts*100:F1}%)");
        }
        
        /// <summary>
        /// Generate a spawn position distributed across angular segments.
        /// Ensures asteroids use the full 3D space around the player.
        /// </summary>
        private Vector3 GenerateDistributedSpawnPosition(int segment, int totalSegments)
        {
            Vector3 centerPoint = playerTransform != null ? playerTransform.position : Vector3.zero;
            
            // Calculate base horizontal angle for this segment with some randomness
            float baseAngle = (segment * 360f / totalSegments) * Mathf.Deg2Rad;
            float angleVariation = (360f / totalSegments * 0.8f) * Mathf.Deg2Rad; // 80% of segment width for variation
            float theta = baseAngle + Random.Range(-angleVariation / 2f, angleVariation / 2f);
            
            // Full vertical angle distribution (0-180 degrees from top to bottom)
            // Use uniform distribution on sphere surface
            float u = Random.Range(-1f, 1f);
            float phi = Mathf.Acos(u);
            
            // Random distance with bias toward filling the space evenly
            float minDistSq = minDistanceFromPlayer * minDistanceFromPlayer;
            float maxDistSq = maxDistanceFromPlayer * maxDistanceFromPlayer;
            float distSq = Random.Range(minDistSq, maxDistSq);
            float distance = Mathf.Sqrt(distSq);
            
            // Convert to Cartesian coordinates (proper spherical conversion)
            float sinPhi = Mathf.Sin(phi);
            float cosPhi = Mathf.Cos(phi);
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            
            Vector3 direction = new Vector3(
                sinPhi * cosTheta,  // X
                cosPhi,             // Y (vertical)
                sinPhi * sinTheta   // Z
            );
            
            return centerPoint + direction * distance;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo || playerTransform == null)
                return;
            
            // Show spawn range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxDistanceFromPlayer);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, despawnDistance);
        }
#endif
    }
}
