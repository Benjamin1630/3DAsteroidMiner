using UnityEngine;
using AsteroidMiner.Data;

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
            
            // Ensure components are cached (in case Awake didn't run or object was disabled)
            if (asteroidRenderer == null)
                asteroidRenderer = GetComponentInChildren<Renderer>();
            if (proceduralMesh == null)
                proceduralMesh = GetComponentInChildren<ProceduralAsteroidMesh>();
            if (visualController == null)
                visualController = GetComponentInChildren<AsteroidVisualController>();
            
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
            // Physics now handles rotation via angular velocity
            // No need for manual rotation with Transform.Rotate
        }
        
        /// <summary>
        /// Apply mining damage to the asteroid.
        /// Returns true if asteroid is destroyed.
        /// </summary>
        public bool TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            
            // Update shrink effect based on damage
            if (proceduralMesh != null && asteroidType != null)
            {
                float miningProgress = GetMiningProgress();
                proceduralMesh.UpdateShrinkEffect(miningProgress);
            }
            
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
