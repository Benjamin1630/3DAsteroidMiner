using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Centralized ship statistics and state management.
    /// This is the single source of truth for all ship-related data including:
    /// - Resources (credits, fuel, hull)
    /// - Cargo inventory with per-asteroid-type tracking
    /// - All 13 upgrade levels
    /// - Current ship state (velocity, position, docked status)
    /// - Player statistics
    /// - Prestige and progression
    /// 
    /// All other scripts should access ship data through getter methods in this class.
    /// </summary>
    public class ShipStats : MonoBehaviour
    {
        #region Events
        
        public event Action OnCreditsChanged;
        public event Action OnCargoChanged;
        public event Action OnHullChanged;
        public event Action OnFuelChanged;
        public event Action OnUpgradeChanged;
        
        #endregion
        
        #region Currency & Resources
        
        [Header("Currency & Resources")]
        [SerializeField] private int credits = 0;
        
        #endregion
        
        #region Ship Status
        
        [Header("Ship Status")]
        [SerializeField] private float hull = 100f;
        [SerializeField] private float maxHull = 100f;
        [SerializeField] private float fuel = 100f;
        [SerializeField] private float maxFuel = 100f;
        
        #endregion
        
        #region Cargo & Inventory
        
        [Header("Cargo & Inventory")]
        [SerializeField] private int maxCargo = 20;
        
        /// <summary>
        /// Tracks quantity of each asteroid type in cargo.
        /// Key: Resource name (e.g., "Iron Ore", "Gold", "Quantum Crystal")
        /// Value: Quantity in cargo hold
        /// </summary>
        private Dictionary<string, int> cargoInventory = new Dictionary<string, int>();
        
        /// <summary>
        /// Current total cargo count (sum of all inventory items).
        /// </summary>
        private int currentCargoCount = 0;
        
        #endregion
        
        #region Upgrades
        
        [Header("Upgrades")]
        [SerializeField] private int speedLevel = 1;
        [SerializeField] private int cargoLevel = 1;
        [SerializeField] private int miningLevel = 1;
        [SerializeField] private int hullLevel = 1;
        [SerializeField] private int fuelCapacityLevel = 1;
        [SerializeField] private int fuelEfficiencyLevel = 1;
        [SerializeField] private int rangeLevel = 1;
        [SerializeField] private int multiMiningLevel = 1;
        [SerializeField] private int scanRangeLevel = 1;
        [SerializeField] private int scanCooldownLevel = 1;
        [SerializeField] private bool hasAdvancedScanner = false;
        [SerializeField] private bool hasCargoDrone = false;
        
        #endregion
        
        #region Progression
        
        [Header("Progression")]
        [SerializeField] private int currentSector = 1;
        [SerializeField] private int prestigeLevel = 0;
        
        #endregion
        
        #region Statistics
        
        [Header("Statistics")]
        [SerializeField] private float distanceTraveled = 0f;
        [SerializeField] private int asteroidsMined = 0;
        [SerializeField] private int totalCreditsEarned = 0;
        [SerializeField] private int hazardsDestroyed = 0;
        [SerializeField] private int sectorsExplored = 0;
        [SerializeField] private int missionsCompleted = 0;
        [SerializeField] private float totalPlayTime = 0f;
        
        #endregion
        
        #region Player Customization
        
        [Header("Customization")]
        [SerializeField] private string shipName = "Prospector";
        
        #endregion
        
        #region Ship State
        
        [Header("Ship State")]
        [SerializeField] private bool isDocked = false;
        
        private Vector3 currentVelocity = Vector3.zero;
        private Rigidbody rb;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // Initialize cargo inventory
            if (cargoInventory == null)
            {
                cargoInventory = new Dictionary<string, int>();
            }
        }
        
        private void Update()
        {
            // Track velocity from Rigidbody
            if (rb != null)
            {
                currentVelocity = rb.linearVelocity;
            }
            
            // Increment play time
            totalPlayTime += Time.deltaTime;
        }
        
        #endregion
        
        #region Currency Methods
        
        /// <summary>
        /// Get current credits.
        /// </summary>
        public int GetCredits()
        {
            return credits;
        }
        
        /// <summary>
        /// Add credits with prestige bonus applied.
        /// </summary>
        public void AddCredits(int amount)
        {
            float prestigeBonus = 1f + (prestigeLevel * 0.1f);
            int earnedAmount = Mathf.RoundToInt(amount * prestigeBonus);
            credits += earnedAmount;
            totalCreditsEarned += earnedAmount;
            
            OnCreditsChanged?.Invoke();
        }
        
        /// <summary>
        /// Remove credits (for purchases).
        /// Returns true if successful, false if insufficient credits.
        /// </summary>
        public bool SpendCredits(int amount)
        {
            if (credits >= amount)
            {
                credits -= amount;
                OnCreditsChanged?.Invoke();
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region Hull Methods
        
        /// <summary>
        /// Get current hull points.
        /// </summary>
        public float GetHull()
        {
            return hull;
        }
        
        /// <summary>
        /// Get maximum hull capacity.
        /// </summary>
        public float GetMaxHull()
        {
            return maxHull;
        }
        
        /// <summary>
        /// Get hull as percentage (0-1).
        /// </summary>
        public float GetHullPercent()
        {
            return maxHull > 0 ? hull / maxHull : 0f;
        }
        
        /// <summary>
        /// Take damage to hull.
        /// </summary>
        public void DamageHull(float amount)
        {
            hull = Mathf.Max(0f, hull - amount);
            OnHullChanged?.Invoke();
            
            if (hull <= 0f)
            {
                Debug.LogWarning("ShipStats: Hull destroyed!");
                // TODO: Trigger game over or respawn logic
            }
        }
        
        /// <summary>
        /// Repair hull (e.g., at station).
        /// </summary>
        public void RepairHull(float amount)
        {
            hull = Mathf.Min(maxHull, hull + amount);
            OnHullChanged?.Invoke();
        }
        
        /// <summary>
        /// Fully repair hull to maximum.
        /// </summary>
        public void FullRepairHull()
        {
            hull = maxHull;
            OnHullChanged?.Invoke();
        }
        
        #endregion
        
        #region Fuel Methods
        
        /// <summary>
        /// Get current fuel amount.
        /// </summary>
        public float GetFuel()
        {
            return fuel;
        }
        
        /// <summary>
        /// Get maximum fuel capacity.
        /// </summary>
        public float GetMaxFuel()
        {
            return maxFuel;
        }
        
        /// <summary>
        /// Get fuel as percentage (0-1).
        /// </summary>
        public float GetFuelPercent()
        {
            return maxFuel > 0 ? fuel / maxFuel : 0f;
        }
        
        /// <summary>
        /// Check if ship has enough fuel for an action.
        /// </summary>
        public bool HasFuel(float requiredAmount = 0f)
        {
            return fuel > requiredAmount;
        }
        
        /// <summary>
        /// Consume fuel based on movement intensity.
        /// Amount is automatically modified by fuel efficiency upgrade.
        /// </summary>
        public void ConsumeFuel(float amount)
        {
            float efficiency = GetFuelEfficiencyMultiplier();
            fuel = Mathf.Max(0f, fuel - (amount * efficiency));
            OnFuelChanged?.Invoke();
        }
        
        /// <summary>
        /// Refuel the ship (e.g., at station).
        /// </summary>
        public void Refuel(float amount)
        {
            fuel = Mathf.Min(maxFuel, fuel + amount);
            OnFuelChanged?.Invoke();
        }
        
        /// <summary>
        /// Fully refuel to maximum capacity.
        /// </summary>
        public void FullRefuel()
        {
            fuel = maxFuel;
            OnFuelChanged?.Invoke();
        }
        
        #endregion
        
        #region Cargo & Inventory Methods
        
        /// <summary>
        /// Get current total cargo count.
        /// </summary>
        public int GetCurrentCargoCount()
        {
            return currentCargoCount;
        }
        
        /// <summary>
        /// Get maximum cargo capacity.
        /// </summary>
        public int GetMaxCargo()
        {
            return maxCargo;
        }
        
        /// <summary>
        /// Get cargo as percentage (0-1).
        /// </summary>
        public float GetCargoPercent()
        {
            return maxCargo > 0 ? (float)currentCargoCount / maxCargo : 0f;
        }
        
        /// <summary>
        /// Check if cargo is full.
        /// </summary>
        public bool IsCargoFull()
        {
            return currentCargoCount >= maxCargo;
        }
        
        /// <summary>
        /// Get quantity of specific resource in cargo.
        /// </summary>
        public int GetCargoAmount(string resourceName)
        {
            return cargoInventory.ContainsKey(resourceName) ? cargoInventory[resourceName] : 0;
        }
        
        /// <summary>
        /// Get entire cargo inventory (read-only).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetCargoInventory()
        {
            return cargoInventory;
        }
        
        /// <summary>
        /// Add resource to cargo inventory.
        /// Returns true if successful, false if cargo is full.
        /// </summary>
        public bool AddToCargo(string resourceName, int quantity = 1)
        {
            if (currentCargoCount + quantity > maxCargo)
                return false;
            
            if (cargoInventory.ContainsKey(resourceName))
                cargoInventory[resourceName] += quantity;
            else
                cargoInventory[resourceName] = quantity;
            
            currentCargoCount += quantity;
            OnCargoChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Remove resource from cargo (e.g., when selling).
        /// Returns true if successful, false if insufficient quantity.
        /// </summary>
        public bool RemoveFromCargo(string resourceName, int quantity = 1)
        {
            if (!cargoInventory.ContainsKey(resourceName) || cargoInventory[resourceName] < quantity)
                return false;
            
            cargoInventory[resourceName] -= quantity;
            currentCargoCount -= quantity;
            
            // Remove entry if quantity reaches 0
            if (cargoInventory[resourceName] <= 0)
                cargoInventory.Remove(resourceName);
            
            OnCargoChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Clear all cargo (e.g., when selling all at station).
        /// </summary>
        public void ClearCargo()
        {
            cargoInventory.Clear();
            currentCargoCount = 0;
            OnCargoChanged?.Invoke();
        }
        
        #endregion
        
        #region Upgrade Level Getters
        
        public int GetSpeedLevel() => speedLevel;
        public int GetCargoLevel() => cargoLevel;
        public int GetMiningLevel() => miningLevel;
        public int GetHullLevel() => hullLevel;
        public int GetFuelCapacityLevel() => fuelCapacityLevel;
        public int GetFuelEfficiencyLevel() => fuelEfficiencyLevel;
        public int GetRangeLevel() => rangeLevel;
        public int GetMultiMiningLevel() => multiMiningLevel;
        public int GetScanRangeLevel() => scanRangeLevel;
        public int GetScanCooldownLevel() => scanCooldownLevel;
        public bool HasAdvancedScanner() => hasAdvancedScanner;
        public bool HasCargoDrone() => hasCargoDrone;
        
        #endregion
        
        #region Upgrade Application Methods
        
        /// <summary>
        /// Upgrade speed level.
        /// </summary>
        public void UpgradeSpeed()
        {
            speedLevel = Mathf.Min(10, speedLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade cargo capacity.
        /// Increases maxCargo by 20 per level.
        /// </summary>
        public void UpgradeCargo()
        {
            cargoLevel++;
            maxCargo = 20 + (cargoLevel - 1) * 20; // Base 20, +20 per level
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade mining speed.
        /// </summary>
        public void UpgradeMining()
        {
            miningLevel = Mathf.Min(10, miningLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade hull capacity.
        /// Increases maxHull by 20 per level and fully repairs.
        /// </summary>
        public void UpgradeHull()
        {
            hullLevel = Mathf.Min(10, hullLevel + 1);
            maxHull = 100f + (hullLevel - 1) * 20f; // Base 100, +20 per level
            hull = maxHull; // Fully repair on upgrade
            OnHullChanged?.Invoke();
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade fuel capacity.
        /// Increases maxFuel by 20% per level and fully refuels.
        /// </summary>
        public void UpgradeFuelCapacity()
        {
            fuelCapacityLevel++;
            maxFuel = 100f * Mathf.Pow(1.2f, fuelCapacityLevel - 1); // Base 100, +20% per level
            fuel = maxFuel; // Fully refuel on upgrade
            OnFuelChanged?.Invoke();
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade fuel efficiency.
        /// </summary>
        public void UpgradeFuelEfficiency()
        {
            fuelEfficiencyLevel = Mathf.Min(10, fuelEfficiencyLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade warp range.
        /// </summary>
        public void UpgradeRange()
        {
            rangeLevel = Mathf.Min(10, rangeLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade multi-mining capability.
        /// </summary>
        public void UpgradeMultiMining()
        {
            multiMiningLevel = Mathf.Min(6, multiMiningLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade scanner range.
        /// </summary>
        public void UpgradeScanRange()
        {
            scanRangeLevel = Mathf.Min(10, scanRangeLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Upgrade scanner cooldown.
        /// </summary>
        public void UpgradeScanCooldown()
        {
            scanCooldownLevel = Mathf.Min(10, scanCooldownLevel + 1);
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Purchase advanced scanner (one-time).
        /// </summary>
        public void PurchaseAdvancedScanner()
        {
            hasAdvancedScanner = true;
            OnUpgradeChanged?.Invoke();
        }
        
        /// <summary>
        /// Purchase cargo drone (one-time).
        /// </summary>
        public void PurchaseCargoDrone()
        {
            hasCargoDrone = true;
            OnUpgradeChanged?.Invoke();
        }
        
        #endregion
        
        #region Upgrade Calculation Methods
        
        /// <summary>
        /// Get acceleration multiplier based on speed upgrade.
        /// Formula: baseAcceleration * (1 + speedLevel * 0.15)
        /// </summary>
        public float GetAccelerationMultiplier()
        {
            return 1f + (speedLevel * 0.15f);
        }
        
        /// <summary>
        /// Get max speed multiplier based on speed upgrade.
        /// Formula: baseMaxSpeed * (1 + speedLevel * 0.2)
        /// </summary>
        public float GetMaxSpeedMultiplier()
        {
            return 1f + (speedLevel * 0.2f);
        }
        
        /// <summary>
        /// Get fuel efficiency multiplier (lower is better).
        /// Formula: 1.0 - (fuelEfficiencyLevel * 0.08)
        /// </summary>
        public float GetFuelEfficiencyMultiplier()
        {
            return 1.0f - (fuelEfficiencyLevel * 0.08f);
        }
        
        /// <summary>
        /// Get mining speed multiplier.
        /// Formula: 1.0 + (miningLevel * 0.2)
        /// </summary>
        public float GetMiningSpeedMultiplier()
        {
            return 1.0f + (miningLevel * 0.2f);
        }
        
        /// <summary>
        /// Get scanner range in meters.
        /// Formula: 100 + (scanRangeLevel - 1) * 100
        /// Base: 100m, +100m per level (Level 10 = 1000m)
        /// </summary>
        public float GetScannerRange()
        {
            return 100f + ((scanRangeLevel - 1) * 100f);
        }
        
        /// <summary>
        /// Get scanner cooldown in seconds.
        /// Formula: 10 * (1 - (scanCooldownLevel - 1) * 0.08)
        /// Base: 10s, -8% per level (Level 10 = 2.6s)
        /// </summary>
        public float GetScannerCooldown()
        {
            float reduction = (scanCooldownLevel - 1) * 0.08f;
            return 10f * (1f - reduction);
        }
        
        #endregion
        
        #region Progression Methods
        
        /// <summary>
        /// Get current sector.
        /// </summary>
        public int GetSector()
        {
            return currentSector;
        }
        
        /// <summary>
        /// Get prestige level.
        /// </summary>
        public int GetPrestigeLevel()
        {
            return prestigeLevel;
        }
        
        /// <summary>
        /// Set current sector.
        /// </summary>
        public void SetSector(int sector)
        {
            currentSector = Mathf.Max(1, sector);
        }
        
        /// <summary>
        /// Increment prestige level.
        /// </summary>
        public void IncreasePrestige()
        {
            prestigeLevel++;
        }
        
        #endregion
        
        #region Statistics Methods
        
        /// <summary>
        /// Add to distance traveled statistic.
        /// </summary>
        public void AddDistanceTraveled(float distance)
        {
            distanceTraveled += distance;
        }
        
        /// <summary>
        /// Increment asteroids mined counter.
        /// </summary>
        public void IncrementAsteroidsMined()
        {
            asteroidsMined++;
        }
        
        /// <summary>
        /// Increment hazards destroyed counter.
        /// </summary>
        public void IncrementHazardsDestroyed()
        {
            hazardsDestroyed++;
        }
        
        /// <summary>
        /// Increment sectors explored counter.
        /// </summary>
        public void IncrementSectorsExplored()
        {
            sectorsExplored++;
        }
        
        /// <summary>
        /// Increment missions completed counter.
        /// </summary>
        public void IncrementMissionsCompleted()
        {
            missionsCompleted++;
        }
        
        // Statistic getters
        public float GetDistanceTraveled() => distanceTraveled;
        public int GetAsteroidsMined() => asteroidsMined;
        public int GetTotalCreditsEarned() => totalCreditsEarned;
        public int GetHazardsDestroyed() => hazardsDestroyed;
        public int GetSectorsExplored() => sectorsExplored;
        public int GetMissionsCompleted() => missionsCompleted;
        public float GetTotalPlayTime() => totalPlayTime;
        
        #endregion
        
        #region Ship State Methods
        
        /// <summary>
        /// Get ship name.
        /// </summary>
        public string GetShipName()
        {
            return shipName;
        }
        
        /// <summary>
        /// Set ship name.
        /// </summary>
        public void SetShipName(string name)
        {
            shipName = name;
        }
        
        /// <summary>
        /// Check if ship is currently docked.
        /// </summary>
        public bool IsDocked()
        {
            return isDocked;
        }
        
        /// <summary>
        /// Set docked status.
        /// </summary>
        public void SetDocked(bool docked)
        {
            isDocked = docked;
        }
        
        /// <summary>
        /// Get current velocity vector.
        /// </summary>
        public Vector3 GetVelocity()
        {
            return currentVelocity;
        }
        
        /// <summary>
        /// Get current speed (magnitude of velocity).
        /// </summary>
        public float GetSpeed()
        {
            return currentVelocity.magnitude;
        }
        
        /// <summary>
        /// Get ship position.
        /// </summary>
        public Vector3 GetPosition()
        {
            return transform.position;
        }
        
        #endregion
    }
}

