# Mining System Documentation

## Overview
The mining system allows players to extract resources from asteroids using mining lasers. It supports multi-target mining (1-6 simultaneous targets) based on the `multiMining` upgrade level.

## Components

### 1. MiningSystem.cs
**Location:** `Assets/Scripts/Systems/MiningSystem.cs`

The core mining system that manages:
- Target acquisition (finds asteroids in range)
- Multi-target mining (up to 6 simultaneous targets)
- Laser beam visualization
- Resource collection and credit awards
- Fuel consumption during mining
- Cargo capacity checks

**Key Settings:**
- `miningRange`: Maximum distance for mining (default: 50m)
- `baseMiningRate`: Base damage per second (default: 1.0)
- `miningFuelConsumption`: Fuel cost per second per target (default: 0.5)
- `laserColor`: Color of mining laser beams (default: green)
- `laserWidth`: Width of laser beams (default: 0.2)

**Setup Instructions:**
1. Attach `MiningSystem` component to your Player ship GameObject
2. Assign `GameState` reference in Inspector
3. Assign `playerTransform` (usually the ship root transform)
4. (Optional) Create multiple laser origin points for visual variety
5. Ensure PlayerInputHandler is on same GameObject or parent

### 2. MiningLaser.cs
**Location:** `Assets/Scripts/Systems/MiningLaser.cs`

Manages individual laser beam visualization using LineRenderer:
- Creates animated laser beams between ship and asteroid
- Pulsing width animation for visual feedback
- Supports dynamic color and width changes
- Auto-creates default material if none provided

**Features:**
- Pulse animation (configurable speed and magnitude)
- Gradient transparency (solid at ship, fading at asteroid)
- Always faces camera for optimal visibility
- No shadow casting for performance

### 3. Asteroid.cs Updates
**Location:** `Assets/Scripts/Entities/Asteroid.cs`

**New/Updated Features:**
- `OnAsteroidDestroyed` event - Fires when asteroid health reaches zero
- `TakeMiningDamage(float damage)` - Specifically for mining damage
- Procedural mesh shrinking effect as health decreases
- `IsBeingMined` flag to prevent duplicate targeting

## Mining Flow

### 1. Player Initiates Mining
- Player presses **Mine button** (Left Mouse / Space / X Button on gamepad)
- `PlayerInputHandler` fires `OnMineStarted` event
- `MiningSystem.StartMining()` is called

### 2. Target Acquisition
```csharp
// MiningSystem acquires targets
int maxTargets = gameState.GetMultiMiningCount(); // 1-6 based on upgrade
Physics.OverlapSphere(playerPosition, miningRange, asteroidLayerMask);
// Sorts by distance, assigns closest asteroids
```

### 3. Mining Loop (Update)
For each active target:
```csharp
// Apply mining damage
float miningDamage = baseMiningRate * miningSpeed * Time.deltaTime;
asteroid.TakeMiningDamage(miningDamage);

// Consume fuel
float fuelCost = miningFuelConsumption * targetCount * Time.deltaTime;
gameState.ConsumeFuel(fuelCost);

// Update laser visualization
laser.UpdateLaser(shipPosition, asteroidPosition);
```

### 4. Asteroid Destruction
```csharp
if (asteroid.CurrentHealth <= 0)
{
    // Award resources
    gameState.AddToInventory(asteroid.Type.resourceName, 1);
    
    // Award credits (with prestige bonus)
    gameState.AddCredits(asteroid.Type.value);
    
    // Update stats
    gameState.asteroidsMined++;
}
```

### 5. Target Validation
Targets are removed if:
- Asteroid is destroyed (health <= 0)
- Out of range (distance > miningRange)
- Player runs out of fuel
- Cargo is full

## Integration with GameState

### Mining Speed Calculation
```csharp
// From GameState.GetMiningSpeed()
float miningSpeed = 1.0f + (miningLevel * 0.2f);
// Level 1 = 1.0x (base speed)
// Level 5 = 2.0x (double speed)
// Level 10 = 3.0x (triple speed)
```

### Multi-Mining Count
```csharp
// From GameState.GetMultiMiningCount()
int targets = upgrades["multiMining"];
// Level 1 = 1 target
// Level 6 = 6 targets (max)
```

### Fuel Consumption
```csharp
// Total fuel cost per second
float fuelPerSecond = miningFuelConsumption * activeTargetCount;
// Modified by fuel efficiency upgrade in GameState.ConsumeFuel()
```

### Credit Awards
```csharp
// GameState.AddCredits() applies prestige bonus
float prestigeBonus = 1f + (prestige * 0.1f);
int earnedAmount = asteroidValue * prestigeBonus;
```

## Input Mapping

Mining uses the **Mine** action from `InputSystem_Actions.inputactions`:

**Keyboard:**
- Left Mouse Button (hold)
- Space (hold)

**Gamepad:**
- X Button / Square Button (hold)
- Right Trigger (hold)

**Touch:**
- Tap and hold on screen

## Visual Feedback

### Laser Beams
- **Green** = Actively mining
- Animated pulse effect (width oscillates)
- LineRenderer with transparency gradient
- Always visible (no occlusion)

