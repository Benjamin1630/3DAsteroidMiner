using UnityEngine;
using System.Collections;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Creates a heat mark/scorch effect on asteroids where the mining laser impacts.
    /// The effect fades out over time to simulate cooling.
    /// Uses a decal or projected texture to show melted/heated rock.
    /// </summary>
    public class LaserImpactEffect : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private float initialSize = 0.5f;
        [SerializeField] private float maxSize = 1.2f;
        [SerializeField] private Color hotColor = new Color(1f, 0.6f, 0.1f, 1f); // Molten orange
        [SerializeField] private Color coolColor = new Color(0.2f, 0.1f, 0.05f, 0.5f); // Dark scorch
        
        [Header("Animation")]
        [SerializeField] private float growDuration = 0.3f;
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Heat Glow")]
        [SerializeField] private bool useGlow = true;
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private float glowPulseSpeed = 3f;
        
        private Material impactMaterial;
        private Renderer impactRenderer;
        private Light impactLight;
        private float lifetime;
        private float elapsedTime = 0f;
        private bool isInitialized = false;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the impact effect with a specific duration.
        /// </summary>
        public void Initialize(float duration)
        {
            lifetime = duration;
            SetupImpactVisual();
            StartCoroutine(AnimateImpact());
            isInitialized = true;
        }
        
        private void Awake()
        {
            if (!isInitialized)
            {
                lifetime = 2f; // Default duration
                SetupImpactVisual();
                StartCoroutine(AnimateImpact());
            }
        }
        
        /// <summary>
        /// Setup the visual representation of the impact mark.
        /// Creates a smoke-like effect.
        /// </summary>
        private void SetupImpactVisual()
        {
            // Create a particle system for smoke effect instead of a quad
            GameObject psObj = new GameObject("ImpactSmoke");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // CRITICAL: Stop the system immediately to prevent "duration while playing" error
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(initialSize, maxSize);
            main.startColor = hotColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 5;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 3, 5, 0, 0.01f)
            });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            
            // Size over lifetime - grow and then shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 0.5f, 0.3f, 1f);
            sizeCurve.AddKey(1f, 0.8f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Color over lifetime - hot to cool smoke
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(hotColor, 0f),
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0.5f),
                    new GradientColorKey(coolColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            // Set renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2); // Fade
            renderer.material = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            impactRenderer = renderer;
            
            // Add point light for glow effect (optional, smaller)
            if (useGlow)
            {
                GameObject lightObj = new GameObject("ImpactGlow");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.forward * 0.1f;
                
                impactLight = lightObj.AddComponent<Light>();
                impactLight.type = LightType.Point;
                impactLight.color = hotColor;
                impactLight.intensity = glowIntensity * 0.5f; // Reduced intensity
                impactLight.range = maxSize * 1.5f; // Reduced range
                impactLight.renderMode = LightRenderMode.ForcePixel;
                impactLight.shadows = LightShadows.None;
            }
            
            // Start the particle system
            ps.Play();
        }
        
        /// <summary>
        /// Create material for the heat mark effect.
        /// </summary>
        private Material CreateImpactMaterial()
        {
            // Use Unlit shader with transparent rendering
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.color = hotColor;
            
            // Try to use Standard shader with emission for better effect
            Material standardMat = new Material(Shader.Find("Standard"));
            if (standardMat != null)
            {
                standardMat.SetFloat("_Mode", 3); // Transparent mode
                standardMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardMat.SetInt("_ZWrite", 0);
                standardMat.DisableKeyword("_ALPHATEST_ON");
                standardMat.EnableKeyword("_ALPHABLEND_ON");
                standardMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardMat.renderQueue = 3000;
                
                standardMat.SetColor("_Color", hotColor);
                standardMat.EnableKeyword("_EMISSION");
                standardMat.SetColor("_EmissionColor", hotColor * glowIntensity);
                standardMat.SetFloat("_Metallic", 0.2f);
                standardMat.SetFloat("_Glossiness", 0.8f);
                
                return standardMat;
            }
            
            return mat;
        }
        
        #endregion
        
        #region Animation
        
        /// <summary>
        /// Animate the impact effect from hot to cool to fade.
        /// Much simpler now that particles handle their own animation.
        /// Just fades the light.
        /// </summary>
        private IEnumerator AnimateImpact()
        {
            // Let the particle system handle its own animation
            // We just need to fade the light
            
            float elapsed = 0f;
            
            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / lifetime);
                
                // Fade light intensity
                if (impactLight != null)
                {
                    // Start bright, fade to nothing
                    float lightFade = 1f - progress;
                    // Add pulsing in the early phase
                    if (progress < 0.3f)
                    {
                        float pulse = Mathf.Sin(Time.time * glowPulseSpeed) * 0.2f + 0.8f;
                        lightFade *= pulse;
                    }
                    impactLight.intensity = glowIntensity * 0.5f * lightFade;
                }
                
                yield return null;
            }
            
            // Phase 3: Destroy
            Destroy(gameObject);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Orient the impact mark to align with a surface normal.
        /// </summary>
        public void SetOrientation(Vector3 surfaceNormal)
        {
            // Make the quad face outward from the asteroid surface
            transform.rotation = Quaternion.LookRotation(-surfaceNormal);
        }
        
        /// <summary>
        /// Attach the impact effect to a parent object (like an asteroid).
        /// </summary>
        public void AttachToSurface(Transform surface)
        {
            transform.SetParent(surface);
        }
        
        /// <summary>
        /// Set custom colors for the effect.
        /// </summary>
        public void SetColors(Color hot, Color cool)
        {
            hotColor = hot;
            coolColor = cool;
            
            if (impactMaterial != null)
            {
                impactMaterial.SetColor("_Color", hotColor);
            }
            
            if (impactLight != null)
            {
                impactLight.color = hotColor;
            }
        }
        
        /// <summary>
        /// Adjust the size of the impact mark.
        /// </summary>
        public void SetSize(float size)
        {
            maxSize = size;
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up material
            if (impactMaterial != null)
            {
                Destroy(impactMaterial);
            }
        }
        
        #endregion
    }
}
