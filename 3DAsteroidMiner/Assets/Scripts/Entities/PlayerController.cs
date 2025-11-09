using UnityEngine;
using UnityEngine.InputSystem;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Handles player ship movement using 6 Degrees of Freedom (6DoF) controls.
    /// Implements space flight physics similar to Elite Dangerous, Star Citizen, and No Man's Sky.
    /// Uses Unity's new Input System for all input handling.
    /// Accesses ship data through ShipStats component.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))] // Ensure player has collision detection
    [RequireComponent(typeof(ShipStats))] // Require ShipStats component
    public class PlayerController : MonoBehaviour
    {
        [Header("Translation Settings")]
        [SerializeField] private float baseThrust = 20f;
        [SerializeField] private float strafeThrust = 15f;
        [SerializeField] private float verticalThrust = 15f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float pitchSpeed = 20f;
        [SerializeField] private float yawSpeed = 20f;
        [SerializeField] private float rollSpeed = 30f;
        
        [Header("Look Sensitivity")]
        [Tooltip("Mouse look sensitivity (for mouse delta input)")]
        [SerializeField] [Range(0.01f, 2f)] private float mouseLookSensitivity = 0.5f;
        [Tooltip("Gamepad look sensitivity (for right stick input)")]
        [SerializeField] [Range(0.001f, 5f)] private float gamepadLookSensitivity = 1.5f;
        
        [SerializeField] [Range(0.1f, 2f)] private float rotationSensitivity = 0.125f;
        
        [Header("Physics Settings")]
        [Tooltip("Linear drag (0 = realistic space physics with no friction, >0 = easier control with auto-slowdown)")]
        [SerializeField] private float linearDrag = 0.25f;
        [Tooltip("Angular drag (0 = realistic space physics with perpetual spin, >0 = easier control with auto-stabilization)")]
        [SerializeField] private float angularDrag = 1.5f;
        [SerializeField] [Range(0.1f, 2f)] private float movementSensitivity = 0.125f;
        
        [Header("Fuel Settings")]
        [SerializeField] private float baseFuelConsumptionRate = 0.125f;
        
        private Rigidbody rb;
        private PlayerInputHandler inputHandler;
        private ShipStats shipStats;
        
        private float pitchInput = 0f;
        private float yawInput = 0f;
        private float rollInput = 0f;
        private float thrustForwardInput = 0f;
        private float thrustStrafeInput = 0f;
        private float thrustVerticalInput = 0f;
        
        private Vector3 lastPosition;
        
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : 0f;
        public bool IsMoving => CurrentSpeed > 0.1f;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inputHandler = GetComponent<PlayerInputHandler>();
            shipStats = GetComponent<ShipStats>();
            
            if (shipStats == null)
            {
                Debug.LogError("PlayerController: ShipStats component not found! Please add ShipStats to the player ship.");
            }
            
            rb.useGravity = false;
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
            rb.constraints = RigidbodyConstraints.None;
            
            lastPosition = transform.position;
        }
        
        private void Start()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPitch += HandlePitchInput;
                inputHandler.OnYaw += HandleYawInput;
                inputHandler.OnRoll += HandleRollInput;
                inputHandler.OnThrustForward += HandleThrustForwardInput;
                inputHandler.OnThrustStrafe += HandleThrustStrafeInput;
                inputHandler.OnThrustVertical += HandleThrustVerticalInput;
            }
        }
        
        private void FixedUpdate()
        {
            if (shipStats == null || shipStats.IsDocked()) return;
            
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
            }
        }
        
        private void HandlePitchInput(float input) { pitchInput = input; }
        private void HandleYawInput(float input) { yawInput = input; }
        private void HandleRollInput(float input) { rollInput = input; }
        private void HandleThrustForwardInput(float input) { thrustForwardInput = input; }
        private void HandleThrustStrafeInput(float input) { thrustStrafeInput = input; }
        private void HandleThrustVerticalInput(float input) { thrustVerticalInput = input; }
        
        private void ApplyRotation()
        {
            if (!shipStats.HasFuel()) return;
            
            float pitch = pitchSpeed * rotationSensitivity;
            float yaw = yawSpeed * rotationSensitivity;
            float roll = rollSpeed * rotationSensitivity;
            
            // Apply different sensitivity based on input device
            if (inputHandler.IsUsingMouse())
            {
                // Mouse provides delta values (pixels moved), needs lower sensitivity
                pitch *= mouseLookSensitivity;
                yaw *= mouseLookSensitivity;
            }
            else
            {
                // Gamepad provides normalized values (-1 to 1), needs higher sensitivity
                pitch *= gamepadLookSensitivity;
                yaw *= gamepadLookSensitivity;
            }
            
            Vector3 torque = new Vector3(-pitchInput * pitch, yawInput * yaw, -rollInput * roll);
            
            if (torque.sqrMagnitude > 0.01f)
            {
                // ForceMode.Acceleration handles deltaTime internally, so we apply it directly
                rb.AddRelativeTorque(torque, ForceMode.Acceleration);
                
                // Fuel consumption uses deltaTime
                float rotationFuel = baseFuelConsumptionRate * 0.2f * torque.magnitude / 100f * Time.fixedDeltaTime;
                shipStats.ConsumeFuel(rotationFuel);
            }
        }
        
        private void ApplyTranslation()
        {
            if (!shipStats.HasFuel()) return;
            
            // Get acceleration and max speed from ship stats (includes upgrade bonuses)
            float baseAcceleration = 10f;
            float baseMaxSpeed = 20f;
            float acceleration = baseAcceleration * shipStats.GetAccelerationMultiplier();
            float maxSpeed = baseMaxSpeed * shipStats.GetMaxSpeedMultiplier();
            
            Vector3 thrustLocal = new Vector3(
                thrustStrafeInput * strafeThrust * acceleration * movementSensitivity,
                thrustVerticalInput * verticalThrust * acceleration * movementSensitivity,
                thrustForwardInput * baseThrust * acceleration * movementSensitivity
            );
            
            if (thrustLocal.sqrMagnitude > 0.01f)
            {
                Vector3 thrustWorld = transform.TransformDirection(thrustLocal);
                
                // ForceMode.Acceleration handles deltaTime internally
                rb.AddForce(thrustWorld, ForceMode.Acceleration);
                
                if (rb.linearVelocity.magnitude > maxSpeed)
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                
                // Fuel consumption uses deltaTime
                float fuelConsumption = baseFuelConsumptionRate * thrustLocal.magnitude / baseThrust * Time.fixedDeltaTime;
                shipStats.ConsumeFuel(fuelConsumption);
            }
        }
        
        private void TrackDistance()
        {
            float distance = Vector3.Distance(transform.position, lastPosition);
            shipStats.AddDistanceTraveled(distance);
            lastPosition = transform.position;
        }
        
        /// <summary>
        /// Get the ShipStats component (for other systems to access ship data).
        /// </summary>
        public ShipStats GetShipStats()
        {
            return shipStats;
        }
        
        public void StopMovement()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            pitchInput = yawInput = rollInput = 0f;
            thrustForwardInput = thrustStrafeInput = thrustVerticalInput = 0f;
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
