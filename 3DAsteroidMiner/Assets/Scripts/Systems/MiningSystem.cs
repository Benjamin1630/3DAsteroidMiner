using System.Collections.Generic;
using UnityEngine;
using AsteroidMiner.Core;
using AsteroidMiner.Entities;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Manages mining operations using camera raycast targeting.
    /// Player aims at asteroid with camera crosshair to mine it.
    /// Mining deals 1 damage per second per laser (base), modified by mining upgrade.
    /// Resources collected at each integer health threshold.
    /// </summary>
    public class MiningSystem : MonoBehaviour
    {
        [Header("Mining Settings")]
        [SerializeField] private float miningRange = 100f;
        [Tooltip("Fuel consumed per second while mining (per active target)")]
        [SerializeField] private float miningFuelConsumption = 0.5f;
        
        [Header("Raycast Targeting")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private bool useScreenCenter = true; // If false, uses mouse position
        [SerializeField] private LayerMask asteroidLayerMask = ~0; // What layers to hit
        
        [Header("Laser Settings")]
        [SerializeField] private GameObject laserPrefab;
        [SerializeField] private Transform[] laserOrigins; // Multiple mount points for multi-mining
        [SerializeField] private Color laserColor = new Color(0f, 1f, 0f, 0.8f);
        [SerializeField] private float laserWidth = 0.2f;
        
        [Header("References")]
        [SerializeField] private ShipStats shipStats;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private AsteroidPool asteroidPool;
        
        // Mining state
        private bool isMining = false;
        private List<MiningTarget> activeTargets = new List<MiningTarget>();
        private List<MiningLaser> activeLasers = new List<MiningLaser>();
        
        // Mining target tracking
        private class MiningTarget
        {
            public Asteroid asteroid;
            public float progress;
            public MiningLaser laser;
            public int lastHealthInteger; // Track integer health to detect threshold crossings
            public Vector3 hitPoint; // Exact point where raycast hit the asteroid
            
            public MiningTarget(Asteroid ast, MiningLaser laserBeam, Vector3 hit)
            {
                asteroid = ast;
                progress = 0f;
                laser = laserBeam;
                lastHealthInteger = Mathf.FloorToInt(ast.CurrentHealth);
                hitPoint = hit;
            }
        }
        
        // Helper struct to pass asteroid + hitpoint data
        private struct AsteroidHitData
        {
            public Asteroid asteroid;
            public Vector3 hitPoint;
            
            public AsteroidHitData(Asteroid ast, Vector3 hit)
            {
                asteroid = ast;
                hitPoint = hit;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Auto-find player camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    Debug.LogError("MiningSystem: No camera found! Please assign Player Camera or ensure main camera is tagged.");
                }
            }
            
            // Setup layer mask if not configured in Inspector
            if (asteroidLayerMask == 0)
            {
                asteroidLayerMask = LayerMask.GetMask("Asteroid");
            }
            
            // If no laser origins specified, use player transform
            if (laserOrigins == null || laserOrigins.Length == 0)
            {
                laserOrigins = new Transform[] { transform };
            }
        }
        
        private void Start()
        {
            // Try to get ShipStats from PlayerController if not assigned
            if (shipStats == null)
            {
                PlayerController playerController = GetComponentInParent<PlayerController>();
                if (playerController != null)
                {
                    shipStats = playerController.GetShipStats();
                    if (shipStats != null)
                    {
                        Debug.Log("MiningSystem: Successfully obtained ShipStats from PlayerController.");
                    }
                }
                
                if (shipStats == null)
                {
                    Debug.LogError("MiningSystem: ShipStats not assigned! Please ensure PlayerController has ShipStats component.");
                }
            }
            
            // Auto-find player transform if not assigned
            if (playerTransform == null)
            {
                playerTransform = transform.parent != null ? transform.parent : transform;
            }
            
            // Auto-find asteroid pool if not assigned
            if (asteroidPool == null)
            {
                asteroidPool = FindFirstObjectByType<AsteroidPool>();
                if (asteroidPool == null)
                {
                    Debug.LogWarning("MiningSystem: No AsteroidPool found! Destroyed asteroids will not be returned to pool.");
                }
            }
            
            // Subscribe to input events
            PlayerInputHandler inputHandler = GetComponentInParent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.OnMineStarted += StartMining;
                inputHandler.OnMineCanceled += StopMining;
            }
            else
            {
                Debug.LogError("MiningSystem: Could not find PlayerInputHandler! Mining input will not work.");
            }
        }
        
        private void Update()
        {
            if (isMining && shipStats != null && !shipStats.IsDocked())
            {
                // Raycast every frame to update targets based on what player is looking at
                UpdateTargetsFromRaycast();
                UpdateMining();
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from input events
            PlayerInputHandler inputHandler = GetComponentInParent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.OnMineStarted -= StartMining;
                inputHandler.OnMineCanceled -= StopMining;
            }
            
            // Clean up any active lasers
            ClearAllTargets();
        }
        
        #endregion
        
        #region Mining Control
        
        /// <summary>
        /// Start mining operation.
        /// </summary>
        private void StartMining()
        {
            if (shipStats == null || shipStats.IsDocked()) return;
            
            isMining = true;
            // Targets will be acquired in Update() via UpdateTargetsFromRaycast()
        }
        
        /// <summary>
        /// Stop mining operation and clear all targets.
        /// </summary>
        private void StopMining()
        {
            isMining = false;
            ClearAllTargets();
        }
        
        /// <summary>
        /// Update mining targets every frame based on raycast from camera.
        /// Only mines asteroids that player is actively looking at.
        /// </summary>
        private void UpdateTargetsFromRaycast()
        {
            if (shipStats == null || playerCamera == null) return;
            
            // Get max number of simultaneous targets from upgrade
            int maxTargets = shipStats.GetMultiMiningLevel();
            
            // Raycast from camera to find asteroid under crosshair
            Ray ray = GetCameraRay();
            List<AsteroidHitData> targetedAsteroids = new List<AsteroidHitData>();
            
            if (Physics.Raycast(ray, out RaycastHit hit, miningRange, asteroidLayerMask))
            {
                Asteroid asteroid = hit.collider.GetComponentInParent<Asteroid>();
                if (asteroid != null && asteroid.CurrentHealth > 0)
                {
                    targetedAsteroids.Add(new AsteroidHitData(asteroid, hit.point));
                }
            }
            
            // Multi-mining: Find additional asteroids near the primary target
            if (targetedAsteroids.Count > 0 && maxTargets > 1)
            {
                Vector3 centerPoint = targetedAsteroids[0].asteroid.transform.position;
                
                // Find other asteroids near the primary target
                Collider[] nearbyColliders = Physics.OverlapSphere(centerPoint, miningRange * 0.3f, asteroidLayerMask);
                
                foreach (Collider col in nearbyColliders)
                {
                    if (targetedAsteroids.Count >= maxTargets) break;
                    
                    Asteroid asteroid = col.GetComponentInParent<Asteroid>();
                    if (asteroid != null && asteroid.CurrentHealth > 0)
                    {
                        // Check if not already in list
                        bool alreadyAdded = false;
                        foreach (var data in targetedAsteroids)
                        {
                            if (data.asteroid == asteroid)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }
                        
                        if (!alreadyAdded)
                        {
                            // Check if asteroid is visible from camera
                            Vector3 dirToAsteroid = asteroid.transform.position - playerCamera.transform.position;
                            if (Physics.Raycast(playerCamera.transform.position, dirToAsteroid, out RaycastHit secondaryHit, miningRange, asteroidLayerMask))
                            {
                                Asteroid hitAsteroid = secondaryHit.collider.GetComponentInParent<Asteroid>();
                                if (hitAsteroid == asteroid)
                                {
                                    targetedAsteroids.Add(new AsteroidHitData(asteroid, secondaryHit.point));
                                }
                            }
                        }
                    }
                }
            }
            
            // Update active targets to match raycasted asteroids
            SyncTargets(targetedAsteroids);
        }
        
        /// <summary>
        /// Synchronize active targets with newly raycasted asteroids.
        /// Removes targets no longer being looked at, adds new targets.
        /// Updates hitpoints for existing targets.
        /// </summary>
        private void SyncTargets(List<AsteroidHitData> newTargets)
        {
            // Remove targets that are no longer being looked at
            for (int i = activeTargets.Count - 1; i >= 0; i--)
            {
                MiningTarget target = activeTargets[i];
                
                // Check if this asteroid is still in the new targets
                bool stillTargeted = false;
                foreach (var data in newTargets)
                {
                    if (data.asteroid == target.asteroid)
                    {
                        stillTargeted = true;
                        break;
                    }
                }
                
                if (!stillTargeted || target.asteroid.CurrentHealth <= 0)
                {
                    // Clean up this target
                    if (target.asteroid != null)
                    {
                        target.asteroid.IsBeingMined = false;
                    }
                    if (target.laser != null)
                    {
                        Destroy(target.laser.gameObject);
                    }
                    activeTargets.RemoveAt(i);
                }
            }
            
            // Add new targets or update existing hitpoints
            for (int i = 0; i < newTargets.Count; i++)
            {
                AsteroidHitData data = newTargets[i];
                
                // Check if already in active targets
                MiningTarget existingTarget = null;
                foreach (MiningTarget existing in activeTargets)
                {
                    if (existing.asteroid == data.asteroid)
                    {
                        existingTarget = existing;
                        break;
                    }
                }
                
                if (existingTarget != null)
                {
                    // Update hitpoint for existing target
                    existingTarget.hitPoint = data.hitPoint;
                }
                else
                {
                    // Add new target
                    MiningLaser laser = CreateLaser(laserOrigins[i % laserOrigins.Length]);
                    MiningTarget target = new MiningTarget(data.asteroid, laser, data.hitPoint);
                    activeTargets.Add(target);
                    data.asteroid.IsBeingMined = true;
                }
            }
        }
        
        /// <summary>
        /// Get the ray from camera based on targeting mode.
        /// </summary>
        private Ray GetCameraRay()
        {
            if (useScreenCenter)
            {
                // Use screen center (crosshair mode)
                return playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                // Use mouse position (mouse aim mode)
                return playerCamera.ScreenPointToRay(Input.mousePosition);
            }
        }
        
        /// <summary>
        /// Update all active mining operations.
        /// Each laser deals 1 damage per second (base) × mining upgrade multiplier.
        /// Multiple lasers stack their damage.
        /// </summary>
        private void UpdateMining()
        {
            if (shipStats == null || activeTargets.Count == 0) return;
            
            // Check if player has fuel
            if (!shipStats.HasFuel())
            {
                StopMining();
                return;
            }
            
            // Get mining speed multiplier from upgrades
            // Base: 1 damage/second per laser at level 1
            // Formula: 1.0 + (miningLevel * 0.2)
            float miningSpeed = shipStats.GetMiningSpeedMultiplier();
            
            // Each laser deals: 1.0 * miningSpeed damage per second
            float damagePerLaserPerSecond = 1.0f * miningSpeed;
            
            // Consume fuel while mining (constant per second, not per damage)
            float fuelCost = miningFuelConsumption * activeTargets.Count * Time.deltaTime;
            shipStats.ConsumeFuel(fuelCost);
            
            // Process each target (each has its own laser)
            for (int i = activeTargets.Count - 1; i >= 0; i--)
            {
                MiningTarget target = activeTargets[i];
                
                // Check if target is still valid
                if (target.asteroid == null || target.asteroid.CurrentHealth <= 0)
                {
                    RemoveTarget(i);
                    continue;
                }
                
                // Check if target is still in range from camera
                float distance = Vector3.Distance(playerCamera.transform.position, target.asteroid.transform.position);
                if (distance > miningRange)
                {
                    RemoveTarget(i);
                    continue;
                }
                
                // Update laser beam (from ship/laser origin to exact raycast hitpoint)
                if (target.laser != null)
                {
                    Vector3 laserOrigin = laserOrigins[i % laserOrigins.Length].position;
                    target.laser.UpdateLaser(laserOrigin, target.hitPoint);
                }
                
                // Apply mining damage for this frame
                float damageThisFrame = damagePerLaserPerSecond * Time.deltaTime;
                float healthBefore = target.asteroid.CurrentHealth;
                int integerBefore = target.lastHealthInteger;
                
                target.asteroid.TakeMiningDamage(damageThisFrame);
                target.progress += damageThisFrame;
                
                // Check if we crossed an integer health threshold
                int integerAfter = Mathf.FloorToInt(target.asteroid.CurrentHealth);
                
                if (integerAfter < integerBefore)
                {
                    // We crossed at least one integer threshold
                    int thresholdsCrossed = integerBefore - integerAfter;
                    
                    // Award resources for each threshold crossed
                    for (int t = 0; t < thresholdsCrossed; t++)
                    {
                        // Check if cargo is full before adding resource
                        if (shipStats.GetCurrentCargoCount() >= shipStats.GetMaxCargo())
                        {
#if UNITY_EDITOR
                            Debug.Log("MiningSystem: Cargo full during mining, stopping");
#endif
                            StopMining();
                            return;
                        }
                        
                        // Add 1 unit of resource
                        bool added = shipStats.AddToCargo(target.asteroid.Type.resourceName, 1);
                        if (added)
                        {
                            shipStats.IncrementAsteroidsMined(); // Track stat
                            
#if UNITY_EDITOR
                            Debug.Log($"MiningSystem: Collected 1x {target.asteroid.Type.resourceName} (Health {integerBefore - t} → {integerBefore - t - 1})");
#endif
                        }
                    }
                    
                    // Update tracked integer
                    target.lastHealthInteger = integerAfter;
                    
                    // Trigger mesh regeneration at integer boundary
                    target.asteroid.OnIntegerHealthThreshold(integerAfter);
                }
                
                // Check if asteroid is destroyed
                if (target.asteroid.CurrentHealth <= 0)
                {
                    OnAsteroidDestroyed(target.asteroid);
                    RemoveTarget(i);
                }
            }
            
            // Note: Target acquisition now handled every frame in UpdateTargetsFromRaycast()
        }
        
        /// <summary>
        /// Handle asteroid destruction.
        /// NOTE: Resources are already collected at integer thresholds.
        /// Credits are NOT awarded here - only when selling cargo at station.
        /// Returns asteroid to pool for reuse.
        /// </summary>
        private void OnAsteroidDestroyed(Asteroid asteroid)
        {
            if (shipStats == null || asteroid == null || asteroid.Type == null) return;
            
#if UNITY_EDITOR
            Debug.Log($"MiningSystem: Asteroid {asteroid.Type.resourceName} fully mined (destroyed)");
#endif
            
            // Return asteroid to pool for reuse
            if (asteroidPool != null)
            {
                asteroidPool.ReturnAsteroid(asteroid.gameObject);
            }
            else
            {
                // Fallback: destroy the GameObject if no pool available
                Debug.LogWarning("MiningSystem: No AsteroidPool available, destroying asteroid GameObject directly");
                Destroy(asteroid.gameObject);
            }
        }
        
        /// <summary>
        /// Remove a mining target and clean up its laser.
        /// </summary>
        private void RemoveTarget(int index)
        {
            if (index < 0 || index >= activeTargets.Count) return;
            
            MiningTarget target = activeTargets[index];
            
            // Mark asteroid as no longer being mined
            if (target.asteroid != null)
            {
                target.asteroid.IsBeingMined = false;
            }
            
            // Destroy laser
            if (target.laser != null)
            {
                Destroy(target.laser.gameObject);
            }
            
            activeTargets.RemoveAt(index);
        }
        
        /// <summary>
        /// Clear all mining targets and lasers.
        /// </summary>
        private void ClearAllTargets()
        {
            foreach (MiningTarget target in activeTargets)
            {
                if (target.asteroid != null)
                {
                    target.asteroid.IsBeingMined = false;
                }
                
                if (target.laser != null)
                {
                    Destroy(target.laser.gameObject);
                }
            }
            
            activeTargets.Clear();
        }
        
        #endregion
        
        #region Laser Management
        
        /// <summary>
        /// Create a laser beam GameObject with LineRenderer.
        /// </summary>
        private MiningLaser CreateLaser(Transform origin)
        {
            GameObject laserObj = new GameObject("MiningLaser");
            laserObj.transform.SetParent(origin);
            laserObj.transform.localPosition = Vector3.zero;
            
            MiningLaser laser = laserObj.AddComponent<MiningLaser>();
            laser.Initialize(laserColor, laserWidth);
            
            activeLasers.Add(laser);
            return laser;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if mining system is currently active.
        /// </summary>
        public bool IsMining => isMining && activeTargets.Count > 0;
        
        /// <summary>
        /// Get the number of active mining targets.
        /// </summary>
        public int ActiveTargetCount => activeTargets.Count;
        
        /// <summary>
        /// Get the current mining range.
        /// </summary>
        public float MiningRange => miningRange;
        
        /// <summary>
        /// Set ShipStats reference (for initialization).
        /// </summary>
        public void SetShipStats(ShipStats stats)
        {
            shipStats = stats;
        }
        
        #endregion
        
        #region Debug Visualization
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw camera raycast for targeting visualization
            if (playerCamera != null && Application.isPlaying)
            {
                Ray ray = GetCameraRay();
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(ray.origin, ray.direction * miningRange);
                
                // Draw hit point if something is hit
                if (Physics.Raycast(ray, out RaycastHit hit, miningRange, asteroidLayerMask))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(hit.point, 1f);
                }
            }
            
            // Draw mining range sphere from camera
            if (playerCamera != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 1f);
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * miningRange);
            }
            
            // Draw laser origin points
            if (laserOrigins != null)
            {
                Gizmos.color = Color.yellow;
                foreach (Transform origin in laserOrigins)
                {
                    if (origin != null)
                    {
                        Gizmos.DrawSphere(origin.position, 0.5f);
                    }
                }
            }
            
            // Draw active mining targets
            if (Application.isPlaying && activeTargets.Count > 0)
            {
                Gizmos.color = Color.red;
                foreach (MiningTarget target in activeTargets)
                {
                    if (target.asteroid != null)
                    {
                        Gizmos.DrawWireSphere(target.asteroid.transform.position, 2f);
                    }
                }
            }
        }
#endif
        
        #endregion
    }
}
