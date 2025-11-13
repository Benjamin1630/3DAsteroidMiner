# Radar System - Integration Guide

## 🔗 Integrating with Existing Systems

This guide shows how to connect the radar system with other game systems in your Asteroid Miner project.

---

## 1️⃣ Integration with PlayerController

### Add Radar Reference to PlayerController

```csharp
// In PlayerController.cs
using AsteroidMiner.Systems;

public class PlayerController : MonoBehaviour
{
    [Header("Radar System")]
    [SerializeField] private RadarSystem radarSystem;
    
    private void Start()
    {
        // Auto-find if not assigned
        if (radarSystem == null)
        {
            radarSystem = GetComponentInChildren<RadarSystem>();
        }
    }
    
    // Public accessor for other systems
    public RadarSystem GetRadarSystem() => radarSystem;
}
```

---

## 2️⃣ Integration with MiningSystem

### Auto-Target Nearest Asteroid

Add this method to your MiningSystem:

```csharp
// In MiningSystem.cs
using AsteroidMiner.Systems;

public class MiningSystem : MonoBehaviour
{
    [SerializeField] private RadarSystem radarSystem;
    
    // Call this when player presses "target nearest" key
    public void TargetNearestAsteroid()
    {
        if (radarSystem == null) return;
        
        RadarSystem.RadarContact nearest = radarSystem.GetClosestContact(
            RadarSystem.ContactType.Asteroid
        );
        
        if (nearest != null && nearest.asteroidComponent != null)
        {
            // Set as mining target
            currentTarget = nearest.asteroidComponent;
            Debug.Log($"Targeting: {nearest.displayName} at {nearest.distance:F1}m");
        }
    }
    
    // Cycle through nearby asteroids
    public void CycleTargets()
    {
        var asteroids = radarSystem.GetContactsByType(RadarSystem.ContactType.Asteroid);
        
        if (asteroids.Count == 0) return;
        
        // Find current target index
        int currentIndex = asteroids.FindIndex(c => c.asteroidComponent == currentTarget);
        
        // Move to next
        currentIndex = (currentIndex + 1) % asteroids.Count;
        currentTarget = asteroids[currentIndex].asteroidComponent;
        
        Debug.Log($"Target switched to: {asteroids[currentIndex].displayName}");
    }
}
```

### Add Input Binding

In your Input Actions asset or PlayerInputHandler:
```csharp
// In PlayerInputHandler.cs
private InputAction targetNearestAction;

private void Awake()
{
    targetNearestAction = new InputAction("TargetNearest", InputActionType.Button);
    targetNearestAction.AddBinding("<Keyboard>/t");
    targetNearestAction.AddBinding("<Gamepad>/buttonNorth"); // Y button
}

private void Update()
{
    if (targetNearestAction.triggered)
    {
        miningSystem.TargetNearestAsteroid();
    }
}
```

---

## 3️⃣ Integration with ScannerSystem

### Highlight Scanned Asteroids

Modify the RadarSystem to show scanned asteroids differently:

```csharp
// In RadarSystem.cs - Add to GetColorForRarity method

private Color GetColorForRarity(AsteroidRarity rarity)
{
    // Check if asteroid is scanned
    if (asteroid.IsScanned) // You'll need to add this property to Asteroid
    {
        // Scanned asteroids show with white/bright overlay
        Color baseColor = GetBaseColorForRarity(rarity);
        return Color.Lerp(baseColor, Color.white, 0.5f); // Mix with white
    }
    
    return GetBaseColorForRarity(rarity);
}

private Color GetBaseColorForRarity(AsteroidRarity rarity)
{
    // Original color logic here
    switch (rarity) { /* ... */ }
}
```

### Trigger Scanner from Radar

```csharp
// In ScannerSystem.cs
public void ScanRadarContacts()
{
    if (radarSystem == null) return;
    
    var contacts = radarSystem.GetContactsByType(RadarSystem.ContactType.Asteroid);
    
    foreach (var contact in contacts)
    {
        if (contact.distance <= scanRange && contact.asteroidComponent != null)
        {
            ScanAsteroid(contact.asteroidComponent);
        }
    }
}
```

---

## 4️⃣ Integration with ShipStats / Upgrade System

### Radar Range Upgrade

Add to your upgrade system:

