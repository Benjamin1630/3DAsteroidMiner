using UnityEngine;
using UnityEngine.InputSystem;
using AsteroidMiner.Core;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Handles player ship movement using 6 Degrees of Freedom (6DoF) controls.
    /// Implements space flight physics similar to Elite Dangerous, Star Citizen, and No Man's Sky.
    /// Uses Unity's new Input System for all input handling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))] // Ensure player has collision detection
    public class PlayerController : MonoBehaviour
    {
        [Header("Translation Settings")]
        [SerializeField] private float baseThrust = 20f;
        [SerializeField] private float strafeThrust = 15f;
        [SerializeField] private float verticalThrust = 15f;
        [SerializeField] private float boostMultiplier = 2f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float pitchSpeed = 60f;
        [SerializeField] private float yawSpeed = 60f;
        [SerializeField] private float rollSpeed = 90f;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] [Range(0.1f, 2f)] private float rotationSensitivity = 1f;
        
        [Header("Physics Settings")]
        [Tooltip("Linear drag (0 = realistic space physics with no friction, >0 = easier control with auto-slowdown)")]
        [SerializeField] private float linearDrag = 0f;
        [Tooltip("Angular drag (0 = realistic space physics with perpetual spin, >0 = easier control with auto-stabilization)")]
        [SerializeField] private float angularDrag = 0f;
        [SerializeField] [Range(0.1f, 2f)] private float movementSensitivity = 1f;
        
        [Header("Fuel Settings")]
        [SerializeField] private float baseFuelConsumptionRate = 0.5f;
        [SerializeField] private float boostFuelMultiplier = 3f;
        
        [Header("References")]
        [SerializeField] private GameState gameState;
        
        private Rigidbody rb;
        private PlayerInputHandler inputHandler;
        
        private float pitchInput = 0f;
        private float yawInput = 0f;
        private float rollInput = 0f;
        private float thrustForwardInput = 0f;
        private float thrustStrafeInput = 0f;
        private float thrustVerticalInput = 0f;
        private bool isBoosting = false;
        
        private Vector3 lastPosition;
        
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : 0f;
        public bool IsMoving => CurrentSpeed > 0.1f;
        public bool IsBoosting => isBoosting;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inputHandler = GetComponent<PlayerInputHandler>();
            
            rb.useGravity = false;
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
            rb.constraints = RigidbodyConstraints.None;
            
            lastPosition = transform.position;
        }
        
        private void Start()
        {
            if (gameState == null)
            {
                gameState = GameState.CreateNew();
                Debug.LogWarning("PlayerController: No GameState assigned, using temporary state for testing.");
            }
            
            if (inputHandler != null)
            {
                inputHandler.OnPitch += HandlePitchInput;
                inputHandler.OnYaw += HandleYawInput;
                inputHandler.OnRoll += HandleRollInput;
                inputHandler.OnThrustForward += HandleThrustForwardInput;
                inputHandler.OnThrustStrafe += HandleThrustStrafeInput;
                inputHandler.OnThrustVertical += HandleThrustVerticalInput;
                inputHandler.OnBoost += HandleBoostInput;
            }
        }
        
        private void FixedUpdate()
        {
            if (gameState == null || gameState.isDocked) return;
            
            ApplyRotation();
            ApplyTranslation();
            TrackDistance();
        }
        
        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPitch -= HandlePitchInput;
                inputHandler.OnYaw -= HandleYawInput;
                inputHandler.OnRoll -= HandleRollInput;
                inputHandler.OnThrustForward -= HandleThrustForwardInput;
                inputHandler.OnThrustStrafe -= HandleThrustStrafeInput;
                inputHandler.OnThrustVertical -= HandleThrustVerticalInput;
                inputHandler.OnBoost -= HandleBoostInput;
            }
        }
        
        private void HandlePitchInput(float input) { pitchInput = input; }
        private void HandleYawInput(float input) { yawInput = input; }
        private void HandleRollInput(float input) { rollInput = input; }
        private void HandleThrustForwardInput(float input) { thrustForwardInput = input; }
        private void HandleThrustStrafeInput(float input) { thrustStrafeInput = input; }
        private void HandleThrustVerticalInput(float input) { thrustVerticalInput = input; }
        private void HandleBoostInput(bool boosting) { isBoosting = boosting; }
        
        private void ApplyRotation()
        {
            if (!gameState.HasFuel()) return;
            
            float pitch = pitchSpeed * rotationSensitivity;
            float yaw = yawSpeed * rotationSensitivity;
            float roll = rollSpeed * rotationSensitivity;
            
            if (inputHandler.IsMouseFlightEnabled())
            {
                pitch *= mouseSensitivity;
                yaw *= mouseSensitivity;
            }
            
            Vector3 torque = new Vector3(-pitchInput * pitch, yawInput * yaw, -rollInput * roll);
            
            if (torque.sqrMagnitude > 0.01f)
            {
                // ForceMode.Acceleration handles deltaTime internally, so we apply it directly
                rb.AddRelativeTorque(torque, ForceMode.Acceleration);
                
                // Fuel consumption uses deltaTime
                float rotationFuel = baseFuelConsumptionRate * 0.2f * torque.magnitude / 100f * Time.fixedDeltaTime;
                gameState.ConsumeFuel(rotationFuel);
            }
        }
        
        private void ApplyTranslation()
        {
            if (!gameState.HasFuel()) return;
            
            float acceleration = gameState.GetAcceleration();
            float maxSpeed = gameState.GetMaxSpeed();
            float currentBoost = isBoosting ? boostMultiplier : 1f;
            
            Vector3 thrustLocal = new Vector3(
                thrustStrafeInput * strafeThrust * acceleration * currentBoost * movementSensitivity,
                thrustVerticalInput * verticalThrust * acceleration * currentBoost * movementSensitivity,
                thrustForwardInput * baseThrust * acceleration * currentBoost * movementSensitivity
            );
            
            if (thrustLocal.sqrMagnitude > 0.01f)
            {
                Vector3 thrustWorld = transform.TransformDirection(thrustLocal);
                
                // ForceMode.Acceleration handles deltaTime internally
                rb.AddForce(thrustWorld, ForceMode.Acceleration);
                
                if (rb.linearVelocity.magnitude > maxSpeed * currentBoost)
                    rb.linearVelocity = rb.linearVelocity.normalized * (maxSpeed * currentBoost);
                
                // Fuel consumption uses deltaTime
                float fuelMultiplier = isBoosting ? boostFuelMultiplier : 1f;
                float fuelConsumption = baseFuelConsumptionRate * thrustLocal.magnitude / baseThrust * fuelMultiplier * Time.fixedDeltaTime;
                gameState.ConsumeFuel(fuelConsumption);
            }
        }
        
        private void TrackDistance()
        {
            float distance = Vector3.Distance(transform.position, lastPosition);
            gameState.distanceTraveled += distance;
            lastPosition = transform.position;
        }
        
        public void SetGameState(GameState state) { gameState = state; }
        
        public void StopMovement()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            pitchInput = yawInput = rollInput = 0f;
            thrustForwardInput = thrustStrafeInput = thrustVerticalInput = 0f;
            isBoosting = false;
        }
        
        public void ApplyExternalForce(Vector3 force)
        {
            if (rb != null) rb.AddForce(force, ForceMode.Force);
        }
        
        public void ApplyExternalTorque(Vector3 torque)
        {
            if (rb != null) rb.AddTorque(torque, ForceMode.Force);
        }
    }
}
