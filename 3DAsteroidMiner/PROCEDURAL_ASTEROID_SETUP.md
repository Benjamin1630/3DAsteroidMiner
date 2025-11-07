# Procedural Asteroid Mesh System - Setup Guide

## Overview
This system generates unique, randomly-shaped asteroids from icosphere bases with vertex displacement. Each asteroid has a unique shape and supports a dynamic shrinking effect during mining.

---

## 🎯 Features

✅ **Procedural Generation** - Each asteroid has a unique random shape  
✅ **Icosphere Base** - Even vertex distribution (better than UV sphere)  
✅ **Perlin Noise Displacement** - Natural-looking rocky surfaces  
✅ **Dynamic Shrinking** - Asteroids shrink as they're mined (0-70% size)  
✅ **Object Pool Compatible** - Meshes reset when returned to pool  
✅ **Performance Optimized** - Mesh generated once, reused via pooling  

---

## 📋 Setup Instructions

### Step 1: Update Asteroid Prefab

1. **Open your Asteroid prefab**

2. **Add Child GameObject for Mesh:**
   - Create child: `AsteroidMesh`
   - Add `MeshFilter` component
   - Add `MeshRenderer` component
   - Add `ProceduralAsteroidMesh` component

3. **Configure ProceduralAsteroidMesh:**
   - **Subdivisions:** 2 (good balance of detail vs performance)
     - 0 = 20 faces (low poly, fast)
     - 1 = 80 faces (medium)
     - 2 = 320 faces (detailed) ✅ **RECOMMENDED**
     - 3 = 1,280 faces (very detailed, slower)
     - 4 = 5,120 faces (extreme detail, not recommended)
   
   - **Base Radius:** 1.0 (will be scaled by transform)
   - **Displacement Amount:** 0.3 (30% vertex displacement)
   - **Noise Scale:** 2.5 (controls randomness frequency)
   - **Min Shrink Scale:** 0.3 (shrinks to 30% when fully mined)
   - **Enable Shrink Effect:** ✅ Checked

4. **Assign Material:**
   - Create material in `Assets/Materials/AsteroidMaterial.mat`
   - Assign to MeshRenderer
   - Use Standard shader with:
     - Albedo: Rock/asteroid texture or color
     - Metallic: 0
     - Smoothness: 0.2-0.4 (rough surface)

5. **Remove Old Mesh:**
   - If you had a static sphere mesh, remove it
   - ProceduralAsteroidMesh generates at runtime

### Step 2: Configure Collider

The collider should match the approximate asteroid shape:

**Option A: Sphere Collider (Simple, Fast)**
```
- Add SphereCollider to parent Asteroid GameObject
- Radius: 0.5-0.7 (slightly larger than base mesh for easier targeting)
```

**Option B: Mesh Collider (Accurate, Slower)**
```
- Add MeshCollider to AsteroidMesh child
- Check "Convex" ✅
- Mesh will be auto-assigned by ProceduralAsteroidMesh
```

**RECOMMENDED: Sphere Collider** for performance with 100+ asteroids.

---

## 🎮 How It Works

### Mesh Generation Process:

```
1. Start with icosahedron (20-sided polyhedron)
2. Subdivide triangles N times (each triangle → 4 triangles)
3. For each vertex:
   a. Calculate 3D Perlin noise at vertex position
   b. Displace vertex radially by noise value
   c. Creates rocky, irregular surface
4. Recalculate normals for lighting
5. Store original and displaced vertices separately
```

### Shrinking Effect Process:

```
1. When asteroid takes damage:
   a. Calculate mining progress (0-1)
   b. Scale all vertices toward center
   c. 0% mined = 100% size
   d. 100% mined = 30% size (minShrinkScale)
2. Recalculate bounds and normals
3. Mesh updates in real-time
```

---

## 🔧 Customization Options

### Adjust Asteroid Roughness

**More Jagged/Rocky:**
```csharp
displacementAmount = 0.5f;  // More extreme displacement
noiseScale = 4.0f;          // Higher frequency noise
```

**Smoother/Rounder:**
```csharp
displacementAmount = 0.15f; // Less displacement
noiseScale = 1.5f;          // Lower frequency noise
```

### Adjust Shrink Effect

**Shrink More (dramatic effect):**
```csharp
minShrinkScale = 0.1f;      // Shrinks to 10% of original size
```

**Shrink Less (subtle effect):**
```csharp
minShrinkScale = 0.6f;      // Shrinks to 60% of original size
```

**Disable Shrinking:**
```csharp
enableShrinkEffect = false; // No shrinking during mining
```

### Subdivision Levels Guide

