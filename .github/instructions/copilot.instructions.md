---
applyTo: '**'
---

# AI Development Instructions for Asteroid Miner: Deep Space Operations

**Project Type:** Unity 3D Game Development  
**Original Source:** WebAsteroidMiner (HTML5/JavaScript)  
**Target Platform:** Unity 2022.3+ (Windows/Mac/Linux/Mobile)  
**Input System:** Unity New Input System (Input System Package)  
**Last Updated:** November 6, 2025

---

## 🎯 Project Overview

You are assisting with the development of **Asteroid Miner: Deep Space Operations**, a Unity 3D remake of a feature-rich HTML5 space mining simulator. This project involves converting 14,843 lines of JavaScript code into a modular Unity C# architecture while maintaining all 200+ features from the original game.

### Core Concept
Players pilot a mining ship through infinite procedurally generated space sectors, mining asteroids (16 types), avoiding hazards (3 types), completing missions (6 types), upgrading their ship (13 systems), competing with AI miners (8 personalities), and progressing through an infinite prestige system.

---

## 📁 Project Structure

### Key Directories
```
3DAsteroidMiner/
├── Assets/
│   ├── Scripts/         # All C# game logic
│   ├── Prefabs/         # Reusable game objects
│   ├── Scenes/          # Unity scenes
│   ├── Materials/       # Shaders and materials
│   ├── Models/          # 3D assets
│   └── Data/            # ScriptableObjects for configs
├── WebAsteroidMiner/    # Original web game (REFERENCE ONLY)
├── UNITY_CONVERSION_GUIDE.md      # Architecture & phases
├── FEATURE_LIST.md                # All 200+ features
├── UNITY_IMPLEMENTATION_EXAMPLES.md # Code examples
└── README.md                      # GitHub documentation
```

### Critical Reference Files
1. **WebAsteroidMiner/asteroid-miner-script.js** - Original game logic (14,843 lines)
2. **FEATURE_LIST.md** - Complete feature specifications
3. **UNITY_CONVERSION_GUIDE.md** - Architecture and implementation roadmap
4. **UNITY_IMPLEMENTATION_EXAMPLES.md** - C# code patterns

---

## 🔧 Development Guidelines

### 1. Code Style & Conventions

#### C# Naming Conventions
```csharp
// Classes, Structs, Enums: PascalCase
public class GameManager : MonoBehaviour { }
public enum AsteroidRarity { Common, Rare, Legendary }

// Public fields/properties: PascalCase
public int MaxHealth { get; set; }
public float MiningSpeed;

// Private fields: camelCase with optional underscore
private int currentHealth;
private float _miningProgress;

// Methods: PascalCase
public void AddCredits(int amount) { }

// Constants: UPPER_SNAKE_CASE or PascalCase
private const float BASE_SPEED = 10f;
private const int MaxUpgradeLevel = 10;

// Local variables: camelCase
int asteroidCount = 0;
float distanceTraveled = 0f;
```

#### File Organization
- **One class per file** (except nested classes)
- **File name matches class name** (e.g., `PlayerController.cs`)
- **Group related scripts** in subfolders (Core/, Systems/, Entities/)

#### Component Structure
```csharp
using UnityEngine;

public class ExampleComponent : MonoBehaviour
{
    // Serialized Fields (visible in Inspector)
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5f;
    
    // Public Properties
    public float CurrentSpeed { get; private set; }
    
    // Private Fields
    private Rigidbody rb;
    private Vector3 velocity;
    
    // Unity Lifecycle
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void FixedUpdate() { }
    private void OnDestroy() { }
    
    // Public Methods
    public void Initialize() { }
    
    // Private Methods
    private void CalculateMovement() { }
}
```

### 2. Unity-Specific Best Practices

