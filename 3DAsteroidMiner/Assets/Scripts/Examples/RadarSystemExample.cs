using UnityEngine;
using AsteroidMiner.Systems;

namespace AsteroidMiner.Examples
{
    /// <summary>
    /// Example script showing how to use the RadarSystem API.
    /// All radar controls are now in the Inspector - no keyboard shortcuts needed.
    /// This script provides code examples for integration with other game systems.
    /// </summary>
    public class RadarSystemExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RadarSystem radarSystem;
        [SerializeField] private RadarDisplay radarDisplay;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // ===== Unity Lifecycle =====
        
        private void Start()
        {
            // Auto-find components if not assigned
            if (radarSystem == null)
                radarSystem = FindFirstObjectByType<RadarSystem>();
            
            if (radarDisplay == null)
                radarDisplay = FindFirstObjectByType<RadarDisplay>();
            
            if (radarSystem == null)
            {
                Debug.LogError("RadarSystemExample: No RadarSystem found in scene!");
            }
            
            if (radarDisplay == null)
            {
                Debug.LogWarning("RadarSystemExample: No RadarDisplay found in scene (optional)");
            }
            
            LogSetupInfo();
        }
        
        private void Update()
        {
            if (radarSystem == null) return;
            
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }
        
        // ===== Example Functions =====
        
        /// <summary>
        /// Example: Find and target the closest asteroid
        /// </summary>
        public void TargetClosestAsteroid()
        {
            RadarSystem.RadarContact closest = radarSystem.GetClosestContact(RadarSystem.ContactType.Asteroid);
            
            if (closest != null)
            {
                Debug.Log($"<color=green>Targeting: {closest.displayName}</color>");
                Debug.Log($"Distance: {closest.distance:F1}m");
                Debug.Log($"Position: {closest.transform.position}");
                
                // Example: You could pass this to your mining system
                // miningSystem.SetTarget(closest.asteroidComponent);
            }
            else
            {
                Debug.Log("<color=yellow>No asteroids detected</color>");
            }
        }
        
        /// <summary>
        /// Example: Warn about nearby hazards
        /// </summary>
        public void CheckForHazards(float warningDistance)
        {
            var hazards = radarSystem.GetContactsByType(RadarSystem.ContactType.Hazard);
            
            foreach (var contact in hazards)
            {
                if (contact.distance <= warningDistance)
                {
                    Debug.LogWarning($"<color=red>HAZARD WARNING: {contact.displayName} at {contact.distance:F1}m!</color>");
                    
                    // Example: Trigger audio warning
                    // audioManager.PlayWarning();
                    
                    // Example: Show UI warning
                    // uiManager.ShowHazardWarning(contact);
                }
            }
        }
        
        /// <summary>
        /// Example: Integration with upgrade system
        /// </summary>
        public void UpgradeRadarRange(int upgradeLevel)
        {
            // Upgrade formula: Base 1000m + 200m per level
            float baseRange = 1000f;
            float rangePerLevel = 200f;
            float newRange = baseRange + (upgradeLevel * rangePerLevel);
            
            radarSystem.SetRadarRange(newRange);
            
            Debug.Log($"<color=cyan>Radar upgraded to Level {upgradeLevel}</color>");
            Debug.Log($"New range: {newRange}m");
        }
        
        /// <summary>
        /// Example: Auto-target valuable asteroids
        /// </summary>
        public void TargetMostValuableAsteroid()
        {
            var asteroids = radarSystem.GetContactsByType(RadarSystem.ContactType.Asteroid);
            
            RadarSystem.RadarContact mostValuable = null;
            int highestValue = 0;
            
            foreach (var contact in asteroids)
            {
                if (contact.asteroidComponent != null && contact.asteroidComponent.Type != null)
                {
                    int value = contact.asteroidComponent.Type.value;
                    if (value > highestValue)
                    {
                        highestValue = value;
                        mostValuable = contact;
                    }
                }
            }
            
            if (mostValuable != null)
            {
                Debug.Log($"<color=gold>Most Valuable: {mostValuable.displayName} (${highestValue})</color>");
                Debug.Log($"Distance: {mostValuable.distance:F1}m");
                
                // Example: Auto-navigate to target
                // navigationSystem.SetDestination(mostValuable.transform.position);
            }
            else
            {
                Debug.Log("<color=yellow>No valuable asteroids detected</color>");
            }
        }
        
        // ===== Debug Display =====
        
        private void DisplayDebugInfo()
        {
            if (radarSystem.DetectedContacts == null) return;
            
            int totalContacts = radarSystem.DetectedContacts.Count;
            
            // Simple debug info
            string debugText = $"Radar Contacts: {totalContacts}\n";
            debugText += $"Range: {radarSystem.RadarRange}m";
            
            // Display in console every 2 seconds
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"<color=cyan>{debugText}</color>");
            }
        }
        
        private void LogSetupInfo()
        {
            Debug.Log("<color=cyan>=== Radar System Setup ===</color>");
            Debug.Log("All radar controls are now in the Inspector:");
            Debug.Log("• RadarSystem: Toggle Asteroids/Hazards/NPCs");
            Debug.Log("• RadarDisplay: Toggle Display Mode, Elevation Lines, Grid, Screen Background");
            Debug.Log("• Adjust radar range directly in RadarSystem component");
            Debug.Log("<color=cyan>=============================</color>");
        }
        
        // ===== Gizmos =====
        
        private void OnDrawGizmos()
        {
            if (radarSystem == null || !Application.isPlaying) return;
            
            // Draw lines to closest asteroid
            RadarSystem.RadarContact closest = radarSystem.GetClosestContact(RadarSystem.ContactType.Asteroid);
            if (closest != null && closest.transform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, closest.transform.position);
            }
        }
    }
}
