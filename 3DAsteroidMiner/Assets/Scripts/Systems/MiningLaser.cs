using UnityEngine;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Manages a single mining laser beam using LineRenderer.
    /// Provides visual feedback for mining operations.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class MiningLaser : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        
        [Header("Visual Settings")]
        [SerializeField] private Color baseColor = new Color(0f, 1f, 0f, 0.8f);
        [SerializeField] private float baseWidth = 0.2f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMagnitude = 0.3f;
        
        [Header("Material Settings")]
        [SerializeField] private Material laserMaterial;
        
        private float pulseTimer = 0f;
        private bool isInitialized = false;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the laser with color and width settings.
        /// </summary>
        public void Initialize(Color color, float width)
        {
            baseColor = color;
            baseWidth = width;
            
            SetupLineRenderer();
            isInitialized = true;
        }
        
        private void Awake()
        {
            if (!isInitialized)
            {
                SetupLineRenderer();
            }
        }
        
        /// <summary>
        /// Configure the LineRenderer component.
        /// </summary>
        private void SetupLineRenderer()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            // Basic LineRenderer settings
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = baseWidth;
            lineRenderer.endWidth = baseWidth * 0.5f; // Taper toward asteroid
            
            // Color gradient (solid color with transparency)
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(baseColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(baseColor.a, 0f),
                    new GradientAlphaKey(baseColor.a * 0.5f, 1f)
                }
            );
            lineRenderer.colorGradient = gradient;
            
            // Material setup
            if (laserMaterial != null)
            {
                lineRenderer.material = laserMaterial;
            }
            else
            {
                // Create default material if none provided
                lineRenderer.material = CreateDefaultLaserMaterial();
            }
            
            // Rendering settings
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View; // Always face camera
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            
            // Sorting and rendering
            lineRenderer.sortingOrder = 100; // Render on top
            lineRenderer.allowOcclusionWhenDynamic = false; // Always visible
        }
        
        /// <summary>
        /// Create a default glowing laser material.
        /// </summary>
        private Material CreateDefaultLaserMaterial()
        {
            // Use Unity's default Sprites/Default shader or create simple unlit material
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = baseColor;
            
            // Try to enable emission if using Standard shader
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", baseColor * 2f);
            }
            
            return mat;
        }
        
        #endregion
        
        #region Update
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // Animate pulse effect
            UpdatePulse();
        }
        
        /// <summary>
        /// Animate laser width pulsing effect.
        /// </summary>
        private void UpdatePulse()
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseMagnitude;
            
            float currentWidth = baseWidth + (baseWidth * pulse);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.5f;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Update laser beam positions from origin to target.
        /// </summary>
        public void UpdateLaser(Vector3 startPosition, Vector3 endPosition)
        {
            if (lineRenderer == null) return;
            
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        
        /// <summary>
        /// Set laser visibility.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = visible;
            }
        }
        
        /// <summary>
        /// Change laser color dynamically.
        /// </summary>
        public void SetColor(Color color)
        {
            baseColor = color;
            
            if (lineRenderer != null)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(color, 0f),
                        new GradientColorKey(color, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(color.a, 0f),
                        new GradientAlphaKey(color.a * 0.5f, 1f)
                    }
                );
                lineRenderer.colorGradient = gradient;
            }
        }
        
        /// <summary>
        /// Change laser width dynamically.
        /// </summary>
        public void SetWidth(float width)
        {
            baseWidth = width;
            
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width * 0.5f;
            }
        }
        
        #endregion
    }
}
