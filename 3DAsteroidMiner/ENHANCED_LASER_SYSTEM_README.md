# Enhanced Mining Laser System

## Overview

The Enhanced Mining Laser system provides realistic, heat-based mining visuals with:
- **Startup Animation** - Laser powers up gradually
- **Active Mining** - Pulsing beam with particle effects
- **Shutdown Animation** - Laser powers down smoothly
- **Heat Marks** - Impact points on asteroids that glow and fade
- **Particle Systems** - Sparks, heat distortion, and molten material effects

## Components

### 1. EnhancedMiningLaser.cs
Main laser beam controller with three-phase animation system.

**Key Features:**
- LineRenderer-based beam with hot orange/yellow gradient
- Startup/shutdown animations with customizable curves
- Five particle systems for different effects
- Pulsing intensity and width for active mining
- Automatic impact effect creation

**Inspector Settings:**
```
Visual Settings:
- Base Color: Hot orange (1, 0.4, 0, 1)
- Hot Color: Yellow-white hot (1, 0.9, 0.4, 1)
- Base Width: 0.15
- Max Width: 0.25

Animation Settings:
- Startup Duration: 0.3s
- Shutdown Duration: 0.2s
- Startup/Shutdown Curves: Customizable animation curves

Pulse Settings:
- Pulse Speed: 8
- Pulse Magnitude: 0.2 (20% width variation)
- Intensity Variation: 0.15 (15% brightness variation)

Particle Systems:
- Startup Particles: Initial burst when laser activates
- Looping Particles: Continuous beam particles
- Shutdown Particles: Final burst when laser deactivates
- Spark Particles: Molten sparks at impact point
- Heat Distortion Particles: Heat waves at impact
```

### 2. LaserImpactEffect.cs
Heat mark effect that appears on asteroid surface.

**Key Features:**
- Grows from small to large heat mark
- Glowing point light for heat emission
- Transitions from molten orange to dark scorch
- Fades out completely after duration
- Automatically destroys itself

**Inspector Settings:**
```
Visual Settings:
- Initial Size: 0.5
- Max Size: 1.2
- Hot Color: Molten orange (1, 0.6, 0.1, 1)
- Cool Color: Dark scorch (0.2, 0.1, 0.05, 0.5)

Animation:
- Grow Duration: 0.3s (how fast mark appears)
- Fade Duration: 1.5s (how long until disappears)
- Grow/Fade Curves: Customizable animation curves

Heat Glow:
- Use Glow: true
- Glow Intensity: 2
- Glow Pulse Speed: 3
```

## Setup Guide

### Basic Setup (Auto-Generated Particles)

1. **Replace MiningLaser component:**
   ```
   - Remove old MiningLaser component
   - Add EnhancedMiningLaser component
   ```

2. **The component will automatically:**
   - Create all five particle systems
   - Setup LineRenderer with hot laser material
   - Generate impact effects procedurally

### Advanced Setup (Custom Particle Systems)

1. **Create custom particle system prefabs** for better performance:
   - Right-click in Project → Effects → Particle System
   - Configure each particle system (see Particle System Presets below)
   - Assign to EnhancedMiningLaser component

2. **Create custom laser material:**
   - Create new Material in Materials folder
   - Use Standard shader
   - Set Rendering Mode to Transparent
   - Enable Emission with hot color
   - Assign to EnhancedMiningLaser → Laser Material

3. **Create custom impact effect prefab:**
   - Create empty GameObject
   - Add LaserImpactEffect component
   - Configure colors and timing
   - Save as prefab
   - Assign to EnhancedMiningLaser → Impact Effect Prefab

## Particle System Presets

### Startup Particles
```
Main Module:
- Duration: 0.3
- Looping: false
- Start Lifetime: 0.3
- Start Speed: 5
- Start Size: 0.2
- Start Color: Orange (1, 0.6, 0.2, 1)

Emission:
- Rate over Time: 50

Shape:
- Shape: Cone
- Angle: 15
- Radius: 0.1
```

