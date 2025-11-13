# 3D Radar/Minimap System - Star Citizen Style

## Overview
A holographic 3D radar system similar to Star Citizen that displays nearby asteroids, hazards, and NPCs on a dashboard-mounted display within the ship's cockpit. The ship is always at the center with contacts plotted around it in real-time.

## Features
- **3D Holographic Display**: Volumetric radar with elevation visualization
- **Multiple Display Modes**: 2D flat, 3D spherical, or cylindrical projection
- **Contact Classification**: Asteroids, hazards, NPCs, and stations with color coding
- **Distance-Based Fading**: Blips fade and scale with distance
- **Holographic Effects**: Scanlines, fresnel rim lighting, flickering, and glowing blips
- **Object Pooling**: High-performance blip management
- **Customizable Range**: Adjustable radar detection range
- **Grid Visualization**: Circular grid rings and radial lines
- **Ship Center Indicator**: Always shows player position at center

## Components

### 1. RadarSystem.cs
**Purpose**: Core radar logic that detects and tracks nearby objects.

**Key Features**:
- Physics-based detection using `OverlapSphereNonAlloc` for performance
- Configurable update rate and detection range
- Contact classification (Asteroid, Hazard, NPC, Station)
- Rarity-based color coding for asteroids
- Filtering options for different object types
- Public API for external systems to query contacts

**Configuration**:
```csharp
[SerializeField] private float radarRange = 1000f;           // Detection radius
[SerializeField] private float updateInterval = 0.1f;        // Update rate (seconds)
[SerializeField] private LayerMask radarLayerMask = -1;      // What layers to detect
[SerializeField] private Transform shipTransform;            // Ship reference
```

**Public Methods**:
- `SetRadarRange(float range)` - Dynamically adjust radar range
- `ToggleAsteroids(bool show)` - Show/hide asteroids
- `ToggleHazards(bool show)` - Show/hide hazards
- `ToggleNPCs(bool show)` - Show/hide NPCs
- `GetContactsByType(ContactType type)` - Get filtered contacts
- `GetClosestContact(ContactType type)` - Find nearest contact of type

### 2. RadarDisplay.cs
**Purpose**: Visual representation of the radar data on a 3D display.

**Display Modes**:
1. **Circular2D**: Flat top-down view (traditional radar)
2. **Circular3D**: Full 3D spherical display with elevation (Star Citizen style)
3. **Cylindrical**: Cylindrical projection for better distance visualization

**Configuration**:
```csharp
[SerializeField] private float displayRadius = 0.15f;        // Physical size in world units
[SerializeField] private float displayHeight = 0.05f;        // Vertical range for 3D mode
[SerializeField] private float blipScale = 0.005f;           // Size of individual blips
[SerializeField] private bool fadeByDistance = true;         // Distance-based fading
[SerializeField] private int maxBlips = 100;                 // Performance limit
[SerializeField] private bool useObjectPooling = true;       // Use pooling for blips
```

**Grid Settings**:
```csharp
[SerializeField] private Color gridColor = new Color(0.2f, 0.8f, 1f, 0.5f);
[SerializeField] private int gridRingCount = 4;              // Number of circular rings
[SerializeField] private int gridRadialLines = 8;            // Number of radial lines
```

**Public Methods**:
- `SetDisplayMode(RadarDisplayMode mode)` - Change display mode
- `SetDisplayRadius(float radius)` - Adjust physical display size
- `SetGridVisible(bool visible)` - Toggle grid lines

### 3. RadarBlip.cs
**Purpose**: Individual blip representation with visual effects.

**Features**:
- Per-instance color and emission using MaterialPropertyBlock
- Distance-based scaling and fading
- Pulse effect for important targets
- Highlight mode for selection/targeting
- Efficient rendering (no material duplication)

**Public Methods**:
- `UpdateBlip(RadarContact contact, bool fadeByDistance, float maxRange)` - Update appearance
- `SetColor(Color color)` - Change blip color
- `SetPulse(bool pulse)` - Enable/disable pulsing effect
- `SetHighlight(bool highlighted)` - Highlight blip for targeting

### 4. HolographicRadar.shader
**Purpose**: Holographic display shader for the radar screen/grid.

**Effects**:
- Fresnel rim lighting (holographic edge glow)
- Animated scanlines (moving horizontal lines)
- Subtle flickering for realism
- Optional grid pattern overlay
- Transparency with emission

**Properties**:
```hlsl
_Color               - Base holographic color
_EmissionColor       - Glow color and intensity
_FresnelPower        - Rim light sharpness (1-10)
_FresnelIntensity    - Rim light brightness (0-5)
_ScanlineSpeed       - Scanline animation speed (0-10)
_ScanlineFrequency   - Scanline density (1-50)
_Opacity             - Overall transparency (0-1)
_FlickerSpeed        - Flicker animation speed (0-20)
_FlickerIntensity    - Flicker strength (0-0.5)
```

