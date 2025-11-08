using UnityEngine;
using AsteroidMiner.Core;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Handles visual feedback for the player ship including:
    /// - Engine thrust effects
    /// - Ship rotation smoothing
    /// - Damage effects
    /// - Speed indicators
    /// </summary>
    public class ShipVisuals : MonoBehaviour
    {
        [Header("Engine Effects")]
        [SerializeField] private ParticleSystem leftEngineParticles;
        [SerializeField] private ParticleSystem rightEngineParticles;
        [SerializeField] private Light leftEngineLight;
        [SerializeField] private Light rightEngineLight;
        [SerializeField] private float maxEngineLightIntensity = 2f;
        
        [Header("Ship Model")]
        [SerializeField] private Transform shipModel;
        [SerializeField] private float modelTiltAmount = 15f;
        [SerializeField] private float modelTiltSpeed = 5f;
        
        [Header("Speed Lines")]
        [SerializeField] private ParticleSystem speedLinesParticles;
        [SerializeField] private float speedLinesThreshold = 15f;
        
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameState gameState;
        
        // State
        private Vector2 lastMoveInput = Vector2.zero;
        private float currentTiltX = 0f;
        private float currentTiltZ = 0f;
        private float lastEngineIntensity = -1f; // Track last intensity to avoid redundant updates
        private float lastSpeedLinesIntensity = -1f;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Get component references if not assigned
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            // Initialize particle systems
            if (leftEngineParticles != null)
                leftEngineParticles.Stop();
            
            if (rightEngineParticles != null)
                rightEngineParticles.Stop();
            
            if (speedLinesParticles != null)
                speedLinesParticles.Stop();
            
            // Initialize lights
            if (leftEngineLight != null)
                leftEngineLight.intensity = 0f;
            
            if (rightEngineLight != null)
                rightEngineLight.intensity = 0f;
        }
        
        private void Update()
        {
            if (playerController == null) return;
            
            UpdateEngineEffects();
            UpdateShipTilt();
            UpdateSpeedLines();
        }
        
        #endregion
        
        #region Visual Effects
        
        /// <summary>
        /// Update engine particle effects and lights based on movement.
        /// </summary>
        private void UpdateEngineEffects()
        {
            bool isMoving = playerController.IsMoving;
            float currentSpeed = playerController.CurrentSpeed;
            
            // Calculate engine intensity based on speed
            float engineIntensity = Mathf.Clamp01(currentSpeed / 20f);
            
            // Only update if intensity changed significantly (>5%)
            bool intensityChanged = Mathf.Abs(engineIntensity - lastEngineIntensity) > 0.05f;
            
            // Update particles
            if (isMoving && gameState != null && gameState.HasFuel())
            {
                if (leftEngineParticles != null && !leftEngineParticles.isPlaying)
                    leftEngineParticles.Play();
                
                if (rightEngineParticles != null && !rightEngineParticles.isPlaying)
                    rightEngineParticles.Play();
                
                // Adjust emission rate only when intensity changed significantly
                if (intensityChanged)
                {
                    UpdateParticleEmission(leftEngineParticles, engineIntensity);
                    UpdateParticleEmission(rightEngineParticles, engineIntensity);
                    lastEngineIntensity = engineIntensity;
                }
            }
            else
            {
                if (leftEngineParticles != null && leftEngineParticles.isPlaying)
                    leftEngineParticles.Stop();
                
                if (rightEngineParticles != null && rightEngineParticles.isPlaying)
                    rightEngineParticles.Stop();
                
                lastEngineIntensity = 0f;
            }
            
            // Update engine lights (smooth lerp is fine here)
            float targetLightIntensity = isMoving ? engineIntensity * maxEngineLightIntensity : 0f;
            
            if (leftEngineLight != null)
                leftEngineLight.intensity = Mathf.Lerp(leftEngineLight.intensity, targetLightIntensity, Time.deltaTime * 5f);
            
            if (rightEngineLight != null)
                rightEngineLight.intensity = Mathf.Lerp(rightEngineLight.intensity, targetLightIntensity, Time.deltaTime * 5f);
        }
        
        /// <summary>
        /// Update particle emission rate based on intensity.
        /// </summary>
        private void UpdateParticleEmission(ParticleSystem particles, float intensity)
        {
            if (particles == null) return;
            
            var emission = particles.emission;
            emission.rateOverTime = intensity * 50f; // Max 50 particles per second
        }
        
        /// <summary>
        /// Tilt the ship model based on movement direction for visual feedback.
        /// </summary>
        private void UpdateShipTilt()
        {
            if (shipModel == null || playerController == null) return;
            
            // Get movement input (would need to expose this from PlayerController)
            // For now, use velocity direction
            Vector3 velocity = playerController.Velocity;
            
            // Calculate target tilt
            float targetTiltZ = 0f;
            float targetTiltX = 0f;
            
            if (velocity.sqrMagnitude > 0.01f)
            {
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                targetTiltZ = -localVelocity.x * modelTiltAmount;
                targetTiltX = localVelocity.z * modelTiltAmount * 0.5f;
            }
            
            // Smooth tilt
            currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, Time.deltaTime * modelTiltSpeed);
            currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, Time.deltaTime * modelTiltSpeed);
            
            // Apply tilt
            shipModel.localRotation = Quaternion.Euler(currentTiltX, 0f, currentTiltZ);
        }
        
        /// <summary>
        /// Show speed lines particle effect when moving fast.
        /// </summary>
        private void UpdateSpeedLines()
        {
            if (speedLinesParticles == null || playerController == null) return;
            
            float currentSpeed = playerController.CurrentSpeed;
            
            if (currentSpeed >= speedLinesThreshold)
            {
                if (!speedLinesParticles.isPlaying)
                    speedLinesParticles.Play();
                
                // Adjust intensity based on speed
                float intensity = Mathf.Clamp01((currentSpeed - speedLinesThreshold) / speedLinesThreshold);
                
                // Only update emission if intensity changed significantly (>10%)
                if (Mathf.Abs(intensity - lastSpeedLinesIntensity) > 0.1f)
                {
                    var emission = speedLinesParticles.emission;
                    emission.rateOverTime = intensity * 100f;
                    lastSpeedLinesIntensity = intensity;
                }
            }
            else
            {
                if (speedLinesParticles.isPlaying)
                    speedLinesParticles.Stop();
                
                lastSpeedLinesIntensity = 0f;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set the game state reference.
        /// </summary>
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        
        /// <summary>
        /// Play damage effect when ship takes damage.
        /// </summary>
        public void PlayDamageEffect()
        {
            // Could add screen shake, flash effect, etc.
            // Will implement when we have camera controller
            Debug.Log("Ship took damage!");
        }
        
        /// <summary>
        /// Play docking effect when arriving at station.
        /// </summary>
        public void PlayDockingEffect()
        {
            // Stop all engine effects
            if (leftEngineParticles != null)
                leftEngineParticles.Stop();
            
            if (rightEngineParticles != null)
                rightEngineParticles.Stop();
            
            if (speedLinesParticles != null)
                speedLinesParticles.Stop();
            
            Debug.Log("Docking...");
        }
        
        #endregion
    }
}
