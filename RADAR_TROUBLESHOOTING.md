# Radar System - Troubleshooting Flowchart

## 🔧 Quick Diagnostic Guide

Use this flowchart to quickly diagnose and fix radar issues.

---

## 🚨 Issue: No Blips Appearing

```
START: Radar displays but no blips show up

├─ Are asteroids spawned in scene?
│  ├─ NO → Spawn asteroids first
│  └─ YES ↓
│
├─ Is RadarSystem component active?
│  ├─ NO → Enable RadarSystem GameObject
│  └─ YES ↓
│
├─ Check Inspector: Is shipTransform assigned?
│  ├─ NO → Assign Player ship transform OR auto-find will assign on Awake
│  └─ YES ↓
│
├─ Check Inspector: Is radarRange large enough?
│  ├─ NO → Increase radarRange (try 2000m for testing)
│  └─ YES ↓
│
├─ Check Inspector: Does radarLayerMask include asteroids?
│  ├─ NO → Set to "Everything" or include asteroid layer
│  └─ YES ↓
│
├─ Do asteroids have colliders?
│  ├─ NO → Add colliders to asteroid prefabs
│  └─ YES ↓
│
├─ Open Console: Any error messages?
│  ├─ YES → Read error, follow specific fix below
│  └─ NO ↓
│
└─ SOLUTION: Enable showDebugInfo and check console logs
   - If logs show "Detected X objects", check RadarDisplay
   - If logs show "Detected 0 objects", verify asteroid setup
```

**Quick Fix**: Set `radarLayerMask` to "Everything" and `radarRange` to 5000m for testing.

---

## 🎨 Issue: Blips in Wrong Positions

```
START: Blips appear but positions are incorrect

├─ Is RadarDisplay a child of the ship?
│  ├─ NO → Move RadarDisplay under Player ship in hierarchy
│  └─ YES ↓
│
├─ Is RadarDisplay using local position?
│  ├─ NO → Reset transform or ensure proper parenting
│  └─ YES ↓
│
├─ Check: Is displayRadius appropriate?
│  ├─ Too large (>1.0) → Reduce to 0.15-0.25
│  └─ Appropriate ↓
│
├─ Check: Are asteroids moving or static?
│  ├─ Moving → This is correct (reflects real position)
│  └─ Static but blips moving ↓
│
└─ SOLUTION: Check if ship's transform is being modified
   - RadarDisplay must be in ship's hierarchy
   - Positions are ship-relative
```

**Quick Fix**: Ensure RadarDisplay GameObject is direct child of Player ship.

---

## 🖼️ Issue: Radar Display Not Visible

```
START: No radar display visible at all

├─ Is RadarDisplay GameObject active?
│  ├─ NO → Enable in Hierarchy
│  └─ YES ↓
│
├─ Is RadarDisplay in camera view?
│  ├─ NO → Reposition on dashboard
│  └─ YES ↓
│
├─ Does RadarDisplay have a Renderer?
│  ├─ NO → Add MeshRenderer (should be on Plane/Cube)
│  └─ YES ↓
│
├─ Is material assigned?
│  ├─ NO → Assign RadarScreen material
│  └─ YES ↓
│
├─ Is shader working?
│  ├─ Pink/magenta material → Shader missing or broken
│  │  └─ FIX: Change shader to Standard, then back to HolographicRadar
│  └─ Shader OK ↓
│
├─ Is material opacity too low?
│  ├─ YES → Increase opacity to 0.7-1.0
│  └─ NO ↓
│
└─ SOLUTION: Check camera near/far clip planes
   - Radar should be between 0.1m and camera far plane
```

**Quick Fix**: Select RadarDisplay, press F to frame in Scene view, check if visible.

---

## ⚡ Issue: Poor Performance / Low FPS