### 5. RadarBlip.shader
**Purpose**: Shader for glowing radar blips with pulsing effect.

**Effects**:
- Additive blending for bright glow
- Animated pulsing brightness
- Fresnel edge glow
- Emission control

**Properties**:
```hlsl
_Color            - Blip base color
_EmissionColor    - Glow color and intensity
_GlowIntensity    - Overall glow brightness (0-10)
_PulseSpeed       - Pulse animation speed (0-10)
_FresnelPower     - Edge glow sharpness (0.1-5)
```

## Setup Instructions

### Step 1: Create Radar System
1. Create empty GameObject in your ship cockpit: `GameObject > Create Empty`
2. Name it "RadarSystem"
3. Add `RadarSystem.cs` component
4. Configure detection range and layers

### Step 2: Create Radar Display
1. Create child GameObject under RadarSystem: `GameObject > 3D Object > Plane` (or Cube)
2. Name it "RadarDisplay"
3. Position it on your ship's dashboard (scale appropriately, e.g., 0.2 x 0.2 x 0.2)
4. Add `RadarDisplay.cs` component
5. Assign the `RadarSystem` reference in inspector
6. Configure display radius and mode

### Step 3: Create Materials
1. **Radar Screen Material**:
   - Create new material: `Assets > Create > Material`
   - Name it "RadarScreen_Mat"
   - Set shader to `Custom/HolographicRadar`
   - Adjust colors: Base color (cyan), Emission (bright cyan)
   - Set opacity to ~0.7
   - Apply to RadarDisplay plane/cube

2. **Radar Blip Material**:
   - Create new material: `Assets > Create > Material`
   - Name it "RadarBlip_Mat"
   - Set shader to `Custom/RadarBlip`
   - Adjust glow intensity and pulse speed
   - Assign to RadarDisplay component's "Blip Material" field

### Step 4: Configure Layers
1. Create layer for asteroids if not exists: `Edit > Project Settings > Tags and Layers`
2. Assign asteroids to appropriate layer
3. Set RadarSystem's `radarLayerMask` to detect desired layers

### Step 5: Position on Dashboard
1. Position RadarDisplay as child of ship's cockpit interior
2. Rotate to face pilot seat (typically tilted ~45 degrees)
3. Scale to appropriate size (0.15-0.3 units typically good)
4. Use local coordinates so it moves with ship

### Step 6: Testing
1. Enter Play mode
2. Spawn asteroids near ship
3. Verify blips appear on radar display
4. Test different display modes
5. Adjust colors, ranges, and effects as desired

## Color Coding Scheme

### By Contact Type
- **Asteroids**: Green (default)
- **Hazards**: Red
- **NPCs**: Yellow
- **Stations**: Blue

### By Asteroid Rarity
- **Common**: Gray (60% spawn rate)
- **Uncommon**: Green (20% spawn rate)
- **Rare**: Blue (12% spawn rate)
- **Epic**: Purple (6% spawn rate)
- **Legendary**: Orange (2% spawn rate)

Colors can be customized in `RadarSystem.GetColorForRarity()` and `RadarContact.GetColorForType()`.

## Performance Optimization

### Object Pooling
The system uses object pooling by default to avoid garbage collection:
- Pool initialized with 20 blips
- Grows dynamically as needed
- Blips reused instead of destroyed/recreated
- Can be disabled by setting `useObjectPooling = false`

### Update Frequency
- Default: 10 updates per second (0.1s interval)
- Increase for smoother tracking: 0.05s (20 fps)
- Decrease for better performance: 0.2s (5 fps)

### Max Blips
- Default limit: 100 blips
- Increase for denser asteroid fields
- Decrease for better performance on low-end hardware

### Physics Detection
- Uses `OverlapSphereNonAlloc` (no allocations)
- Preallocated buffer for 200 colliders
- Culls contacts outside radar range
- Filters by layer mask for efficiency

## Customization Examples

### Example 1: Larger Radar Range
```csharp
RadarSystem radarSystem = GetComponent<RadarSystem>();
radarSystem.SetRadarRange(2000f); // Double the range
```

### Example 2: Change Display Mode
```csharp
RadarDisplay radarDisplay = GetComponent<RadarDisplay>();
radarDisplay.SetDisplayMode(RadarDisplay.RadarDisplayMode.Circular3D);
```

### Example 3: Filter Only Asteroids
```csharp
radarSystem.ToggleHazards(false);
radarSystem.ToggleNPCs(false);
radarSystem.ToggleAsteroids(true);
```

### Example 4: Find Nearest Asteroid
```csharp
RadarSystem.RadarContact closest = radarSystem.GetClosestContact(RadarSystem.ContactType.Asteroid);
if (closest != null)
{
    Debug.Log($"Nearest asteroid: {closest.displayName} at {closest.distance}m");
}
```

### Example 5: Custom Blip Colors
Edit `RadarSystem.cs` in `GetColorForRarity()`:
```csharp
case AsteroidRarity.Legendary:
    return new Color(1f, 0f, 1f, 1f); // Magenta instead of orange
```

