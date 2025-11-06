using UnityEngine;

/// <summary>
/// Controls space dust particle effect that creates motion streaks when the ship moves.
/// Particles move opposite to ship velocity to create the illusion of movement through space.
/// Emission rate and particle velocity scale dynamically with ship speed.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class SpaceDustEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ship's Rigidbody component to read velocity from")]
    [SerializeField] private Rigidbody shipRigidbody;
    
    [Tooltip("Optional settings asset. If null, uses default values below")]
    [SerializeField] private SpaceDustSettings settings;
    
    [Header("Emission Settings")]
    [Tooltip("Particle emission rate when ship is stationary")]
    [SerializeField] private float baseEmissionRate = 10f;
    
    [Tooltip("Maximum particle emission rate at full speed")]
    [SerializeField] private float maxEmissionRate = 200f;
    
    [Tooltip("Ship speed at which max emission is reached")]
    [SerializeField] private float maxSpeedThreshold = 50f;
    
    [Header("Velocity Settings")]
    [Tooltip("Multiplier for particle velocity relative to ship speed")]
    [SerializeField] private float velocityMultiplier = 1.5f;
    
    [Tooltip("Minimum ship speed to show any particles")]
    [SerializeField] private float minimumSpeedThreshold = 0.5f;
    
    [Header("Advanced Settings")]
    [Tooltip("Smooth emission rate changes over time")]
    [SerializeField] private float emissionSmoothTime = 0.2f;
    
    [Tooltip("Enable debug logging")]
    [SerializeField] private bool debugMode = false;
    
    // Particle system modules (cached for performance)
    private ParticleSystem dustParticles;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    
    // Smoothing variables
    private float currentEmissionRate;
    private float emissionVelocity;
    
    private void Awake()
    {
        // Get particle system component
        dustParticles = GetComponent<ParticleSystem>();
        
        if (dustParticles == null)
        {
            Debug.LogError("SpaceDustEffect: ParticleSystem component not found!");
            enabled = false;
            return;
        }
        
        // Cache particle system modules
        emission = dustParticles.emission;
        mainModule = dustParticles.main;
        velocityModule = dustParticles.velocityOverLifetime;
        
        // Load settings from ScriptableObject if provided
        if (settings != null)
        {
            baseEmissionRate = settings.baseEmissionRate;
            maxEmissionRate = settings.maxEmissionRate;
            maxSpeedThreshold = settings.maxSpeedThreshold;
            velocityMultiplier = settings.velocityMultiplier;
            minimumSpeedThreshold = settings.minimumSpeedThreshold;
            emissionSmoothTime = settings.emissionSmoothTime;
        }
        
        // Initialize emission rate
        currentEmissionRate = baseEmissionRate;
        
        // Ensure velocity module is set up correctly
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.World;
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("SpaceDustEffect initialized successfully");
        }
#endif
    }
    
    private void Start()
    {
        // Try to find ship Rigidbody if not assigned
        if (shipRigidbody == null)
        {
            // Look for Rigidbody on parent objects
            shipRigidbody = GetComponentInParent<Rigidbody>();
            
            if (shipRigidbody == null)
            {
                // Try to find player ship by tag
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    shipRigidbody = player.GetComponent<Rigidbody>();
                }
            }
            
            if (shipRigidbody == null)
            {
                Debug.LogWarning("SpaceDustEffect: Ship Rigidbody not assigned and could not be found automatically. Please assign it in the Inspector.");
            }
        }
    }
    
    private void Update()
    {
        if (shipRigidbody == null) return;
        
        UpdateParticleEffect();
    }
    
    /// <summary>
    /// Updates particle emission and velocity based on ship movement
    /// </summary>
    private void UpdateParticleEffect()
    {
        // Get ship velocity
        Vector3 shipVelocity = shipRigidbody.linearVelocity;
        float speed = shipVelocity.magnitude;
        
        // Calculate target emission rate based on speed
        float targetEmissionRate;
        
        if (speed < minimumSpeedThreshold)
        {
            // Ship is stationary or moving very slowly - minimal particles
            targetEmissionRate = baseEmissionRate;
        }
        else
        {
            // Normalize speed (0 to 1 based on max threshold)
            float normalizedSpeed = Mathf.Clamp01(speed / maxSpeedThreshold);
            
            // Interpolate emission rate
            targetEmissionRate = Mathf.Lerp(baseEmissionRate, maxEmissionRate, normalizedSpeed);
        }
        
        // Smooth emission rate changes
        currentEmissionRate = Mathf.SmoothDamp(
            currentEmissionRate, 
            targetEmissionRate, 
            ref emissionVelocity, 
            emissionSmoothTime
        );
        
        // Apply emission rate
        emission.rateOverTime = currentEmissionRate;
        
        // Update particle velocity to move opposite to ship direction
        if (speed > minimumSpeedThreshold)
        {
            // Calculate particle velocity (opposite to ship movement)
            Vector3 particleVelocity = -shipVelocity * velocityMultiplier;
            
            // Apply velocity to particle system
            velocityModule.x = particleVelocity.x;
            velocityModule.y = particleVelocity.y;
            velocityModule.z = particleVelocity.z;
            
#if UNITY_EDITOR
            if (debugMode)
            {
                Debug.DrawRay(transform.position, particleVelocity, Color.cyan, Time.deltaTime);
            }
#endif
        }
        else
        {
            // Ship is stationary - minimal particle movement
            velocityModule.x = 0f;
            velocityModule.y = 0f;
            velocityModule.z = 0f;
        }
        
#if UNITY_EDITOR
        if (debugMode && Time.frameCount % 60 == 0) // Log once per second at 60fps
        {
            Debug.Log($"SpaceDust - Speed: {speed:F2}, Emission: {currentEmissionRate:F0}, Particles: {dustParticles.particleCount}");
        }
#endif
    }
    
    /// <summary>
    /// Manually set the ship Rigidbody reference (useful for runtime initialization)
    /// </summary>
    public void SetShipRigidbody(Rigidbody rb)
    {
        shipRigidbody = rb;
    }
    
    /// <summary>
    /// Override settings at runtime
    /// </summary>
    public void SetSettings(SpaceDustSettings newSettings)
    {
        settings = newSettings;
        
        if (settings != null)
        {
            baseEmissionRate = settings.baseEmissionRate;
            maxEmissionRate = settings.maxEmissionRate;
            maxSpeedThreshold = settings.maxSpeedThreshold;
            velocityMultiplier = settings.velocityMultiplier;
            minimumSpeedThreshold = settings.minimumSpeedThreshold;
            emissionSmoothTime = settings.emissionSmoothTime;
        }
    }
    
    /// <summary>
    /// Enable or disable the dust effect
    /// </summary>
    public void SetEffectEnabled(bool enabled)
    {
        if (dustParticles != null)
        {
            if (enabled)
            {
                dustParticles.Play();
            }
            else
            {
                dustParticles.Stop();
            }
        }
    }
    
    /// <summary>
    /// Get current particle count (useful for performance monitoring)
    /// </summary>
    public int GetParticleCount()
    {
        return dustParticles != null ? dustParticles.particleCount : 0;
    }
    
#if UNITY_EDITOR
    // Visualize spawn area in Scene view
    private void OnDrawGizmosSelected()
    {
        if (dustParticles != null)
        {
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            
            var shape = dustParticles.shape;
            Vector3 scale = shape.scale;
            
            // Draw the spawn volume
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, scale);
        }
    }
#endif
}
