# Cursor Lock & Hide System - Setup Guide

This system automatically manages the mouse cursor during gameplay, hiding and locking it to the center of the screen for better control.

## 📁 Files Created

- **CursorManager.cs** - Simple cursor lock/hide manager (`Assets/Scripts/Core/`)
- **GameStateCursorManager.cs** - Advanced manager with pause/menu integration (`Assets/Scripts/Core/`)

---

## 🚀 Quick Setup (Choose One Option)

### Option 1: Simple Cursor Manager (Recommended for Most Cases)

**Best for:** Basic cursor hiding and locking without complex menu systems.

#### Setup Steps:

1. **Create a GameObject** in your main game scene:
   - Right-click in Hierarchy → **Create Empty**
   - Name it **"CursorManager"**

2. **Add the Script:**
   - Select the CursorManager GameObject
   - Click **Add Component** → Search for **"CursorManager"**

3. **Configure Settings** (in Inspector):
   - **Lock Cursor On Start:** ✅ Checked
   - **Hide Cursor On Start:** ✅ Checked
   - **Toggle Key:** Escape (press ESC to unlock cursor)
   - **Debug Mode:** ✅ (optional - check to see cursor state in Console)

4. **Done!** The cursor will now:
   - ✅ Lock to center of screen when game starts
   - ✅ Hide from view during gameplay
   - ✅ Unlock when you press ESC
   - ✅ Re-lock when game regains focus

#### Usage in Code:

```csharp
// Lock cursor programmatically
CursorManager.Instance.LockCursor();

// Unlock cursor (for showing a menu)
CursorManager.Instance.UnlockCursor();

// Toggle lock state
CursorManager.Instance.ToggleCursorLock();

// Check if locked
bool isLocked = CursorManager.Instance.IsCursorLocked();

// Confine cursor to window (visible but can't leave window)
CursorManager.Instance.ConfineCursor();
```

---

### Option 2: Game State Cursor Manager (Advanced)

**Best for:** Games with pause menus, multiple scenes, and complex UI systems.

#### Setup Steps:

1. **Create a GameObject** in your startup/main scene:
   - Right-click in Hierarchy → **Create Empty**
   - Name it **"GameCursorManager"**

2. **Add the Script:**
   - Select the GameObject
   - Click **Add Component** → Search for **"GameStateCursorManager"**

3. **Configure Settings:**
   - **Lock In Gameplay:** ✅ Checked
   - **Show In Pause Menu:** ✅ Checked
   - **Show In Menu Scenes:** ✅ Checked
   - **Allow Escape Toggle:** ✅ Checked
   - **Pause Key:** Escape
   - **Menu Scene Names:** Add your menu scene names (e.g., "MainMenu", "Settings")

4. **Benefits:**
   - Automatically shows cursor in menu scenes
   - Handles pause menu (ESC to pause/unpause)
   - Auto-pauses when game loses focus
   - Integrates with Time.timeScale for pause functionality

#### Usage in Code:

```csharp
// Pause game (shows cursor)
GameStateCursorManager.Instance.Pause();

// Resume game (hides cursor)
GameStateCursorManager.Instance.Resume();

// Toggle pause
GameStateCursorManager.Instance.TogglePause();

// Check pause state
bool isPaused = GameStateCursorManager.Instance.IsPaused();

// Subscribe to events
GameStateCursorManager.Instance.OnPaused += HandleGamePaused;
GameStateCursorManager.Instance.OnResumed += HandleGameResumed;
```

---

## 🎮 Integration Examples

### Integration with Pause Menu UI

```csharp
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    
    private void OnEnable()
    {
        if (GameStateCursorManager.Instance != null)
        {
            GameStateCursorManager.Instance.OnPaused += ShowPauseMenu;
            GameStateCursorManager.Instance.OnResumed += HidePauseMenu;
        }
    }
    
    private void OnDisable()
    {
        if (GameStateCursorManager.Instance != null)
        {
            GameStateCursorManager.Instance.OnPaused -= ShowPauseMenu;
            GameStateCursorManager.Instance.OnResumed -= HidePauseMenu;
        }
    }
    
    private void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
    }
    
    private void HidePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
    }
    
    public void OnResumeButtonClicked()
    {
        GameStateCursorManager.Instance.Resume();
    }
}
```

### Integration with Station Docking (Show Cursor for UI)

```csharp
public class StationDockingUI : MonoBehaviour
{
    private void OnDockingStarted()
    {
        // Show cursor for station UI interaction
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.UnlockCursor();
        }
    }
    
    private void OnDockingEnded()
    {
        // Hide cursor when leaving station
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.LockCursor();
        }
    }
}
```

### Integration with Settings Menu

```csharp
public class SettingsMenu : MonoBehaviour
{
    private void OnEnable()
    {
        // Always show cursor in settings
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.UnlockCursor();
        }
    }
    
    private void OnDisable()
    {
        // Lock cursor when closing settings (if in gameplay)
        if (!GameStateCursorManager.Instance.IsInMenuScene())
        {
            CursorManager.Instance.LockCursor();
        }
    }
}
```

