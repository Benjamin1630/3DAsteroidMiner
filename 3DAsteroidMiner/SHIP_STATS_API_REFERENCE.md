# ShipStats Quick API Reference

## Getting ShipStats Reference

```csharp
// On same GameObject
ShipStats shipStats = GetComponent<ShipStats>();

// On parent (for child components like MiningSystem)
ShipStats shipStats = GetComponentInParent<ShipStats>();

// From PlayerController
PlayerController playerController = GetComponent<PlayerController>();
ShipStats shipStats = playerController.GetShipStats();
```

## Most Common Methods

### Currency
```csharp
int credits = shipStats.GetCredits();
shipStats.AddCredits(100);           // Includes prestige bonus
bool success = shipStats.SpendCredits(50);
```

### Hull
```csharp
float hull = shipStats.GetHull();
float maxHull = shipStats.GetMaxHull();
float percent = shipStats.GetHullPercent();  // 0-1
shipStats.DamageHull(10f);
shipStats.RepairHull(20f);
```

### Fuel
```csharp
float fuel = shipStats.GetFuel();
bool hasFuel = shipStats.HasFuel();
shipStats.ConsumeFuel(0.5f);  // Auto-applies efficiency
shipStats.Refuel(10f);
```

### Cargo
```csharp
int count = shipStats.GetCurrentCargoCount();
int max = shipStats.GetMaxCargo();
bool full = shipStats.IsCargoFull();

shipStats.AddToCargo("Iron Ore", 1);
shipStats.RemoveFromCargo("Gold", 5);

// Get all cargo
var inventory = shipStats.GetCargoInventory();
foreach (var item in inventory)
{
    Debug.Log($"{item.Key}: {item.Value}");
}
```

### Upgrades
```csharp
// Get levels
int speedLvl = shipStats.GetSpeedLevel();
int miningLvl = shipStats.GetMiningLevel();
int multiMining = shipStats.GetMultiMiningLevel();

// Get calculated multipliers
float accelMultiplier = shipStats.GetAccelerationMultiplier();
float speedMultiplier = shipStats.GetMaxSpeedMultiplier();
float miningMultiplier = shipStats.GetMiningSpeedMultiplier();

// Apply upgrades
shipStats.UpgradeSpeed();
shipStats.UpgradeMining();
```

### Stats
```csharp
shipStats.IncrementAsteroidsMined();
shipStats.AddDistanceTraveled(distance);
int mined = shipStats.GetAsteroidsMined();
```

### State
```csharp
bool docked = shipStats.IsDocked();
shipStats.SetDocked(true);
Vector3 vel = shipStats.GetVelocity();
float speed = shipStats.GetSpeed();
```

## Event Subscription

```csharp
void OnEnable()
{
    shipStats.OnCreditsChanged += HandleCreditsChanged;
    shipStats.OnCargoChanged += HandleCargoChanged;
    shipStats.OnHullChanged += HandleHullChanged;
    shipStats.OnFuelChanged += HandleFuelChanged;
    shipStats.OnUpgradeChanged += HandleUpgradeChanged;
}

void OnDisable()
{
    shipStats.OnCreditsChanged -= HandleCreditsChanged;
    shipStats.OnCargoChanged -= HandleCargoChanged;
    shipStats.OnHullChanged -= HandleHullChanged;
    shipStats.OnFuelChanged -= HandleFuelChanged;
    shipStats.OnUpgradeChanged -= HandleUpgradeChanged;
}
```
