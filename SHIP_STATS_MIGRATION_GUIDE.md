# Ship Stats System Migration Guide

## Overview

The ship data management system has been completely refactored to use a single, centralized `ShipStats` component. This provides better organization, type safety, and easier access to ship-related data.

## What Changed

### Before (Old System)
- **GameState.cs** - A serializable data class (not MonoBehaviour) containing all game state
- Other scripts held references to `GameState` instance
- Direct field access (e.g., `gameState.credits`, `gameState.hull`)
- Manual null checking required

### After (New System)
- **ShipStats.cs** - A MonoBehaviour component attached to the player ship
- Centralized ship statistics and state management
- **All access through getter/setter methods** for consistency
- Better encapsulation and data validation
- Event system for state changes (credits, cargo, hull, fuel, upgrades)

## Key Benefits

1. **Single Source of Truth** - All ship data in one component
2. **Type Safety** - MonoBehaviour ensures component exists on GameObject
3. **Consistent Access** - All data accessed through getter methods
4. **Event-Driven** - UI and other systems can subscribe to change events
5. **Better Organization** - Clear separation of concerns
6. **Inspector Visibility** - All values visible and editable in Unity Inspector

## ShipStats Component Structure

### Data Categories

#### 1. **Currency & Resources**
```csharp
int GetCredits()
void AddCredits(int amount)  // Includes prestige bonus
bool SpendCredits(int amount)
```

#### 2. **Hull Management**
```csharp
float GetHull()
float GetMaxHull()
float GetHullPercent()
void DamageHull(float amount)
void RepairHull(float amount)
void FullRepairHull()
```

#### 3. **Fuel Management**
```csharp
float GetFuel()
float GetMaxFuel()
float GetFuelPercent()
bool HasFuel(float requiredAmount = 0f)
void ConsumeFuel(float amount)  // Auto-applies efficiency upgrade
void Refuel(float amount)
void FullRefuel()
```

#### 4. **Cargo & Inventory**
```csharp
int GetCurrentCargoCount()
int GetMaxCargo()
float GetCargoPercent()
bool IsCargoFull()
int GetCargoAmount(string resourceName)
IReadOnlyDictionary<string, int> GetCargoInventory()
bool AddToCargo(string resourceName, int quantity = 1)
bool RemoveFromCargo(string resourceName, int quantity = 1)
void ClearCargo()
```

#### 5. **Upgrade Levels**
```csharp
// Level getters (all return int except bool ones)
int GetSpeedLevel()
int GetCargoLevel()
int GetMiningLevel()
int GetHullLevel()
int GetFuelCapacityLevel()
int GetFuelEfficiencyLevel()
int GetRangeLevel()
int GetMultiMiningLevel()
int GetScanRangeLevel()
int GetScanCooldownLevel()
bool HasAdvancedScanner()
bool HasCargoDrone()

// Upgrade application
void UpgradeSpeed()
void UpgradeCargo()
void UpgradeMining()
void UpgradeHull()
void UpgradeFuelCapacity()
void UpgradeFuelEfficiency()
void UpgradeRange()
void UpgradeMultiMining()
void UpgradeScanRange()
void UpgradeScanCooldown()
void PurchaseAdvancedScanner()
void PurchaseCargoDrone()
```

#### 6. **Upgrade Calculations**
```csharp
float GetAccelerationMultiplier()     // Returns multiplier (e.g., 1.3x)
float GetMaxSpeedMultiplier()         // Returns multiplier (e.g., 1.4x)
float GetFuelEfficiencyMultiplier()   // Returns multiplier (e.g., 0.84x)
float GetMiningSpeedMultiplier()      // Returns multiplier (e.g., 1.4x)
float GetScannerRange()               // Returns meters (100-1000m)
float GetScannerCooldown()            // Returns seconds (10-2.6s)
```

#### 7. **Progression**
```csharp
int GetSector()
int GetPrestigeLevel()
void SetSector(int sector)
void IncreasePrestige()
```

