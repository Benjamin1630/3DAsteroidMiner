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
        [SerializeField] private float despawnCheckInterval = 0.5f; // Check despawning 2x per second (performance optimization)
        [SerializeField] private float minDistanceFromPlayer = 100f;
        [SerializeField] private float maxDistanceFromPlayer = 500f;
        [SerializeField] private float despawnDistance = 700f;
        [SerializeField] private float minDistanceBetweenAsteroids = 35f; // Space-scale distances
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // ===== Cached Data =====
        private List<GameObject> spawnedAsteroids = new List<GameObject>();
        private float spawnTimer = 0f;
        private float despawnTimer = 0f; // Separate timer for despawn checks
        private int currentSector = 1;
        private int minAsteroidCount; // Set from pool's initialPoolSize
        private int maxAsteroidCount; // Set from pool's maxPoolSize
        
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
                    return;
                }
            }
            
            // Get min/max asteroid counts from the pool configuration
            minAsteroidCount = asteroidPool.InitialPoolSize;
            maxAsteroidCount = asteroidPool.MaxPoolSize;
            
            UpdateSectorParameters();
            
            // Spawn initial asteroids to populate the field
            Debug.Log($"AsteroidSpawner: Spawning {minAsteroidCount} initial asteroids (min={minAsteroidCount}, max={maxAsteroidCount} from AsteroidPool)");
            ForceSpawnAsteroids(minAsteroidCount);
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
            
            // Despawn distant asteroids less frequently (performance optimization)
            despawnTimer += Time.deltaTime;
            if (despawnTimer >= despawnCheckInterval)
            {
                despawnTimer = 0f;
                DespawnDistantAsteroids();
            }
        }
        
        // ===== Spawning Logic =====
        
        /// <summary>
        /// Update spawn parameters based on current sector.
        /// Kept for compatibility but no longer affects asteroid counts.
        /// </summary>
        private void UpdateSectorParameters()
        {
            currentSector = gameState != null ? gameState.sector : 1;
            
#if UNITY_EDITOR
            if (showDebugInfo)
            {
                Debug.Log($"Sector {currentSector}: Asteroid count controlled by minAsteroidCount={minAsteroidCount}");
            }
#endif
        }
        
        /// <summary>
        /// Attempt to spawn a new asteroid if conditions are met.
        /// </summary>
        private void TrySpawnAsteroid()
        {
            // Check if at max capacity (from pool's maxPoolSize)
            if (spawnedAsteroids.Count >= maxAsteroidCount)
            {
#if UNITY_EDITOR
                if (showDebugInfo && Time.frameCount % 300 == 0) // Log every 5 seconds at 60fps
                {
                    Debug.Log($"At max asteroid capacity: {spawnedAsteroids.Count}/{maxAsteroidCount}");
                }
#endif
                return;
            }
            
            // Always allow spawning when below minimum, otherwise use random chance
            bool shouldSpawn = spawnedAsteroids.Count < minAsteroidCount || Random.value < 0.05f;
            
            if (!shouldSpawn)
            {
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
                    Debug.Log($"Successfully spawned asteroid! Total: {spawnedAsteroids.Count}/{maxAsteroidCount} - Type: {selectedType.resourceName}");
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
        /// If below minimum count, forcefully spawns replacement asteroids on the opposite side.
        /// </summary>
        private void DespawnDistantAsteroids()
        {
            if (playerTransform == null)
                return;
            
            float despawnDistSq = despawnDistance * despawnDistance;
            int despawnedCount = 0;
            List<Vector3> despawnedPositions = new List<Vector3>(); // Track exact positions for opposite spawning
            
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
                    // Store the EXACT position relative to player before despawning
                    Vector3 relativePosition = asteroid.transform.position - playerTransform.position;
                    despawnedPositions.Add(relativePosition);
                    
                    asteroidPool.ReturnAsteroid(asteroid);
                    spawnedAsteroids.RemoveAt(i);
                    despawnedCount++;
                }
            }
            
            // If we're below minimum and despawned asteroids, forcefully spawn replacements
            if (despawnedCount > 0 && spawnedAsteroids.Count < minAsteroidCount)
            {
                int asteroidsToSpawn = Mathf.Min(despawnedCount, minAsteroidCount - spawnedAsteroids.Count);
                ForceSpawnOppositeAsteroids(despawnedPositions, asteroidsToSpawn);
                
#if UNITY_EDITOR
                if (showDebugInfo)
                {
                    Debug.Log($"Despawned {despawnedCount} asteroids, force spawned {asteroidsToSpawn} replacements. Count: {spawnedAsteroids.Count}/{minAsteroidCount} (min)");
                }
#endif
            }
            else
            {
#if UNITY_EDITOR
                if (showDebugInfo && despawnedCount > 0)
                {
                    Debug.Log($"Despawned {despawnedCount} distant asteroids. Remaining: {spawnedAsteroids.Count}/{maxAsteroidCount}");
                }
#endif
            }
        }
        
        /// <summary>
        /// Force spawn asteroids on the opposite side of the player from where asteroids were despawned.
        /// This maintains the minimum asteroid count as the player moves through space.
        /// Spawns at the same distance on the opposite side to create seamless field continuity.
        /// </summary>
        private void ForceSpawnOppositeAsteroids(List<Vector3> despawnedRelativePositions, int count)
        {
            int spawned = 0;
            
            for (int i = 0; i < count && i < despawnedRelativePositions.Count; i++)
            {
                Vector3 despawnedRelativePos = despawnedRelativePositions[i];
                
                // Calculate the OPPOSITE position (flip 180 degrees around player)
                Vector3 oppositeRelativePos = -despawnedRelativePos;
                
                // Add slight randomization to prevent exact grid patterns
                // Randomize within a small cone (10 degrees) to maintain natural distribution
                Vector3 randomizedRelativePos = RandomizeDirection(oppositeRelativePos.normalized, 10f) * oppositeRelativePos.magnitude;
                
                // Add some distance variation (+/- 50 units) to avoid all asteroids at exact same distance
                float distanceVariation = Random.Range(-50f, 50f);
                float finalDistance = randomizedRelativePos.magnitude + distanceVariation;
                finalDistance = Mathf.Max(finalDistance, minDistanceFromPlayer); // Ensure not too close
                
                // Calculate final spawn position
                Vector3 spawnPosition = playerTransform.position + randomizedRelativePos.normalized * finalDistance;
                
                // Select asteroid type
                AsteroidType selectedType = SelectRandomAsteroidType();
                if (selectedType == null)
                    continue;
                
                // Spawn from pool
                GameObject asteroid = asteroidPool.GetAsteroid(selectedType, spawnPosition);
                if (asteroid != null)
                {
                    spawnedAsteroids.Add(asteroid);
                    spawned++;
                    
#if UNITY_EDITOR
                    if (showDebugInfo)
                    {
                        Debug.Log($"Force spawned {selectedType.resourceName} on opposite side at distance {finalDistance:F1}m (original was {despawnedRelativePos.magnitude:F1}m away)");
                    }
#endif
                }
            }
            
#if UNITY_EDITOR
            if (showDebugInfo && spawned > 0)
            {
                Debug.Log($"Successfully force spawned {spawned}/{count} replacement asteroids on opposite side");
            }
#endif
        }
        
        /// <summary>
        /// Randomize a direction vector within a cone angle.
        /// Used to add variation to opposite-side spawning.
        /// </summary>
        private Vector3 RandomizeDirection(Vector3 direction, float maxAngleDegrees)
        {
            // Convert to quaternion rotation
            Quaternion baseRotation = Quaternion.LookRotation(direction);
            
            // Add random rotation within cone
            float randomAngle = Random.Range(0f, maxAngleDegrees);
            float randomRotation = Random.Range(0f, 360f);
            
            Quaternion randomOffset = Quaternion.Euler(
                Mathf.Cos(randomRotation * Mathf.Deg2Rad) * randomAngle,
                Mathf.Sin(randomRotation * Mathf.Deg2Rad) * randomAngle,
                0f
            );
            
            Quaternion finalRotation = baseRotation * randomOffset;
            return finalRotation * Vector3.forward;
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
