using UnityEngine;
using System.Collections;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Enhanced mining laser with particle effects and animations.
    /// Features startup, looping, and shutdown animations for realistic mining operations.
    /// Creates intense heat-based visuals with sparks, molten material, and heat distortion.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class EnhancedMiningLaser : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private LineRenderer lineRenderer;
        
        [Header("Visual Settings")]
        [SerializeField] private Color baseColor = new Color(1f, 0.4f, 0f, 1f); // Hot orange
        [SerializeField] private Color hotColor = new Color(1f, 0.9f, 0.4f, 1f); // Yellow-white hot
        [SerializeField] private float baseWidth = 0.15f;
        [SerializeField] private float maxWidth = 0.25f; // Used for impact light range calculation
        
        [Header("Animation Settings")]
        [SerializeField] private float startupDuration = 0.3f;
        [SerializeField] private float shutdownDuration = 0.2f;
        [SerializeField] private AnimationCurve startupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve shutdownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Pulse Settings")]
        [SerializeField] private float pulseSpeed = 8f;
        [SerializeField] private float pulseMagnitude = 0.2f;
        [SerializeField] private float intensityVariation = 0.15f;
        
        [Header("Material Settings")]
        [SerializeField] private Material laserMaterial;
        
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem startupParticles;
        [SerializeField] private ParticleSystem loopingParticles;
        [SerializeField] private ParticleSystem shutdownParticles;
        [SerializeField] private ParticleSystem sparkParticles;
        [SerializeField] private ParticleSystem heatDistortionParticles;
        
        [Header("Impact Effect")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private float impactEffectDuration = 2f;
        
        // State
        private LaserState currentState = LaserState.Inactive;
        private float animationProgress = 0f;
        private float pulseTimer = 0f;
        private Vector3 currentStart;
        private Vector3 currentEnd;
        private GameObject currentImpactEffect;
        private Coroutine currentAnimation;
        
        private enum LaserState
        {
            Inactive,
            StartingUp,
            Active,
            ShuttingDown
        }
        
        #region Initialization
        
        private void Awake()
        {
            SetupLineRenderer();
            SetupParticleSystems();
            SetInactive();
        }
        
        /// <summary>
        /// Configure the LineRenderer for hot laser beam appearance.
        /// </summary>
        private void SetupLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = baseWidth;
            lineRenderer.endWidth = baseWidth * 0.3f;
            
            // Create hot gradient (orange to yellow-white)
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(hotColor, 0.5f),
                    new GradientColorKey(baseColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 1f)
                }
            );
            lineRenderer.colorGradient = gradient;
            
            // Material setup
            if (laserMaterial != null)
            {
                // Use custom material if assigned
                lineRenderer.material = laserMaterial;
            }
            else
            {
                // Use Unity's built-in default line material (respects color gradient)
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                // Don't need to set colors - LineRenderer gradient handles it
            }
            
            // Rendering settings
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch; // Changed from Tile for better appearance
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sortingOrder = 100;
            lineRenderer.allowOcclusionWhenDynamic = false;
            
            // Disable lighting data generation (not needed for unlit shaders)
            lineRenderer.generateLightingData = false;
        }
        
        /// <summary>
        /// Setup particle systems if not assigned via Inspector.
        /// </summary>
        private void SetupParticleSystems()
        {
            // Create particle systems if they don't exist
            if (startupParticles == null)
                startupParticles = CreateStartupParticles();
            
            if (loopingParticles == null)
                loopingParticles = CreateLoopingParticles();
            
            if (shutdownParticles == null)
                shutdownParticles = CreateShutdownParticles();
            
            if (sparkParticles == null)
                sparkParticles = CreateSparkParticles();
            
            if (heatDistortionParticles == null)
                heatDistortionParticles = CreateHeatDistortionParticles();
            
            // Stop all particles initially
            StopAllParticles();
        }
        
        /// <summary>
        /// Create default hot laser material with emission.
        /// NOTE: This is only used if you want to assign a custom material manually.
        /// By default, the LineRenderer uses "Sprites/Default" which respects the color gradient.
        /// </summary>
        private Material CreateHotLaserMaterial()
        {
            // Simple material that works with LineRenderer color gradient
            Material mat = new Material(Shader.Find("Sprites/Default"));
            // Don't set color here - let LineRenderer's gradient control it
            return mat;
        }
        
        #endregion
        
        #region Particle System Creation
        
        /// <summary>
        /// Get or create a simple particle material that works everywhere.
        /// </summary>
        private Material GetDefaultParticleMaterial()
        {
            // Use Unity's built-in particle shader
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetFloat("_Mode", 2); // Fade mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            return mat;
        }
        
        private ParticleSystem CreateStartupParticles()
        {
            GameObject psObj = new GameObject("StartupParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the particle system first to prevent errors
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false; // Disable auto-play
            main.duration = startupDuration;
            main.loop = false;
            main.startLifetime = 0.3f;
            main.startSpeed = 5f;
            main.startSize = 0.2f;
            main.startColor = new Color(1f, 0.6f, 0.2f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = ps.emission;
            emission.rateOverTime = 50f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;
            
            // Set renderer to use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetDefaultParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            return ps;
        }
        
        private ParticleSystem CreateLoopingParticles()
        {
            GameObject psObj = new GameObject("LoopingParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the particle system first to prevent errors
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false; // Disable auto-play
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = 0.15f;
            main.startColor = new Color(1f, 0.8f, 0.3f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = ps.emission;
            emission.rateOverTime = 30f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.05f;
            
            // Set renderer to use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetDefaultParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            return ps;
        }
        
        private ParticleSystem CreateShutdownParticles()
        {
            GameObject psObj = new GameObject("ShutdownParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the particle system first to prevent errors
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false; // Disable auto-play
            main.duration = shutdownDuration;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = 3f;
            main.startSize = 0.25f;
            main.startColor = new Color(1f, 0.5f, 0f, 0.6f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = ps.emission;
            emission.rateOverTime = 40f;
            
            // Set renderer to use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetDefaultParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            return ps;
        }
        
        private ParticleSystem CreateSparkParticles()
        {
            GameObject psObj = new GameObject("SparkParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the particle system first to prevent errors
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false; // Disable auto-play
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            // Bright sparks - will be colored based on asteroid material later
            main.startColor = new Color(1f, 0.9f, 0.6f, 1f); // Bright yellow-white default
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;
            
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            
            // Color over lifetime - fade from bright to dim
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.3f, 0.1f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            // Set renderer to use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetDefaultParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            return ps;
        }
        
        private ParticleSystem CreateHeatDistortionParticles()
        {
            GameObject psObj = new GameObject("HeatDistortionParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            
            // Stop the particle system first to prevent errors
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = ps.main;
            main.playOnAwake = false; // Disable auto-play
            main.loop = true;
            main.startLifetime = 1f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            // Grey semi-transparent for heat shimmer effect
            main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.15f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = ps.emission;
            emission.rateOverTime = 15f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.2f;
            
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, 1.5f));
            
            // Fade out over lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.4f, 0.4f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.15f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            // Set renderer to use default particle material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetDefaultParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            return ps;
        }
        
        private void StopAllParticles()
        {
            if (startupParticles != null) startupParticles.Stop();
            if (loopingParticles != null) loopingParticles.Stop();
            if (shutdownParticles != null) shutdownParticles.Stop();
            if (sparkParticles != null) sparkParticles.Stop();
            if (heatDistortionParticles != null) heatDistortionParticles.Stop();
        }
        
        #endregion
        
        #region Update
        
        private void Update()
        {
            if (currentState == LaserState.Active)
            {
                UpdateActiveLaser();
            }
        }
        
        /// <summary>
        /// Update active laser with pulsing and intensity variation.
        /// </summary>
        private void UpdateActiveLaser()
        {
            // Pulse effect
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseMagnitude;
            float intensityVar = Mathf.PerlinNoise(Time.time * 2f, 0f) * intensityVariation;
            
            // Calculate width with pulse, clamped to maxWidth
            float currentWidth = Mathf.Min(baseWidth + (baseWidth * pulse), maxWidth);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.3f;
            
            // Update color gradient intensity for pulsing effect
            float intensityMultiplier = 1f + intensityVar;
            Color pulsingBase = baseColor * intensityMultiplier;
            Color pulsingHot = hotColor * intensityMultiplier;
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(pulsingBase, 0f),
                    new GradientColorKey(pulsingHot, 0.5f),
                    new GradientColorKey(pulsingBase, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 1f)
                }
            );
            lineRenderer.colorGradient = gradient;
            
            // Update particle system positions to target
            UpdateParticlePositions();
        }
        
        /// <summary>
        /// Position particle systems at the laser impact point.
        /// </summary>
        private void UpdateParticlePositions()
        {
            if (sparkParticles != null)
            {
                sparkParticles.transform.position = currentEnd;
            }
            if (heatDistortionParticles != null)
            {
                heatDistortionParticles.transform.position = currentEnd;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Start the laser with startup animation.
        /// </summary>
        public void StartLaser(Vector3 startPosition, Vector3 endPosition)
        {
            if (currentState != LaserState.Inactive)
            {
                StopLaser();
            }
            
            currentStart = startPosition;
            currentEnd = endPosition;
            
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(StartupAnimation());
        }
        
        /// <summary>
        /// Stop the laser with shutdown animation.
        /// </summary>
        public void StopLaser()
        {
            if (currentState == LaserState.Inactive)
                return;
            
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            currentAnimation = StartCoroutine(ShutdownAnimation());
        }
        
        /// <summary>
        /// Update laser beam positions while active.
        /// </summary>
        public void UpdateLaser(Vector3 startPosition, Vector3 endPosition)
        {
            currentStart = startPosition;
            currentEnd = endPosition;
            
            if (lineRenderer != null && currentState != LaserState.Inactive)
            {
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, endPosition);
            }
            
            // Update impact effect position
            if (currentImpactEffect != null)
            {
                currentImpactEffect.transform.position = endPosition;
            }
        }
        
        /// <summary>
        /// Check if laser is currently active.
        /// </summary>
        public bool IsActive()
        {
            return currentState == LaserState.Active || currentState == LaserState.StartingUp;
        }
        
        /// <summary>
        /// Force immediate deactivation without animation.
        /// </summary>
        public void SetInactive()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            
            currentState = LaserState.Inactive;
            lineRenderer.enabled = false;
            StopAllParticles();
            
            if (currentImpactEffect != null)
            {
                Destroy(currentImpactEffect);
                currentImpactEffect = null;
            }
        }
        
        #endregion
        
        #region Animations
        
        private IEnumerator StartupAnimation()
        {
            currentState = LaserState.StartingUp;
            animationProgress = 0f;
            
            // Start particles
            if (startupParticles != null)
            {
                startupParticles.transform.position = currentStart;
                startupParticles.Play();
            }
            
            lineRenderer.enabled = true;
            
            float elapsed = 0f;
            while (elapsed < startupDuration)
            {
                elapsed += Time.deltaTime;
                animationProgress = Mathf.Clamp01(elapsed / startupDuration);
                float curveValue = startupCurve.Evaluate(animationProgress);
                
                // Animate width
                float width = baseWidth * curveValue;
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width * 0.3f;
                
                // Animate alpha
                Color currentColor = Color.Lerp(Color.clear, baseColor, curveValue);
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(currentColor, 0f),
                        new GradientColorKey(hotColor, 0.5f),
                        new GradientColorKey(currentColor, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(curveValue, 0f),
                        new GradientAlphaKey(curveValue * 0.8f, 1f)
                    }
                );
                lineRenderer.colorGradient = gradient;
                
                // Update positions
                lineRenderer.SetPosition(0, currentStart);
                lineRenderer.SetPosition(1, currentEnd);
                
                yield return null;
            }
            
            // Transition to active state
            currentState = LaserState.Active;
            
            // Start looping particles
            if (loopingParticles != null)
            {
                loopingParticles.transform.position = currentStart;
                loopingParticles.Play();
            }
            if (sparkParticles != null)
            {
                sparkParticles.transform.position = currentEnd;
                sparkParticles.Play();
            }
            if (heatDistortionParticles != null)
            {
                heatDistortionParticles.transform.position = currentEnd;
                heatDistortionParticles.Play();
            }
            
            // Create impact effect on asteroid
            CreateImpactEffect();
            
            currentAnimation = null;
        }
        
        private IEnumerator ShutdownAnimation()
        {
            LaserState previousState = currentState;
            currentState = LaserState.ShuttingDown;
            animationProgress = 0f;
            
            // Stop looping particles
            if (loopingParticles != null) loopingParticles.Stop();
            if (sparkParticles != null) sparkParticles.Stop();
            if (heatDistortionParticles != null) heatDistortionParticles.Stop();
            
            // Start shutdown particles
            if (shutdownParticles != null)
            {
                shutdownParticles.transform.position = currentEnd;
                shutdownParticles.Play();
            }
            
            float elapsed = 0f;
            while (elapsed < shutdownDuration)
            {
                elapsed += Time.deltaTime;
                animationProgress = Mathf.Clamp01(elapsed / shutdownDuration);
                float curveValue = shutdownCurve.Evaluate(animationProgress);
                
                // Animate width
                float width = baseWidth * curveValue;
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width * 0.3f;
                
                // Animate alpha
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(baseColor, 0f),
                        new GradientColorKey(hotColor, 0.5f),
                        new GradientColorKey(baseColor, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(curveValue, 0f),
                        new GradientAlphaKey(curveValue * 0.8f, 1f)
                    }
                );
                lineRenderer.colorGradient = gradient;
                
                yield return null;
            }
            
            // Fully deactivate
            SetInactive();
            
            currentAnimation = null;
        }
        
        private void CreateImpactEffect()
        {
            if (impactEffectPrefab != null)
            {
                currentImpactEffect = Instantiate(impactEffectPrefab, currentEnd, Quaternion.identity);
                
                // Auto-destroy after duration
                Destroy(currentImpactEffect, impactEffectDuration);
            }
            else
            {
                // Create procedural impact effect
                currentImpactEffect = CreateProceduralImpactEffect();
            }
        }
        
        private GameObject CreateProceduralImpactEffect()
        {
            GameObject impactObj = new GameObject("LaserImpactEffect");
            impactObj.transform.position = currentEnd;
            
            // Add heat mark component
            LaserImpactEffect impactEffect = impactObj.AddComponent<LaserImpactEffect>();
            impactEffect.Initialize(impactEffectDuration);
            
            return impactObj;
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            
            if (currentImpactEffect != null)
            {
                Destroy(currentImpactEffect);
            }
        }
        
        #endregion
    }
}