#### 8. **Statistics**
```csharp
void AddDistanceTraveled(float distance)
void IncrementAsteroidsMined()
void IncrementHazardsDestroyed()
void IncrementSectorsExplored()
void IncrementMissionsCompleted()

// Getters
float GetDistanceTraveled()
int GetAsteroidsMined()
int GetTotalCreditsEarned()
int GetHazardsDestroyed()
int GetSectorsExplored()
int GetMissionsCompleted()
float GetTotalPlayTime()
```

#### 9. **Ship State**
```csharp
string GetShipName()
void SetShipName(string name)
bool IsDocked()
void SetDocked(bool docked)
Vector3 GetVelocity()
float GetSpeed()
Vector3 GetPosition()
```

## Migration Examples

### Example 1: Accessing Credits

**Before:**
```csharp
[SerializeField] private GameState gameState;

void CheckCredits()
{
    if (gameState.credits >= 100)
    {
        gameState.credits -= 100;
    }
}
```

**After:**
```csharp
private ShipStats shipStats;

void Awake()
{
    shipStats = GetComponent<ShipStats>();
}

void CheckCredits()
{
    if (shipStats.GetCredits() >= 100)
    {
        shipStats.SpendCredits(100);
    }
}
```

### Example 2: Mining System Access

**Before:**
```csharp
[SerializeField] private GameState gameState;

void UpdateMining()
{
    if (!gameState.HasFuel()) return;
    
    float miningSpeed = gameState.GetMiningSpeed();
    int maxTargets = gameState.GetMultiMiningCount();
    
    gameState.AddToInventory("Iron Ore", 1);
    gameState.asteroidsMined++;
    gameState.ConsumeFuel(fuelCost);
}
```

**After:**
```csharp
private ShipStats shipStats;

void UpdateMining()
{
    if (!shipStats.HasFuel()) return;
    
    float miningSpeed = shipStats.GetMiningSpeedMultiplier();
    int maxTargets = shipStats.GetMultiMiningLevel();
    
    shipStats.AddToCargo("Iron Ore", 1);
    shipStats.IncrementAsteroidsMined();
    shipStats.ConsumeFuel(fuelCost);
}
```

### Example 3: Player Movement with Upgrades

**Before:**
```csharp
[SerializeField] private GameState gameState;

void ApplyThrust()
{
    float acceleration = gameState.GetAcceleration();
    float maxSpeed = gameState.GetMaxSpeed();
    // ...
    gameState.ConsumeFuel(fuelAmount);
}
```

**After:**
```csharp
private ShipStats shipStats;

void ApplyThrust()
{
    float baseAcceleration = 10f;
    float acceleration = baseAcceleration * shipStats.GetAccelerationMultiplier();
    
    float baseMaxSpeed = 20f;
    float maxSpeed = baseMaxSpeed * shipStats.GetMaxSpeedMultiplier();
    // ...
    shipStats.ConsumeFuel(fuelAmount);
}
```

### Example 4: Subscribing to Events

**New Feature - Event System:**
```csharp
private ShipStats shipStats;
private TextMeshProUGUI creditsText;

void Start()
{
    shipStats = GetComponent<ShipStats>();
    shipStats.OnCreditsChanged += UpdateCreditsUI;
    shipStats.OnCargoChanged += UpdateCargoUI;
    shipStats.OnHullChanged += UpdateHullUI;
    shipStats.OnFuelChanged += UpdateFuelUI;
}

void OnDestroy()
{
    shipStats.OnCreditsChanged -= UpdateCreditsUI;
    shipStats.OnCargoChanged -= UpdateCargoUI;
    shipStats.OnHullChanged -= UpdateHullUI;
    shipStats.OnFuelChanged -= UpdateFuelUI;
}

void UpdateCreditsUI()
{
    creditsText.text = $"Credits: {shipStats.GetCredits():N0}";
}
```

## Files Updated