#### Performance Optimization
```csharp
// ✅ DO: Cache component references
private Rigidbody rb;
private void Awake() { rb = GetComponent<Rigidbody>(); }

// ❌ DON'T: Repeated GetComponent calls
private void Update() { 
    GetComponent<Rigidbody>().velocity = Vector3.zero; // BAD!
}

// ✅ DO: Use object pooling for frequently spawned objects
public class AsteroidPool : MonoBehaviour
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    public GameObject GetAsteroid() { /* return from pool */ }
    public void ReturnAsteroid(GameObject asteroid) { /* return to pool */ }
}

// ✅ DO: Use Time.deltaTime for frame-independent movement
velocity += acceleration * Time.deltaTime;

// ✅ DO: Use squared distance to avoid expensive Sqrt
float distSq = (target.position - transform.position).sqrMagnitude;
if (distSq < range * range) { /* in range */ }
```

#### Component Communication
```csharp
// ✅ DO: Use singleton for managers
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}

// ✅ DO: Use events for decoupled communication
public class AsteroidDestroyer : MonoBehaviour
{
    public event System.Action<int> OnAsteroidDestroyed;
    
    private void DestroyAsteroid()
    {
        OnAsteroidDestroyed?.Invoke(asteroidValue);
    }
}

// ✅ DO: Use ScriptableObjects for shared data
[CreateAssetMenu(fileName = "AsteroidType", menuName = "Game/Asteroid Type")]
public class AsteroidType : ScriptableObject
{
    public string resourceName;
    public int value;
    public float health;
}
```

#### Input Handling - New Input System (REQUIRED)
```csharp
// ❌ DON'T: Use old Input class (Project uses new Input System)
if (Input.GetKeyDown(KeyCode.Space)) { } // BAD!
Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // BAD!

// ✅ DO: Use new Input System with InputAction
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction fireAction;
    
    private void Awake()
    {
        // Create actions programmatically
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Keyboard>/space");
    }
    
    private void OnEnable()
    {
        moveAction?.Enable();
        fireAction?.Enable();
    }
    
    private void OnDisable()
    {
        moveAction?.Disable();
        fireAction?.Disable();
    }
    
    private void OnDestroy()
    {
        moveAction?.Dispose();
        fireAction?.Dispose();
    }
    
    private void Update()
    {
        // Read input values
        Vector2 movement = moveAction.ReadValue<Vector2>();
        bool isFiring = fireAction.IsPressed();
    }
}

// ✅ BETTER: Use InputActionAsset for complex input
[SerializeField] private InputActionAsset inputActions;
private InputAction moveAction;

private void Awake()
{
    moveAction = inputActions.FindActionMap("Player").FindAction("Move");
}

// ✅ BEST: Use event-based callbacks for responsiveness
private void OnEnable()
{
    fireAction.started += OnFireStarted;
    fireAction.canceled += OnFireCanceled;
    fireAction.Enable();
}

private void OnDisable()
{
    fireAction.started -= OnFireStarted;
    fireAction.canceled -= OnFireCanceled;
    fireAction.Disable();
}

private void OnFireStarted(InputAction.CallbackContext context)
{
    StartFiring();
}

private void OnFireCanceled(InputAction.CallbackContext context)
{
    StopFiring();
}
```

### 3. Converting Web Code to Unity

#### Web Worker → Unity Job System
```csharp
// Original Web: collision-worker.js
self.onmessage = function(e) {
    // Collision detection in web worker
};

// Unity Equivalent: Use Job System
using Unity.Jobs;
using Unity.Collections;

struct CollisionJob : IJob
{
    public NativeArray<Vector3> positions;
    public void Execute()
    {
        // Collision detection logic
    }
}
```

#### JavaScript State → C# GameState
```javascript
// Original Web: gameState object
const gameState = {
    credits: 0,
    hull: 100,
    upgrades: { speed: 1, cargo: 1 }
};

// Unity Equivalent: Serializable class
[System.Serializable]
public class GameState
{
    public int credits = 0;
    public float hull = 100f;
    public Dictionary<string, int> upgrades = new Dictionary<string, int>();
}
```

