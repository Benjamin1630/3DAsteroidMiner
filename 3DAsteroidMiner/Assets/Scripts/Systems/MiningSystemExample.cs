using UnityEngine;
using AsteroidMiner.Core;
using AsteroidMiner.Entities;
using System.Collections.Generic;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Example integration of EnhancedMiningLaser into the existing MiningSystem.
    /// This shows how to upgrade from the old MiningLaser to the new system.
    /// 
    /// MIGRATION STEPS:
    /// 1. Replace [SerializeField] private GameObject laserPrefab;
    ///    with [SerializeField] private GameObject enhancedLaserPrefab;
    /// 
    /// 2. Replace MiningLaser references with EnhancedMiningLaser
    /// 
    /// 3. Update laser creation logic (see CreateEnhancedLaser method)
    /// 
    /// 4. Update mining start/stop logic (see StartMining/StopMining methods)
    /// </summary>
    public class MiningSystemExample : MonoBehaviour
    {
        [Header("Enhanced Laser Settings")]
        [SerializeField] private GameObject enhancedLaserPrefab;
        [SerializeField] private Transform[] laserOrigins;
        [SerializeField] private int maxSimultaneousTargets = 6;
        
        [Header("Impact Effect Limits")]
        [SerializeField] private int maxImpactEffects = 10;
        
        // Laser management
        private List<EnhancedMiningLaser> laserPool = new List<EnhancedMiningLaser>();
        private List<EnhancedMiningLaser> activeLasers = new List<EnhancedMiningLaser>();
        private List<GameObject> activeImpactEffects = new List<GameObject>();
        
        private bool isMining = false;
        
        #region Initialization
        
        private void Start()
        {
            // Pre-create laser pool for better performance
            InitializeLaserPool();
        }
        
        /// <summary>
        /// Create a pool of lasers to avoid instantiation during gameplay.
        /// </summary>
        private void InitializeLaserPool()
        {
            for (int i = 0; i < maxSimultaneousTargets; i++)
            {
                EnhancedMiningLaser laser = CreateEnhancedLaser();
                laser.SetInactive();
                laserPool.Add(laser);
            }
            
            Debug.Log($"MiningSystem: Initialized laser pool with {laserPool.Count} lasers.");
        }
        
        /// <summary>
        /// Create a new enhanced mining laser instance.
        /// </summary>
        private EnhancedMiningLaser CreateEnhancedLaser()
        {
            GameObject laserObj = Instantiate(enhancedLaserPrefab);
            laserObj.name = "EnhancedMiningLaser";
            laserObj.transform.SetParent(transform);
            
            EnhancedMiningLaser laser = laserObj.GetComponent<EnhancedMiningLaser>();
            if (laser == null)
            {
                Debug.LogError("EnhancedMiningLaser component not found on prefab!");
                laser = laserObj.AddComponent<EnhancedMiningLaser>();
            }
            
            return laser;
        }
        
        #endregion
        
        #region Mining Operations
        
        /// <summary>
        /// Start mining operation with enhanced laser effects.
        /// </summary>
        public void StartMining(List<Asteroid> targets)
        {
            if (isMining) return;
            
            isMining = true;
            
            // Activate one laser per target (up to max)
            int laserCount = Mathf.Min(targets.Count, maxSimultaneousTargets);
            
            for (int i = 0; i < laserCount; i++)
            {
                if (i >= laserPool.Count) break;
                
                EnhancedMiningLaser laser = laserPool[i];
                Asteroid target = targets[i];
                
                // Get laser origin point
                Transform origin = laserOrigins[i % laserOrigins.Length];
                Vector3 startPos = origin.position;
                Vector3 endPos = target.transform.position;
                
                // Start the laser with animation
                laser.StartLaser(startPos, endPos);
                activeLasers.Add(laser);
                
                Debug.Log($"Started laser {i} targeting {target.name}");
            }
        }
        
        /// <summary>
        /// Stop all active mining operations.
        /// </summary>
        public void StopMining()
        {
            if (!isMining) return;
            
            isMining = false;
            
            // Gracefully shutdown all active lasers
            foreach (EnhancedMiningLaser laser in activeLasers)
            {
                laser.StopLaser(); // Triggers shutdown animation
            }
            
            activeLasers.Clear();
            
            Debug.Log("Stopped all mining lasers.");
        }
        
        /// <summary>
        /// Update laser positions each frame while mining.
        /// </summary>
        private void Update()
        {
            if (!isMining || activeLasers.Count == 0)
                return;
            
            // Update each active laser's position
            for (int i = 0; i < activeLasers.Count; i++)
            {
                if (i >= laserOrigins.Length) break;
                
                EnhancedMiningLaser laser = activeLasers[i];
                Transform origin = laserOrigins[i % laserOrigins.Length];
                
                // In real implementation, track actual asteroid targets
                // For example: Vector3 targetPos = miningTargets[i].asteroid.transform.position;
                Vector3 targetPos = origin.position + origin.forward * 10f; // Placeholder
                
                laser.UpdateLaser(origin.position, targetPos);
            }
        }
        
        #endregion
        
        #region Impact Effect Management
        
        /// <summary>
        /// Create an impact effect with limit enforcement.
        /// </summary>
        public void CreateImpactEffect(Vector3 position, Vector3 normal)
        {
            // Remove oldest effect if at limit
            if (activeImpactEffects.Count >= maxImpactEffects)
            {
                GameObject oldest = activeImpactEffects[0];
                if (oldest != null)
                {
                    Destroy(oldest);
                }
                activeImpactEffects.RemoveAt(0);
            }
            
            // Create new impact effect (handled by EnhancedMiningLaser automatically)
            // But if you want manual control:
            GameObject impactObj = new GameObject("LaserImpact");
            impactObj.transform.position = position;
            
            LaserImpactEffect impact = impactObj.AddComponent<LaserImpactEffect>();
            impact.Initialize(2f);
            impact.SetOrientation(normal);
            
            activeImpactEffects.Add(impactObj);
        }
        
        /// <summary>
        /// Attach impact effect to asteroid surface.
        /// </summary>
        public void CreateSurfaceImpact(Asteroid asteroid, Vector3 hitPoint, Vector3 hitNormal)
        {
            GameObject impactObj = new GameObject("LaserImpact");
            impactObj.transform.position = hitPoint;
            
            LaserImpactEffect impact = impactObj.AddComponent<LaserImpactEffect>();
            impact.Initialize(2f);
            impact.SetOrientation(hitNormal);
            impact.AttachToSurface(asteroid.transform); // Moves with asteroid rotation
            
            activeImpactEffects.Add(impactObj);
        }
        
        #endregion
        
        #region Laser Pool Management
        
        /// <summary>
        /// Get an available laser from the pool.
        /// </summary>
        private EnhancedMiningLaser GetAvailableLaser()
        {
            // Find inactive laser
            foreach (EnhancedMiningLaser laser in laserPool)
            {
                if (!laser.IsActive())
                {
                    return laser;
                }
            }
            
            // If all busy, create new one (or wait)
            EnhancedMiningLaser newLaser = CreateEnhancedLaser();
            laserPool.Add(newLaser);
            return newLaser;
        }
        
        /// <summary>
        /// Return laser to pool for reuse.
        /// </summary>
        private void ReturnLaserToPool(EnhancedMiningLaser laser)
        {
            laser.SetInactive();
            activeLasers.Remove(laser);
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up all lasers
            foreach (EnhancedMiningLaser laser in laserPool)
            {
                if (laser != null)
                {
                    Destroy(laser.gameObject);
                }
            }
            laserPool.Clear();
            activeLasers.Clear();
            
            // Clean up impact effects
            foreach (GameObject effect in activeImpactEffects)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }
            activeImpactEffects.Clear();
        }
        
        #endregion
    }
}
