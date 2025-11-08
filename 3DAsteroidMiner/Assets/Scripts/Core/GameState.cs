using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsteroidMiner.Core
{
    /// <summary>
    /// Central game state container maintaining all player progress, resources, and statistics.
    /// This class is serializable for save/load functionality.
    /// </summary>
    [Serializable]
    public class GameState
    {
        // ===== Currency & Resources =====
        public int credits = 0;
        
        // ===== Ship Status =====
        public float hull = 100f;
        public float maxHull = 100f;
        public float fuel = 100f;
        public float maxFuel = 100f;
        
        // ===== Cargo & Inventory =====
        public int currentCargoCount = 0;
        public int maxCargo = 20;
        public Dictionary<string, int> inventory = new Dictionary<string, int>();
        
        // ===== Upgrades (13 types) =====
        public Dictionary<string, int> upgrades = new Dictionary<string, int>()
        {
            { "speed", 1 },
            { "cargo", 1 },
            { "mining", 1 },
            { "hull", 1 },
            { "fuelCapacity", 1 },
            { "fuelEfficiency", 1 },
            { "range", 1 },
            { "multiMining", 1 },
            { "scanRange", 1 },
            { "scanCooldown", 1 },
            { "advancedScanner", 0 },  // one-time purchase
            { "cargoDrone", 0 }         // one-time purchase
        };
        
        // ===== Progression =====
        public int sector = 1;
        public int prestige = 0;
        
        // ===== Statistics =====
        public float distanceTraveled = 0f;
        public int asteroidsMined = 0;
        public int totalCreditsEarned = 0;
        public int hazardsDestroyed = 0;
        public int sectorsExplored = 0;
        public int missionsCompleted = 0;
        public float totalPlayTime = 0f;
        
        // ===== Player Customization =====
        public string shipName = "Prospector";
        
        // ===== Ship State =====
        public bool isDocked = false;
        public Vector3 shipPosition = Vector3.zero;
        public Vector3 shipVelocity = Vector3.zero;
        
        /// <summary>
        /// Get the current acceleration based on speed upgrade level.
        /// Formula: baseAcceleration * (1 + speedLevel * 0.15)
        /// </summary>
        public float GetAcceleration()
        {
            float baseAcceleration = 10f;
            int speedLevel = upgrades["speed"];
            return baseAcceleration * (1f + speedLevel * 0.15f);
        }
        
        /// <summary>
        /// Get the current max speed based on speed upgrade level.
        /// Formula: baseMaxSpeed * (1 + speedLevel * 0.2)
        /// </summary>
        public float GetMaxSpeed()
        {
            float baseMaxSpeed = 20f;
            int speedLevel = upgrades["speed"];
            return baseMaxSpeed * (1f + speedLevel * 0.2f);
        }
        
        /// <summary>
        /// Get the fuel efficiency multiplier (lower is better).
        /// Formula: 1.0 - (fuelEfficiencyLevel * 0.08)
        /// </summary>
        public float GetFuelEfficiency()
        {
            int efficiencyLevel = upgrades["fuelEfficiency"];
            return 1.0f - (efficiencyLevel * 0.08f);
        }
        
        /// <summary>
        /// Get the mining speed multiplier.
        /// Formula: 1.0 + (miningLevel * 0.2)
        /// </summary>
        public float GetMiningSpeed()
        {
            int miningLevel = upgrades["mining"];
            return 1.0f + (miningLevel * 0.2f);
        }
        
        /// <summary>
        /// Get the number of simultaneous mining targets allowed.
        /// Based on multiMining upgrade level (1-6).
        /// </summary>
        public int GetMultiMiningCount()
        {
            return upgrades["multiMining"];
        }
        
        /// <summary>
        /// Get the scanner range based on upgrade level.
        /// Formula: 100 + (scanRangeLevel - 1) * 100
        /// Base: 100m, +100m per level (Level 10 = 1000m)
        /// </summary>
        public float GetScanRange()
        {
            int upgradeLevel = upgrades.ContainsKey("scanRange") ? upgrades["scanRange"] : 1;
            return 100f + ((upgradeLevel - 1) * 100f);
        }
        
        /// <summary>
        /// Get the scanner cooldown based on upgrade level.
        /// Formula: 10 * (1 - (scanCooldownLevel - 1) * 0.08)
        /// Base: 10s, -8% per level (Level 10 = 2.6s)
        /// </summary>
        public float GetScanCooldown()
        {
            int upgradeLevel = upgrades.ContainsKey("scanCooldown") ? upgrades["scanCooldown"] : 1;
            float reduction = (upgradeLevel - 1) * 0.08f;
            return 10f * (1f - reduction);
        }
        
        /// <summary>
        /// Check if advanced scanner is unlocked (shows asteroid values).
        /// </summary>
        public bool HasAdvancedScanner()
        {
            return upgrades.ContainsKey("advancedScanner") && upgrades["advancedScanner"] > 0;
        }
        
        /// <summary>
        /// Add credits with prestige bonus applied.
        /// </summary>
        public void AddCredits(int amount)
        {
            float prestigeBonus = 1f + (prestige * 0.1f);
            int earnedAmount = Mathf.RoundToInt(amount * prestigeBonus);
            credits += earnedAmount;
            totalCreditsEarned += earnedAmount;
        }
        
        /// <summary>
        /// Add item to inventory and update cargo count.
        /// </summary>
        public bool AddToInventory(string itemName, int quantity = 1)
        {
            if (currentCargoCount + quantity > maxCargo)
                return false;
            
            if (inventory.ContainsKey(itemName))
                inventory[itemName] += quantity;
            else
                inventory[itemName] = quantity;
            
            currentCargoCount += quantity;
            return true;
        }
        
        /// <summary>
        /// Consume fuel based on movement intensity.
        /// </summary>
        public void ConsumeFuel(float amount)
        {
            float efficiency = GetFuelEfficiency();
            fuel = Mathf.Max(0f, fuel - (amount * efficiency));
        }
        
        /// <summary>
        /// Check if ship has enough fuel for an action.
        /// </summary>
        public bool HasFuel(float requiredAmount = 0f)
        {
            return fuel > requiredAmount;
        }
        
        /// <summary>
        /// Initialize a new game state with default values.
        /// </summary>
        public static GameState CreateNew()
        {
            GameState state = new GameState();
            state.maxHull = 100f;
            state.hull = state.maxHull;
            state.maxFuel = 100f;
            state.fuel = state.maxFuel;
            state.maxCargo = 20;
            return state;
        }
    }
}