```csharp
// In ShipStats.cs or UpgradeSystem.cs

[Header("Radar Upgrades")]
[SerializeField] private float baseRadarRange = 1000f;
[SerializeField] private float radarRangePerLevel = 200f;
public int radarLevel = 1;

public void UpgradeRadar()
{
    if (!CanAffordUpgrade(GetRadarUpgradeCost())) return;
    
    radarLevel++;
    float newRange = baseRadarRange + (radarLevel * radarRangePerLevel);
    
    // Update radar system
    RadarSystem radar = GetComponent<RadarSystem>();
    if (radar != null)
    {
        radar.SetRadarRange(newRange);
    }
    
    Debug.Log($"Radar upgraded to Level {radarLevel} - Range: {newRange}m");
}

public int GetRadarUpgradeCost()
{
    // Exponential cost scaling
    return 100 * (int)Mathf.Pow(2, radarLevel - 1);
}
```

### Add to UI

```csharp
// In UpgradeUIManager.cs or similar

public void OnRadarUpgradeButtonClick()
{
    shipStats.UpgradeRadar();
    UpdateUpgradeUI();
}

private void UpdateRadarUpgradeDisplay()
{
    radarLevelText.text = $"Radar Level: {shipStats.radarLevel}";
    radarRangeText.text = $"Range: {shipStats.GetRadarRange()}m";
    radarCostText.text = $"Upgrade Cost: {shipStats.GetRadarUpgradeCost()} credits";
}
```

---

## 5️⃣ Integration with Hazard Warning System

### Proximity Warning

Create a new component or add to existing hazard manager:

```csharp
// HazardWarningSystem.cs
using AsteroidMiner.Systems;
using UnityEngine;

public class HazardWarningSystem : MonoBehaviour
{
    [SerializeField] private RadarSystem radarSystem;
    [SerializeField] private float warningDistance = 150f;
    [SerializeField] private AudioClip warningSound;
    
    private AudioSource audioSource;
    private float lastWarningTime;
    private float warningCooldown = 2f; // Don't spam warnings
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        CheckForNearbyHazards();
    }
    
    private void CheckForNearbyHazards()
    {
        if (Time.time - lastWarningTime < warningCooldown) return;
        
        var hazards = radarSystem.GetContactsByType(RadarSystem.ContactType.Hazard);
        
        foreach (var hazard in hazards)
        {
            if (hazard.distance <= warningDistance)
            {
                TriggerWarning(hazard);
                lastWarningTime = Time.time;
                break;
            }
        }
    }
    
    private void TriggerWarning(RadarSystem.RadarContact hazard)
    {
        Debug.LogWarning($"<color=red>HAZARD ALERT: {hazard.displayName} at {hazard.distance:F0}m!</color>");
        
        // Play audio
        if (audioSource != null && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Update UI
        // uiManager.ShowHazardWarning(hazard);
    }
}
```

---

## 6️⃣ Integration with Navigation/Autopilot

### Obstacle Avoidance

```csharp
// In NavigationSystem.cs or AutopilotSystem.cs

public Vector3 CalculateAvoidanceVector()
{
    Vector3 avoidance = Vector3.zero;
    
    var obstacles = radarSystem.DetectedContacts;
    
    foreach (var obstacle in obstacles)
    {
        if (obstacle.distance < avoidanceRadius)
        {
            // Calculate repulsion vector (away from obstacle)
            Vector3 awayFromObstacle = -obstacle.relativePosition.normalized;
            float strength = 1f - (obstacle.distance / avoidanceRadius);
            avoidance += awayFromObstacle * strength;
        }
    }
    
    return avoidance.normalized;
}
```

---

## 7️⃣ Integration with Mission System

### Mission: Mine Specific Asteroids

