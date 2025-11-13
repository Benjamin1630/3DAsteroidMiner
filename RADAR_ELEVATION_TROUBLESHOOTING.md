# Radar Elevation Line Troubleshooting Guide

## Overview
This guide provides systematic troubleshooting steps for debugging elevation line issues in the Star Citizen-style radar system.

---

## 🔍 Quick Diagnostic Steps

### Step 1: Run the Debugger
1. **Add the debugger component:**
   - Select your `RadarDisplay` GameObject in the hierarchy
   - Add Component → `RadarElevationDebugger`
   - Check "Run On Start" if you want automatic diagnostics

2. **Run diagnostics in Play mode:**
   - Press `D` key during Play mode
   - Check the Console for detailed diagnostic output

3. **Review the diagnostic report:**
   - Look for errors in red
   - Check the checklist at the bottom
   - Review individual blip details

---

## 🔧 Common Issues & Solutions

### Issue 1: Elevation Lines Not Visible

**Symptoms:**
- Blips appear on radar
- No vertical lines or circles visible
- Gizmos show lines in Scene view but not Game view

**Diagnostic Steps:**
1. Check if `RadarDisplay.showElevationLines` is `true`
2. Verify blips have Y position != 0 (vertical offset)
3. Check if LineRenderer components are enabled
4. Verify line width is not too small

**Solutions:**
```csharp
// In Inspector:
RadarDisplay.showElevationLines = true
RadarDisplay.displayMode = Circular3D (not Circular2D)

// Check in code:
Debug.Log($"Show Elevation: {radarDisplay.showElevationLines}");
Debug.Log($"Display Mode: {radarDisplay.displayMode}");
```

**Manual Fix:**
- Select a RadarBlip in Play mode
- Check "Debug Elevation Lines" on the RadarBlip component
- Look for elevation line debug output in Console

---

### Issue 2: Lines Appear in Wrong Position

**Symptoms:**
- Lines are visible but don't connect blip to ship plane
- Lines point in wrong direction
- Circles are offset

**Diagnostic Steps:**
1. Check blip hierarchy: `RadarBlip > ElevationLine` (child)
2. Verify `useWorldSpace = false` on LineRenderer
3. Check `transform.localPosition` of blip
4. Enable Debug Elevation Lines and check console logs

**Root Cause:**
The elevation line is calculated in the blip's **local space**. The line endpoint must be:
```csharp
Vector3 lineEnd = new Vector3(0f, -verticalOffset, 0f);
```
Where `verticalOffset = transform.localPosition.y`