### Looping Particles
```
Main Module:
- Duration: 1.0
- Looping: true
- Start Lifetime: 0.5
- Start Speed: 2
- Start Size: 0.15
- Start Color: Hot yellow (1, 0.8, 0.3, 0.8)

Emission:
- Rate over Time: 30

Shape:
- Shape: Cone
- Angle: 5
- Radius: 0.05
```

### Spark Particles
```
Main Module:
- Looping: true
- Start Lifetime: 0.2-0.5 (random)
- Start Speed: 5-15 (random)
- Start Size: 0.05-0.15 (random)
- Start Color: Bright yellow-white (1, 0.9, 0.6, 1)
- Gravity Modifier: 0.2

Emission:
- Rate over Time: 20

Shape:
- Shape: Sphere
- Radius: 0.3
```

### Heat Distortion Particles
```
Main Module:
- Looping: true
- Start Lifetime: 1.0
- Start Speed: 1.0
- Start Size: 0.5-1.0 (random)
- Start Color: Semi-transparent orange (1, 0.5, 0, 0.3)

Emission:
- Rate over Time: 15

Shape:
- Shape: Cone
- Angle: 10
- Radius: 0.2

Size over Lifetime:
- Size: Grows from 0.5 to 1.5 over lifetime
```

## Integration with MiningSystem

### Updating MiningSystem.cs

Replace the old laser instantiation with the new system:

```csharp
// OLD CODE:
private MiningLaser CreateLaser()
{
    GameObject laserObj = Instantiate(laserPrefab);
    MiningLaser laser = laserObj.GetComponent<MiningLaser>();
    laser.Initialize(laserColor, laserWidth);
    return laser;
}

// NEW CODE:
private EnhancedMiningLaser CreateLaser()
{
    GameObject laserObj = Instantiate(enhancedLaserPrefab);
    EnhancedMiningLaser laser = laserObj.GetComponent<EnhancedMiningLaser>();
    return laser;
}
```

### Starting Mining

```csharp
// OLD CODE:
laser.SetVisible(true);
laser.UpdateLaser(startPos, endPos);

// NEW CODE:
laser.StartLaser(startPos, endPos); // Triggers startup animation
```

### Stopping Mining

```csharp
// OLD CODE:
laser.SetVisible(false);

// NEW CODE:
laser.StopLaser(); // Triggers shutdown animation
```

### Updating Laser Position (each frame)

```csharp
// Same for both:
laser.UpdateLaser(startPos, endPos);
```

## API Reference

### EnhancedMiningLaser

#### Public Methods

**`void StartLaser(Vector3 startPosition, Vector3 endPosition)`**
- Begins laser with startup animation
- Creates impact effect at end position
- Starts all particle systems

**`void StopLaser()`**
- Ends laser with shutdown animation
- Stops looping particles
- Plays shutdown particles
- Removes impact effect

**`void UpdateLaser(Vector3 startPosition, Vector3 endPosition)`**
- Updates beam positions while active
- Moves impact effect to new position
- Updates particle system positions

**`bool IsActive()`**
- Returns true if laser is starting up or fully active
- Returns false if inactive or shutting down

**`void SetInactive()`**
- Immediately deactivates without animation
- Use for emergency stops or cleanup

### LaserImpactEffect

#### Public Methods

**`void Initialize(float duration)`**
- Sets total lifetime of effect
- Starts animation sequence

**`void SetOrientation(Vector3 surfaceNormal)`**
- Aligns effect to asteroid surface normal
- Call this to make mark appear correctly on curved surfaces

**`void AttachToSurface(Transform surface)`**
- Parents effect to asteroid
- Makes mark move with rotating asteroids

**`void SetColors(Color hot, Color cool)`**
- Customize hot and cool colors
- Call before Initialize()

**`void SetSize(float size)`**
- Customize maximum size
- Call before Initialize()

## Performance Considerations

### Particle System Optimization

1. **Use Object Pooling** for laser instances
2. **Limit max particles** in each system:
   - Startup: 15 particles max
   - Looping: 20 particles max
   - Sparks: 25 particles max
   - Heat distortion: 15 particles max
3. **Disable particle systems** when not in camera view
4. **Use ParticleSystem.Simulate()** for consistent behavior

### Impact Effect Optimization

