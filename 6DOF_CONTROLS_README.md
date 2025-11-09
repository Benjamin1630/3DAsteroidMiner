# 6DoF Player Ship Setup Guide

This guide explains how to set up the 6 Degrees of Freedom (6DoF) player ship controls in Unity - similar to Elite Dangerous, Star Citizen, and No Man's Sky.

## 📋 What is 6DoF?

**6 Degrees of Freedom** means the ship can move and rotate independently in all directions:

### Translation (Movement):
- **Forward/Backward** - Thrust along Z axis (W/S)
- **Strafe Left/Right** - Thrust along X axis (A/D)
- **Vertical Up/Down** - Thrust along Y axis (Space/Ctrl)

### Rotation (Orientation):
- **Pitch** - Nose up/down (Mouse Y / Right Stick Y)
- **Yaw** - Nose left/right (Mouse X / Right Stick X)
- **Roll** - Barrel roll (Q/E / Shoulder Buttons)

## 🚀 Quick Setup

### Step 1: Create the Player Ship GameObject

1. In Unity, create a new **Empty GameObject** in your scene
2. Name it `Player`
3. Position it at (0, 0, 0)

### Step 2: Add Components

Add the following components to the Player GameObject:

1. **Rigidbody**
   - Use Gravity: `OFF`
   - Linear Damping: `0.5`
   - Angular Damping: `3`
   - Constraints: `None` (full 6DoF!)

2. **PlayerController** (script)
   - Base Thrust: `20`
   - Strafe Thrust: `15`
   - Vertical Thrust: `15`
   - Boost Multiplier: `2`
   - Pitch Speed: `60`
   - Yaw Speed: `60`
   - Roll Speed: `90`
   - Mouse Sensitivity: `1`

3. **PlayerInputHandler** (script)
   - No settings needed - automatically configured

4. **PlayerStats** (script)
   - No settings needed for now

5. **ShipVisuals** (script)
   - Assign particle systems and lights when you create the ship model

### Step 3: Create a Visual Model (Temporary)

For testing, create a simple ship representation:

1. Add a **Capsule** as a child of Player
2. Name it `ShipModel`
3. Rotate it to (90, 0, 0) so it points forward
4. Scale it to (1, 2, 1) for a ship-like shape
5. Add a material with a distinct color

Add a direction indicator (nose cone):
- Add a **Cone** as a child of ShipModel
- Position: (0, 2, 0)
- Rotation: (180, 0, 0)
- Scale: (0.5, 0.5, 0.5)
- Different color material to show front

### Step 4: Add a Camera

Create a chase camera setup:

1. Create an **Empty GameObject** named `CameraRig` as child of Player
2. Position CameraRig at (0, 3, -10)
3. Create a **Camera** as child of CameraRig
4. Set Camera rotation to (10, 0, 0) to look slightly down

This gives you a third-person view that follows the ship's rotation.

### Step 5: Create a Test Environment

1. Create a **Plane** for visual reference
   - Scale: (100, 1, 100)
   - Position: (0, -10, 0)
   - Add a grid material if available

2. Add some **Cubes** as reference points
   - Scatter them around at different heights
   - Different sizes and colors
   - Use them to test 3D movement

## 🎮 6DoF Controls

### Keyboard & Mouse
**Translation (Movement):**
- **W** - Thrust Forward
- **S** - Thrust Backward
- **A** - Strafe Left
- **D** - Strafe Right
- **Space** - Thrust Up
- **Left Ctrl** - Thrust Down
- **Left Shift** - Boost (2x speed/fuel)

**Rotation (Hold Right Mouse Button for mouse flight):**
- **Mouse Y** - Pitch (nose up/down)
- **Mouse X** - Yaw (nose left/right)
- **Q** - Roll Left
- **E** - Roll Right

**Actions:**
- **Left Mouse** - Mining laser (not yet implemented)
- **R** - Scanner (not yet implemented)
- **F** - Dock at station (not yet implemented)
- **Escape** - Pause menu (not yet implemented)

### Gamepad
**Translation:**
- **Left Stick** - Strafe (X) and Forward/Back (Y)
- **Right Trigger** - Thrust Up
- **Left Trigger** - Thrust Down
- **Left Stick Click** - Boost

**Rotation:**
- **Right Stick** - Pitch (Y) and Yaw (X)
- **Left Shoulder** - Roll Left
- **Right Shoulder** - Roll Right

**Actions:**
- **X Button (West)** - Mining laser
- **Y Button (North)** - Scanner
- **A Button (South)** - Dock
- **Start** - Pause

## 🧪 Testing the 6DoF Controls

1. Press **Play** in Unity
2. Test each axis independently:
   - **W/S** - Should move forward/backward in the direction you're facing
   - **A/D** - Should strafe left/right
   - **Space/Ctrl** - Should move up/down
   - **Hold Right Mouse + Move Mouse** - Should rotate ship
   - **Q/E** - Should roll the ship
   - **Shift + W** - Should boost forward

3. Test combined movement:
   - Pitch up (mouse), then thrust forward (W) - should arc upward
   - Roll 90° (Q), then strafe (D) - should move "up" relative to world
   - Combine rotation and translation for complex maneuvers