#### Canvas Rendering → Unity GameObjects
```javascript
// Original Web: Canvas drawing
ctx.fillStyle = asteroid.color;
ctx.fillRect(asteroid.x, asteroid.y, asteroid.size, asteroid.size);

// Unity Equivalent: GameObject with Renderer
GameObject asteroid = Instantiate(asteroidPrefab);
asteroid.transform.position = new Vector3(x, 0, z);
asteroid.GetComponent<Renderer>().material.color = asteroidColor;
```

---

## 🎮 Game Systems Reference

### 1. Core Systems (Must Understand)

#### GameState System
**Location:** `Assets/Scripts/Core/GameState.cs`

Key properties to maintain:
- `credits` - Player currency (int)
- `hull` / `maxHull` - Ship health (float)
- `fuel` / `maxFuel` - Ship fuel (float)
- `cargo` / `maxCargo` - Inventory capacity (int)
- `upgrades` - Dictionary<string, int> with 13 upgrade types
- `inventory` - Dictionary<string, int> for resources
- `sector` - Current sector number (infinite progression)
- `prestige` - Prestige level (resets on prestige)
- `activeMissions` - List<Mission> of current missions

#### Upgrade System
**13 Upgrade Types:**
1. `speed` (1-10) - Ship acceleration
2. `cargo` (unlimited) - Cargo capacity (+20 per level)
3. `mining` (1-10) - Mining speed
4. `hull` (1-10) - Max hull (+20 per level)
5. `fuelCapacity` (unlimited) - Max fuel (+20% per level)
6. `fuelEfficiency` (1-10) - Fuel consumption (-8% per level)
7. `range` (1-10) - Sector jump distance
8. `multiMining` (1-6) - Simultaneous mining targets
9. `scanRange` (1-10) - Scanner radius (+100 per level)
10. `scanCooldown` (1-10) - Scanner cooldown (-8% per level)
11. `advancedScanner` (one-time) - Shows asteroid values
12. `cargoDrone` (one-time) - Remote cargo selling
13. Additional lasers based on multiMining level

**Upgrade Cost Formula:**
```csharp
// For levels 1-10: Use predefined array
int[] speedCosts = { 10, 25, 50, 100, 200, 400, 800, 1600, 3200, 6400 };

// For unlimited upgrades (cargo, fuel): Exponential scaling
int CalculateCost(int level)
{
    int baseCost = 20;
    return baseCost * (int)Mathf.Pow(2, level - 1);
}
```

### 2. Asteroid System

#### 16 Asteroid Types (Tiered Rarity)
```csharp
// Common (60% total spawn)
{ "Iron Ore", value: 2, health: 10, chance: 0.40 }
{ "Copper", value: 5, health: 9, chance: 0.20 }

// Uncommon (20% total)
{ "Nickel", value: 12, health: 7, chance: 0.10 }
{ "Silver", value: 18, health: 6, chance: 0.07 }
{ "Titanium", value: 25, health: 8, chance: 0.03 }

// Rare (12% total)
{ "Gold", value: 40, health: 5, chance: 0.05 }
{ "Emerald", value: 55, health: 4, chance: 0.04 }
{ "Platinum", value: 70, health: 4, chance: 0.03 }

// Epic (6% total)
{ "Ruby", value: 100, health: 3, chance: 0.025 }
{ "Sapphire", value: 120, health: 3, chance: 0.020 }
{ "Obsidian", value: 140, health: 5, chance: 0.015 }

// Legendary (2% total)
{ "Quantum Crystal", value: 200, health: 2, chance: 0.010 }
{ "Nebulite", value: 250, health: 2, chance: 0.007 }
{ "Dark Matter", value: 350, health: 3, chance: 0.003 }
```

**Spawning Logic:**
- Max asteroids = `150 + (sector - 1) * 50`
- Spawn chance per frame = `0.05 * (1 + sector * 0.1)`
- World size = `3000 + (sector - 1) * 250` per axis

### 3. Hazard System

