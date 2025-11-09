# Mining System - Quick Setup Guide

## ✅ What's Been Implemented

The complete **raycast-based mining system** is now ready! Here's what was created:

### New Scripts
1. **MiningSystem.cs** - Core mining controller with camera raycast targeting
   - **Aim-to-mine** system using camera crosshair
   - Automatically targets asteroid you're looking at
   - Multi-target mining (1-6 simultaneous asteroids near primary target)
   - Automatic target acquisition
   - Fuel consumption tracking
   - Resource and credit awards
   - Cargo capacity checking

2. **MiningLaser.cs** - Laser beam visualization
   - Animated LineRenderer beams
   - Pulsing width effect
   - Color and transparency gradients
   - Faces camera for optimal visibility

### Updated Scripts
3. **Asteroid.cs** - Added mining support
   - `TakeMiningDamage()` method
   - `OnAsteroidDestroyed` event
   - Fixed System.Random ambiguity with UnityEngine.Random

### Materials
4. **MiningLaserMaterial.mat** - Green glowing laser beam material

### Documentation
5. **MINING_SYSTEM_README.md** - Complete system documentation

---

## 🚀 How to Set Up in Unity

### Step 1: Add MiningSystem to Player
1. Open your scene in Unity
2. Select your **Player ship** GameObject in the Hierarchy
3. Click **Add Component** in the Inspector
4. Search for "Mining System" and add it
5. Configure references (most are auto-detected):
   - **Player Camera**: Leave empty for auto-detection (uses Camera.main)
   - **Use Screen Center**: ✅ Check this for crosshair aiming (recommended)
   - **Asteroid Layer Mask**: Set to "Asteroid" layer
   - **Game State**: Leave empty for auto-detection from PlayerController
   - **Player Transform**: Leave empty for auto-detection
   - **Laser Origins**: (Optional) Create child GameObjects for laser mount points

**Note:** The system will automatically find the main camera and GameState from PlayerController!

### Step 2: (Optional) Create Laser Mount Points
For better visual variety with multiple lasers:
1. Create empty child GameObjects under your Player ship
2. Name them "LaserMount_1", "LaserMount_2", etc.
3. Position them where you want lasers to originate (e.g., front corners of ship)
4. Drag these into the **Laser Origins** array in MiningSystem Inspector

If you skip this step, lasers will originate from the ship center (still works fine).

### Step 3: Configure Mining Settings (Optional)
In the MiningSystem Inspector, you can adjust:
- **Mining Range**: 100m (how far you can mine with camera raycast)
- **Base Mining Rate**: 1.0 (damage per second)
- **Mining Fuel Consumption**: 0.5 (fuel cost per second per target)
- **Use Screen Center**: ✅ Enabled = aim with camera center (crosshair mode)
  - Disabled = aim with mouse cursor (mouse aiming mode)
- **Laser Color**: Green (color of laser beams)
- **Laser Width**: 0.2 (thickness of beams)

### Step 4: Ensure Asteroid Layer Exists
1. Go to **Edit → Project Settings → Tags and Layers**
2. Make sure you have a layer called "Asteroid"
3. Assign this layer to your asteroid prefabs

### Step 5: Test It!
1. Press Play
2. **Aim your camera** at an asteroid (center crosshair on it)
3. Hold **Left Mouse Button** or **Space** to mine
4. Watch for:
   - ✅ Green laser beams appear from ship to asteroid you're aiming at
   - ✅ Asteroid shrinks as health decreases
   - ✅ Credits awarded when asteroid destroyed
   - ✅ Resources added to inventory
   - ✅ Fuel consumption while mining
   - ✅ Multiple nearby asteroids mined if multiMining upgrade level > 1

**Pro Tip:** In Scene view with MiningSystem selected, you'll see:
- **Cyan ray** = Your aiming direction
- **Green sphere** = Raycast hit point on asteroid
- **Red wire spheres** = Active mining targets

---

## 🎮 Controls

### Keyboard & Mouse
- **Hold Left Mouse Button** - Mine asteroids in range
- **Hold Space** - Alternative mining button

### Gamepad
- **Hold X Button / Square** - Mine asteroids
- **Hold Right Trigger** - Alternative mining trigger

---

## 🔧 How Mining Works

### 1. Target Acquisition (Raycast-Based)
- When you press mine button, system **raycasts from camera center** (crosshair)
- Detects asteroid under crosshair up to mining range (100m default)
- If multiMining upgrade > 1, also finds nearby asteroids around primary target
- Creates laser beams for each target

### 2. Aiming System
- **Crosshair Mode** (default): Aim with camera center - where you look is where you mine
- **Mouse Mode** (optional): Aim with mouse cursor position on screen
- More precise than proximity-based mining - you choose exactly what to mine!

