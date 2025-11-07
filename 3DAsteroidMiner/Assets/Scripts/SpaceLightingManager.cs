using UnityEngine;

/// <summary>
/// Manages lighting for the space environment, ensuring black sky with proper directional lighting.
/// This prevents Unity's default ambient skybox behavior which would tint the scene with blue/sky colors.
/// </summary>
public class SpaceLightingManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    [SerializeField] private Color ambientColor = Color.black;
    [Tooltip("The main directional light (sun) in the scene")]
    [SerializeField] private Light sunLight;
    
    [Header("Sun Light Settings")]
    [SerializeField] private Color sunColor = new Color(1f, 0.95f, 0.85f); // Slightly warm white
    [SerializeField] private float sunIntensity = 1.5f;
    [SerializeField] private Vector3 sunRotation = new Vector3(50f, -30f, 0f);
    
    [Header("Skybox Settings")]
    [SerializeField] private Material spaceSkyboxMaterial;
    [SerializeField] private bool useCustomSkybox = true;
    [SerializeField] private bool syncSkyboxSunWithLight = true;
    
    private void Awake()
    {
        ConfigureSpaceLighting();
    }
    
    private void Update()
    {
        // Sync skybox sun direction with directional light
        if (syncSkyboxSunWithLight && sunLight != null && spaceSkyboxMaterial != null)
        {
            UpdateSkyboxSunDirection();
        }
    }
    
    /// <summary>
    /// Configures all lighting settings for the space environment
    /// </summary>
    private void ConfigureSpaceLighting()
    {
        // Set ambient lighting to pure black (no ambient light in space!)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = 0f;
        
        // Disable ambient skybox reflection
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
        RenderSettings.reflectionIntensity = 0.3f; // Minimal reflections
        
        // Set black skybox if material is assigned
        if (useCustomSkybox && spaceSkyboxMaterial != null)
        {
            RenderSettings.skybox = spaceSkyboxMaterial;
        }
        else
        {
            RenderSettings.skybox = null; // No skybox = solid camera clear color
        }
        
        // Configure the directional light (sun)
        ConfigureSunLight();
        
        // Configure camera to show skybox
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            if (useCustomSkybox && spaceSkyboxMaterial != null)
            {
                // Use Skybox to see the sun
                mainCamera.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                // Use solid color if no skybox
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = Color.black;
            }
        }
        
        Debug.Log("[SpaceLightingManager] Space lighting configured: Black ambient, directional sun light, skybox enabled");
    }
    
    /// <summary>
    /// Configures the main directional light (sun)
    /// </summary>
    private void ConfigureSunLight()
    {
        // Try to find sun light if not assigned
        if (sunLight == null)
        {
            // Look for existing directional light
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sunLight = light;
                    break;
                }
            }
            
            // Create one if none exists
            if (sunLight == null)
            {
                GameObject sunObject = new GameObject("Directional Light (Sun)");
                sunLight = sunObject.AddComponent<Light>();
            }
        }
        
        // Configure the sun light
        sunLight.type = LightType.Directional;
        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;
        sunLight.transform.rotation = Quaternion.Euler(sunRotation);
        
        // Disable shadows for better performance (optional - you can enable if needed)
        sunLight.shadows = LightShadows.Hard;
        
        // Set light to render on all layers
        sunLight.cullingMask = -1;
        
        // Initial skybox sun direction sync
        if (syncSkyboxSunWithLight && spaceSkyboxMaterial != null)
        {
            UpdateSkyboxSunDirection();
        }
    }
    
    /// <summary>
    /// Updates skybox sun settings at runtime (useful for adjusting in Inspector during play mode)
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying && sunLight != null)
        {
            sunLight.color = sunColor;
            sunLight.intensity = sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(sunRotation);
            
            // Update skybox if syncing
            if (syncSkyboxSunWithLight && spaceSkyboxMaterial != null)
            {
                UpdateSkyboxSunDirection();
            }
        }
    }
    
    /// <summary>
    /// Enable/disable the sun light
    /// </summary>
    public void SetSunLightEnabled(bool enabled)
    {
        if (sunLight != null)
        {
            sunLight.enabled = enabled;
        }
    }
    
    /// <summary>
    /// Update sun light intensity at runtime
    /// </summary>
    public void SetSunIntensity(float intensity)
    {
        sunIntensity = intensity;
        if (sunLight != null)
        {
            sunLight.intensity = intensity;
        }
    }
    
    /// <summary>
    /// Update sun light color at runtime
    /// </summary>
    public void SetSunColor(Color color)
    {
        sunColor = color;
        if (sunLight != null)
        {
            sunLight.color = color;
        }
        
        // Also update skybox sun color
        if (spaceSkyboxMaterial != null && spaceSkyboxMaterial.HasProperty("_SunColor"))
        {
            spaceSkyboxMaterial.SetColor("_SunColor", color);
        }
    }
    
    /// <summary>
    /// Updates the skybox sun direction to match the directional light
    /// </summary>
    private void UpdateSkyboxSunDirection()
    {
        if (spaceSkyboxMaterial.HasProperty("_SunDirection"))
        {
            // Convert light forward direction to world space direction
            Vector3 sunDirection = -sunLight.transform.forward;
            spaceSkyboxMaterial.SetVector("_SunDirection", sunDirection);
        }
    }
    
    /// <summary>
    /// Set skybox sun size
    /// </summary>
    public void SetSkyboxSunSize(float size)
    {
        if (spaceSkyboxMaterial != null && spaceSkyboxMaterial.HasProperty("_SunSize"))
        {
            spaceSkyboxMaterial.SetFloat("_SunSize", size);
        }
    }
    
    /// <summary>
    /// Set skybox sun intensity (separate from directional light)
    /// </summary>
    public void SetSkyboxSunIntensity(float intensity)
    {
        if (spaceSkyboxMaterial != null && spaceSkyboxMaterial.HasProperty("_SunIntensity"))
        {
            spaceSkyboxMaterial.SetFloat("_SunIntensity", intensity);
        }
    }
}
