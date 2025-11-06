using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages cursor visibility and lock state during gameplay.
/// Automatically hides and locks cursor in game, shows it in menus.
/// Press ESC to toggle cursor lock state.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [Tooltip("Lock cursor to center of screen during gameplay")]
    [SerializeField] private bool lockCursorOnStart = true;
    
    [Tooltip("Hide cursor during gameplay")]
    [SerializeField] private bool hideCursorOnStart = true;
    
    [Tooltip("Key to toggle cursor lock (default: Escape)")]
    [SerializeField] private Key toggleKey = Key.Escape;
    
    [Header("State Management")]
    [Tooltip("Is the cursor currently locked?")]
    [SerializeField] private bool isCursorLocked = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Singleton instance
    public static CursorManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Apply initial cursor state
        if (lockCursorOnStart)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log($"CursorManager initialized - Locked: {isCursorLocked}");
        }
#endif
    }
    
    private void Update()
    {
        // Toggle cursor lock with ESC key (or configured key)
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleCursorLock();
        }
    }
    
    /// <summary>
    /// Lock cursor to center of screen and hide it
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !hideCursorOnStart;
        isCursorLocked = true;
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Cursor locked and hidden");
        }
#endif
    }
    
    /// <summary>
    /// Unlock cursor and make it visible
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Cursor unlocked and visible");
        }
#endif
    }
    
    /// <summary>
    /// Toggle between locked and unlocked cursor states
    /// </summary>
    public void ToggleCursorLock()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
    
    /// <summary>
    /// Set cursor lock state explicitly
    /// </summary>
    public void SetCursorLocked(bool locked)
    {
        if (locked)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
    }
    
    /// <summary>
    /// Check if cursor is currently locked
    /// </summary>
    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }
    
    /// <summary>
    /// Confine cursor to game window without centering (useful for menus)
    /// </summary>
    public void ConfineCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        isCursorLocked = false;
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Cursor confined to window");
        }
#endif
    }
    
    /// <summary>
    /// Handle application focus changes
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && lockCursorOnStart)
        {
            // Re-lock cursor when game regains focus
            LockCursor();
        }
        else if (!hasFocus)
        {
            // Unlock cursor when game loses focus
            UnlockCursor();
        }
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log($"Application focus changed: {hasFocus}");
        }
#endif
    }
}
