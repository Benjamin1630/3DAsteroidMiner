# Scanner System Implementation Guide

## Overview
The scanner system provides a holographic pulse effect that expands from the player ship, detecting and highlighting asteroids within range. The system respects ship upgrades for scan range and cooldown.

## Components Created

### 1. **ScannerSystem.cs**
Location: `Assets/Scripts/Systems/ScannerSystem.cs`

Main scanner logic component that:
- Manages scan activation and cooldown
- Creates expanding visual pulse effect
- Detects asteroids within range using Physics.OverlapSphere
- Highlights detected asteroids with emission glow
- Respects upgrade levels for range and cooldown

**Key Properties:**
- `baseScanRange`: 100m (before upgrades)
- `rangePerUpgrade`: +100m per upgrade level
- `baseCooldown`: 10 seconds (before upgrades)
- `cooldownReductionPerLevel`: -8% per upgrade level
- `pulseDuration`: 2 seconds (visual effect duration)
- `highlightDuration`: 5 seconds (asteroid glow duration)

**Key Methods:**
- `ActivateScan()`: Trigger a scan pulse (called from PlayerController)
- `GetScanRange()`: Calculate current range based on upgrades
- `GetScanCooldown()`: Calculate current cooldown based on upgrades

**Events:**
- `OnScanActivated`: Fired when scan is activated
- `OnAsteroidsDetected(int count)`: Fired with count of detected asteroids

### 2. **ScanPulse.shader**
Location: `Assets/Shaders/ScanPulse.shader`

Holographic shader effect featuring:
- Expanding sphere pulse from ship position
- Fresnel effect for holographic rim glow
- Sharp edge highlighting as pulse expands
- Scan line animation (horizontal bands)
- Hexagonal grid pattern for sci-fi look
- Progressive fade out as pulse expands

**Shader Properties:**
- `_Color`: Pulse color (default: cyan 0.2, 0.8, 1)
- `_Progress`: Animation progress (0-1)
- `_Radius`: Current pulse radius
- `_EdgeThickness`: Width of bright edge (0.01-0.5)
- `_FresnelPower`: Rim glow intensity (0.5-5)
- `_EdgeGlow`: Edge brightness multiplier (0-10)

### 3. **Asteroid.cs Updates**
Added scanning highlight capability:

**New Fields:**
- `isScanned`: Current scan state
- `scanHighlightTimer`: Time remaining for highlight
- `originalEmissionColor`: Stored emission color
- `scanPropertyBlock`: MaterialPropertyBlock for efficient highlighting

**New Methods:**
- `SetScanned(bool scanned, float duration)`: Enable/disable scan highlight
  - Stores original material emission state
  - Applies cyan holographic glow using MaterialPropertyBlock
  - Restores original emission after duration

**Update Loop:**
- Tracks scan highlight timer
- Automatically removes highlight after duration expires

### 4. **PlayerController.cs Updates**
Integrated scanner input:

**New References:**
- `scannerSystem`: Reference to ScannerSystem component

**New Input Handler:**
- `HandleScannerInput()`: Responds to scanner button press (R/Middle Mouse/Y Button)
- Calls `scannerSystem.ActivateScan()` when not docked

**Initialization:**
- Auto-finds ScannerSystem component if not assigned
- Subscribes to `OnScannerPressed` event from PlayerInputHandler
- Unsubscribes on destroy to prevent memory leaks

### 5. **GameState.cs Updates**
Added scanner upgrade helper methods:

**New Methods:**
- `GetScanRange()`: Returns 100 + (level - 1) * 100 meters
  - Level 1: 100m
  - Level 5: 500m
  - Level 10: 1000m

- `GetScanCooldown()`: Returns 10 * (1 - (level - 1) * 0.08) seconds
  - Level 1: 10.0s
  - Level 5: 6.8s
  - Level 10: 2.6s

- `HasAdvancedScanner()`: Returns true if one-time upgrade purchased
  - Can be used later to show asteroid resource values in UI