```csharp
// In MissionSystem.cs

public class MiningMission
{
    public AsteroidRarity targetRarity;
    public int requiredCount;
    private int currentCount;
    
    public void OnAsteroidMined(Asteroid asteroid)
    {
        if (asteroid.Type.rarity == targetRarity)
        {
            currentCount++;
            
            if (currentCount >= requiredCount)
            {
                CompleteMission();
            }
            else
            {
                // Show nearest target on radar
                FindNextTarget();
            }
        }
    }
    
    private void FindNextTarget()
    {
        var asteroids = radarSystem.GetContactsByType(RadarSystem.ContactType.Asteroid);
        
        RadarSystem.RadarContact closestMatch = null;
        float closestDistance = float.MaxValue;
        
        foreach (var contact in asteroids)
        {
            if (contact.asteroidComponent != null && 
                contact.asteroidComponent.Type.rarity == targetRarity &&
                contact.distance < closestDistance)
            {
                closestMatch = contact;
                closestDistance = contact.distance;
            }
        }
        
        if (closestMatch != null)
        {
            Debug.Log($"Next {targetRarity} asteroid: {closestMatch.displayName} at {closestMatch.distance:F0}m");
            // Could highlight on radar
        }
    }
}
```

---

## 8️⃣ Integration with UI System

### Radar Info Panel

Create a UI panel that shows radar info:

```csharp
// RadarInfoPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AsteroidMiner.Systems;

public class RadarInfoPanel : MonoBehaviour
{
    [SerializeField] private RadarSystem radarSystem;
    [SerializeField] private TextMeshProUGUI contactCountText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI nearestObjectText;
    
    private void Update()
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (radarSystem == null) return;
        
        // Contact count
        int count = radarSystem.DetectedContacts.Count;
        contactCountText.text = $"Contacts: {count}";
        
        // Radar range
        rangeText.text = $"Range: {radarSystem.RadarRange:F0}m";
        
        // Nearest object
        var nearest = radarSystem.GetClosestContact(RadarSystem.ContactType.Asteroid);
        if (nearest != null)
        {
            nearestObjectText.text = $"Nearest: {nearest.displayName} ({nearest.distance:F0}m)";
        }
        else
        {
            nearestObjectText.text = "Nearest: None";
        }
    }
}
```

---

## 9️⃣ Integration with Audio System

### Radar Ping Audio

```csharp
// RadarAudioFeedback.cs
using UnityEngine;
using AsteroidMiner.Systems;

public class RadarAudioFeedback : MonoBehaviour
{
    [SerializeField] private RadarSystem radarSystem;
    [SerializeField] private AudioClip radarPing;
    [SerializeField] private AudioClip targetLockSound;
    
    private AudioSource audioSource;
    private int previousContactCount;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        int currentCount = radarSystem.DetectedContacts.Count;
        
        // Play ping when new contacts appear
        if (currentCount > previousContactCount)
        {
            PlayPing();
        }
        
        previousContactCount = currentCount;
    }
    
    private void PlayPing()
    {
        if (audioSource != null && radarPing != null)
        {
            audioSource.PlayOneShot(radarPing, 0.3f);
        }
    }
    
    public void PlayTargetLock()
    {
        if (audioSource != null && targetLockSound != null)
        {
            audioSource.PlayOneShot(targetLockSound);
        }
    }
}
```

---

## 🔟 Integration with Save/Load System

### Save Radar Settings

```csharp
// In GameState.cs or SaveSystem.cs

[System.Serializable]
public class RadarSettings
{
    public float radarRange = 1000f;
    public int radarLevel = 1;
    public bool asteroidsVisible = true;
    public bool hazardsVisible = true;
    public bool npcsVisible = true;
    public int displayMode = 1; // 0=2D, 1=3D, 2=Cylindrical
}

public class GameState
{
    public RadarSettings radarSettings = new RadarSettings();
    
    // Save
    public void SaveRadarState(RadarSystem radar, RadarDisplay display)
    {
        radarSettings.radarRange = radar.RadarRange;
        radarSettings.displayMode = (int)display.displayMode;
        // Save other settings...
    }
    
    // Load
    public void LoadRadarState(RadarSystem radar, RadarDisplay display)
    {
        radar.SetRadarRange(radarSettings.radarRange);
        display.SetDisplayMode((RadarDisplay.RadarDisplayMode)radarSettings.displayMode);
        radar.ToggleAsteroids(radarSettings.asteroidsVisible);
        // Load other settings...
    }
}
```

---

## 🎮 Complete Input System Integration

### Add Radar Controls to Input Actions

In your InputSystem_Actions.inputactions:

