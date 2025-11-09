using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AsteroidMiner.Core;
using AsteroidMiner.Entities;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Manages the ship's scanner system with visual pulse effect and asteroid detection.
    /// Scanner highlights all asteroids within range and respects upgrade levels for range and cooldown.
    /// </summary>
    public class ScannerSystem : MonoBehaviour
    {
        [Header("Scanner Settings")]
        [SerializeField] private float baseScanRange = 100f; // Fallback if ShipStats not found
        [SerializeField] private float baseCooldown = 10f; // Fallback if ShipStats not found
        [SerializeField] private float pulseDuration = 2f; // How long the pulse lasts
        [SerializeField] private float highlightDuration = 5f; // How long asteroids stay highlighted
        
        [Header("Visual Settings")]
        [SerializeField] private Material scanPulseMaterial; // Material with scan pulse shader
        [SerializeField] private Color pulseColor = new Color(0.2f, 0.8f, 1f, 1f); // Cyan holographic color
        [SerializeField] private AnimationCurve pulseExpansionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Audio Settings")]
        [SerializeField] private AudioClip scanActivateSound;
        [SerializeField] private AudioClip scanPulseSound;
        
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        
        [Header("References")]
        private ShipStats shipStats;
        [SerializeField] private Transform shipTransform;
        
        // Runtime state
        private float currentCooldown = 0f;
        private bool isScanning = false;
        private GameObject pulseObject;
        private Renderer pulseRenderer;
        private AudioSource audioSource;
        private List<Asteroid> highlightedAsteroids = new List<Asteroid>();
        private InputAction scanAction;
        
        // Public properties
        public bool CanScan => currentCooldown <= 0f && !isScanning;
        public float CooldownProgress => currentCooldown > 0f ? currentCooldown / GetScanCooldown() : 0f;
        public float CurrentRange => GetScanRange();
        
        // Events
        public event System.Action OnScanActivated;
        public event System.Action<int> OnAsteroidsDetected;
        
        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound (UI feedback)
            
            if (shipTransform == null)
            {
                shipTransform = transform;
            }
            
            // Get ShipStats component from ship
            if (shipTransform != null)
            {
                shipStats = shipTransform.GetComponent<ShipStats>();
                if (shipStats == null)
                {
                    Debug.LogError("ScannerSystem: No ShipStats component found on ship! Please add ShipStats component.");
                }
            }
            
            // Setup Input System action
            if (inputActions != null)
            {
                scanAction = inputActions.FindActionMap("Player").FindAction("Scan");
                if (scanAction == null)
                {
                    Debug.LogError("ScannerSystem: Could not find 'Scan' action in InputActionAsset!");
                }
            }
            else
            {
                Debug.LogError("ScannerSystem: InputActionAsset not assigned! Please assign InputSystem_Actions asset in inspector.");
            }
            
            CreatePulseObject();
        }
        
        private void OnEnable()
        {
            if (scanAction != null)
            {
                scanAction.performed += OnScanPerformed;
                scanAction.Enable();
            }
        }
        
        private void OnDisable()
        {
            if (scanAction != null)
            {
                scanAction.performed -= OnScanPerformed;
                scanAction.Disable();
            }
        }
        
        private void OnScanPerformed(InputAction.CallbackContext context)
        {
            ActivateScan();
        }
        
        private void Update()
        {
            // Update cooldown timer
            if (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown < 0f)
                {
                    currentCooldown = 0f;
                }
            }
        }
        
        /// <summary>
        /// Activate the scanner pulse. Called from Input System.
        /// </summary>
        public void ActivateScan()
        {
            if (!CanScan)
            {
#if UNITY_EDITOR
                Debug.Log($"Scanner on cooldown: {currentCooldown:F1}s remaining");
#endif
                return;
            }
            
            if (shipStats == null)
            {
                Debug.LogWarning("ScannerSystem: No ShipStats reference!");
                return;
            }
            
            // Don't allow scanning while docked
            if (shipStats.IsDocked())
            {
#if UNITY_EDITOR
                Debug.Log("Scanner cannot be used while docked");
#endif
                return;
            }
            
            StartCoroutine(ScanPulseRoutine());
        }
        
        /// <summary>
        /// Main scan pulse coroutine that handles visual effect and asteroid detection.
        /// </summary>
        private IEnumerator ScanPulseRoutine()
        {
            isScanning = true;
            currentCooldown = GetScanCooldown();
            
            // Play scan activation sound
            if (scanActivateSound != null)
            {
                audioSource.PlayOneShot(scanActivateSound);
            }
            
            OnScanActivated?.Invoke();
            
            // Position pulse at ship location
            pulseObject.transform.position = shipTransform.position;
            pulseObject.SetActive(true);
            
            float scanRange = GetScanRange();
            float elapsedTime = 0f;
            
            // Clear previously highlighted asteroids
            ClearHighlightedAsteroids();
            
            // Detect all asteroids in range
            List<Asteroid> detectedAsteroids = DetectAsteroids(scanRange);
            
#if UNITY_EDITOR
            Debug.Log($"Scanner activated! Range: {scanRange:F0}m, Cooldown: {GetScanCooldown():F1}s, Detected: {detectedAsteroids.Count} asteroids");
#endif
            
            OnAsteroidsDetected?.Invoke(detectedAsteroids.Count);
            
            // Expand pulse sphere
            while (elapsedTime < pulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / pulseDuration;
                float curvedProgress = pulseExpansionCurve.Evaluate(progress);
                
                // Scale pulse sphere - use currentRadius directly since icosphere has radius = 1
                float currentRadius = scanRange * curvedProgress;
                pulseObject.transform.localScale = Vector3.one * currentRadius;
                
                // Update shader properties - pass the ACTUAL radius based on scale
                if (pulseRenderer != null && pulseRenderer.material != null)
                {
                    pulseRenderer.material.SetFloat("_Progress", progress);
                    pulseRenderer.material.SetFloat("_Radius", currentRadius); // Match the scale
                    pulseRenderer.material.SetColor("_Color", pulseColor);
                }
                
                // Highlight asteroids as pulse reaches them
                foreach (Asteroid asteroid in detectedAsteroids)
                {
                    if (asteroid != null && !highlightedAsteroids.Contains(asteroid))
                    {
                        float distanceToAsteroid = Vector3.Distance(shipTransform.position, asteroid.transform.position);
                        if (distanceToAsteroid <= currentRadius)
                        {
                            HighlightAsteroid(asteroid);
                            highlightedAsteroids.Add(asteroid);
                        }
                    }
                }
                
                yield return null;
            }
            
            // Hide pulse object
            pulseObject.SetActive(false);
            
            // Start unhighlighting coroutine
            StartCoroutine(UnhighlightAsteroidsAfterDelay());
            
            isScanning = false;
        }
        
        /// <summary>
        /// Detect all asteroids within scan range.
        /// </summary>
        private List<Asteroid> DetectAsteroids(float range)
        {
            List<Asteroid> detected = new List<Asteroid>();
            
            // Use OverlapSphere to find all colliders in range
            Collider[] colliders = Physics.OverlapSphere(shipTransform.position, range);
            
            foreach (Collider col in colliders)
            {
                // Check if collider belongs to an asteroid (check parent since collider is on child)
                Asteroid asteroid = col.GetComponentInParent<Asteroid>();
                if (asteroid != null && !detected.Contains(asteroid))
                {
                    detected.Add(asteroid);
                }
            }
            
            return detected;
        }
        
        /// <summary>
        /// Highlight an asteroid with emission glow.
        /// </summary>
        private void HighlightAsteroid(Asteroid asteroid)
        {
            if (asteroid != null)
            {
                asteroid.SetScanned(true, highlightDuration);
            }
        }
        
        /// <summary>
        /// Clear highlights from all previously scanned asteroids.
        /// </summary>
        private void ClearHighlightedAsteroids()
        {
            foreach (Asteroid asteroid in highlightedAsteroids)
            {
                if (asteroid != null)
                {
                    asteroid.SetScanned(false, 0f);
                }
            }
            highlightedAsteroids.Clear();
        }
        
        /// <summary>
        /// Unhighlight asteroids after delay.
        /// </summary>
        private IEnumerator UnhighlightAsteroidsAfterDelay()
        {
            yield return new WaitForSeconds(highlightDuration);
            ClearHighlightedAsteroids();
        }
        
        /// <summary>
        /// Create the pulse sphere GameObject with shader.
        /// </summary>
        private void CreatePulseObject()
        {
            // Create GameObject with icosphere mesh for uniform dot distribution
            pulseObject = new GameObject("ScanPulse");
            pulseObject.transform.SetParent(transform);
            pulseObject.transform.localPosition = Vector3.zero;
            pulseObject.transform.localScale = Vector3.one;
            
            // Add mesh components
            MeshFilter meshFilter = pulseObject.AddComponent<MeshFilter>();
            pulseRenderer = pulseObject.AddComponent<MeshRenderer>();
            
            // Generate icosphere mesh (subdivisions = 2 gives good balance)
            // Icosphere has uniform triangle distribution unlike Unity's default sphere
            meshFilter.mesh = IcosphereGenerator.Create(1f, 2);
            
            if (scanPulseMaterial != null)
            {
                pulseRenderer.material = scanPulseMaterial;
                
                // Ensure preview mode is OFF for runtime
                pulseRenderer.material.SetFloat("_PreviewMode", 0f);
                
                // Set initial shader properties
                pulseRenderer.material.SetFloat("_Progress", 0f);
                pulseRenderer.material.SetFloat("_Radius", 1f);
                pulseRenderer.material.SetColor("_Color", pulseColor);
                
                Debug.Log($"ScanPulse material assigned. Shader: {pulseRenderer.material.shader.name}");
            }
            else
            {
                // Create a basic transparent material if no custom shader assigned
                Material defaultMat = new Material(Shader.Find("Standard"));
                defaultMat.SetFloat("_Mode", 3); // Transparent rendering mode
                defaultMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                defaultMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                defaultMat.SetInt("_ZWrite", 0);
                defaultMat.DisableKeyword("_ALPHATEST_ON");
                defaultMat.EnableKeyword("_ALPHABLEND_ON");
                defaultMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                defaultMat.renderQueue = 3000;
                defaultMat.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0.3f);
                pulseRenderer.material = defaultMat;
                
                Debug.LogWarning("ScannerSystem: No scan pulse material assigned, using default transparent material.");
            }
            
            pulseObject.SetActive(false);
        }
        
        /// <summary>
        /// Get current scan range based on upgrade level.
        /// Uses ShipStats pre-calculated value.
        /// </summary>
        private float GetScanRange()
        {
            if (shipStats == null) return baseScanRange;
            return shipStats.GetScannerRange();
        }
        
        /// <summary>
        /// Get current scan cooldown based on upgrade level.
        /// Uses ShipStats pre-calculated value.
        /// </summary>
        private float GetScanCooldown()
        {
            if (shipStats == null) return baseCooldown;
            return shipStats.GetScannerCooldown();
        }
        
        /// <summary>
        /// Visualize scan range in editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (shipTransform == null) shipTransform = transform;
            
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(shipTransform.position, GetScanRange());
            
            // Draw cooldown indicator
            if (currentCooldown > 0f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(shipTransform.position, 10f);
            }
        }
    }
}
