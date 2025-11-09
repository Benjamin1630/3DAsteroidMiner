# Mining System Update - Raycast-Based Targeting

## 🎯 What Changed

The mining system has been **completely redesigned** from a proximity-based system to a **precision raycast-based targeting system**.

### Before (Proximity-Based)
- ❌ Automatically mined closest asteroids in range
- ❌ No player control over which asteroid to mine
- ❌ Could mine asteroids behind you or off-screen
- ❌ 50m range from player position

### After (Raycast-Based) ✅
- ✅ **Aim with camera crosshair** - mine exactly what you're looking at
- ✅ **Precision targeting** - player has full control
- ✅ **Visual feedback** - see raycast and hit detection in Scene view
- ✅ **100m range** from camera position
- ✅ **Intuitive gameplay** - look at asteroid, hold button, mine!

---

## 🔧 Technical Changes

### MiningSystem.cs Updates

#### New Fields
```csharp
[Header("Raycast Targeting")]
[SerializeField] private Camera playerCamera;
[SerializeField] private bool useScreenCenter = true; // Crosshair vs mouse aiming
[SerializeField] private LayerMask asteroidLayerMask = ~0; // What to raycast against
```

#### New Method: GetCameraRay()
```csharp
private Ray GetCameraRay()
{
    if (useScreenCenter)
        return playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Crosshair
    else
        return playerCamera.ScreenPointToRay(Input.mousePosition); // Mouse
}
```

#### Redesigned: AcquireTargets()
**Old Approach:**
- Used `Physics.OverlapSphere()` to find all asteroids in radius
- Sorted by distance, took closest ones
- No player aim required

**New Approach:**
```csharp
// 1. Raycast from camera center/mouse position
Ray primaryRay = GetCameraRay();
if (Physics.Raycast(primaryRay, out RaycastHit hit, miningRange, asteroidLayerMask))
{
    // Get asteroid at raycast hit point
    Asteroid asteroid = hit.collider.GetComponentInParent<Asteroid>();
    targetedAsteroids.Add(asteroid);
}

// 2. If multi-mining enabled, find nearby asteroids around primary target
if (maxTargets > 1 && targetedAsteroids.Count > 0)
{
    Vector3 centerPoint = targetedAsteroids[0].transform.position;
    Collider[] nearbyColliders = Physics.OverlapSphere(centerPoint, miningRange * 0.3f, asteroidLayerMask);
    // Add visible nearby asteroids to target list
}
```

#### Updated: Range Checks
- Now checks distance from **camera** instead of player ship
- Laser origins still on ship, but targeting is camera-based

#### Enhanced: Debug Visualization
- **Cyan ray** shows camera raycast direction
- **Green sphere** shows raycast hit point
- **Red wire spheres** show active mining targets
- **Green wire sphere** shows mining range from camera

---

## 🎮 Gameplay Impact

### For Players
1. **More Engaging** - Active aiming creates skill-based gameplay
2. **Predictable** - You know exactly what you're mining
3. **Strategic** - Choose high-value asteroids by looking at them
4. **Compatible with Flight** - Works naturally with 6DoF space flight
5. **No Frustration** - Won't accidentally mine wrong asteroids

### For Designers
- **Mining Range**: Increased to 100m (was 50m) - long-range precision mining
- **Multi-Mining**: Still works - finds asteroids near the primary target
- **Upgrades**: All existing upgrades still apply (mining speed, multiMining, fuel efficiency)

---

## 📋 Setup Instructions

### In Unity Inspector:
1. **Player Camera** - Auto-finds Camera.main (or assign manually)
2. **Use Screen Center** - ✅ Check for crosshair aiming (recommended)
3. **Asteroid Layer Mask** - Set to "Asteroid" layer
4. **Mining Range** - 100m (adjust to taste)

### Requirements:
- ✅ Asteroids must have **Collider** components (for raycasting)
- ✅ Asteroids must be on "Asteroid" layer
- ✅ Camera must be tagged as "MainCamera" or assigned manually

---

## 🎯 Aiming Modes

### Mode 1: Screen Center (Crosshair) - **RECOMMENDED**
```csharp
useScreenCenter = true;
```
- Mines asteroid at **camera center**
- Best for **first-person** and **third-person** views
- Intuitive: "look at it, mine it"
- Works great with gamepad and keyboard controls

### Mode 2: Mouse Position (Mouse Aim)
```csharp
useScreenCenter = false;
```
- Mines asteroid under **mouse cursor**
- Best for **RTS-style** or **top-down** views
- More precise mouse control
- Requires mouse input (not gamepad-friendly)

---

## 🔍 Debug Tools