```
START: FPS drops when radar is active

├─ How many asteroids in range?
│  ├─ >150 → This is expected, optimize settings below
│  └─ <150 ↓
│
├─ Check Inspector: maxBlips setting
│  ├─ >100 → Reduce to 50-75
│  └─ ≤100 ↓
│
├─ Check Inspector: updateInterval
│  ├─ <0.1 → Increase to 0.15-0.2 (less frequent updates)
│  └─ ≥0.1 ↓
│
├─ Check Inspector: useObjectPooling
│  ├─ FALSE → Enable object pooling
│  └─ TRUE ↓
│
├─ Open Profiler: What's expensive?
│  ├─ Physics.OverlapSphere → Reduce radarRange
│  ├─ Instantiate calls → Enable pooling
│  ├─ Material.SetColor → Already optimized with PropertyBlocks
│  └─ UpdateRadarDisplay → Increase updateInterval ↓
│
└─ SOLUTION: Optimize settings
   - maxBlips: 50
   - updateInterval: 0.2
   - radarRange: 1000
   - useObjectPooling: true
```

**Quick Fix**: Set `maxBlips = 50`, `updateInterval = 0.2`, enable `useObjectPooling`.

---

## 🎨 Issue: Shaders Not Working (Pink Material)

```
START: Radar or blips are bright pink/magenta

├─ Are shader files in Assets/Shaders/?
│  ├─ NO → Import shader files to Assets/Shaders/
│  └─ YES ↓
│
├─ Select material: What shader is assigned?
│  ├─ "Hidden/..." → Shader compilation failed
│  │  └─ Check Console for shader errors
│  └─ "Custom/HolographicRadar" or "Custom/RadarBlip" ↓
│
├─ Does shader have compile errors?
│  ├─ YES → Check Console, fix shader syntax
│  └─ NO ↓
│
├─ Try switching to Standard shader
│  ├─ Still pink → Unity/Material issue
│  └─ Works → Custom shader issue ↓
│
└─ SOLUTION: Shader compatibility
   - Built-in render pipeline: Should work
   - URP: Need shader conversion
   - HDRP: Need shader conversion
```

**Quick Fix**: Use Standard shader with Emission enabled as fallback.

**For URP**: Convert shaders using Edit → Render Pipeline → Universal Render Pipeline → ...

---

## 🔧 Issue: Setup Tool Not Working

```
START: "Tools > Asteroid Miner > Setup Radar System" fails

├─ Is ship tagged with "Player"?
│  ├─ NO → Tag ship GameObject with "Player"
│  └─ YES ↓
│
├─ Is in Play mode?
│  ├─ YES → Exit Play mode, run in Edit mode only
│  └─ NO ↓
│
├─ Check Console: What error appears?
│  ├─ "No Player found" → Tag ship with "Player"
│  ├─ "Shader not found" → Import shaders first
│  ├─ "Cannot create folder" → Check Assets/Materials/ exists
│  └─ Other error ↓
│
└─ SOLUTION: Manual setup
   - Follow RADAR_QUICK_SETUP.md → "Manual Setup"
```

**Quick Fix**: Tag your ship with "Player" tag before using setup tool.

---

## 🎯 Issue: Radar Not Detecting Specific Objects

```
START: Some objects not appearing on radar

├─ What type not showing?
│  ├─ Asteroids → Check showAsteroids = true
│  ├─ Hazards → Check showHazards = true
│  └─ NPCs → Check showNPCs = true
│
├─ Are they within radarRange?
│  ├─ NO → Increase range or move objects closer
│  └─ YES ↓
│
├─ Do they have colliders?
│  ├─ NO → Add colliders
│  └─ YES ↓
│
├─ Are colliders on the right layer?
│  ├─ NO → Change layer OR update radarLayerMask
│  └─ YES ↓
│
├─ Check DetermineContactType() logic
│  └─ May not recognize object type correctly
│
└─ SOLUTION: Update classification logic in RadarSystem.cs
   - Add tags/layers for new object types
   - Update DetermineContactType() method
```

**Quick Fix**: Set `radarLayerMask` to "Everything" and `showX = true` for all types.

---

## 🌈 Issue: Colors Not Showing Correctly

```
START: Blips are wrong color or all same color

├─ Is blipMaterial assigned in RadarDisplay?
│  ├─ NO → Create and assign material
│  └─ YES ↓
│
├─ Does material use RadarBlip shader?
│  ├─ NO → Change shader to Custom/RadarBlip
│  └─ YES ↓
│
├─ Check: Are asteroids different rarities?
│  ├─ All common → Spawn different types
│  └─ Mixed rarities ↓
│
├─ Is GetColorForRarity() being called?
│  ├─ Add Debug.Log in method to verify
│  └─ Check if asteroidComponent is null ↓
│
└─ SOLUTION: Verify asteroid data
   - Asteroids need AsteroidType ScriptableObject
   - Type must have rarity set
   - RadarContact must find asteroidComponent
```