### ✅ Completed
1. **ShipStats.cs** (NEW) - Centralized ship data management
2. **PlayerController.cs** - Now uses `ShipStats` instead of `GameState`
3. **MiningSystem.cs** - Updated to use `ShipStats` getter methods

### ✅ Completed
1. **ShipStats.cs** (NEW) - Centralized ship data management
2. **PlayerController.cs** - Now uses `ShipStats` instead of `GameState`
3. **MiningSystem.cs** - Updated to use `ShipStats` getter methods
4. **AsteroidSpawner.cs** - Removed `GameState` dependency, created `WarpToNextSector(int newSector)` method
5. **ScannerSystem.cs** - Updated to use `ShipStats.IsDocked()`, `GetScannerRange()`, `GetScannerCooldown()`
6. **PlayerStats.cs** - **DELETED** (completely redundant with `ShipStats`)
7. **ShipVisuals.cs** - Updated to use `ShipStats` via `playerController.GetShipStats()`

### 🔄 Additional Notes

#### AsteroidSpawner Changes
The `AsteroidSpawner` no longer tracks sector changes automatically. Instead, it provides a public method:
```csharp
public void WarpToNextSector(int newSector)
```
This method should be called by your warp/sector transition system to:
- Clear all existing asteroids
- Spawn a fresh field for the new sector

This decouples sector management from asteroid spawning for better architecture.

#### PlayerStats Deletion
`PlayerStats.cs` has been deleted as all its functionality was redundant with `ShipStats`:
- Statistics tracking (asteroids mined, distance traveled, etc.)
- Session tracking
- Increment methods
All these features are now in `ShipStats` with the same or better functionality.

## Setup Instructions

### For New Ships
1. Add `ShipStats` component to player ship GameObject
2. Remove old `GameState` script assignments
3. Components on ship will auto-find `ShipStats` via `GetComponent<ShipStats>()`

### For Existing Systems
Replace this pattern:
```csharp
[SerializeField] private GameState gameState;
```

With this:
```csharp
private ShipStats shipStats;

void Awake()
{
    shipStats = GetComponent<ShipStats>();  // If on same GameObject
    // OR
    shipStats = GetComponentInParent<ShipStats>();  // If on child
    // OR
    shipStats = FindFirstObjectByType<ShipStats>();  // If elsewhere in scene
}
```

## Important Notes

### Multipliers vs Absolute Values
- **GetAccelerationMultiplier()** returns the **multiplier** (e.g., 1.3), not the final value
- Apply it to your base values: `baseAccel * multiplier`
- Same for `GetMaxSpeedMultiplier()`, `GetMiningSpeedMultiplier()`, etc.

### Cargo System
- Tracks **per-asteroid-type** quantities
- Use `AddToCargo()` and `RemoveFromCargo()` for inventory management
- `GetCargoInventory()` returns read-only dictionary for UI display

### Fuel Efficiency
- `ConsumeFuel()` automatically applies efficiency upgrade
- No need to manually calculate efficiency in calling code

### Events
- Subscribe to `OnCreditsChanged`, `OnCargoChanged`, etc. for reactive UI updates
- Always unsubscribe in `OnDestroy()` to prevent memory leaks

## Testing Checklist

After migration, verify:
- [ ] Player movement works with speed upgrades
- [ ] Mining system collects resources to cargo
- [ ] Fuel consumption works correctly
- [ ] Cargo full stops mining
- [ ] Credits are earned and spent correctly
- [ ] Hull damage/repair functions
- [ ] All UI displays update properly
- [ ] Scanner uses correct upgrade values
- [ ] Prestige bonus applies to credits
- [ ] Statistics increment correctly

## Backwards Compatibility

**GameState.cs** is still present in the codebase for reference but should no longer be used for ship data. It may be repurposed for:
- Global game settings
- Save/load data serialization wrapper
- Mission system state
- NPC trader state

Or it can be deprecated entirely in favor of ShipStats + other specialized managers.

---

**Migration Status:** ✅ **COMPLETE** - All ship-related scripts now use ShipStats system.  
**Date:** November 9, 2025
