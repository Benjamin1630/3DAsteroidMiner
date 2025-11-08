using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Handles all player input using Unity's new Input System with InputActionAsset.
    /// Implements 6 Degrees of Freedom (6DoF) space flight controls.
    /// Supports keyboard, mouse, and gamepad controls similar to Elite Dangerous/Star Citizen.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input System")]
        [SerializeField] private InputActionAsset inputActions;
        
        // Input Actions from asset
        private InputAction moveAction;          // W/S/A/D / Left Stick
        private InputAction lookAction;          // Mouse Delta / Right Stick
        private InputAction rotateAction;        // Q/E Roll / Shoulder Buttons
        private InputAction upDownAction;        // Space/Ctrl / Triggers
        private InputAction mineAction;          // Left Mouse / X Button
        private InputAction scanAction;          // F / B Button
        private InputAction interactAction;      // E (Hold) / A Button
        private InputAction spaceBrakeAction;    // X / Right Stick Press
        private InputAction pauseAction;         // Escape / Start
        
        // Events for 6DoF input
        public event Action<float> OnPitch;           // -1 to 1 (down to up)
        public event Action<float> OnYaw;             // -1 to 1 (left to right)
        public event Action<float> OnRoll;            // -1 to 1 (left to right)
        public event Action<float> OnThrustForward;   // -1 to 1 (backward to forward)
        public event Action<float> OnThrustStrafe;    // -1 to 1 (left to right)
        public event Action<float> OnThrustVertical;  // -1 to 1 (down to up)
        
        // Events for other input
        public event Action OnMineStarted;
        public event Action OnMineCanceled;
        public event Action OnScannerPressed;
        public event Action OnDockPressed;
        public event Action OnPausePressed;
        
        // State
        private bool isEnabled = true;
        private bool mouseFlightEnabled = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeInputActions();
        }
        
        private void OnEnable()
        {
            EnableInputActions();
        }
        
        private void OnDisable()
        {
            DisableInputActions();
        }
        
        private void OnDestroy()
        {
            DisposeInputActions();
        }
        
        private void Update()
        {
            if (!isEnabled) return;
            
            // Read continuous 6DoF input from InputActionAsset
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            float rollInput = rotateAction.ReadValue<float>();
            float upDownInput = upDownAction.ReadValue<float>();
            
            // Invoke events with processed values
            OnThrustStrafe?.Invoke(moveInput.x);        // Left/Right from Move X
            OnThrustForward?.Invoke(moveInput.y);       // Forward/Back from Move Y
            OnYaw?.Invoke(lookInput.x);                 // Yaw from Look X
            OnPitch?.Invoke(lookInput.y);               // Pitch from Look Y
            OnRoll?.Invoke(rollInput);                  // Roll from Rotate
            OnThrustVertical?.Invoke(upDownInput);      // Up/Down from Up/Down action
        }
        
        #endregion
        
        #region Input System Setup
        
        /// <summary>
        /// Initialize all input actions from InputActionAsset.
        /// </summary>
        private void InitializeInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("PlayerInputHandler: InputActionAsset not assigned! Please assign InputSystem_Actions in inspector.");
                return;
            }
            
            // Get Player action map
            InputActionMap playerMap = inputActions.FindActionMap("Player");
            if (playerMap == null)
            {
                Debug.LogError("PlayerInputHandler: Could not find 'Player' action map in InputActionAsset!");
                return;
            }
            
            // Get actions from asset
            moveAction = playerMap.FindAction("Move");
            lookAction = playerMap.FindAction("Look");
            rotateAction = playerMap.FindAction("Rotate");
            upDownAction = playerMap.FindAction("Up/Down");
            mineAction = playerMap.FindAction("Mine");
            scanAction = playerMap.FindAction("Scan");
            interactAction = playerMap.FindAction("Interact");
            spaceBrakeAction = playerMap.FindAction("SpaceBrake");
            pauseAction = playerMap.FindAction("Pause");
            
            // Validate required actions
            if (moveAction == null) Debug.LogError("PlayerInputHandler: 'Move' action not found!");
            if (lookAction == null) Debug.LogError("PlayerInputHandler: 'Look' action not found!");
            if (rotateAction == null) Debug.LogError("PlayerInputHandler: 'Rotate' action not found!");
            if (upDownAction == null) Debug.LogError("PlayerInputHandler: 'Up/Down' action not found!");
            
            // Subscribe to button events
            if (mineAction != null)
            {
                mineAction.started += OnMineStartedCallback;
                mineAction.canceled += OnMineCanceledCallback;
            }
            
            if (scanAction != null)
            {
                scanAction.performed += OnScannerCallback;
            }
            
            if (interactAction != null)
            {
                interactAction.performed += OnDockCallback;
            }
            
            if (pauseAction != null)
            {
                pauseAction.performed += OnPauseCallback;
            }
        }
        
        /// <summary>
        /// Enable all input actions.
        /// </summary>
        private void EnableInputActions()
        {
            if (inputActions != null)
            {
                inputActions.Enable();
            }
        }
        
        /// <summary>
        /// Disable all input actions.
        /// </summary>
        private void DisableInputActions()
        {
            if (inputActions != null)
            {
                inputActions.Disable();
            }
        }
        
        /// <summary>
        /// Dispose of all input actions to prevent memory leaks.
        /// </summary>
        private void DisposeInputActions()
        {
            // Unsubscribe from events
            if (mineAction != null)
            {
                mineAction.started -= OnMineStartedCallback;
                mineAction.canceled -= OnMineCanceledCallback;
            }
            
            if (scanAction != null)
            {
                scanAction.performed -= OnScannerCallback;
            }
            
            if (interactAction != null)
            {
                interactAction.performed -= OnDockCallback;
            }
            
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPauseCallback;
            }
        }
        
        #endregion
        
        #region Input Callbacks
        
        private void OnMineStartedCallback(InputAction.CallbackContext context)
        {
            OnMineStarted?.Invoke();
        }
        
        private void OnMineCanceledCallback(InputAction.CallbackContext context)
        {
            OnMineCanceled?.Invoke();
        }
        
        private void OnScannerCallback(InputAction.CallbackContext context)
        {
            OnScannerPressed?.Invoke();
        }
        
        private void OnDockCallback(InputAction.CallbackContext context)
        {
            OnDockPressed?.Invoke();
        }
        
        private void OnPauseCallback(InputAction.CallbackContext context)
        {
            OnPausePressed?.Invoke();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Enable or disable input handling.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            isEnabled = enabled;
            
            if (enabled)
                EnableInputActions();
            else
                DisableInputActions();
        }
        
        /// <summary>
        /// Check if the mine action is currently being held.
        /// </summary>
        public bool IsMinePressed()
        {
            return mineAction != null && mineAction.IsPressed();
        }
        
        /// <summary>
        /// Check if mouse flight control is currently enabled.
        /// </summary>
        public bool IsMouseFlightEnabled()
        {
            return mouseFlightEnabled;
        }
        
        /// <summary>
        /// Check if the player is currently using mouse for looking around.
        /// Returns true if the last input device used for the Look action was a mouse.
        /// </summary>
        public bool IsUsingMouse()
        {
            if (lookAction == null) return false;
            
            // Check if the active control is from a mouse/pointer device
            var activeControl = lookAction.activeControl;
            if (activeControl == null) return false;
            
            // Check if the device is a Mouse or Pointer
            return activeControl.device is Mouse || activeControl.device is Pointer;
        }
        
        /// <summary>
        /// Check if the player is currently using a gamepad for looking around.
        /// Returns true if the last input device used for the Look action was a gamepad.
        /// </summary>
        public bool IsUsingGamepad()
        {
            if (lookAction == null) return false;
            
            var activeControl = lookAction.activeControl;
            if (activeControl == null) return false;
            
            return activeControl.device is Gamepad;
        }
        
        /// <summary>
        /// Get current pitch input value (-1 to 1).
        /// </summary>
        public float GetPitch()
        {
            return lookAction != null ? lookAction.ReadValue<Vector2>().y : 0f;
        }
        
        /// <summary>
        /// Get current yaw input value (-1 to 1).
        /// </summary>
        public float GetYaw()
        {
            return lookAction != null ? lookAction.ReadValue<Vector2>().x : 0f;
        }
        
        /// <summary>
        /// Get current roll input value (-1 to 1).
        /// </summary>
        public float GetRoll()
        {
            return rotateAction != null ? rotateAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current forward/backward thrust value (-1 to 1).
        /// </summary>
        public float GetThrustForward()
        {
            return moveAction != null ? moveAction.ReadValue<Vector2>().y : 0f;
        }
        
        /// <summary>
        /// Get current strafe thrust value (-1 to 1).
        /// </summary>
        public float GetThrustStrafe()
        {
            return moveAction != null ? moveAction.ReadValue<Vector2>().x : 0f;
        }
        
        /// <summary>
        /// Get current vertical thrust value (-1 to 1).
        /// </summary>
        public float GetThrustVertical()
        {
            return upDownAction != null ? upDownAction.ReadValue<float>() : 0f;
        }
        
        #endregion
    }
}