### Scene View Visualization (Select MiningSystem GameObject)
When selected in editor and playing:
- **Cyan Line** = Camera raycast (where you're aiming)
- **Green Sphere** = Raycast hit point (what you'll mine)
- **Green Wire Sphere** = Mining range radius from camera
- **Yellow Spheres** = Laser origin mount points
- **Red Wire Spheres** = Currently active mining targets

### Console Debug Logs
```csharp
Debug.Log($"MiningSystem: Acquired {targetCount} targets (max: {maxTargets})");
Debug.Log("MiningSystem: No asteroid found under crosshair");
```

---

## ⚙️ Configuration Guide

### For Long-Range Mining
```csharp
miningRange = 150f; // Snipe asteroids from afar
```

### For Close-Range Mining
```csharp
miningRange = 50f; // Short range, intense mining
```

### For Multi-Target Mining
```csharp
// In UpdateMining(), adjust nearby search radius:
Physics.OverlapSphere(centerPoint, miningRange * 0.5f, asteroidLayerMask); // Wider search
```

### For Precision-Only Mining
```csharp
// In GameState, set multiMining to 1
// Player can only mine one asteroid at a time (what they're aiming at)
```

---

## 🐛 Common Issues

### Issue: "No asteroid found under crosshair"
**Cause:** Raycast not hitting asteroid
**Fix:**
1. Ensure asteroid has a **Collider** component
2. Check asteroid is on "Asteroid" layer
3. Verify Asteroid Layer Mask includes "Asteroid"
4. Make sure asteroid is within 100m range

### Issue: Laser beams point to wrong location
**Cause:** Laser origins not set up correctly
**Fix:**
1. Assign laser origin Transforms in Inspector
2. Or leave empty to use player ship center

### Issue: Can't mine asteroids behind obstacles
**Feature:** Raycasting respects occlusion - you can't mine through things!
**Solution:** Move to a position with clear line of sight

---

## 🚀 Future Enhancements

### Potential Additions
- [ ] **Crosshair UI** - Visual indicator of what you're aiming at
- [ ] **Target Lock** - Press button to lock onto asteroid, then mine
- [ ] **Smart Targeting** - Cycle through nearby asteroids with button press
- [ ] **Mining Reticle** - Changes color when valid target detected
- [ ] **Range Indicator** - UI showing distance to aimed asteroid
- [ ] **Charge-Up Mechanic** - Hold longer for more powerful mining beam

### Advanced Features
- [ ] **Penetration Mining** - Upgrade to mine through multiple asteroids in line
- [ ] **Cone Targeting** - Mine all asteroids in a cone in front of camera
- [ ] **Laser Sweep** - Drag mouse to sweep laser across multiple targets
- [ ] **Aim Assist** - Subtle magnetism to nearby asteroids

---

## 📊 Performance Notes

### Optimization
- ✅ Single raycast per frame (when acquiring targets)
- ✅ Cached camera reference
- ✅ LayerMask filtering (only checks Asteroid layer)
- ✅ No GetComponent in update loop

### Physics Queries
- **AcquireTargets**: 1 raycast + optional sphere overlap (for multi-mining)
- **UpdateMining**: Only distance checks (no raycasts during mining)
- **Frequency**: Only when acquiring new targets or losing old ones

---

## 📖 Code Example: Custom Targeting

### Example: Mining Specific Asteroid Type Only
```csharp
// In AcquireTargets(), after raycast hit:
Asteroid asteroid = hit.collider.GetComponentInParent<Asteroid>();
if (asteroid != null && asteroid.Type.resourceName == "Gold") // Only mine gold!
{
    targetedAsteroids.Add(asteroid);
}
```

### Example: Aim Assist for Rare Asteroids
```csharp
// Modify raycast to favor rare asteroids
Ray primaryRay = GetCameraRay();
RaycastHit[] hits = Physics.RaycastAll(primaryRay, miningRange, asteroidLayerMask);

// Sort by asteroid rarity, prefer legendary/epic
Array.Sort(hits, (a, b) => {
    Asteroid astA = a.collider.GetComponentInParent<Asteroid>();
    Asteroid astB = b.collider.GetComponentInParent<Asteroid>();
    return astB.Type.value.CompareTo(astA.Type.value); // Higher value first
});
```

---

## ✅ Upgrade Compatibility

All existing upgrades work seamlessly:

| Upgrade | Effect with Raycast System |
|---------|---------------------------|
| **mining** | Increases mining speed (same as before) |
| **multiMining** | Adds nearby asteroids around aimed target |
| **fuelEfficiency** | Reduces fuel consumption (same as before) |
| **cargo** | Inventory capacity (same as before) |
| **scanRange** | Not directly used, but scan still helps identify valuable targets |

---

## 🎓 Best Practices

### For Space Combat Feel
- Set mining range to 100-150m
- Use crosshair aiming mode
- Add UI crosshair graphic in center of screen
- Consider adding "mining lead indicator" for moving asteroids

### For Resource Gathering Feel
- Set mining range to 50-75m
- Use multi-mining (level 4-6)
- Auto-target nearby asteroids aggressively
- Add particle effects at hit points

### For Precision Mining
- Set mining range to 200m+
- Disable multi-mining (level 1 only)
- Add zoom camera feature
- Add distance indicator UI

---

**Status:** ✅ Fully Implemented and Tested  
**Last Updated:** November 9, 2025  
**Version:** 2.0 (Raycast-Based)
