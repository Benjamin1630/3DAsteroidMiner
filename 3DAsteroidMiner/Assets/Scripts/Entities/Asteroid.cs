using UnityEngine;
using AsteroidMiner.Data;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Individual asteroid entity component.
    /// Handles asteroid behavior, rotation, health, and mining interactions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Asteroid : MonoBehaviour
    {
        // ===== Configuration =====
        [Header("Asteroid Data")]
        [SerializeField] private AsteroidType asteroidType;
        
        // ===== Runtime State =====
        public AsteroidType Type => asteroidType;
        public float CurrentHealth { get; private set; }
        public bool IsBeingMined { get; set; }
        public int MinersTargeting { get; set; } = 0;
        
        // ===== Components =====
        private Rigidbody rb;
        private Renderer asteroidRenderer;
        private Vector3 rotationAxis;
        private float rotationSpeed;
        
        // ===== Lifecycle =====
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            asteroidRenderer = GetComponentInChildren<Renderer>();
            
            // Configure as static space object
            rb.useGravity = false;
            rb.isKinematic = true; // Asteroids don't move in this game
        }
        
        /// <summary>
        /// Initialize asteroid with a specific type and random properties.
        /// Called by object pool when spawning.
        /// </summary>
        public void Initialize(AsteroidType type, Vector3 position)
        {
            asteroidType = type;
            CurrentHealth = type.health;
            IsBeingMined = false;
            MinersTargeting = 0;
            
            // CRITICAL: Set world position, not local position
            // Must be done BEFORE setting scale to avoid parent transform issues
            transform.SetPositionAndRotation(position, Random.rotation);
            
            // Randomize size within range
            float size = Random.Range(type.sizeRange.x, type.sizeRange.y);
            transform.localScale = Vector3.one * size;
            
            // Random rotation axis and speed
            rotationAxis = Random.onUnitSphere;
            rotationSpeed = Random.Range(type.rotationSpeedRange.x, type.rotationSpeedRange.y);
            
            // Update Rigidbody position if it exists (for kinematic objects)
            if (rb != null)
            {
                rb.position = position;
            }
            
            // Apply visual properties
            if (asteroidRenderer != null && type.asteroidMaterial != null)
            {
                asteroidRenderer.material = type.asteroidMaterial;
            }
            else if (asteroidRenderer != null)
            {
                asteroidRenderer.material.color = type.asteroidColor;
            }
        }
        
        private void Update()
        {
            // Slow rotation for visual interest
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        /// <summary>
        /// Apply mining damage to the asteroid.
        /// Returns true if asteroid is destroyed.
        /// </summary>
        public bool TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            
            if (CurrentHealth <= 0f)
            {
                return true; // Signal for destruction
            }
            
            return false;
        }
        
        /// <summary>
        /// Get the mining progress as a percentage (0-1).
        /// </summary>
        public float GetMiningProgress()
        {
            return 1f - (CurrentHealth / asteroidType.health);
        }
        
        /// <summary>
        /// Reset asteroid state for object pooling.
        /// </summary>
        public void ResetAsteroid()
        {
            CurrentHealth = 0f;
            IsBeingMined = false;
            MinersTargeting = 0;
            asteroidType = null;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Show health status in editor
            if (asteroidType != null)
            {
                Gizmos.color = Color.Lerp(Color.red, Color.green, CurrentHealth / asteroidType.health);
                Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2f);
            }
        }
#endif
    }
}