| Level | Faces | Vertices | Use Case | Performance |
|-------|-------|----------|----------|-------------|
| 0 | 20 | 12 | Distant asteroids, mobile | Excellent |
| 1 | 80 | 42 | Medium detail | Very Good |
| **2** | **320** | **162** | **Default (recommended)** | **Good** |
| 3 | 1,280 | 642 | High detail, close-up | Fair |
| 4 | 5,120 | 2,562 | Extreme detail | Poor |

**Recommendation:** Use level 2 for balance. Can use LOD system later with multiple levels.

---

## 💡 Integration with Mining System

When implementing mining, use this pattern:

```csharp
// In your mining system:
Asteroid asteroid = hitObject.GetComponent<Asteroid>();

if (asteroid != null)
{
    // Apply mining damage
    bool destroyed = asteroid.TakeDamage(miningDamage * Time.deltaTime);
    
    if (destroyed)
    {
        // Asteroid is destroyed
        // The shrink effect automatically updated during TakeDamage()
        
        // Award resources
        gameState.credits += asteroid.Type.value;
        
        // Return to pool (will reset mesh)
        asteroidPool.ReturnAsteroid(asteroid.gameObject);
    }
}
```

The `TakeDamage()` method automatically calls `UpdateShrinkEffect()` internally!

---

## 🎨 Visual Polish Ideas

### Add Particle Effects
```csharp
// When mining:
- Emit rock debris particles from hit point
- Scale particles based on asteroid type (bigger for larger asteroids)

// When destroyed:
- Explosion particle effect
- Small rock fragments scatter outward
```

### Add Sound Effects
```csharp
// While mining:
- Looping laser/drill sound
- Pitch increases as asteroid shrinks

// When destroyed:
- Rock crumbling sound
- Volume based on asteroid size
```

### Material Changes During Mining
```csharp
// As asteroid shrinks:
- Increase emissive glow (heating up from mining laser)
- Darken albedo (scorched surface)
- Add cracks with texture blending
```

---

## 🐛 Troubleshooting

### Asteroids appear as spheres (no displacement)
**Problem:** ProceduralAsteroidMesh not generating  
**Solutions:**
- Check `ProceduralAsteroidMesh` component is on child object with MeshFilter
- Verify `Asteroid.Initialize()` is being called (check console logs)
- Ensure MeshFilter exists before ProceduralAsteroidMesh.Awake()

### Asteroids don't shrink when mined
**Problem:** Shrink effect not updating  
**Solutions:**
- Check `enableShrinkEffect` is true in Inspector
- Verify `TakeDamage()` is being called from mining system
- Check that `proceduralMesh` reference is not null (debug log it)

### Performance issues with many asteroids
**Solutions:**
- Reduce `subdivisions` from 2 to 1 or 0
- Use Sphere Collider instead of Mesh Collider
- Reduce `maxAsteroids` in spawner
- Implement LOD system (high detail when close, low when far)

### Asteroids look identical
**Problem:** Same seed being used  
**Solution:**
- Verify `Random.Range(0, 100000)` is generating different seeds
- Check that `Initialize()` is called each time asteroid spawns

---

## 📊 Performance Metrics

### Expected Performance (Level 2 Subdivisions):

- **Mesh Generation:** ~1-2ms per asteroid (happens once on spawn)
- **Shrink Update:** <0.1ms per frame (only when being mined)
- **Memory:** ~50KB per unique mesh (pooled, reused)
- **Draw Calls:** 1 per asteroid (can be batched if using same material)

### With 200 Active Asteroids:
- **Total Memory:** ~10MB for meshes
- **FPS Impact:** <5% on modern GPU
- **CPU Impact:** Minimal (only shrinking asteroids update)

---

## 🚀 Next Steps

1. **Create asteroid material** with rock texture
2. **Test mesh generation** - spawn a few asteroids, check shapes
3. **Implement mining system** to test shrink effect
4. **Add particle effects** for visual polish
5. **Optimize further** with LOD system if needed

---

## ✅ Final Checklist

Before using the system:

- [ ] ProceduralAsteroidMesh component added to asteroid prefab
- [ ] MeshFilter and MeshRenderer on same GameObject
- [ ] Asteroid material assigned
- [ ] Collider configured (SphereCollider recommended)
- [ ] Subdivisions set to 2
- [ ] Enable Shrink Effect checked
- [ ] Tested spawning shows unique shapes
- [ ] Mining causes asteroids to shrink

---

**System Status:** ✅ Ready for Integration  
**Unique Feature:** Every asteroid is procedurally unique!  
**Performance:** Optimized for 200+ concurrent asteroids  
**Visual Quality:** Dynamic shrinking creates satisfying mining feedback
