using UnityEngine;

/// <summary>
/// ScriptableObject configuration for Space Dust particle effect.
/// Create instances via Assets > Create > Game/Space Dust Settings
/// </summary>
[CreateAssetMenu(fileName = "SpaceDustSettings", menuName = "Game/Space Dust Settings", order = 100)]
public class SpaceDustSettings : ScriptableObject
{
    [Header("Emission Settings")]
    [Tooltip("Particle emission rate when ship is stationary")]
    [Range(0f, 50f)]
    public float baseEmissionRate = 10f;
    
    [Tooltip("Maximum particle emission rate at full speed")]
    [Range(50f, 500f)]
    public float maxEmissionRate = 200f;
    
    [Tooltip("Ship speed at which max emission is reached")]
    [Range(10f, 200f)]
    public float maxSpeedThreshold = 50f;
    
    [Header("Velocity Settings")]
    [Tooltip("Multiplier for particle velocity relative to ship speed")]
    [Range(0.5f, 5f)]
    public float velocityMultiplier = 1.5f;
    
    [Tooltip("Minimum ship speed to show any particles")]
    [Range(0.1f, 5f)]
    public float minimumSpeedThreshold = 0.5f;
    
    [Header("Smoothing")]
    [Tooltip("Smooth emission rate changes over time (lower = smoother)")]
    [Range(0.05f, 1f)]
    public float emissionSmoothTime = 0.2f;
    
    [Header("Particle Appearance (Reference Only)")]
    [Tooltip("Suggested particle lifetime (set in Particle System)")]
    public float particleLifetime = 2f;
    
    [Tooltip("Suggested particle size range (set in Particle System)")]
    public Vector2 particleSizeRange = new Vector2(0.05f, 0.15f);
    
    [Tooltip("Suggested particle color")]
    public Color particleColor = new Color(0.8f, 0.9f, 1f, 0.8f);
    
    [Header("Performance")]
    [Tooltip("Maximum number of particles (set in Particle System)")]
    public int maxParticles = 1000;
    
    [Tooltip("Enable GPU instancing on particle material")]
    public bool useGPUInstancing = true;
    
    /// <summary>
    /// Validates settings when changed in Inspector
    /// </summary>
    private void OnValidate()
    {
        // Ensure max emission is greater than base
        if (maxEmissionRate < baseEmissionRate)
        {
            maxEmissionRate = baseEmissionRate + 10f;
        }
        
        // Ensure positive values
        baseEmissionRate = Mathf.Max(0f, baseEmissionRate);
        maxEmissionRate = Mathf.Max(0f, maxEmissionRate);
        maxSpeedThreshold = Mathf.Max(1f, maxSpeedThreshold);
        velocityMultiplier = Mathf.Max(0.1f, velocityMultiplier);
        minimumSpeedThreshold = Mathf.Max(0f, minimumSpeedThreshold);
        emissionSmoothTime = Mathf.Max(0.01f, emissionSmoothTime);
        particleLifetime = Mathf.Max(0.1f, particleLifetime);
        maxParticles = Mathf.Max(10, maxParticles);
    }
}
