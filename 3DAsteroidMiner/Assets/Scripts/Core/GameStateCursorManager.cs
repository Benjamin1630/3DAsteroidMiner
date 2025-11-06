using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Advanced cursor manager that integrates with game states.
/// Automatically manages cursor based on game state (playing, paused, menu, etc.)
/// </summary>
public class GameStateCursorManager : MonoBehaviour
{
    [Header("Cursor Behavior")]
    [Tooltip("Lock and hide cursor during active gameplay")]
    [SerializeField] private bool lockInGameplay = true;
    
    [Tooltip("Show cursor in pause menu")]
    [SerializeField] private bool showInPauseMenu = true;
    
    [Tooltip("Show cursor in main menu / UI scenes")]
    [SerializeField] private bool showInMenuScenes = true;
    
    [Header("Toggle Settings")]
    [Tooltip("Allow ESC key to toggle pause and unlock cursor")]
    [SerializeField] private bool allowEscapeToggle = true;
    
    [Tooltip("Key to toggle pause menu")]
    [SerializeField] private Key pauseKey = Key.Escape;
    
    [Header("Scene Detection")]
    [Tooltip("Scene names that should show cursor (e.g., MainMenu, Settings)")]
    [SerializeField] private string[] menuSceneNames = { "MainMenu", "Menu", "Settings", "Credits" };
    
    [Header("State")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool isInMenuScene = false;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Singleton
    public static GameStateCursorManager Instance { get; private set; }
    
    // Events
    public event System.Action OnPaused;
    public event System.Action OnResumed;
    
    private void Awake()
    {
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
        
        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void Start()
    {
        CheckCurrentScene();
        UpdateCursorState();
    }
    
    private void Update()
    {
        // Handle pause toggle
        if (allowEscapeToggle && Keyboard.current != null && Keyboard.current[pauseKey].wasPressedThisFrame)
        {
            if (!isInMenuScene)
            {
                TogglePause();
            }
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
        
        // Reset pause state when loading new scene
        if (mode == LoadSceneMode.Single)
        {
            isPaused = false;
        }
        
        UpdateCursorState();
    }
    
    /// <summary>
    /// Check if current scene is a menu scene
    /// </summary>
    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        isInMenuScene = System.Array.Exists(menuSceneNames, name => 
            currentSceneName.Contains(name, System.StringComparison.OrdinalIgnoreCase));
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log($"Scene: {currentSceneName}, IsMenuScene: {isInMenuScene}");
        }
#endif
    }
    
    /// <summary>
    /// Update cursor state based on current game state
    /// </summary>
    private void UpdateCursorState()
    {
        if (isInMenuScene)
        {
            // In menu - show and unlock cursor
            if (showInMenuScenes)
            {
                SetCursorState(CursorLockMode.None, true);
            }
        }
        else if (isPaused)
        {
            // Game paused - show cursor
            if (showInPauseMenu)
            {
                SetCursorState(CursorLockMode.None, true);
            }
        }
        else
        {
            // Active gameplay - lock and hide cursor
            if (lockInGameplay)
            {
                SetCursorState(CursorLockMode.Locked, false);
            }
        }
    }
    
    /// <summary>
    /// Set cursor lock state and visibility
    /// </summary>
    private void SetCursorState(CursorLockMode lockMode, bool visible)
    {
        Cursor.lockState = lockMode;
        Cursor.visible = visible;
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log($"Cursor state changed - Lock: {lockMode}, Visible: {visible}");
        }
#endif
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        UpdateCursorState();
        OnPaused?.Invoke();
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Game paused");
        }
#endif
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        UpdateCursorState();
        OnResumed?.Invoke();
        
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Game resumed");
        }
#endif
    }
    
    /// <summary>
    /// Force cursor to be locked and hidden (for gameplay)
    /// </summary>
    public void LockCursor()
    {
        SetCursorState(CursorLockMode.Locked, false);
    }
    
    /// <summary>
    /// Force cursor to be unlocked and visible (for menus)
    /// </summary>
    public void UnlockCursor()
    {
        SetCursorState(CursorLockMode.None, true);
    }
    
    /// <summary>
    /// Confine cursor to window (useful for UI interaction during gameplay)
    /// </summary>
    public void ConfineCursor()
    {
        SetCursorState(CursorLockMode.Confined, true);
    }
    
    /// <summary>
    /// Check if game is paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Check if in menu scene
    /// </summary>
    public bool IsInMenuScene()
    {
        return isInMenuScene;
    }
    
    /// <summary>
    /// Handle application focus
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused && !isInMenuScene)
        {
            // Auto-pause when losing focus during gameplay
            Pause();
        }
        else if (hasFocus)
        {
            UpdateCursorState();
        }
    }
}