## Integration with Existing Systems

### Mining System Integration
Target nearest asteroid on radar:
```csharp
RadarSystem.RadarContact target = radarSystem.GetClosestContact(RadarSystem.ContactType.Asteroid);
if (target != null && target.asteroidComponent != null)
{
    miningSystem.SetTarget(target.asteroidComponent);
}
```

### Scanner System Integration
Highlight scanned asteroids:
```csharp
// In ScannerSystem.cs after scanning
foreach (Asteroid scannedAsteroid in scannedAsteroids)
{
    // Mark as scanned, radar will show with different color
    scannedAsteroid.MarkAsScanned();
}
```

### Upgrade System Integration
Upgrade radar range:
```csharp
public void UpgradeRadarRange(int level)
{
    float newRange = 1000f + (level * 200f); // +200m per level
    radarSystem.SetRadarRange(newRange);
}
```

## Troubleshooting

### Problem: No blips appearing
**Solutions**:
1. Check RadarSystem has ship transform assigned
2. Verify asteroids are within radarRange
3. Check radarLayerMask includes asteroid layer
4. Ensure asteroids have colliders
5. Verify RadarDisplay has RadarSystem reference

### Problem: Blips in wrong positions
**Solutions**:
1. Ensure RadarDisplay is child of ship (or transform hierarchy correct)
2. Check displayRadius is appropriate scale
3. Verify ship transform is correctly assigned
4. Check display mode matches desired visualization

### Problem: Performance issues
**Solutions**:
1. Reduce maxBlips count
2. Increase updateInterval (less frequent updates)
3. Reduce radarRange
4. Enable object pooling if disabled
5. Optimize radarLayerMask to exclude unnecessary layers

### Problem: Shaders not working
**Solutions**:
1. Ensure shaders are in Assets/Shaders folder
2. Check Unity version compatibility (shaders use CG, compatible with 2019.4+)
3. For URP/HDRP, shaders may need conversion
4. Verify materials are assigned correct shaders

### Problem: Blips too small/large
**Solutions**:
1. Adjust blipScale in RadarDisplay (default 0.005)
2. Modify displayRadius for overall radar size
3. Check camera distance from radar display
4. Adjust fadeByDistance scaling in RadarBlip.UpdateBlip()

## Advanced Features

### Custom Contact Types
Add new contact types in `RadarSystem.ContactType`:
```csharp
public enum ContactType
{
    Asteroid,
    Hazard,
    NPC,
    Station,
    Cargo,      // NEW: Floating cargo
    Wreckage,   // NEW: Ship wreckage
    Unknown
}
```

Update detection logic accordingly.

### Distance Rings
Add distance indicators to display:
```csharp
// In RadarDisplay.InitializeRadarGrid()
// Add text labels for each ring showing distance
```

### Targeted Asteroid Highlight
Highlight current mining target:
```csharp
// When mining system selects target
RadarBlip targetBlip = FindBlipForAsteroid(targetAsteroid);
if (targetBlip != null)
{
    targetBlip.SetHighlight(true);
    targetBlip.SetPulse(true);
}
```

### Audio Feedback
Add audio pings for nearby contacts:
```csharp
// In RadarSystem.UpdateRadarContacts()
if (detectedContacts.Count > previousCount)
{
    audioSource.PlayOneShot(radarPingSound);
}
```

## Technical Notes

### Coordinate System
- Radar uses ship's local space (rotates with ship)
- RelativePosition transformed via `InverseTransformDirection`
- Display positions calculated in local coordinates of RadarDisplay

### Material Property Blocks
- Used for per-instance color without material duplication
- More efficient than creating materials per blip
- Allows hundreds of uniquely colored blips

### LineRenderer for Grid
- Grid uses LineRenderer for smooth circles and radial lines
- Width set to 0.001 for thin holographic lines
- Uses local space (useWorldSpace = false)

### Shader Considerations
- HolographicRadar: Uses two-sided rendering (Cull Off)
- RadarBlip: Uses additive blending (Blend One One)
- Both use transparency and emission
- Compatible with forward and deferred rendering

## Future Enhancements

1. **Elevation Indicator**: Visual lines showing height difference
2. **Zoom Levels**: Switch between close/far radar ranges
3. **Contact Icons**: Different shapes for different types (instead of cubes)
4. **Threat Assessment**: Color based on danger level
5. **Waypoint System**: Mark positions on radar
6. **Historical Trails**: Show movement history of contacts
7. **Radar Jamming**: Interference effects and signal loss
8. **3D Audio Positioning**: Spatial audio for radar contacts

## Credits
Inspired by Star Citizen, Elite Dangerous, and No Man's Sky radar systems.

## Version
- **Version**: 1.0
- **Last Updated**: November 13, 2025
- **Unity Version**: 2022.3+
- **Author**: AI Assistant