### 3. Mining Process
- Laser beams drain asteroid health over time
- Mining speed affected by "mining" upgrade level:
  - Level 1 = 1.0x speed (base)
  - Level 5 = 2.0x speed (double)
  - Level 10 = 3.0x speed (triple)
- Consumes fuel while mining (affected by fuel efficiency upgrade)

### 3. Resource Collection
- When asteroid health reaches 0, it's destroyed
- Resources automatically added to inventory (if space available)
- Credits awarded with prestige bonus applied
- Statistics updated (asteroidsMined counter)

### 4. Automatic Stops
Mining automatically stops when:
- ❌ You release mine button
- ❌ You run out of fuel
- ❌ Your cargo is full
- ❌ All targets are destroyed or out of range

---

## 🎯 Upgrade System Integration

The mining system automatically reads these upgrades from GameState:

| Upgrade | Effect | Max Level |
|---------|--------|-----------|
| **mining** | Mining speed multiplier | 10 |
| **multiMining** | Simultaneous targets | 6 |
| **fuelEfficiency** | Reduces fuel consumption | 10 |
| **cargo** | Inventory capacity | Unlimited |

Example: With multiMining level 3 and mining level 5, you can mine 3 asteroids simultaneously at 2x speed!

---

## 🐛 Troubleshooting

### Mining button does nothing
**Fix:** Make sure PlayerInputHandler is on the same GameObject or parent of MiningSystem

### No laser beams visible
**Fix:** 
1. Make sure you're **aiming at an asteroid** with your camera crosshair
2. Check that asteroid is in range (100m default)
3. Use Scene view Gizmos to see the cyan raycast line showing your aim direction
4. Ensure asteroids have colliders (for raycast detection)

### "GameState not assigned" warning
**Fix:** MiningSystem will automatically get GameState from PlayerController if they're on the same GameObject. If you see this error:
1. Make sure MiningSystem is on the Player ship GameObject (same as PlayerController)
2. Or manually assign the same GameState instance in both components' Inspectors
3. Check that PlayerController.Start() has run (GameState is created there)

### Asteroids not taking damage
**Fix:** 
1. Ensure asteroids have **colliders** (required for raycasting)
2. Ensure asteroids have the "Asteroid" layer assigned
3. Check Asteroid Layer Mask in MiningSystem includes "Asteroid" layer
4. Make sure you're aiming directly at the asteroid

### Mining stops immediately
**Fix:** Check fuel level - mining consumes fuel. Also check cargo isn't full.

---

## 📊 Testing Checklist

Use this to verify everything works:

- [ ] Mining System component added to Player
- [ ] GameState reference assigned
- [ ] Mine button press triggers mining (watch console for debug logs)
- [ ] Green laser beams appear from ship to asteroids
- [ ] Asteroids shrink as health decreases
- [ ] Credits increase when asteroid destroyed
- [ ] Inventory shows collected resources
- [ ] Fuel decreases while mining
- [ ] Mining stops when fuel empty
- [ ] Mining stops when cargo full
- [ ] Multiple asteroids can be mined simultaneously (with multiMining upgrade)
- [ ] Mining stops when button released

---

## 🎨 Customization Tips

### Change Laser Color
```csharp
// In MiningSystem Inspector, change "Laser Color"
// Or via code:
miningSystem.laserColor = Color.cyan; // Blue lasers!
```

### Adjust Mining Speed Balance
If mining feels too fast/slow:
1. Adjust **Base Mining Rate** in MiningSystem
2. Or adjust asteroid health values in AsteroidType ScriptableObjects
3. Or adjust mining upgrade multipliers in GameState.GetMiningSpeed()

### Add Sound Effects
In MiningSystem.cs, add audio in these methods:
- `StartMining()` - Play laser start sound
- `StopMining()` - Play laser stop sound
- `OnAsteroidDestroyed()` - Play explosion/collection sound

---

## 📖 Next Steps

Now that mining works, you might want to add:

1. **UI Elements**
   - Mining progress bars
   - Target reticles on asteroids
   - Fuel gauge warning

2. **Visual Polish**
   - Particle effects at mining impact points
   - Screen shake when asteroid destroyed
   - Laser beam texture scrolling

3. **Gameplay Features**
   - Upgrade system UI
   - Mining missions
   - Rare asteroid alerts

4. **Audio**
   - Mining laser hum sound
   - Asteroid destruction sound
   - Low fuel warning beep

Check **MINING_SYSTEM_README.md** for full documentation and code examples!

---

**Status:** ✅ Ready to Use  
**Last Updated:** November 9, 2025
