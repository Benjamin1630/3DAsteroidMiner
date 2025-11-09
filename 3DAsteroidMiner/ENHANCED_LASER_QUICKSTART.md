# Enhanced Mining Laser - Quick Setup Guide

## 5-Minute Setup

### Step 1: Create Enhanced Laser Prefab

1. **Create Empty GameObject:**
   - Hierarchy → Right Click → Create Empty
   - Name it `EnhancedMiningLaser`

2. **Add Components:**
   - Add Component → EnhancedMiningLaser
   - Line Renderer will be added automatically

3. **Configure Inspector:**
   ```
   Base Color: RGB(255, 102, 0) Alpha 255 (Hot orange)
   Hot Color: RGB(255, 230, 102) Alpha 255 (Yellow-white)
   Base Width: 0.15
   Max Width: 0.25
   
   Startup Duration: 0.3
   Shutdown Duration: 0.2
   
   Pulse Speed: 8
   Pulse Magnitude: 0.2
   Intensity Variation: 0.15
   
   Emission Intensity: 3
   ```

4. **Particle Systems (Optional):**
   - Leave empty for auto-generation
   - OR create custom particle systems and assign them

5. **Save as Prefab:**
   - Drag GameObject to `Assets/Prefabs/` folder
   - Delete from scene

### Step 2: Update MiningSystem

1. **Open MiningSystem.cs**

2. **Replace laser prefab field:**
   ```csharp
   // OLD:
   [SerializeField] private GameObject laserPrefab;
   private List<MiningLaser> activeLasers;
   
   // NEW:
   [SerializeField] private GameObject enhancedLaserPrefab;
   private List<EnhancedMiningLaser> activeLasers;
   ```

3. **Update laser creation:**
   ```csharp
   // OLD:
   private MiningLaser CreateLaser()
   {
       GameObject laserObj = Instantiate(laserPrefab);
       MiningLaser laser = laserObj.GetComponent<MiningLaser>();
       laser.Initialize(laserColor, laserWidth);
       return laser;
   }
   
   // NEW:
   private EnhancedMiningLaser CreateLaser()
   {
       GameObject laserObj = Instantiate(enhancedLaserPrefab);
       return laserObj.GetComponent<EnhancedMiningLaser>();
   }
   ```

4. **Update StartMining method:**
   ```csharp
   // OLD:
   laser.SetVisible(true);
   laser.UpdateLaser(startPos, endPos);
   
   // NEW:
   laser.StartLaser(startPos, endPos);
   ```

5. **Update StopMining method:**
   ```csharp
   // OLD:
   laser.SetVisible(false);
   
   // NEW:
   laser.StopLaser();
   ```

### Step 3: Assign in Unity Editor

1. **Select Player GameObject** (or MiningSystem GameObject)
2. **Find MiningSystem component** in Inspector
3. **Assign EnhancedLaserPrefab:**
   - Drag `EnhancedMiningLaser` prefab from Project to field
4. **Set Laser Origins** (if using multi-mining)

### Step 4: Test!

1. Press Play
2. Start mining an asteroid
3. Observe:
   - ✅ Laser powers up gradually
   - ✅ Beam pulses and glows
   - ✅ Particles appear at impact
   - ✅ Heat mark appears on asteroid
   - ✅ Laser powers down smoothly when stopped

---

## Customization Presets

### Preset 1: Classic Green Laser
```csharp
Base Color: RGB(0, 255, 0) Alpha 204
Hot Color: RGB(153, 255, 153) Alpha 255
Emission Intensity: 2
```

### Preset 2: Blue Plasma Laser
```csharp
Base Color: RGB(51, 153, 255) Alpha 255
Hot Color: RGB(204, 230, 255) Alpha 255
Emission Intensity: 4
Pulse Speed: 10
```

### Preset 3: Red Danger Laser
```csharp
Base Color: RGB(255, 25, 0) Alpha 255
Hot Color: RGB(255, 128, 77) Alpha 255
Emission Intensity: 3.5
Pulse Speed: 12
```

### Preset 4: Industrial Heavy Laser
```csharp
Base Width: 0.3
Max Width: 0.5
Pulse Speed: 4
Pulse Magnitude: 0.1
Startup Duration: 0.5
```

### Preset 5: Quick Tactical Laser
```csharp
Base Width: 0.1
Max Width: 0.15
Startup Duration: 0.1
Shutdown Duration: 0.1
Pulse Speed: 15
```

---

## Troubleshooting

### Laser Not Visible
**Symptom:** Can't see the laser beam