#### 3 Hazard Types
```csharp
// Space Debris
{ damage: 10, size: 14, speed: 0.75, behavior: "moving" }

// Proximity Mine
{ damage: 25, size: 12, speed: 0.2, behavior: "stationary/explode" }

// Gravity Vortex
{ damage: 5/tick, size: 60, pullForce: 0.4, behavior: "pull_player" }
```

### 4. Mission System

#### 6 Mission Types
```csharp
public enum MissionType
{
    MiningContract,    // Collect X of resource
    Exploration,       // Travel X distance
    MineralSurvey,     // Mine X rare asteroids
    Delivery,          // Reach sector with cargo
    HazardClearance,   // Destroy X hazards
    Survival           // Mine X without damage
}
```

**Mission Generation:**
- Each station has 3-6 unique missions
- Rewards scale with sector: `baseReward * sector * (1 + prestigeBonus)`
- Player can have max 3 active missions
- Missions can be abandoned with penalty

### 5. NPC System

#### 8 Personality Types
```csharp
public enum NPCPersonality
{
    Cautious,      // Avoids hazards 1.5x, slow, returns at 60%
    Aggressive,    // Ignores hazards, fast, returns at 95%
    Efficient,     // Balanced, 10% faster mining
    Greedy,        // Chases valuable asteroids, returns at 100%
    Lazy,          // Slow, takes closest targets, returns at 50%
    Professional,  // Consistent, reliable performance
    Opportunist,   // Random behavior, unpredictable
    Reckless       // Minimal hazard avoidance, very fast
}
```

**Radio Chatter Contexts:**
- `greeting` - When player approaches
- `tooClose` - When player invades space
- `lostAsteroid` - When player steals target
- `success` - When NPC destroys asteroid
- `danger` - When near hazard
- `fullCargo` - When returning to station

### 6. Prestige System

**Requirements:**
- Minimum 500,000 credits
- Sector 10 or higher

**Effects:**
- Resets: credits, upgrades, sector, inventory, missions
- Keeps: prestige level, ship name, statistics
- Bonus: +10% credits per prestige level (compounding)

**Calculation:**
```csharp
float earnedCredits = baseCredits * (1 + prestige * 0.10f);
// Prestige 3 = +33% credits (1.1 * 1.1 * 1.1 = 1.331)
```

---

## 🚨 Common Pitfalls to Avoid

### 1. Performance Issues
```csharp
// ❌ BAD: Finding objects every frame
void Update() {
    GameObject player = GameObject.Find("Player"); // SLOW!
}

// ✅ GOOD: Cache references
private GameObject player;
void Start() {
    player = GameObject.FindGameObjectWithTag("Player");
}

// ❌ BAD: Using Camera.main in Update
void Update() {
    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
}

// ✅ GOOD: Cache camera reference
private Camera mainCamera;
void Start() { mainCamera = Camera.main; }
```

### 2. Memory Leaks
```csharp
// ❌ BAD: Not unsubscribing from events
void OnEnable() {
    GameManager.Instance.OnGameOver += HandleGameOver;
}
// Missing OnDisable!

// ✅ GOOD: Always unsubscribe
void OnEnable() {
    GameManager.Instance.OnGameOver += HandleGameOver;
}
void OnDisable() {
    GameManager.Instance.OnGameOver -= HandleGameOver;
}

// ❌ BAD: Instantiating without cleanup
void SpawnAsteroid() {
    Instantiate(asteroidPrefab); // Will accumulate!
}

// ✅ GOOD: Use object pooling or track instances
private List<GameObject> asteroids = new List<GameObject>();
void SpawnAsteroid() {
    var asteroid = poolManager.GetAsteroid();
    asteroids.Add(asteroid);
}
```

### 3. Null Reference Errors
```csharp
// ❌ BAD: Assuming components exist
void Start() {
    GetComponent<Rigidbody>().velocity = Vector3.zero; // Might be null!
}

// ✅ GOOD: Null checking
void Start() {
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null) {
        rb.velocity = Vector3.zero;
    } else {
        Debug.LogError("Rigidbody component missing!");
    }
}

// ✅ BETTER: RequireComponent attribute
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour { }
```