## 🔍 What to Look For

### Physics Behavior
- Ship should drift in space (no friction/gravity)
- Rotation should be smooth and responsive
- Boost should double speed and fuel consumption
- Movement should be relative to ship orientation

### Visual Feedback (Scene View Gizmos)
- **Blue line** - Forward direction
- **Red line** - Right direction
- **Green line** - Up direction
- **Cyan line** - Velocity vector
- **Yellow/Magenta** - Forward thrust indicator
- **White** - Strafe thrust indicator
- **Grey** - Vertical thrust indicator

### Performance
- Should maintain 60+ FPS
- No errors in Console
- Smooth responsive controls
- Fuel decreases when moving

## 📊 Monitoring GameState

Add this simple debug display script:

```csharp
using UnityEngine;
using AsteroidMiner.Entities;

public class DebugDisplay : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerController playerController;
    
    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        playerController = FindObjectOfType<PlayerController>();
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Box("Ship Status");
        
        if (playerController != null)
        {
            GUILayout.Label($"Speed: {playerController.CurrentSpeed:F2}");
            GUILayout.Label($"Velocity: {playerController.Velocity}");
            GUILayout.Label($"Boosting: {playerController.IsBoosting}");
        }
        
        if (playerStats != null)
        {
            GUILayout.Label($"\n{playerStats.GetStatsSummary()}");
        }
        
        GUILayout.EndArea();
    }
}
```

## 🐛 Troubleshooting

### Ship doesn't rotate
- Check Rigidbody constraints - should be `None` for full 6DoF
- Verify Angular Damping is set (try 3.0)
- Hold Right Mouse Button for mouse flight controls

### Ship rotates too fast/slow
- Adjust Pitch/Yaw/Roll Speed in PlayerController
- Adjust Mouse Sensitivity for mouse flight
- Check Angular Damping (higher = more resistance)

### Ship moves oddly
- Verify Rigidbody has Use Gravity OFF
- Check Linear Damping (0.5 recommended)
- Ensure Forward direction is correct (blue gizmo line)

### Mouse flight doesn't work
- Hold Right Mouse Button to enable mouse flight
- Check InputHandler events are subscribed
- Verify MouseControlAction is enabled

### Boost not working
- Hold Left Shift while thrusting
- Check fuel levels (boost uses 3x fuel)
- Verify Boost Multiplier is set to 2

## 🎯 Advanced Tips

### Flight Techniques

**Strafing Run:**
1. Face target
2. Strafe sideways (A or D)
3. Keeps target in view while dodging

**Loop Maneuver:**
1. Pitch up continuously (mouse up)
2. Add forward thrust (W)
3. Creates circular flight path

**FA-Off Style (Flight Assist Off):**
- Ship naturally drifts due to low drag
- Build up speed in one direction
- Rotate to face different direction
- Thrust in new direction while still drifting

**Combat Positioning:**
1. Boost toward target (Shift+W)
2. As you pass, pitch to track (mouse)
3. Strafe to adjust aim (A/D)
4. Vertical thrust to avoid return fire (Space/Ctrl)

### Physics Tuning

**More Arcade Feel:**
- Increase Linear Damping: `2.0`
- Increase Angular Damping: `5.0`
- Increase Thrust values

**More Sim Feel:**
- Decrease Linear Damping: `0.1`
- Decrease Angular Damping: `1.0`
- Decrease Thrust values
- Lower Mouse Sensitivity

## ✅ Verification Checklist

- [ ] Ship moves forward/backward (W/S)
- [ ] Ship strafes left/right (A/D)
- [ ] Ship moves up/down (Space/Ctrl)
- [ ] Ship pitches (mouse Y with RMB)
- [ ] Ship yaws (mouse X with RMB)
- [ ] Ship rolls (Q/E)
- [ ] Boost increases speed (Shift)
- [ ] Fuel decreases when moving
- [ ] Distance tracking works
- [ ] No console errors
- [ ] 60 FPS performance
- [ ] Gizmos show correctly in Scene view
- [ ] Gamepad controls work (if available)

## 🎓 Key Differences from Simple Movement

### Old System (2D-style):
- Movement locked to horizontal plane
- Auto-rotation toward movement
- Fixed "up" direction

### New 6DoF System:
- **Full 3D movement** in all directions
- **Independent rotation** from movement
- **No "up" constraint** - can fly upside down
- **Realistic space physics** - drift continues
- **Mouse flight mode** - immersive controls
- **Boost system** - tactical speed management

## 🚀 Next Steps

After confirming 6DoF movement works:

1. **Fine-tune physics** - Adjust drag/thrust to taste
2. **Implement mining system** - Add laser beams
3. **Create asteroids** - Test 3D navigation
4. **Add UI** - Display pitch/roll indicators
5. **Camera improvements** - Add look-ahead, shake effects
6. **Visual feedback** - Engine trails, boost effects

---

**Status:** 6 Degrees of Freedom controls complete ✅  
**Next:** Test in Unity and adjust feel to preference