**Solutions:**
1. Check LineRenderer component is enabled
2. Set Base Width to at least 0.1
3. Ensure material is assigned
4. Check laser color alpha is > 0
5. Verify laser is on a visible layer

### No Startup Animation
**Symptom:** Laser appears instantly

**Solutions:**
1. Check Startup Duration > 0
2. Verify StartupCurve is assigned
3. Ensure Time.timeScale = 1
4. Check no errors in console blocking coroutines

### Particles Missing
**Symptom:** No particle effects visible

**Solutions:**
1. Leave particle fields empty for auto-generation
2. If using custom particles, ensure they're assigned
3. Check particle emission rate > 0
4. Verify particle materials exist

### Poor Performance
**Symptom:** Frame rate drops when mining

**Solutions:**
1. Reduce particle emission rates by 50%
2. Lower Max Particles in particle systems
3. Disable Heat Distortion Particles
4. Remove point light from impact effects
5. Set Impact Effect Duration to 1.0 instead of 2.0

### Impact Effect Not Showing
**Symptom:** No heat marks on asteroids

**Solutions:**
1. Check Impact Effect Duration > 0
2. Verify LaserImpactEffect.cs is in project
3. Ensure sufficient lighting in scene
4. Check effect isn't spawning behind asteroid

---

## Advanced: Custom Particle Systems

If you want full control over particle appearance:

### Creating Custom Startup Particles

1. **Create Particle System:**
   - GameObject → Effects → Particle System
   - Name: `LaserStartupParticles`

2. **Configure Main Module:**
   ```
   Duration: 0.3
   Looping: OFF
   Start Lifetime: 0.3
   Start Speed: 5
   Start Size: 0.2
   Start Color: Orange (255, 153, 51)
   Simulation Space: World
   ```

3. **Configure Emission:**
   ```
   Rate over Time: 50
   Bursts: None
   ```

4. **Configure Shape:**
   ```
   Shape: Cone
   Angle: 15
   Radius: 0.1
   Emit from: Base
   ```

5. **Save as Prefab** and assign to EnhancedMiningLaser

### Creating Custom Spark Particles

1. **Create Particle System:**
   - GameObject → Effects → Particle System
   - Name: `LaserSparks`

2. **Configure Main Module:**
   ```
   Duration: 1.0
   Looping: ON
   Start Lifetime: Random between 0.2 and 0.5
   Start Speed: Random between 5 and 15
   Start Size: Random between 0.05 and 0.15
   Start Color: Bright yellow (255, 230, 153)
   Gravity Modifier: 0.2
   Simulation Space: World
   ```

3. **Configure Emission:**
   ```
   Rate over Time: 20
   ```

4. **Configure Shape:**
   ```
   Shape: Sphere
   Radius: 0.3
   ```

5. **Add Color over Lifetime:**
   ```
   Gradient: Yellow to Red to Black
   ```

6. **Save and Assign**

---

## Performance Tips

### For Mobile Devices
```csharp
// In EnhancedMiningLaser component settings:
Max Impact Effects: 5 (instead of 10)
Impact Effect Duration: 1.0 (instead of 2.0)

// Disable expensive particle systems:
Heat Distortion Particles: (none)
Spark Particles: Reduce emission to 10

// Simplify material:
Use Unlit shader instead of Standard
Disable emission on impact effects
```

### For Desktop
```csharp
// Full quality:
Max Impact Effects: 15
Impact Effect Duration: 2.5

// Add extra particle systems:
Add debris particles
Add shockwave effect on impact
Add light flicker animation
```

---

## Audio Integration (Optional)

Add these audio clips for complete immersion:

```csharp
[Header("Audio")]
[SerializeField] private AudioClip startupSound;
[SerializeField] private AudioClip loopingSound;
[SerializeField] private AudioClip shutdownSound;
[SerializeField] private AudioClip impactSound;

private AudioSource audioSource;

// In StartupAnimation:
audioSource.PlayOneShot(startupSound);

// In Active state:
audioSource.clip = loopingSound;
audioSource.loop = true;
audioSource.Play();

// In ShutdownAnimation:
audioSource.Stop();
audioSource.PlayOneShot(shutdownSound);
```

---

## Next Steps

1. ✅ **Test basic functionality** - Ensure laser works
2. ✅ **Customize colors** - Match your game's visual style
3. ✅ **Optimize particles** - Balance quality vs performance
4. ✅ **Add audio** - Enhance immersion
5. ✅ **Test multi-mining** - Verify multiple lasers work
6. ✅ **Profile performance** - Check frame rate impact

---

**Ready to mine!** 🚀⛏️

For detailed documentation, see `ENHANCED_LASER_SYSTEM_README.md`