```json
{
  "name": "Radar",
  "actions": [
    {
      "name": "TargetNearest",
      "type": "Button",
      "bindings": [
        { "path": "<Keyboard>/t" },
        { "path": "<Gamepad>/buttonNorth" }
      ]
    },
    {
      "name": "CycleTargets",
      "type": "Button",
      "bindings": [
        { "path": "<Keyboard>/y" },
        { "path": "<Gamepad>/dpad/right" }
      ]
    },
    {
      "name": "ToggleRadar",
      "type": "Button",
      "bindings": [
        { "path": "<Keyboard>/r" }
      ]
    },
    {
      "name": "IncreaseRange",
      "type": "Button",
      "bindings": [
        { "path": "<Keyboard>/equals" }
      ]
    },
    {
      "name": "DecreaseRange",
      "type": "Button",
      "bindings": [
        { "path": "<Keyboard>/minus" }
      ]
    }
  ]
}
```

---

## 📋 Integration Checklist

Use this checklist to ensure proper integration:

### PlayerController Integration
- [ ] RadarSystem reference added
- [ ] Auto-find logic implemented
- [ ] Public accessor method created

### MiningSystem Integration
- [ ] TargetNearestAsteroid() method added
- [ ] CycleTargets() method added
- [ ] Input bindings configured
- [ ] Tested targeting from radar

### ScannerSystem Integration
- [ ] Scanned asteroids highlighted differently
- [ ] Scan from radar function added
- [ ] Visual feedback working

### Upgrade System Integration
- [ ] Radar upgrade function added
- [ ] Cost calculation implemented
- [ ] UI updated with radar info
- [ ] Save/load of upgrades working

### Hazard Warning Integration
- [ ] Warning system created
- [ ] Audio feedback added
- [ ] UI alerts working
- [ ] Distance threshold configurable

### Navigation Integration
- [ ] Obstacle avoidance using radar data
- [ ] Autopilot considers radar contacts
- [ ] Collision prediction implemented

### Mission System Integration
- [ ] Missions can reference radar data
- [ ] Target guidance from radar
- [ ] Completion tracking working

### UI Integration
- [ ] Info panel created
- [ ] Real-time updates working
- [ ] Settings accessible from UI
- [ ] Visual feedback connected

### Audio Integration
- [ ] Radar ping sounds added
- [ ] Target lock sound added
- [ ] Warning sounds connected
- [ ] Volume configurable

### Save/Load Integration
- [ ] Radar settings saved
- [ ] Upgrades persisted
- [ ] Preferences restored
- [ ] Tested save/load cycle

---

## 🚀 Quick Integration Test

After integrating, test these scenarios:

1. **Basic Functionality**
   - Spawn asteroids
   - Verify they appear on radar
   - Check colors match rarity

2. **Mining Integration**
   - Press T to target nearest
   - Verify mining targets correctly
   - Test cycle targets

3. **Upgrade Integration**
   - Purchase radar upgrade
   - Verify range increases
   - Check UI updates

4. **Hazard Integration**
   - Spawn hazards near ship
   - Verify warning triggers
   - Check audio plays

5. **Save/Load Integration**
   - Change radar settings
   - Save game
   - Load game
   - Verify settings restored

---

## 📞 Common Integration Issues

### Issue: Radar not finding ship
**Solution**: Ensure ship has "Player" tag or RadarSystem has shipTransform assigned.

### Issue: Mining system can't access radar
**Solution**: Add public getter in PlayerController for radar reference.

### Issue: Radar doesn't persist upgrades
**Solution**: Add radar settings to GameState serialization.

### Issue: Input conflicts with other systems
**Solution**: Use separate Input Action Map for radar controls.

### Issue: Performance drops with many contacts
**Solution**: Reduce update frequency or max blips limit.

---

## 🎓 Advanced Integration Ideas

1. **Threat Assessment**: Color hazards by danger level
2. **Waypoint System**: Mark positions on radar
3. **Contact History**: Show movement trails
4. **Formation Display**: Show NPC squadrons
5. **Sector Map**: Zoom out to show multiple sectors
6. **Resource Density Map**: Heatmap of asteroid richness
7. **Signal Jamming**: Interference from hazards
8. **Cooperative Radar**: Share contacts with NPCs

---

**Your radar system is now fully integrated!** 🎯

For questions about integration, refer to the example scripts in `Assets/Scripts/Examples/RadarSystemExample.cs`.
