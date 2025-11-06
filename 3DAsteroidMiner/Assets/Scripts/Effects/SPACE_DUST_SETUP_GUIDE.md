# Space Dust Particle Effect - Setup Guide

This guide will walk you through setting up the space dust "motion streak" effect for the Asteroid Miner game.

## 📁 Files Created

- **SpaceDustEffect.cs** - Main controller script (`Assets/Scripts/Effects/`)
- **SpaceDustSettings.cs** - ScriptableObject configuration (`Assets/Scripts/Data/`)
- **SpaceDustMaterialSetup.cs** - Editor utility for material setup (`Assets/Scripts/Editor/`)

---

## 🚀 Quick Setup (5 Minutes)

### Step 1: Create the Particle System GameObject

1. In the Unity **Hierarchy**, locate your **Player Ship** GameObject
2. Right-click on the Player Ship → **Effects** → **Particle System**
3. Rename the new GameObject to **"SpaceDustParticles"**
4. Position it at the **center** of your ship (Transform position: 0, 0, 0 relative to parent)

### Step 2: Configure the Particle System

Select the **SpaceDustParticles** GameObject and configure these settings in the Inspector:

#### **Main Module**
- **Duration:** `5` (with Looping enabled)
- **Looping:** ✅ Checked
- **Start Lifetime:** `2` seconds
- **Start Speed:** `0`
- **Start Size:** Random between `0.05` and `0.15`
- **Start Rotation:** Random between `0` and `360`
- **Start Color:** White `(255, 255, 255, 200)` - slight transparency
- **Gravity Modifier:** `0`
- **Simulation Space:** **World**
- **Max Particles:** `1000`
- **Stop Action:** None

#### **Emission Module**
- **Rate over Time:** `50` (this will be controlled by the script)

#### **Shape Module**
- **Shape:** Box
- **Scale:** `(30, 30, 30)` - Adjust based on your ship size
- **Box Thickness:** `(1, 1, 1)`

#### **Velocity over Lifetime Module**
- ✅ **Enabled**
- Leave all values at `0` (controlled by script)
- **Space:** World

#### **Color over Lifetime Module** (Optional but recommended)
- ✅ **Enabled**
- Create a gradient that fades from **white → transparent**
  - Left key (0%): White, Alpha 255
  - Right key (100%): White, Alpha 0

#### **Size over Lifetime Module** (Optional)
- ✅ **Enabled**
- Curve: Slight shrink from `1.0` to `0.5` over lifetime

