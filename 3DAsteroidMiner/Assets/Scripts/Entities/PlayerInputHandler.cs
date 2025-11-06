using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Handles all player input using Unity's new Input System.
    /// Implements 6 Degrees of Freedom (6DoF) space flight controls.
    /// Supports keyboard, mouse, and gamepad controls similar to Elite Dangerous/Star Citizen.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        // Input Actions - 6DoF Movement
        private InputAction pitchAction;        // Mouse Y / Right Stick Y
        private InputAction yawAction;          // Mouse X / Right Stick X
        private InputAction rollAction;         // Q/E keys / Shoulder buttons
        private InputAction thrustForwardAction; // W/S keys / Left Stick Y
        private InputAction thrustStrafeAction;  // A/D keys / Left Stick X
        private InputAction thrustVerticalAction; // Space/Ctrl / Triggers
        
        // Input Actions - Other
        private InputAction boostAction;
        private InputAction mineAction;
        private InputAction scannerAction;
        private InputAction dockAction;
        private InputAction pauseAction;
        private InputAction mouseControlAction; // Hold to enable mouse flight
        
        // Events for 6DoF input
        public event Action<float> OnPitch;           // -1 to 1 (down to up)
        public event Action<float> OnYaw;             // -1 to 1 (left to right)
        public event Action<float> OnRoll;            // -1 to 1 (left to right)
        public event Action<float> OnThrustForward;   // -1 to 1 (backward to forward)
        public event Action<float> OnThrustStrafe;    // -1 to 1 (left to right)
        public event Action<float> OnThrustVertical;  // -1 to 1 (down to up)
        
        // Events for other input
        public event Action<bool> OnBoost;
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
            
            // Read continuous 6DoF input
            OnPitch?.Invoke(pitchAction.ReadValue<float>());
            OnYaw?.Invoke(yawAction.ReadValue<float>());
            OnRoll?.Invoke(rollAction.ReadValue<float>());
            OnThrustForward?.Invoke(thrustForwardAction.ReadValue<float>());
            OnThrustStrafe?.Invoke(thrustStrafeAction.ReadValue<float>());
            OnThrustVertical?.Invoke(thrustVerticalAction.ReadValue<float>());
            
            // Update mouse flight state
            mouseFlightEnabled = mouseControlAction.IsPressed();
        }
        
        #endregion
        
        #region Input System Setup
        
        /// <summary>
        /// Initialize all input actions with 6DoF bindings.
        /// </summary>
        private void InitializeInputActions()
        {
            // ===== ROTATION CONTROLS =====
            
            // Pitch (nose up/down) - Mouse Y / Right Stick Y
            pitchAction = new InputAction("Pitch", InputActionType.Value);
            pitchAction.AddBinding("<Mouse>/delta/y", processors: "scale(factor=0.05)");
            pitchAction.AddBinding("<Gamepad>/rightStick/y");
            
            // Yaw (nose left/right) - Mouse X / Right Stick X
            yawAction = new InputAction("Yaw", InputActionType.Value);
            yawAction.AddBinding("<Mouse>/delta/x", processors: "scale(factor=0.05)");
            yawAction.AddBinding("<Gamepad>/rightStick/x");
            
            // Roll (barrel roll) - Q/E / Shoulder Buttons
            rollAction = new InputAction("Roll", InputActionType.Value);
            rollAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");
            rollAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftShoulder")
                .With("Positive", "<Gamepad>/rightShoulder");
            
            // ===== TRANSLATION CONTROLS =====
            
            // Forward/Backward Thrust - W/S / Left Stick Y
            thrustForwardAction = new InputAction("ThrustForward", InputActionType.Value);
            thrustForwardAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/s")
                .With("Positive", "<Keyboard>/w");
            thrustForwardAction.AddBinding("<Gamepad>/leftStick/y");
            
            // Strafe Left/Right - A/D / Left Stick X
            thrustStrafeAction = new InputAction("ThrustStrafe", InputActionType.Value);
            thrustStrafeAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            thrustStrafeAction.AddBinding("<Gamepad>/leftStick/x");
            
            // Vertical Thrust Up/Down - Space/Ctrl / Triggers
            thrustVerticalAction = new InputAction("ThrustVertical", InputActionType.Value);
            thrustVerticalAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftCtrl")
                .With("Positive", "<Keyboard>/space");
            thrustVerticalAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Gamepad>/leftTrigger")
                .With("Positive", "<Gamepad>/rightTrigger");
            
            // ===== ACTION CONTROLS =====
            
            // Boost - Left Shift / Left Stick Click
            boostAction = new InputAction("Boost", InputActionType.Button);
            boostAction.AddBinding("<Keyboard>/leftShift");
            boostAction.AddBinding("<Gamepad>/leftStickPress");
            
            // Mouse Control Toggle - Right Mouse Button
            mouseControlAction = new InputAction("MouseControl", InputActionType.Button);
            mouseControlAction.AddBinding("<Mouse>/rightButton");
            
            // Mining (Left Mouse when not in mouse control / X Button)
            // Mining (Left Mouse when not in mouse control / X Button)
            mineAction = new InputAction("Mine", InputActionType.Button);
            mineAction.AddBinding("<Mouse>/leftButton");
            mineAction.AddBinding("<Gamepad>/buttonWest");
            
            // Scanner (E / Middle Mouse / Y Button)
            scannerAction = new InputAction("Scanner", InputActionType.Button);
            scannerAction.AddBinding("<Keyboard>/r");
            scannerAction.AddBinding("<Mouse>/middleButton");
            scannerAction.AddBinding("<Gamepad>/buttonNorth");
            
            // Dock (F / B Button)
            dockAction = new InputAction("Dock", InputActionType.Button);
            dockAction.AddBinding("<Keyboard>/f");
            dockAction.AddBinding("<Gamepad>/buttonSouth");
            
            // Pause (Escape / Start Button)
            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");
            
            // Subscribe to button events
            boostAction.started += OnBoostStartedCallback;
            boostAction.canceled += OnBoostCanceledCallback;
            mineAction.started += OnMineStartedCallback;
            mineAction.canceled += OnMineCanceledCallback;
            scannerAction.performed += OnScannerCallback;
            dockAction.performed += OnDockCallback;
            pauseAction.performed += OnPauseCallback;
        }
        
        /// <summary>
        /// Enable all input actions.
        /// </summary>
        private void EnableInputActions()
        {
            pitchAction?.Enable();
            yawAction?.Enable();
            rollAction?.Enable();
            thrustForwardAction?.Enable();
            thrustStrafeAction?.Enable();
            thrustVerticalAction?.Enable();
            boostAction?.Enable();
            mouseControlAction?.Enable();
            mineAction?.Enable();
            scannerAction?.Enable();
            dockAction?.Enable();
            pauseAction?.Enable();
        }
        
        /// <summary>
        /// Disable all input actions.
        /// </summary>
        private void DisableInputActions()
        {
            pitchAction?.Disable();
            yawAction?.Disable();
            rollAction?.Disable();
            thrustForwardAction?.Disable();
            thrustStrafeAction?.Disable();
            thrustVerticalAction?.Disable();
            boostAction?.Disable();
            mouseControlAction?.Disable();
            mineAction?.Disable();
            scannerAction?.Disable();
            dockAction?.Disable();
            pauseAction?.Disable();
        }
        
        /// <summary>
        /// Dispose of all input actions to prevent memory leaks.
        /// </summary>
        private void DisposeInputActions()
        {
            // Unsubscribe from events
            if (boostAction != null)
            {
                boostAction.started -= OnBoostStartedCallback;
                boostAction.canceled -= OnBoostCanceledCallback;
            }
            
            if (mineAction != null)
            {
                mineAction.started -= OnMineStartedCallback;
                mineAction.canceled -= OnMineCanceledCallback;
            }
            
            if (scannerAction != null)
                scannerAction.performed -= OnScannerCallback;
            
            if (dockAction != null)
                dockAction.performed -= OnDockCallback;
            
            if (pauseAction != null)
                pauseAction.performed -= OnPauseCallback;
            
            // Dispose actions
            pitchAction?.Dispose();
            yawAction?.Dispose();
            rollAction?.Dispose();
            thrustForwardAction?.Dispose();
            thrustStrafeAction?.Dispose();
            thrustVerticalAction?.Dispose();
            boostAction?.Dispose();
            mouseControlAction?.Dispose();
            mineAction?.Dispose();
            scannerAction?.Dispose();
            dockAction?.Dispose();
            pauseAction?.Dispose();
        }
        
        #endregion
        
        #region Input Callbacks
        
        private void OnBoostStartedCallback(InputAction.CallbackContext context)
        {
            OnBoost?.Invoke(true);
        }
        
        private void OnBoostCanceledCallback(InputAction.CallbackContext context)
        {
            OnBoost?.Invoke(false);
        }
        
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
        /// Check if boost is currently active.
        /// </summary>
        public bool IsBoostPressed()
        {
            return boostAction != null && boostAction.IsPressed();
        }
        
        /// <summary>
        /// Check if mouse flight control is currently enabled.
        /// </summary>
        public bool IsMouseFlightEnabled()
        {
            return mouseFlightEnabled;
        }
        
        /// <summary>
        /// Get current pitch input value (-1 to 1).
        /// </summary>
        public float GetPitch()
        {
            return pitchAction != null ? pitchAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current yaw input value (-1 to 1).
        /// </summary>
        public float GetYaw()
        {
            return yawAction != null ? yawAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current roll input value (-1 to 1).
        /// </summary>
        public float GetRoll()
        {
            return rollAction != null ? rollAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current forward/backward thrust value (-1 to 1).
        /// </summary>
        public float GetThrustForward()
        {
            return thrustForwardAction != null ? thrustForwardAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current strafe thrust value (-1 to 1).
        /// </summary>
        public float GetThrustStrafe()
        {
            return thrustStrafeAction != null ? thrustStrafeAction.ReadValue<float>() : 0f;
        }
        
        /// <summary>
        /// Get current vertical thrust value (-1 to 1).
        /// </summary>
        public float GetThrustVertical()
        {
            return thrustVerticalAction != null ? thrustVerticalAction.ReadValue<float>() : 0f;
        }
        
        #endregion
    }
}