## Setup Instructions

### 1. Create Scanner Material
1. In Unity, create a new Material: `Right-click in Materials folder → Create → Material`
2. Name it `ScanPulseMaterial`
3. Set shader to `Custom/ScanPulse`
4. Adjust properties:
   - Color: (R:0.2, G:0.8, B:1, A:1) - Cyan holographic
   - Edge Thickness: 0.1
   - Fresnel Power: 2
   - Edge Glow: 3
   - Inner Alpha: 0.1
   - Outer Alpha: 0.5

### 2. Setup Player Ship
1. Select your Player ship GameObject in the hierarchy
2. Add the **ScannerSystem** component:
   - `Add Component → Scripts → Systems → Scanner System`
3. Configure ScannerSystem inspector:
   - **Scan Pulse Material**: Drag `ScanPulseMaterial` here
   - **Game State**: Drag your GameState asset/reference
   - **Input Actions**: Drag `InputSystem_Actions` asset from Assets folder
   - **Ship Transform**: Leave empty (auto-assigns to self)

### 3. Input Setup (Uses InputSystem_Actions.inputactions)
The scanner uses the **"Scan"** action from the **"Player"** action map in your InputSystem_Actions asset:
- **Keyboard**: F key
- **Gamepad**: B button (East button)

The ScannerSystem subscribes directly to the Input System action - no PlayerController integration needed!

## Usage

### In-Game Controls
- Press **F** (or Gamepad B Button) to activate scanner
- Scanner shows cooldown timer when on cooldown
- Detected asteroids glow cyan for 5 seconds
- Pulse sphere expands from ship to max range over 2 seconds
- Cannot scan while docked at station

### Upgrade System Integration
The scanner respects these GameState upgrades:

**scanRange** (Levels 1-10):
```csharp
gameState.upgrades["scanRange"] = 5; // 500m range
```

**scanCooldown** (Levels 1-10):
```csharp
gameState.upgrades["scanCooldown"] = 8; // ~3.8s cooldown
```

**advancedScanner** (One-time purchase):
```csharp
gameState.upgrades["advancedScanner"] = 1; // Unlocked
// Can be used to show asteroid values in UI
```

## Visual Customization

### Adjusting Pulse Appearance
Edit shader properties in the material:
- **Brighter pulse**: Increase `Edge Glow` and `Outer Alpha`
- **Sharper edge**: Decrease `Edge Thickness`
- **Different color**: Change `Color` property (try magenta, green, etc.)
- **Softer glow**: Decrease `Fresnel Power`

### Adjusting Highlight Duration
Edit ScannerSystem component:
- **Longer asteroid glow**: Increase `highlightDuration` (default: 5s)
- **Faster pulse**: Decrease `pulseDuration` (default: 2s)
- **Different range**: Adjust `baseScanRange` and `rangePerUpgrade`

## Performance Considerations

### Optimizations Implemented
1. **MaterialPropertyBlock**: Used for asteroid highlighting to avoid creating material instances
2. **Physics.OverlapSphere**: Efficient spatial query for asteroid detection
3. **Coroutine-based pulse**: Single coroutine per scan, no Update overhead
4. **Conditional compilation**: Debug logs only in editor builds
5. **Event-driven**: No polling, uses C# events for communication

### Expected Performance Impact
- **CPU**: ~0.1-0.5ms per scan activation (depends on asteroid count)
- **GPU**: Minimal, single transparent sphere renderer
- **Memory**: ~100KB for material and property blocks
- **Draw Calls**: +1 during pulse animation, +0 for highlighted asteroids (uses MaterialPropertyBlock)

## Debugging

### Gizmos
In Scene view, select the Player GameObject:
- **Cyan wire sphere**: Current scan range (based on upgrades)
- **Red wire sphere**: Scanner on cooldown (10m radius indicator)
- **Highlighted asteroids**: Cyan wire sphere (+0.5m radius)