#### **Renderer Module**
- **Render Mode:** Billboard
- **Material:** (We'll create this in Step 3)
- **Trail Material:** None
- **Min Particle Size:** `0`
- **Max Particle Size:** `0.5`

### Step 3: Create the Particle Material

**Option A: Using the Editor Utility (Recommended)**
1. **Wait for Unity to recompile** the scripts (check bottom-right corner for progress)
2. Go to **Window** → **Space Dust Material Setup**
3. Click **"Create New Particle Material"**
4. Save it as **"SpaceDustMaterial"** in `Assets/Materials/`
5. Adjust the color to a subtle blue-white: `(200, 230, 255, 200)`
6. Enable **Soft Particles** ✅
7. Enable **GPU Instancing** ✅
8. Click **"Apply Settings to Material"**
9. Assign this material to the **Renderer Module** of your Particle System

**Note:** If the Window menu option doesn't appear, make sure the script compiled successfully and check the Console for errors. The script must be in `Assets/Scripts/Editor/` folder.

**Option B: Manual Creation**
1. Create a new Material in `Assets/Materials/`
2. Name it **"SpaceDustMaterial"**
3. Set Shader to **"Particles/Standard Unlit"**
4. Set **Rendering Mode** to **Fade** (or Additive for glowing effect)
5. Set **Color** to subtle blue-white: `(200, 230, 255, 200)`
6. Enable **GPU Instancing** in material inspector (checkbox at bottom)
7. Assign to Particle System Renderer

### Step 3.5: Change Particle Shape (Circular/Star Instead of Square)

**Quick Method - Use Built-in Texture:**
1. Select your **SpaceDustMaterial** in the Project window
2. In the Inspector, find the **Base Map** (or **Albedo** or **Texture**) slot
3. Click the small circle icon next to it
4. Search for **"Default-Particle"**
5. Select it - your particles are now circular! ✨

**Advanced Method - Generate Custom Shapes:**
1. Go to **Window** → **Generate Particle Textures**
2. Choose a preset:
   - **Soft White Dust** - Smooth circular particles
   - **Twinkling Star** - 4-pointed star shape
   - **Sparkle Effect** - Cross/sparkle pattern
   - **Glowing Orb** - Bright glowing sphere
3. Or customize:
   - Shape: Circle, Star, Sparkle, Glow, Diamond
   - Size: 64-512 pixels
   - Star Points: 4-8 (for star shapes)
   - Softness: Adjust edge falloff
4. Click **"Generate Texture"**
5. Save to `Assets/Textures/Particles/`
6. Assign the generated texture to your material's **Base Map** slot

### Step 4: Add the SpaceDustEffect Script

1. Select the **SpaceDustParticles** GameObject
2. Click **Add Component** → Search for **"SpaceDustEffect"**
3. In the script's Inspector properties:
   - **Ship Rigidbody:** Drag your Player Ship's Rigidbody component here
   - **Base Emission Rate:** `10`
   - **Max Emission Rate:** `200`
   - **Max Speed Threshold:** `50` (adjust based on your ship's top speed)
   - **Velocity Multiplier:** `1.5`
   - **Minimum Speed Threshold:** `0.5`
   - **Debug Mode:** ✅ (check this initially to see debug logs)

### Step 5: Test the Effect

1. Enter **Play Mode**
2. Move your ship around
3. You should see particles streaming backward as you move
4. Check the Console for debug messages showing speed and particle count

**Expected Behavior:**
- ✅ No/few particles when stationary
- ✅ More particles as ship accelerates
- ✅ Particles move opposite to ship direction
- ✅ Smooth transitions between emission rates

---

## ⚙️ Advanced Configuration

### Creating Custom Settings Asset (Optional)

1. In Project window, right-click in `Assets/Data/`
2. Select **Create** → **Game** → **Space Dust Settings**
3. Name it **"DefaultSpaceDustSettings"**
4. Adjust values in Inspector:
   - Base Emission Rate
   - Max Emission Rate
   - Max Speed Threshold
   - Velocity Multiplier
5. Drag this asset into the **Settings** field of SpaceDustEffect component

### Performance Tuning

**For Low-End Hardware:**
```
Max Particles: 500
Max Emission Rate: 100
Particle Lifetime: 1.5 seconds
```

**For High-End Hardware:**
```
Max Particles: 2000
Max Emission Rate: 300
Particle Lifetime: 2.5 seconds
Add Color over Lifetime
Add Size over Lifetime
```

### Visual Variations

**Subtle/Realistic Look:**
- Color: Soft white `(240, 245, 250, 150)`
- Size: 0.03 - 0.08
- Blending: Alpha Blend
- No glow

**Stylized/Sci-Fi Look:**
- Color: Bright cyan `(100, 200, 255, 255)`
- Size: 0.08 - 0.20
- Blending: Additive
- Add bloom post-processing

**Multi-Colored Space Dust:**
- Use **Start Color** as random between two colors
- Example: Purple `(180, 100, 255)` ↔ Blue `(100, 180, 255)`

---

## 🎮 Integration with Game Systems

### Prestige System Integration

Add visual variety per prestige level:

```csharp
// In your prestige/game state manager
public void OnPrestigeUp(int newPrestigeLevel)
{
    SpaceDustEffect dustEffect = FindObjectOfType<SpaceDustEffect>();
    
    // Change dust color based on prestige
    Color dustColor = CalculatePrestigeColor(newPrestigeLevel);
    
    // Apply to particle system
    ParticleSystem ps = dustEffect.GetComponent<ParticleSystem>();
    var main = ps.main;
    main.startColor = dustColor;
}

private Color CalculatePrestigeColor(int prestige)
{
    switch (prestige % 5)
    {
        case 0: return new Color(0.8f, 0.9f, 1f, 0.8f);    // White-Blue
        case 1: return new Color(1f, 0.8f, 0.9f, 0.8f);    // Pink
        case 2: return new Color(0.9f, 1f, 0.8f, 0.8f);    // Green
        case 3: return new Color(1f, 0.9f, 0.6f, 0.8f);    // Gold
        case 4: return new Color(0.9f, 0.8f, 1f, 0.8f);    // Purple
        default: return Color.white;
    }
}
```

### Fuel System Integration

Make dust intensity reflect fuel level:

```csharp
// In SpaceDustEffect.cs, add this method:
public void SetFuelMultiplier(float fuelPercent)
{
    // Reduce particle emission when low on fuel
    float fuelMultiplier = Mathf.Lerp(0.3f, 1f, fuelPercent);
    
    emission.rateOverTime = currentEmissionRate * fuelMultiplier;
}

// Call from your fuel manager:
// dustEffect.SetFuelMultiplier(currentFuel / maxFuel);
```

### Different Effects for Different Ship Types

```csharp
// Create multiple SpaceDustSettings assets:
// - FastShipDust.asset (higher emission, brighter)
// - HeavyShipDust.asset (slower velocity, darker)
// - PrestigeShipDust.asset (colorful, high emission)

// Switch at runtime:
dustEffect.SetSettings(fastShipDustSettings);
```

---

## 🐛 Troubleshooting

### Problem: No particles appear
**Solutions:**
1. Check that Particle System is **Playing** (not stopped)
2. Verify **Ship Rigidbody** is assigned in SpaceDustEffect component
3. Check that ship is actually moving (velocity > 0.5)
4. Ensure **Max Particles** is not 0
5. Verify material is assigned to Particle System Renderer

### Problem: Particles don't move with ship motion
**Solutions:**
1. Check **Simulation Space** is set to **World** (not Local)
2. Verify **Velocity over Lifetime** module is enabled
3. Ensure Ship Rigidbody reference is correct
4. Check Debug Mode to see velocity in console

### Problem: Particles appear as white squares
**Solutions:**
1. Particle material is missing - assign SpaceDustMaterial
2. Shader not supported - switch to "Legacy Shaders/Particles/Alpha Blended"
3. Texture slot is empty - use default particle texture

### Problem: Poor performance / FPS drops
**Solutions:**
1. Reduce **Max Particles** to 500 or less
2. Reduce **Max Emission Rate** to 100
3. Disable **Color over Lifetime** and **Size over Lifetime**
4. Enable **GPU Instancing** on material
5. Reduce particle **Lifetime** to 1 second
6. Reduce **Shape** scale to spawn fewer particles

### Problem: Particles move in wrong direction
**Solutions:**
1. Check that ship is using Rigidbody for movement (not Transform)
2. Verify velocity is being calculated correctly in your ship controller
3. Try adjusting **Velocity Multiplier** (negative value reverses direction)

---

## 📊 Performance Metrics

**Expected Performance:**
- **Particle Count:** 100-500 active particles during movement
- **CPU Usage:** < 0.5ms per frame
- **GPU Usage:** < 1ms per frame (with GPU instancing)
- **Memory:** ~5-10 MB for particle system

**Monitor in Unity Profiler:**
1. Window → Analysis → Profiler
2. Check "ParticleSystem.Update" in CPU usage
3. Check "Draw Particles" in GPU usage

---

## 🎨 Visual Examples & Presets

### Preset 1: Subtle Realistic Dust
```
Color: (240, 245, 250, 120)
Size: 0.03 - 0.06
Emission: 10 → 150
Lifetime: 2 seconds
Blending: Alpha
```

### Preset 2: Intense Sci-Fi Streaks
```
Color: (100, 200, 255, 255)
Size: 0.08 - 0.20
Emission: 50 → 300
Lifetime: 1.5 seconds
Blending: Additive
Use LineRenderer mode instead of Billboard
```

### Preset 3: Nebula Dust
```
Color: Random between (255, 150, 200) and (150, 200, 255)
Size: 0.05 - 0.15
Emission: 20 → 200
Lifetime: 2.5 seconds
Add Noise module for swirling effect
```

---

## 🔧 Extending the System

### Adding Sound Effects

```csharp
// Add to SpaceDustEffect.cs
[SerializeField] private AudioSource windSound;

private void UpdateParticleEffect()
{
    // ... existing code ...
    
    // Scale wind sound volume with speed
    if (windSound != null)
    {
        float normalizedSpeed = Mathf.Clamp01(speed / maxSpeedThreshold);
        windSound.volume = Mathf.Lerp(0f, 0.3f, normalizedSpeed);
    }
}
```

### Camera Shake on High Speed

```csharp
// Trigger camera shake when moving very fast
if (speed > maxSpeedThreshold * 0.8f)
{
    CameraShake.Instance?.Shake(0.1f, 0.05f);
}
```

### Particle Trails for Boost

```csharp
public void SetBoostMode(bool isBoosting)
{
    if (isBoosting)
    {
        emission.rateOverTime = maxEmissionRate * 1.5f; // Extra particles
        velocityMultiplier = 3f; // Faster streaks
    }
    else
    {
        velocityMultiplier = 1.5f; // Normal
    }
}
```

---

## ✅ Final Checklist

Before finishing setup, verify:

- [ ] Particle System is on Player Ship GameObject
- [ ] Simulation Space is set to **World**
- [ ] Ship Rigidbody is assigned in SpaceDustEffect script
- [ ] Material is assigned to Particle System Renderer
- [ ] GPU Instancing is enabled on material
- [ ] Particles appear and move when ship moves
- [ ] Emission scales with ship speed
- [ ] Performance is acceptable (60+ FPS)
- [ ] No errors in Console
- [ ] Debug mode can be disabled for production

---

## 📝 Notes

- The effect works best with **constant ship movement** (players should rarely be stationary)
- Adjust **Max Speed Threshold** to match your ship's actual top speed for best results
- Consider adding **multiple particle systems** for layered depth (small distant + large close particles)
- For VR, reduce particle count to maintain performance

---

**Need Help?** 
- Check the debug logs when **Debug Mode** is enabled
- Use the Scene view Gizmos to visualize the spawn area
- Experiment with different color gradients for unique looks

**Created:** November 6, 2025  
**Version:** 1.0