**Quick Fix**: Check that asteroids have AsteroidType assigned with rarity set.

---

## 🔄 Issue: Radar Updates Slowly or Not at All

```
START: Blips don't update in real-time

├─ Check Inspector: updateInterval value
│  ├─ >1.0 → Very slow, reduce to 0.1-0.2
│  └─ <1.0 ↓
│
├─ Is RadarSystem component enabled?
│  ├─ NO → Enable component
│  └─ YES ↓
│
├─ Is Update() being called?
│  ├─ Add Debug.Log in Update to verify
│  └─ If called ↓
│
├─ Is UpdateRadarContacts() working?
│  └─ Add Debug.Log at start of method
│
└─ SOLUTION: Check for exceptions
   - Open Console
   - Look for errors pausing execution
   - Fix any null references
```

**Quick Fix**: Set `updateInterval = 0.1` and enable `showDebugInfo`.

---

## 🎮 Issue: Input Not Working

```
START: Can't control radar with keyboard/gamepad

├─ Are you using RadarSystemExample.cs?
│  ├─ NO → Add component or implement input handling
│  └─ YES ↓
│
├─ Is old Input class conflicting?
│  ├─ Check if InputManager is set to "Both"
│  └─ Set to "Input System Package" only
│
├─ Are input actions enabled?
│  └─ Check OnEnable() called ↓
│
└─ SOLUTION: Use new Input System
   - Ensure Input System package installed
   - Configure in Edit → Project Settings → Player
```

**Quick Fix**: Check Project Settings → Player → Active Input Handling = "Input System Package".

---

## 💾 Issue: Settings Not Saving

```
START: Radar settings reset after reload

├─ Is Save/Load system implemented?
│  ├─ NO → Implement save integration (see RADAR_INTEGRATION_GUIDE.md)
│  └─ YES ↓
│
├─ Is RadarSettings class [Serializable]?
│  ├─ NO → Add [System.Serializable] attribute
│  └─ YES ↓
│
├─ Is SaveRadarState() being called?
│  └─ Add Debug.Log to verify ↓
│
└─ SOLUTION: Verify serialization
   - Check save file contains radar data
   - Verify LoadRadarState() called on game start
```

**Quick Fix**: Add radar settings to your GameState save data.

---

## 🎯 Success Verification Checklist

After fixing issues, verify:
- [ ] Blips appear for asteroids in range
- [ ] Blips positioned correctly relative to ship
- [ ] Colors match asteroid rarity
- [ ] Grid lines visible
- [ ] Ship indicator at center
- [ ] Holographic effects animating
- [ ] Performance 60+ FPS
- [ ] No console errors
- [ ] Radar rotates with ship
- [ ] Display visible on dashboard

---

## 📞 Still Having Issues?

### Debug Checklist:
1. Enable `showDebugInfo` in RadarSystem
2. Check Unity Console for errors
3. Use Scene view Gizmos to visualize detection range
4. Check Profiler for performance bottlenecks
5. Verify all references assigned in Inspector
6. Test with simple scene (just ship + few asteroids)

### Common Root Causes:
- Missing component references
- Incorrect layer masks
- Wrong shader/render pipeline
- Objects outside detection range
- Disabled GameObjects
- Input system conflicts

### Last Resort:
1. Delete RadarSystem and RadarDisplay
2. Run setup tool again
3. Manually assign all references
4. Test in isolation

---

## 🆘 Emergency Quick Fixes

### If Nothing Works:
```csharp
// Paste this in a test script to verify basic detection:
void Update()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, 1000f);
    Debug.Log($"Found {hits.Length} objects within 1000m");
}
```

If this shows 0 objects, problem is with your asteroids/colliders, not radar.
If this shows objects, problem is in RadarSystem logic.

---

**Most issues are fixed by:**
1. Checking layer masks
2. Verifying range
3. Ensuring proper hierarchy
4. Enabling debug logging

Good luck! 🚀