### Asteroid Effects
- Procedural mesh shrinks as health decreases
- `IsBeingMined` flag prevents multiple miners targeting same asteroid
- Emission glow if scanned (from ScannerSystem)

## Performance Considerations

### Object Pooling
Mining lasers are created/destroyed dynamically but should be pooled in future optimization:
```csharp
// TODO: Implement laser pooling for >6 lasers
ObjectPool<MiningLaser> laserPool;
```

### Physics Queries
- Uses `Physics.OverlapSphere()` with layer mask
- Only queries on target acquisition, not every frame
- Asteroid layer mask cached in Awake()

### Update Frequency
- Mining damage applied every frame (smooth progress)
- Target validation every frame (checks range, health, fuel)
- Re-acquisition only when all targets lost

## Debugging

### Gizmos
When `MiningSystem` is selected in editor:
- **Green wire sphere** = Mining range
- **Yellow spheres** = Laser origin points

### Debug Logs
Enable debug logs by uncommenting `#if UNITY_EDITOR` blocks:
```csharp
Debug.Log($"MiningSystem: Acquired {targetCount} targets (max: {maxTargets})");
Debug.Log($"MiningSystem: Mined {resourceName} (+{value} credits)");
Debug.Log("MiningSystem: Cargo full, stopping mining");
```

## Common Issues & Solutions

### Issue: Mining doesn't work
**Check:**
1. Is `PlayerInputHandler` on player GameObject?
2. Is `MiningSystem` subscribed to input events?
3. Is `GameState` assigned in Inspector?
4. Does player have fuel?
5. Is cargo full?

### Issue: No laser beams visible
**Check:**
1. Are laser origin points assigned?
2. Is `MiningLaserMaterial` assigned?
3. Are asteroids in range (50m default)?
4. Is asteroid layer mask correct?

### Issue: Mining too slow/fast
**Adjust:**
- `baseMiningRate` - Base damage per second
- Mining upgrade level - Multiplies mining speed
- Asteroid health values in AsteroidType ScriptableObjects

### Issue: Can't target specific asteroid
**Solution:**
- Mining auto-targets closest asteroids
- Increase `multiMining` upgrade level for more targets
- Move closer to desired asteroid (targets sorted by distance)

## Upgrade Integration

The mining system reads these upgrades from GameState:

| Upgrade | Effect | Formula |
|---------|--------|---------|
| `mining` | Mining speed multiplier | `1.0 + (level * 0.2)` |
| `multiMining` | Max simultaneous targets | `level` (1-6) |
| `fuelEfficiency` | Fuel consumption reduction | `1.0 - (level * 0.08)` |
| `cargo` | Inventory capacity | Base + (level * 20) |

## Future Enhancements

### Planned Features
- [ ] Mining laser color variation by upgrade level
- [ ] Particle effects on asteroid impact point
- [ ] Sound effects for mining start/stop/complete
- [ ] UI showing mining progress per asteroid
- [ ] Critical hit chance (bonus damage/resources)
- [ ] Laser beam texture scrolling animation
- [ ] Heat buildup mechanic (overheat after extended mining)

### Performance Optimizations
- [ ] Object pool for laser beams
- [ ] Spatial partitioning for asteroid queries
- [ ] LOD system for distant laser beams
- [ ] Batch laser rendering (single mesh)

## Testing Checklist

- [ ] Mining starts when button pressed
- [ ] Mining stops when button released
- [ ] Laser beams visible and animated
- [ ] Multiple targets work (with multiMining upgrade)
- [ ] Credits awarded on asteroid destruction
- [ ] Resources added to inventory
- [ ] Cargo capacity respected
- [ ] Fuel consumed during mining
- [ ] Mining stops when fuel depleted
- [ ] Mining stops when cargo full
- [ ] Asteroids shrink as health decreases
- [ ] Target re-acquisition works
- [ ] Out-of-range targets removed
- [ ] Input works with keyboard and gamepad

## Code Examples

### Adding Mining System to Player
```csharp
// In Unity Editor:
// 1. Select Player ship GameObject
// 2. Add Component -> Mining System
// 3. Assign GameState reference
// 4. (Optional) Create child GameObjects for laser origins
//    - Name them "LaserMount_1", "LaserMount_2", etc.
//    - Assign to laserOrigins array in Inspector
```

### Customizing Mining Speed
```csharp
// Adjust in Inspector or code
miningSystem.baseMiningRate = 2.0f; // Double mining speed
```

### Listening for Mining Events
```csharp
// Subscribe to asteroid destruction
Asteroid asteroid = GetComponent<Asteroid>();
asteroid.OnAsteroidDestroyed += HandleAsteroidDestroyed;

void HandleAsteroidDestroyed(Asteroid asteroid)
{
    Debug.Log($"Asteroid destroyed: {asteroid.Type.resourceName}");
}
```

### Custom Laser Colors
```csharp
// Change laser color dynamically
MiningLaser laser = GetComponent<MiningLaser>();
laser.SetColor(Color.red); // Change to red laser
laser.SetWidth(0.5f); // Make thicker
```

---

**Last Updated:** November 9, 2025  
**Version:** 1.0  
**Status:** ✅ Complete and Ready for Testing
