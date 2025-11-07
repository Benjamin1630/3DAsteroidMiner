using System.Collections.Generic;
using UnityEngine;
using AsteroidMiner.Entities;
using AsteroidMiner.Data;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Object pool manager for asteroids to avoid constant instantiation/destruction.
    /// Improves performance significantly when spawning hundreds of asteroids.
    /// </summary>
    public class AsteroidPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField] private GameObject asteroidPrefab;
        [SerializeField] private int initialPoolSize = 200;
        [SerializeField] private int maxPoolSize = 500;
        [SerializeField] private Transform poolParent;
        
        // ===== Public Properties =====
        public int InitialPoolSize => initialPoolSize;
        public int MaxPoolSize => maxPoolSize;
        
        // ===== Pool Data Structures =====
        private Queue<GameObject> availableAsteroids = new Queue<GameObject>();
        private HashSet<GameObject> activeAsteroids = new HashSet<GameObject>();
        
        // ===== Initialization =====
        
        private void Awake()
        {
            // Create pool parent if not assigned
            if (poolParent == null)
            {
                poolParent = new GameObject("Asteroid Pool").transform;
                poolParent.SetParent(transform);
            }
            
            // Pre-populate pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewAsteroid();
            }
        }
        
        /// <summary>
        /// Get an asteroid from the pool.
        /// </summary>
        public GameObject GetAsteroid(AsteroidType type, Vector3 position)
        {
            GameObject asteroid;
            
            // Try to get from pool
            if (availableAsteroids.Count > 0)
            {
                asteroid = availableAsteroids.Dequeue();
            }
            else if (activeAsteroids.Count < maxPoolSize)
            {
                // Pool empty but not at max size - create new and immediately use it
                CreateNewAsteroid();
                asteroid = availableAsteroids.Dequeue();
            }
            else
            {
                // Pool at max capacity - reuse oldest active asteroid
                Debug.LogWarning($"Asteroid pool at max capacity ({maxPoolSize}). Reusing active asteroid.");
                return null;
            }
            
            // Initialize and activate
            // IMPORTANT: Activate BEFORE initialization to ensure all components work properly
            asteroid.SetActive(true);
            
            Asteroid asteroidComponent = asteroid.GetComponent<Asteroid>();
            if (asteroidComponent != null)
            {
                asteroidComponent.Initialize(type, position);
                
#if UNITY_EDITOR
                // Verify position was set correctly
                float actualDist = Vector3.Distance(asteroid.transform.position, position);
                if (actualDist > 1f)
                {
                    Debug.LogWarning($"Asteroid position mismatch! Expected: {position}, Actual: {asteroid.transform.position}, Diff: {actualDist:F2}m");
                }
#endif
            }
            else
            {
                Debug.LogError("Asteroid prefab is missing Asteroid component!");
                asteroid.SetActive(false); // Deactivate if component missing
                return null;
            }
            
            activeAsteroids.Add(asteroid);
            
            return asteroid;
        }
        
        /// <summary>
        /// Return an asteroid to the pool.
        /// </summary>
        public void ReturnAsteroid(GameObject asteroid)
        {
            if (asteroid == null) return;
            
            // Reset state
            Asteroid asteroidComponent = asteroid.GetComponent<Asteroid>();
            asteroidComponent?.ResetAsteroid();
            
            // Deactivate and return to pool
            asteroid.SetActive(false);
            activeAsteroids.Remove(asteroid);
            availableAsteroids.Enqueue(asteroid);
        }
        
        /// <summary>
        /// Return all active asteroids to the pool.
        /// Useful for sector transitions or resetting game state.
        /// </summary>
        public void ReturnAllAsteroids()
        {
            // Copy to list to avoid modifying collection during iteration
            List<GameObject> asteroidsToReturn = new List<GameObject>(activeAsteroids);
            
            foreach (GameObject asteroid in asteroidsToReturn)
            {
                ReturnAsteroid(asteroid);
            }
        }
        
        /// <summary>
        /// Get count of currently active asteroids.
        /// </summary>
        public int GetActiveCount()
        {
            return activeAsteroids.Count;
        }
        
        /// <summary>
        /// Get count of available asteroids in pool.
        /// </summary>
        public int GetAvailableCount()
        {
            return availableAsteroids.Count;
        }
        
        // ===== Private Methods =====
        
        private GameObject CreateNewAsteroid()
        {
            GameObject asteroid = Instantiate(asteroidPrefab, poolParent);
            asteroid.SetActive(false);
            availableAsteroids.Enqueue(asteroid);
            return asteroid;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (asteroidPrefab != null && asteroidPrefab.GetComponent<Asteroid>() == null)
            {
                Debug.LogError("Asteroid prefab must have an Asteroid component!");
            }
        }
#endif
    }
}