---

## ⚙️ Configuration Options

### CursorManager Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Lock Cursor On Start | Lock cursor when game starts | ✅ True |
| Hide Cursor On Start | Hide cursor when locked | ✅ True |
| Toggle Key | Key to unlock cursor | Escape |
| Debug Mode | Log cursor state changes | ❌ False |

### GameStateCursorManager Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Lock In Gameplay | Lock cursor during active gameplay | ✅ True |
| Show In Pause Menu | Show cursor when paused | ✅ True |
| Show In Menu Scenes | Show cursor in menu scenes | ✅ True |
| Allow Escape Toggle | ESC key toggles pause | ✅ True |
| Pause Key | Key to pause game | Escape |
| Menu Scene Names | Array of menu scene names | ["MainMenu", "Menu"] |
| Debug Mode | Log state changes | ❌ False |

---

## 🐛 Troubleshooting

### Problem: Cursor still visible after setup
**Solutions:**
1. Check that **Hide Cursor On Start** is enabled
2. Verify the script is on an active GameObject
3. Check Console for errors (enable Debug Mode)
4. Make sure you're not in the Unity Editor with "Always Show Cursor" enabled

### Problem: Can't unlock cursor with ESC
**Solutions:**
1. Verify **Toggle Key** is set to Escape
2. Check that another script isn't consuming the ESC key
3. Try clicking the Game window to ensure it has focus
4. Check if **Allow Escape Toggle** is enabled (GameStateCursorManager)

### Problem: Cursor unlocks when switching windows
**Solutions:**
- This is expected behavior (OnApplicationFocus)
- The cursor will re-lock when you click back into the game
- For GameStateCursorManager, the game will auto-pause

### Problem: Cursor shows in wrong scenes
**Solutions:**
1. Check **Menu Scene Names** array in GameStateCursorManager
2. Ensure scene names match exactly (case-sensitive)
3. Manually call `LockCursor()` or `UnlockCursor()` in scene Start methods

---

## 🎯 Best Practices

### ✅ DO:
- Use **CursorManager** for simple games without pause menus
- Use **GameStateCursorManager** for games with pause/menu systems
- Unlock cursor when showing UI that requires mouse input
- Lock cursor during active ship control gameplay
- Test cursor behavior when alt-tabbing

### ❌ DON'T:
- Don't use both managers at the same time (choose one)
- Don't forget to unlock cursor in UI/menu scenes
- Don't manually set `Cursor.lockState` elsewhere (conflicts with manager)
- Don't disable cursor manager during gameplay (will break locking)

---

## 🔧 Advanced Features

### Custom Lock States

```csharp
// Lock cursor but keep it visible (for UI over gameplay)
Cursor.lockState = CursorLockMode.Locked;
Cursor.visible = true;

// Confine cursor to window (can move but can't leave)
CursorManager.Instance.ConfineCursor();
```

### Custom Toggle Keys

Change the toggle key in Inspector or via code:

```csharp
// In CursorManager Inspector, change Toggle Key from Escape to P
// Or access it via code (requires making toggleKey public)
```

### Scene-Specific Behavior

```csharp
using UnityEngine.SceneManagement;

public class SceneCursorControl : MonoBehaviour
{
    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "MainMenu":
            case "Settings":
                CursorManager.Instance.UnlockCursor();
                break;
                
            case "Gameplay":
            case "SpaceCombat":
                CursorManager.Instance.LockCursor();
                break;
        }
    }
}
```

---

## 📊 Performance Notes

- **CPU Impact:** Negligible (single Update check per frame)
- **Memory:** < 1 KB
- **Compatibility:** Works on Windows, Mac, Linux, WebGL
- **Mobile:** No effect (mobile doesn't have cursor)

---

## ✅ Quick Test Checklist

After setup, verify:

- [ ] Cursor hides when game starts
- [ ] Cursor locks to center when moving mouse
- [ ] Pressing ESC unlocks cursor
- [ ] Pressing ESC again re-locks cursor
- [ ] Alt-tabbing unlocks cursor
- [ ] Returning to game re-locks cursor
- [ ] Cursor shows in pause menu (if using GameStateCursorManager)
- [ ] Cursor shows in menu scenes
- [ ] No errors in Console

---

## 🆘 Quick Reference

### Simple Lock/Unlock
```csharp
// Lock and hide
CursorManager.Instance.LockCursor();

// Unlock and show
CursorManager.Instance.UnlockCursor();
```

### With Pause Integration
```csharp
// Pause (shows cursor)
GameStateCursorManager.Instance.Pause();

// Resume (hides cursor)
GameStateCursorManager.Instance.Resume();
```

---

**Created:** November 6, 2025  
**Version:** 1.0
