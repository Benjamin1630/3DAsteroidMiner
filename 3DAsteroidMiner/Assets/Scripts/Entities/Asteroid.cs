using System;
using UnityEngine;
using AsteroidMiner.Data;
using Random = UnityEngine.Random;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Individual asteroid entity component.
    /// Handles asteroid behavior, rotation, health, mining interactions, and procedural mesh.
    /// Note: Collider is on the child ProceduralAsteroidMesh GameObject, not this parent.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Asteroid : MonoBehaviour
    {
        // ===== Events =====
        public event Action<Asteroid> OnAsteroidDestroyed;
        
        // ===== Configuration =====
        [Header("Asteroid Data")]
        [SerializeField] private AsteroidType asteroidType;
        
        // ===== Runtime State =====
        public AsteroidType Type => asteroidType;
        public float CurrentHealth { get; private set; }
        public bool IsBeingMined { get; set; }
        public int MinersTargeting { get; set; } = 0;
        
        // ===== Scanning State =====
        private bool isScanned = false;
        private float scanHighlightTimer = 0f;
        private Color originalEmissionColor;
        private bool hadEmission = false;
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock scanPropertyBlock;
        
        // ===== Components =====
        private Rigidbody rb;
        private Renderer asteroidRenderer;
        private ProceduralAsteroidMesh proceduralMesh;
        private AsteroidVisualController visualController;
        private Vector3 rotationAxis;
        private float rotationSpeed;
        
        // ===== Lifecycle =====
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            asteroidRenderer = GetComponentInChildren<Renderer>();
            proceduralMesh = GetComponentInChildren<ProceduralAsteroidMesh>();
            visualController = GetComponentInChildren<AsteroidVisualController>();
            
            // Initialize material property block for scan highlighting
            scanPropertyBlock = new MaterialPropertyBlock();
            
            // Configure as dynamic physics object in space (zero friction environment)
            if (rb != null)
            {
                rb.useGravity = false; // No gravity in space
                rb.isKinematic = false; // Enable physics simulation
                rb.linearDamping = 0f; // No air resistance in space - objects stay in motion
                rb.angularDamping = 0f; // No rotational drag - perpetual spin
                rb.mass = 100f; // Base mass (will vary by size)
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision for moving objects
            }
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
            
            // Components should be cached in Awake - only check once in editor for debugging
#if UNITY_EDITOR
            if (asteroidRenderer == null || proceduralMesh == null)
            {
                Debug.LogError($"Asteroid components not cached in Awake! Renderer: {asteroidRenderer != null}, ProceduralMesh: {proceduralMesh != null}");
            }
#endif
            
            // CRITICAL: Set world position, not local position
            // Must be done BEFORE setting scale to avoid parent transform issues
            transform.SetPositionAndRotation(position, Random.rotation);
            
            // Randomize size within range
            float size = Random.Range(type.sizeRange.x, type.sizeRange.y);
            transform.localScale = Vector3.one * size;
            
            // CRITICAL: Ensure material is set BEFORE generating mesh
            // Determine which material to use
            Material materialToUse = null;
            
            // Priority 1: Material from visual controller
            if (visualController != null && visualController.asteroidMaterial != null)
            {
                materialToUse = visualController.asteroidMaterial;
                Debug.Log($"Asteroid: Using material from AsteroidVisualController: {materialToUse.name}");
            }
            // Priority 2: Material from asteroid type
            else if (type.asteroidMaterial != null)
            {
                materialToUse = type.asteroidMaterial;
                Debug.Log($"Asteroid: Using material from AsteroidType: {materialToUse.name}");
            }
            
            // Set material on both renderer and procedural mesh
            if (materialToUse != null)
            {
                if (asteroidRenderer != null)
                {
                    asteroidRenderer.sharedMaterial = materialToUse;
                }
                if (proceduralMesh != null)
                {
                    proceduralMesh.SetMaterial(materialToUse);
                }
            }
            else
            {
                Debug.LogError($"Asteroid: No material found! Check AsteroidVisualController.asteroidMaterial or AsteroidType.asteroidMaterial for type '{type.resourceName}'");
            }
            
            // Generate unique procedural mesh (material should now be preserved)
            if (proceduralMesh != null)
            {
                int randomSeed = Random.Range(0, 100000);
                proceduralMesh.GenerateMesh(randomSeed, type.health);
                
                // Verify material is still assigned after mesh generation
                if (proceduralMesh.GetMaterial() == null)
                {
                    Debug.LogError($"Asteroid: Material was lost during GenerateMesh! Re-applying...");
                    if (materialToUse != null)
                    {
                        proceduralMesh.SetMaterial(materialToUse);
                    }
                }
            }
            
            // Random rotation axis and speed
            rotationAxis = Random.onUnitSphere;
            rotationSpeed = Random.Range(type.rotationSpeedRange.x, type.rotationSpeedRange.y);
            
            // Set physics properties based on size
            if (rb != null)
            {
                rb.position = position;
                rb.rotation = transform.rotation;
                
                // Mass scales with volume (size^3)
                rb.mass = 100f * (size * size * size);
                
                // Give asteroid a small initial random velocity for natural movement
                Vector3 randomVelocity = Random.onUnitSphere * Random.Range(0.1f, 0.5f);
                rb.linearVelocity = randomVelocity; // Updated API
                
                // Give asteroid a small random angular velocity (tumbling)
                Vector3 randomAngularVelocity = Random.onUnitSphere * Random.Range(0.5f, 2f);
                rb.angularVelocity = randomAngularVelocity;
            }
            
            // Apply visual properties
            if (visualController != null && type.visualData != null)
            {
                // Use new visual system (swiss cheese shader)
                // Material should already be assigned to prefab - just update properties via MaterialPropertyBlock
                visualController.SetAsteroidType(type.visualData);
                visualController.RandomizeHolePattern(0.15f); // Add variation between asteroids of same type
            }
            else if (asteroidRenderer != null && type.asteroidMaterial != null)
            {
                // Fallback to legacy material system (only when NOT using visual controller)
                asteroidRenderer.sharedMaterial = type.asteroidMaterial;
            }
            // NOTE: No color fallback - prefab should have material assigned
            // If using visual controller, material must be set on prefab beforehand
        }
        
        private void Update()
        {
            // Handle scan highlight timer
            if (isScanned && scanHighlightTimer > 0f)
            {
                scanHighlightTimer -= Time.deltaTime;
                if (scanHighlightTimer <= 0f)
                {
                    SetScanned(false, 0f);
                }
            }
        }
        
        // REMOVED: Empty Update() method for performance
        // Physics handles rotation via angular velocity in Rigidbody
        
        /// <summary>
        /// Apply mining damage to the asteroid.
        /// Returns true if asteroid is destroyed.
        /// NOTE: Continuous shrinking removed - mesh only updates at integer thresholds via OnIntegerHealthThreshold().
        /// </summary>
        public bool TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            
            // NOTE: Mesh regeneration now handled by OnIntegerHealthThreshold()
            // No continuous shrinking here
            
            if (CurrentHealth <= 0f)
            {
                OnAsteroidDestroyed?.Invoke(this);
                return true; // Signal for destruction
            }
            
            return false;
        }
        
        /// <summary>
        /// Apply mining damage specifically from mining lasers.
        /// This is a wrapper for TakeDamage() for clarity.
        /// </summary>
        public bool TakeMiningDamage(float damage)
        {
            CurrentHealth -= damage;
            
            if (CurrentHealth <= 0f)
            {
                OnAsteroidDestroyed?.Invoke(this);
                return true; // Signal for destruction
            }
            
            return false;
        }
        
        /// <summary>
        /// Called when asteroid health crosses an integer threshold (10→9, 9→8, etc).
        /// Triggers procedural mesh deformation to remove a "chunk".
        /// </summary>
        public void OnIntegerHealthThreshold(int newHealthInteger)
        {
            if (proceduralMesh != null && asteroidType != null)
            {
                // Calculate shrink factor based on remaining health
                float healthPercent = newHealthInteger / asteroidType.health;
                float shrinkFactor = 1.0f - healthPercent;
                
                // Trigger chunk-based mesh deformation
                proceduralMesh.RemoveChunk(shrinkFactor);
                
#if UNITY_EDITOR
                Debug.Log($"Asteroid: Health threshold {newHealthInteger} - removing chunk (shrink factor: {shrinkFactor:F2})");
#endif
            }
        }
        
        /// <summary>
        /// Get the mining progress as a percentage (0-1).
        /// </summary>
        public float GetMiningProgress()
        {
            if (asteroidType == null)
                return 0f;
            
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
            
            // Reset physics
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // Updated API
                rb.angularVelocity = Vector3.zero;
            }
            
            // Reset mesh to full size
            if (proceduralMesh != null)
            {
                proceduralMesh.ResetMesh();
            }
        }
        
        /// <summary>
        /// Apply force to asteroid (e.g., from player collision or weapon impact).
        /// </summary>
        public void ApplyImpact(Vector3 force, Vector3 hitPoint)
        {
            if (rb != null)
            {
                rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
            }
        }
        
        /// <summary>
        /// Get the asteroid's current velocity.
        /// </summary>
        public Vector3 GetVelocity()
        {
            return rb != null ? rb.linearVelocity : Vector3.zero; // Updated API
        }
        
        /// <summary>
        /// Set the scanned state of the asteroid (highlight with emission glow).
        /// </summary>
        public void SetScanned(bool scanned, float duration)
        {
            if (asteroidRenderer == null)
                return;
            
            if (scanned)
            {
                // Store original emission state
                if (!isScanned)
                {
                    Material mat = asteroidRenderer.sharedMaterial;
                    if (mat != null)
                    {
                        hadEmission = mat.IsKeywordEnabled("_EMISSION");
                        if (hadEmission)
                        {
                            originalEmissionColor = mat.GetColor(EmissionColorProperty);
                        }
                    }
                }
                
                isScanned = true;
                scanHighlightTimer = duration;
                
                // Apply scan highlight effect
                Color scanColor = new Color(0.2f, 0.8f, 1f, 1f); // Cyan holographic color
                
                // Use MaterialPropertyBlock to avoid creating material instances
                asteroidRenderer.GetPropertyBlock(scanPropertyBlock);
                scanPropertyBlock.SetColor(EmissionColorProperty, scanColor * 2f); // Boost emission
                asteroidRenderer.SetPropertyBlock(scanPropertyBlock);
                
                // Enable emission keyword on the material
                Material mat2 = asteroidRenderer.sharedMaterial;
                if (mat2 != null)
                {
                    mat2.EnableKeyword("_EMISSION");
                }
            }
            else
            {
                // Remove scan highlight
                isScanned = false;
                scanHighlightTimer = 0f;
                
                // Restore original emission state
                Material mat = asteroidRenderer.sharedMaterial;
                if (mat != null)
                {
                    if (hadEmission)
                    {
                        asteroidRenderer.GetPropertyBlock(scanPropertyBlock);
                        scanPropertyBlock.SetColor(EmissionColorProperty, originalEmissionColor);
                        asteroidRenderer.SetPropertyBlock(scanPropertyBlock);
                    }
                    else
                    {
                        // Clear the property block
                        asteroidRenderer.SetPropertyBlock(null);
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
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
            
            // Show scan highlight status
            if (isScanned)
            {
                Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2f + 0.5f);
            }
        }
#endif
    }
}

