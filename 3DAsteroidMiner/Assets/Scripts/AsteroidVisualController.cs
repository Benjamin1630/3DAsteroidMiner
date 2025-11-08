using UnityEngine;

/// <summary>
/// Helper component for configuring asteroid visual properties at runtime.
/// Attach to asteroid prefabs or create on instantiation.
/// </summary>
public class AsteroidVisualController : MonoBehaviour
{
    [Header("Material Configuration")]
    [SerializeField] public Material asteroidMaterial; // Public so Asteroid.cs can access it
    [SerializeField] private Renderer asteroidRenderer;
    
    [Header("Type Configuration")]
    [SerializeField] private AsteroidTypeVisualData typeData;
    
    // Shader property IDs (cached for performance)
    private static readonly int TypeColorID = Shader.PropertyToID("_TypeColor");
    private static readonly int TypeColorIntensityID = Shader.PropertyToID("_TypeColorIntensity");
    private static readonly int TypeColorEmissionID = Shader.PropertyToID("_TypeColorEmission");
    private static readonly int HoleDensityID = Shader.PropertyToID("_HoleDensity");
    private static readonly int HoleSizeID = Shader.PropertyToID("_HoleSize");
    
    private MaterialPropertyBlock propertyBlock;
    
    /// <summary>
    /// Gets the assigned asteroid material.
    /// </summary>
    public Material GetAsteroidMaterial()
    {
        return asteroidMaterial;
    }
    
    private void Awake()
    {
        if (asteroidRenderer == null)
        {
            // Renderer should be on the SAME GameObject as this component
            asteroidRenderer = GetComponent<Renderer>();
            
            if (asteroidRenderer == null)
            {
                Debug.LogError($"AsteroidVisualController on {gameObject.name}: No Renderer found! This component should be on the same GameObject as the MeshRenderer.");
            }
        }
        
        // Apply material if specified and renderer doesn't have one
        if (asteroidRenderer != null && asteroidMaterial != null)
        {
            if (asteroidRenderer.sharedMaterial == null)
            {
                asteroidRenderer.sharedMaterial = asteroidMaterial;
                Debug.Log($"AsteroidVisualController: Assigned material '{asteroidMaterial.name}' in Awake");
            }
        }
        else if (asteroidMaterial == null)
        {
            Debug.LogWarning($"AsteroidVisualController on {gameObject.name}: No asteroid material assigned! Please assign in Inspector.");
        }
        
        propertyBlock = new MaterialPropertyBlock();
    }
    
    private void Start()
    {
        // Apply type data if available
        if (typeData != null)
        {
            ApplyTypeVisuals(typeData);
        }
    }
    
    /// <summary>
    /// Sets the asteroid type and applies visual properties.
    /// </summary>
    public void SetAsteroidType(AsteroidTypeVisualData data)
    {
        if (data == null)
        {
            Debug.LogWarning("AsteroidTypeVisualData is null!");
            return;
        }
        
        // Ensure renderer is cached (might be null if called before Awake)
        if (asteroidRenderer == null)
        {
            asteroidRenderer = GetComponent<Renderer>();
        }
        
        // Ensure material is assigned if specified
        if (asteroidRenderer != null && asteroidMaterial != null)
        {
            if (asteroidRenderer.sharedMaterial == null)
            {
                asteroidRenderer.sharedMaterial = asteroidMaterial;
            }
        }
        
        typeData = data;
        ApplyTypeVisuals(data);
    }
    
    /// <summary>
    /// Sets the asteroid type color only (quick method).
    /// </summary>
    public void SetTypeColor(Color color, float intensity = 1.5f, float emission = 0.3f)
    {
        // Renderer should be cached in Awake, but guard against null
        if (asteroidRenderer == null)
        {
            Debug.LogWarning($"AsteroidVisualController.SetTypeColor: Renderer is null on {gameObject.name}");
            return;
        }
        
        asteroidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(TypeColorID, color);
        propertyBlock.SetFloat(TypeColorIntensityID, intensity);
        propertyBlock.SetFloat(TypeColorEmissionID, emission);
        asteroidRenderer.SetPropertyBlock(propertyBlock);
    }
    
    /// <summary>
    /// Applies all visual properties from AsteroidTypeVisualData.
    /// Only sets color properties - hole configuration comes from the material.
    /// </summary>
    private void ApplyTypeVisuals(AsteroidTypeVisualData data)
    {
        if (asteroidRenderer == null)
        {
            Debug.LogError("Asteroid renderer not found!");
            return;
        }
        
        // Get current property block
        asteroidRenderer.GetPropertyBlock(propertyBlock);
        
        // Apply colors (only colors, not hole configuration)
        propertyBlock.SetColor(TypeColorID, data.typeColor);
        propertyBlock.SetFloat(TypeColorIntensityID, data.typeColorIntensity);
        propertyBlock.SetFloat(TypeColorEmissionID, data.typeColorEmission);
        
        // Apply to renderer
        asteroidRenderer.SetPropertyBlock(propertyBlock);
    }
    
    /// <summary>
    /// Randomizes hole pattern slightly (for variation between asteroids of same type).
    /// Reads current material values and applies small tweaks via MaterialPropertyBlock.
    /// </summary>
    public void RandomizeHolePattern(float variationAmount = 0.15f)
    {
        // Renderer should be cached in Awake
        if (asteroidRenderer == null || asteroidRenderer.sharedMaterial == null)
        {
            Debug.LogWarning($"AsteroidVisualController.RandomizeHolePattern: Renderer or material is null on {gameObject.name}");
            return;
        }
        
        // Get material's default hole configuration
        Material mat = asteroidRenderer.sharedMaterial;
        float baseDensity = mat.GetFloat(HoleDensityID);
        float baseSize = mat.GetFloat(HoleSizeID);
        
        // Apply small random variations
        float densityVariation = Random.Range(-variationAmount, variationAmount);
        float sizeVariation = Random.Range(-variationAmount * 0.05f, variationAmount * 0.05f);
        
        float newDensity = Mathf.Clamp(baseDensity + densityVariation, 3f, 15f);
        float newSize = Mathf.Clamp(baseSize + sizeVariation, 0.2f, 0.6f);
        
        // Apply via property block (per-instance variation)
        asteroidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(HoleDensityID, newDensity);
        propertyBlock.SetFloat(HoleSizeID, newSize);
        asteroidRenderer.SetPropertyBlock(propertyBlock);
    }
    
    /// <summary>
    /// Updates hole density and size at runtime (for rare special cases).
    /// Generally not needed - hole configuration comes from material.
    /// </summary>
    public void SetHoleProperties(float density, float size)
    {
        // Renderer should be cached in Awake
        if (asteroidRenderer == null)
        {
            Debug.LogWarning($"AsteroidVisualController.SetHoleProperties: Renderer is null on {gameObject.name}");
            return;
        }
        
        asteroidRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(HoleDensityID, density);
        propertyBlock.SetFloat(HoleSizeID, size);
        asteroidRenderer.SetPropertyBlock(propertyBlock);
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor-only method to preview changes in edit mode.
    /// </summary>
    [ContextMenu("Preview Type Visuals")]
    private void PreviewInEditor()
    {
        if (typeData != null)
        {
            if (asteroidRenderer == null)
            {
                asteroidRenderer = GetComponent<Renderer>();
            }
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
            ApplyTypeVisuals(typeData);
        }
    }
#endif
}