### Console Logging
Enable debug logs by viewing Editor Console:
```
Scanner activated! Range: 500m, Cooldown: 6.8s, Detected: 12 asteroids
```

### Common Issues

**Scanner not activating:**
- Check cooldown is complete (red gizmo)
- Verify not docked (`gameState.isDocked == false`)
- Check ScannerSystem component is enabled
- Verify GameState reference is assigned

**Asteroids not highlighting:**
- Ensure asteroids have colliders (on child ProceduralAsteroidMesh)
- Verify asteroid materials support emission (`_EMISSION` keyword)
- Check asteroid layer is not filtered in Physics settings

**Pulse not visible:**
- Assign `ScanPulseMaterial` to ScannerSystem
- Check material uses `Custom/ScanPulse` shader
- Verify alpha values are > 0 in material properties
- Ensure camera can see Transparent render queue

## Future Enhancements

### Potential Features
1. **Audio feedback**: Add scan activation and pulse sounds
2. **Particle effects**: Spawn particles at detected asteroids
3. **Radar UI**: Display detected asteroids on minimap
4. **Resource indication**: Show asteroid values with advanced scanner
5. **Hazard detection**: Extend system to detect space debris and mines
6. **Distance markers**: Show distance to detected asteroids in HUD
7. **Sector scanning**: Detect space stations and NPCs

### Integration Points
- **UI System**: Display cooldown timer, detected count
- **Mission System**: Track scanner usage for survey missions
- **Achievement System**: "Scan 100 asteroids" achievement
- **Tutorial System**: Teach scanner in early game tutorial

## Code Examples

### Programmatically Trigger Scan
```csharp
ScannerSystem scanner = GetComponent<ScannerSystem>();
if (scanner.CanScan)
{
    scanner.ActivateScan();
}
```

### Listen for Scan Events
```csharp
void Start()
{
    ScannerSystem scanner = GetComponent<ScannerSystem>();
    scanner.OnScanActivated += HandleScanActivated;
    scanner.OnAsteroidsDetected += HandleAsteroidsDetected;
}

void HandleScanActivated()
{
    Debug.Log("Scanner activated!");
}

void HandleAsteroidsDetected(int count)
{
    Debug.Log($"Detected {count} asteroids");
    // Update UI, play sound, etc.
}
```

### Check Scan State
```csharp
ScannerSystem scanner = GetComponent<ScannerSystem>();
bool canScan = scanner.CanScan; // true if cooldown complete
float cooldownProgress = scanner.CooldownProgress; // 0-1
float currentRange = scanner.CurrentRange; // meters
```

### Manually Highlight Asteroid
```csharp
Asteroid asteroid = GetComponent<Asteroid>();
asteroid.SetScanned(true, 10f); // Highlight for 10 seconds
```

## Technical Details

### Shader Rendering
- **Render Queue**: Transparent (3000)
- **Blend Mode**: SrcAlpha OneMinusSrcAlpha (standard alpha blending)
- **Culling**: Back face culling (shows interior)
- **ZWrite**: Off (doesn't block other transparent objects)

### Physics Detection
- **Method**: Physics.OverlapSphere (single call per scan)
- **Layer Mask**: All layers (could be optimized with asteroid layer)
- **Collider Type**: Requires colliders on asteroids (sphere/mesh)
- **Parent Check**: Uses GetComponentInParent for asteroid detection

### Material System
- **Emission**: Uses `_EmissionColor` shader property
- **Keyword**: Toggles `_EMISSION` keyword on/off
- **Property Block**: Avoids material instances for performance
- **Restoration**: Stores original emission state before highlighting

## Credits
- **Implementation**: Following Unity best practices and project architecture
- **Shader**: Custom HLSL/CG shader with holographic effects
- **Input System**: Unity New Input System integration
- **Physics**: Unity Physics API for spatial queries

---

**Last Updated**: November 8, 2025  
**Version**: 1.0  
**Status**: Complete and tested