---

## 📋 Implementation Checklist

When implementing a new feature, follow this checklist:

### For Any New System
- [ ] Review original web code in `WebAsteroidMiner/` folder
- [ ] Check `FEATURE_LIST.md` for complete specifications
- [ ] Review `UNITY_IMPLEMENTATION_EXAMPLES.md` for similar code patterns
- [ ] Create ScriptableObject for configuration data (if applicable)
- [ ] Implement with object pooling if objects spawn frequently
- [ ] Add null checks and error handling
- [ ] Cache all GetComponent calls in Awake/Start
- [ ] Use events for system communication (avoid direct coupling)
- [ ] Use new Input System (never old Input class)
- [ ] Test in Unity Editor with Debug.Log statements
- [ ] Update UI elements when state changes
- [ ] Document public methods with XML comments
- [ ] Ensure serialization works with save system

### For UI Implementation
- [ ] Update UI only when values change (dirty flags)
- [ ] Cache TextMeshPro/UI component references
- [ ] Use UIManager for centralized UI updates
- [ ] Test with different resolutions
- [ ] Ensure gamepad navigation works

### For Game Balance
- [ ] Reference original web game values first
- [ ] Maintain upgrade cost formulas from FEATURE_LIST.md
- [ ] Ensure prestige bonus calculations are correct
- [ ] Test sector scaling (difficulty increases per sector)
- [ ] Verify mission rewards scale appropriately

---

## 🔍 Debugging Guidelines

### Enable Debug Logging
```csharp
// Add conditional compilation for debug logs
#if UNITY_EDITOR
    Debug.Log($"Mining progress: {miningProgress}/{asteroid.Health}");
#endif

// Use Debug.DrawLine for visual debugging
Debug.DrawLine(transform.position, targetPosition, Color.red, 2f);

// Use Gizmos for persistent visualization
void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, miningRange);
}
```

### Common Issues & Solutions

**Issue:** Asteroids not spawning
```csharp
// Check:
1. Is spawner active? (GameObject enabled)
2. Is max asteroid count reached?
3. Is spawn chance being calculated correctly?
4. Are asteroid prefabs assigned in Inspector?
```

**Issue:** Mining not working
```csharp
// Check:
1. Is asteroid within miningRange?
2. Is player holding Space/mine button?
3. Does player have fuel?
4. Is asteroid layer mask correct?
```

**Issue:** Save/Load not working
```csharp
// Check:
1. Is SaveSystem.SaveGame() being called?
2. Is Application.persistentDataPath accessible?
3. Are all classes marked [Serializable]?
4. Are Dictionaries being serialized correctly? (May need custom serialization)
```

---

## 🎯 Specific Implementation Instructions

### When asked to implement player movement:
1. Reference `UNITY_IMPLEMENTATION_EXAMPLES.md` → PlayerController section
2. Use new Input System for all input (REQUIRED - never use old Input class)
3. Use Rigidbody for physics-based movement
4. Apply friction: `velocity *= 0.92f;` each frame
5. Consume fuel based on movement: `fuel -= baseFuelConsumption * fuelEfficiency * Time.deltaTime`
6. Track distance traveled: `stats.distanceTraveled += velocity.magnitude * Time.deltaTime`

### When asked to implement mining:
1. Reference `UNITY_IMPLEMENTATION_EXAMPLES.md` → MiningSystem section
2. Use new Input System for mining button/trigger (REQUIRED)
3. Support multi-mining (1-6 targets based on upgrade)
4. Use Physics.OverlapSphere to find asteroids in range
5. Create LineRenderer for each laser beam
6. Track progress per asteroid: `progress += miningRate * Time.deltaTime`
7. When complete: add to inventory, award credits, destroy asteroid