1. **Limit simultaneous effects:**
   ```csharp
   // Track active effects
   private List<LaserImpactEffect> activeEffects = new List<LaserImpactEffect>();
   private const int MAX_EFFECTS = 10;
   
   void CreateImpact()
   {
       if (activeEffects.Count >= MAX_EFFECTS)
       {
           Destroy(activeEffects[0].gameObject);
           activeEffects.RemoveAt(0);
       }
       
       // Create new effect
       var effect = Instantiate(impactPrefab);
       activeEffects.Add(effect);
   }
   ```

2. **Use simpler materials** for low-end devices
3. **Disable lights** on impact effects if frame rate drops

### Memory Management

```csharp
// Clean up on level change
private void OnDestroy()
{
    foreach (var effect in activeEffects)
    {
        if (effect != null)
            Destroy(effect.gameObject);
    }
    activeEffects.Clear();
}
```

## Customization Examples

### Blue Plasma Laser

```csharp
// Set in Inspector or code:
baseColor = new Color(0.2f, 0.6f, 1f, 1f); // Cyan blue
hotColor = new Color(0.8f, 0.9f, 1f, 1f); // Bright blue-white
emissionIntensity = 4f; // Brighter glow
```

### Green Mining Laser (Original Style)

```csharp
baseColor = new Color(0f, 1f, 0f, 0.8f); // Green
hotColor = new Color(0.6f, 1f, 0.6f, 1f); // Light green
emissionIntensity = 2f;
```

### Red Hazard/Warning Laser

```csharp
baseColor = new Color(1f, 0.1f, 0f, 1f); // Red
hotColor = new Color(1f, 0.5f, 0.3f, 1f); // Orange-red
pulseSpeed = 12f; // Faster pulse for warning effect
```

### Thick Industrial Laser

```csharp
baseWidth = 0.3f;
maxWidth = 0.5f;
pulseSpeed = 4f; // Slower, more stable
pulseMagnitude = 0.1f; // Less variation
```

### Quick Response Laser

```csharp
startupDuration = 0.1f; // Near instant
shutdownDuration = 0.1f;
startupCurve = AnimationCurve.Linear(0, 0, 1, 1);
```

## Troubleshooting

### Laser Not Visible
1. Check LineRenderer is enabled
2. Verify material is assigned
3. Ensure width values > 0
4. Check camera can see laser layer

### Particles Not Showing
1. Verify particle systems are assigned
2. Check emission rates > 0
3. Ensure Play On Awake is disabled
4. Verify particle materials exist

### Impact Effect Not Appearing
1. Check impactEffectPrefab is assigned
2. Verify LaserImpactEffect component exists
3. Check effect duration > 0
4. Ensure impact position is valid

### Performance Issues
1. Reduce particle emission rates
2. Disable heat distortion particles
3. Lower particle max particles count
4. Use simpler shader for impact effects
5. Disable impact lights on low-end devices

### Animation Not Smooth
1. Ensure Time.timeScale = 1
2. Check animation curves are set
3. Verify coroutines aren't being interrupted
4. Increase animation durations

## Future Enhancements

### Possible Additions

1. **Multi-Mining Visual States:**
   - Different colors per laser
   - Intensity based on target value
   - Special effects for rare asteroids

2. **Advanced Particle Effects:**
   - Debris chunks flying off
   - Molten material dripping down
   - Shockwave on impact
   - Steam/vapor effects

3. **Audio Integration:**
   - Startup sound (power-up)
   - Looping hum (active mining)
   - Shutdown sound (power-down)
   - Impact/sizzle sounds

4. **Asteroid Surface Interaction:**
   - Actual mesh deformation
   - Temperature mapping on surface
   - Multiple overlapping heat marks
   - Procedural crack patterns

5. **Upgrade Visual Changes:**
   - Wider beam with higher mining level
   - Multiple beam colors for multi-mining
   - Enhanced particles with upgrades
   - Longer-lasting impact marks

## Credits

Created for **Asteroid Miner: Deep Space Operations**  
Based on original web game mechanics  
Enhanced for Unity 3D with modern VFX techniques

---

**Version:** 1.0  
**Last Updated:** November 9, 2025  
**Compatibility:** Unity 2022.3+