**Fix Applied:**
The UpdateElevationLine method now correctly calculates:
- Line start: `Vector3.zero` (blip center in local space)
- Line end: `(0, -localPosition.y, 0)` (ship plane in blip's local space)
- Circle Y: `-localPosition.y` (ship plane position)

---

### Issue 3: Lines Not Updating

**Symptoms:**
- Lines appear but don't follow asteroids
- Lines stay in one position
- Lines don't update when ship moves

**Diagnostic Steps:**
1. Check if `UpdateBlip()` is being called each frame
2. Verify position tracking: `lastLocalPosition` comparison
3. Enable debug logging to see update frequency

**Solution:**
The code now updates elevation lines whenever position changes:
```csharp
if (transform.localPosition != lastLocalPosition)
{
    lastLocalPosition = transform.localPosition;
    UpdateElevationLine(); // Recalculate immediately
}
```

---

### Issue 4: Hierarchy Issues

**Symptoms:**
- Elevation line GameObjects are null
- Components exist but aren't children of blip
- Lines disappear when blip moves

**Diagnostic Steps:**
1. Run `ValidateElevationSetup()` on a blip
2. Check Unity Hierarchy view in Play mode
3. Look for orphaned "ElevationLine" objects

**Expected Hierarchy:**
```
RadarDisplay
  └─ BlipContainer
      └─ RadarBlip (Clone)
          ├─ ElevationLine (LineRenderer)
          └─ ElevationCircle (LineRenderer)
```

**Fix:**
Ensure SetParent uses `false` for worldPositionStays:
```csharp
elevationLineObject.transform.SetParent(transform, false);
```

---

### Issue 5: Lines Too Thin/Invisible

**Symptoms:**
- Gizmos show lines
- Console shows lines are enabled
- Still not visible in Game view

**Diagnostic Steps:**
1. Check line width: Should be `0.002f` or larger
2. Check material/shader: Should use "Particles/Standard Unlit"
3. Check camera rendering: Lines might be culled
4. Check layer: Lines should be on same layer as blips

**Solutions:**
```csharp
// Increase line width (in RadarBlip.cs)
private float elevationLineWidth = 0.005f; // Try larger values

// Check shader in Console diagnostic
// Should show: "Particles/Standard Unlit"

// Verify rendering settings
elevationLine.alignment = LineAlignment.View; // Always face camera
elevationLine.sortingOrder = 1000; // Render on top
```

---

### Issue 6: Circles Not Visible

**Symptoms:**
- Vertical lines work
- Horizontal circles don't appear

**Diagnostic Steps:**
1. Check `elevationCircleRadius` (default: 0.02f)
2. Verify circle has enough segments (default: 16)
3. Check if circle LineRenderer is enabled
4. Verify circle positions are being set

**Solution:**
Circle is drawn at the ship's plane:
```csharp
float circleY = -verticalOffset; // Ship's plane in blip's local space

for (int i = 0; i <= elevationCircleSegments; i++)
{
    float angle = (float)i / elevationCircleSegments * Mathf.PI * 2f;
    float x = Mathf.Cos(angle) * elevationCircleRadius;
    float z = Mathf.Sin(angle) * elevationCircleRadius;
    Vector3 circlePoint = new Vector3(x, circleY, z);
    elevationCircle.SetPosition(i, circlePoint);
}
```

---

## 🎯 Systematic Troubleshooting Procedure

### Phase 1: Verify Components Exist
1. Enter Play mode
2. Press `D` to run diagnostics
3. Check "Blips with Elevation Components" count
4. If 0: CreateElevationLine() is not being called

### Phase 2: Verify Components Are Enabled
1. Check "Enabled Elevation Lines" count
2. If 0 but components exist:
   - Check `RadarDisplay.showElevationLines = true`
   - Check blips have Y position != 0
   - Check `showElevationLine` flag on blips

### Phase 3: Verify Rendering Settings
1. Select a blip in Hierarchy during Play mode
2. Expand to see `ElevationLine` and `ElevationCircle` children
3. Click on `ElevationLine`
4. Check Inspector:
   - LineRenderer enabled: ✓
   - Position Count: 2
   - Start Width / End Width: 0.002 or larger
   - Use World Space: ✗ (should be false)
   - Material: assigned
   - Positions [0] and [1]: should have different Y values

### Phase 4: Verify Positions
1. Enable "Debug Elevation Lines" on a RadarBlip
2. Check Console for position logs
3. Verify:
   - `verticalOffset` is not near zero
   - `lineEnd.y` equals `-verticalOffset`
   - `circleY` equals `-verticalOffset`
   - World space positions make sense

### Phase 5: Visual Debug
1. Open Scene view alongside Game view
2. Check if gizmos show elevation lines (yellow for lines, cyan for circles)
3. If gizmos show but Game view doesn't:
   - Camera culling issue
   - Layer mismatch
   - Material/shader issue

---

## 📊 Debug Logging

### Enable Detailed Logging
On any `RadarBlip` component:
1. Check "Debug Elevation Lines"
2. Run the game
3. Console will show:
   - When elevation line is created
   - Line positions (local and world space)
   - Whether lines are enabled
   - Vertical offset calculations

### What to Look For
**Good Output:**
```
RadarBlip: Created elevation line - parent=RadarBlip, useWorldSpace=False
RadarBlip RadarBlip (Clone): localPos=(0.05, 0.03, -0.02), verticalOffset=0.03, 
  lineStart=(0,0,0), lineEnd=(0,-0.03,0), circleY=-0.03, 
  lineEnabled=True, circleEnabled=True
World space: start=(x1,y1,z1), end=(x2,y2,z2)
```

**Bad Output (Problem Indicators):**
```
RadarBlip: Elevation components missing on RadarBlip (Clone), creating...
// Means components are being destroyed/lost

RadarBlip: No elevation (offset=0.000)
// Means all asteroids are at exactly ship's height (unlikely unless using Circular2D mode)

elevationLine LineRenderer is NULL!
// Means CreateElevationLine failed
```

---

## 🔬 Manual Inspection Checklist

While game is running in Play mode:

- [ ] RadarDisplay exists and is active
- [ ] RadarDisplay.showElevationLines = true
- [ ] RadarDisplay.displayMode = Circular3D or Cylindrical
- [ ] RadarSystem is detecting asteroids (check Detected Contacts count)
- [ ] RadarBlip objects exist in Hierarchy under BlipContainer
- [ ] RadarBlip has child "ElevationLine" GameObject
- [ ] RadarBlip has child "ElevationCircle" GameObject
- [ ] ElevationLine has LineRenderer component
- [ ] LineRenderer.enabled = true
- [ ] LineRenderer.positionCount = 2
- [ ] LineRenderer.startWidth >= 0.002
- [ ] LineRenderer.useWorldSpace = false
- [ ] LineRenderer.material is assigned
- [ ] LineRenderer position[0] and position[1] are different
- [ ] ElevationCircle.positionCount = 17 (16 segments + 1)
- [ ] ElevationCircle.loop = true

---

## 💡 Quick Fixes

### If nothing works after all checks:

**Nuclear Option 1: Increase Line Width**
```csharp
// In RadarBlip.cs, change:
private float elevationLineWidth = 0.002f;
// To:
private float elevationLineWidth = 0.01f; // 5x larger
```

**Nuclear Option 2: Force World Space**
```csharp
// In CreateElevationLine(), change:
elevationLine.useWorldSpace = false;
// To:
elevationLine.useWorldSpace = true;

// Then update UpdateElevationLine() to calculate world positions
```

**Nuclear Option 3: Use Debug.DrawLine**
```csharp
// Add to RadarBlip.Update():
if (showElevationLine && Mathf.Abs(transform.localPosition.y) > 0.001f)
{
    Vector3 start = transform.position;
    Vector3 end = transform.position + new Vector3(0, -transform.localPosition.y, 0);
    Debug.DrawLine(start, end, Color.yellow);
}
```
This will at least confirm if the positions are correct.

---

## 📝 Additional Notes

### Coordinate Spaces Explained
The radar system uses nested coordinate spaces:

1. **World Space**: The game world
2. **Ship Space**: Relative to ship (ship is origin)
3. **Radar Display Space**: The radar UI element's local space
4. **Blip Space**: The individual blip's local space (child of radar display)

The elevation line is drawn in **Blip Space** (local space of the blip), which means:
- `(0, 0, 0)` = blip center
- `(0, -localPosition.y, 0)` = ship's plane

This is why `useWorldSpace = false` is critical.

### Why Circles Help
The circle indicates where the asteroid would be horizontally if you were at its elevation. This helps with 3D navigation:
- See an asteroid above you?
- Look at the circle on the ship's plane
- That's where it will be horizontally when you reach that altitude

---

## 🆘 If All Else Fails

1. **Capture diagnostic output:**
   - Press `D` in Play mode
   - Copy entire Console output
   - Share with developer

2. **Check Scene view:**
   - Do gizmos show lines?
   - If yes: Rendering issue
   - If no: Position calculation issue

3. **Verify test scenario:**
   - Make sure asteroids are above/below ship (not on same plane)
   - Try moving ship up/down significantly
   - Look at asteroids with clear vertical offset

4. **Simplify:**
   - Disable all other radar features
   - Test with just 1 asteroid
   - Set displayRadius large (0.3f) to see clearly

---

## Contact & Support

If issues persist after following this guide:
- Include diagnostic output (press `D` key)
- Include screenshots of Inspector during Play mode
- Note which display mode you're using
- Describe what you see vs. what you expect

**Last Updated:** November 13, 2025