### When asked to implement upgrades:
1. Reference cost arrays from `FEATURE_LIST.md` → Upgrade System
2. Check if player is docked before allowing purchase
3. Apply effects immediately after purchase
4. For cargo/fuel: unlimited levels with exponential cost scaling
5. For multiMining: max level 6
6. For one-time purchases: disable button after purchase

### When asked to implement missions:
1. Generate missions per station (3-6 unique)
2. Use templates from `FEATURE_LIST.md` → Mission System
3. Scale rewards: `baseReward * sector * (1 + prestigeBonus)`
4. Track progress in real-time (check each Update or event)
5. Allow max 3 active missions
6. Require docking to claim rewards

### When asked to implement NPCs:
1. Reference personality traits from `FEATURE_LIST.md` → NPC Systems
2. Implement pathfinding to nearest/valuable asteroids
3. Apply personality modifiers (hazard avoidance, mining speed, cargo threshold)
4. Return to station when cargo threshold reached
5. Generate radio chatter based on context and personality
6. Avoid player ship and other NPCs

---

## 📚 Quick Reference Links

### Code Examples
- **GameState Management:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 1-100
- **Player Controller:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 150-250
- **Mining System:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 300-450
- **Asteroid Spawning:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 500-650
- **Upgrade System:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 700-850
- **Mission System:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 900-1050
- **Scanner System:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 1100-1250
- **Save System:** `UNITY_IMPLEMENTATION_EXAMPLES.md` lines 1300-1400

### Feature Specifications
- **All Asteroid Types:** `FEATURE_LIST.md` lines 60-200
- **All Upgrade Types:** `FEATURE_LIST.md` lines 240-400
- **All Mission Types:** `FEATURE_LIST.md` lines 450-550
- **All NPC Personalities:** `FEATURE_LIST.md` lines 600-750
- **Radio Chatter:** Original web code `asteroid-miner-script.js` lines 400-600

### Architecture
- **Project Structure:** `UNITY_CONVERSION_GUIDE.md` lines 150-300
- **Implementation Phases:** `UNITY_CONVERSION_GUIDE.md` lines 400-600
- **Technical Specs:** `UNITY_CONVERSION_GUIDE.md` lines 650-800

---

## 🎓 Learning Resources

### Unity Documentation
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
- [Job System](https://docs.unity3d.com/Manual/JobSystem.html)
- [Object Pooling](https://learn.unity.com/tutorial/introduction-to-object-pooling)

### C# Best Practices
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Unity Performance Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html)

---

## ✅ Final Checklist Before Committing Code

Before marking any task as complete:

- [ ] Code follows C# naming conventions
- [ ] All public methods have XML documentation comments
- [ ] No compiler warnings or errors
- [ ] Tested in Unity Editor (Play mode)
- [ ] Performance is acceptable (60+ FPS)
- [ ] Save/Load works correctly with changes
- [ ] UI updates when state changes
- [ ] Gamepad controls work (if applicable)
- [ ] No null reference exceptions
- [ ] Memory usage is reasonable (no leaks)
- [ ] Feature matches original web game behavior
- [ ] Code is modular and maintainable

---

## 🤝 Communication Guidelines

### When providing code:
- Always include context comments
- Reference original web code when relevant
- Explain Unity-specific choices
- Highlight performance considerations
- Note any deviations from original design

### When explaining features:
- Start with high-level overview
- Break down into implementation steps
- Provide code examples
- Link to relevant documentation files
- Anticipate edge cases

### When debugging:
- Ask for Unity version and platform
- Request relevant error messages
- Check Inspector settings
- Verify component references
- Test in isolation

---

**Remember:** This project is a faithful recreation of a complex web game. Always reference the original `WebAsteroidMiner` code and documentation files when uncertain. Maintain feature parity while leveraging Unity's strengths for performance and modularity.

**Development Philosophy:** "Make it work, make it right, make it fast" - in that order.

---

*Last Updated: November 6, 2025*  
*Document Version: 1.0*
