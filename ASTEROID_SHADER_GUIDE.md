# Asteroid Hybrid Shader Documentation

## Overview
The **AsteroidHybridShader** is a custom Unity shader that combines semi-realistic and cel-shaded rendering techniques to create visually striking asteroids with procedurally generated "swiss cheese" holes that reveal the asteroid's type color.

## Features

### 1. **Hybrid Rendering Style**
- Semi-realistic lighting with normal mapping
- Cel-shaded lighting bands for stylized look
- Smooth blend between realistic and cartoon aesthetics

### 2. **Procedural Swiss Cheese Holes**
- Voronoi-based hole pattern generation
- 3D noise for natural variation
- Configurable density, size, and depth
- Sharp or smooth hole edges

### 3. **Type Color Reveal System**
- Each asteroid type has a unique color
- Color is revealed inside the procedural holes
- Optional emission/glow effect in holes
- Creates visual distinction between asteroid types

### 4. **Advanced Lighting**
- Custom cel-shaded lighting model
- Rim lighting for edge highlights
- Ambient color support
- Metallic and smoothness properties

## Shader Properties

### Base Asteroid Texture
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Asteroid Texture** | Texture2D | Grey | Main texture for the asteroid surface |
| **Base Asteroid Color** | Color | (0.4, 0.4, 0.4, 1) | Tint color for the base asteroid |
| **Normal Map** | Texture2D | Bump | Normal map for surface detail |
| **Normal Strength** | Float (0-2) | 1.0 | Intensity of normal mapping |

### Asteroid Type Color (Revealed in Holes)
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Type Color** | Color | (1, 0.5, 0, 1) | The color representing this asteroid type |
| **Type Color Intensity** | Float (0-3) | 1.5 | Brightness multiplier for type color |
| **Type Color Emission** | Float (0-2) | 0.3 | Glow/emission from holes |

### Swiss Cheese Holes
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Hole Density** | Float (0-20) | 8.0 | Number of holes per unit area |
| **Hole Size** | Float (0-1) | 0.35 | Size of individual holes |
| **Hole Depth** | Float (0-0.5) | 0.15 | Visual depth of holes (affects normals) |
| **Hole Edge Sharpness** | Float (1-10) | 3.0 | How sharp the hole edges are |
| **Hole Noise Scale** | Float (1-50) | 15.0 | Scale of noise overlay on holes |

### Cel Shading
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Cel Shading Bands** | Float (2-10) | 4 | Number of discrete lighting levels |
| **Cel Band Smoothness** | Float (0-0.5) | 0.05 | Smoothness of band transitions |

### Lighting
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Smoothness** | Float (0-1) | 0.2 | Surface smoothness for specular |
| **Metallic** | Float (0-1) | 0.1 | Metallic property |
| **Rim Light Color** | Color | (0.3, 0.3, 0.4, 1) | Color of rim lighting |
| **Rim Power** | Float (0.1-8) | 3.0 | Falloff of rim lighting |
| **Rim Intensity** | Float (0-2) | 0.5 | Strength of rim lighting |

### Ambient
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Ambient Color** | Color | (0.1, 0.1, 0.15, 1) | Ambient lighting color |
| **Ambient Intensity** | Float (0-1) | 0.3 | Strength of ambient lighting |

## Usage Guide

### Step 1: Create Materials for Each Asteroid Type

1. In Unity, navigate to `Assets/Materials/Asteroids/`
2. Create a new material for each asteroid type (e.g., "IronOreMaterial", "GoldMaterial")
3. Set the shader to **Custom/AsteroidHybridShader**

### Step 2: Configure Type Colors

Set the **Type Color** property for each asteroid type based on the original game:

```csharp
// Common Asteroids (grey/brown base)
Iron Ore:     RGB(120, 120, 130)  - Blueish-grey
Copper:       RGB(184, 115, 51)   - Copper orange

// Uncommon (metallic)
Nickel:       RGB(200, 200, 180)  - Silvery
Silver:       RGB(192, 192, 192)  - Pure silver
Titanium:     RGB(150, 150, 160)  - Dark metallic

// Rare (vibrant)
Gold:         RGB(255, 215, 0)    - Bright gold
Emerald:      RGB(0, 201, 87)     - Emerald green
Platinum:     RGB(229, 228, 226)  - Platinum white

// Epic (saturated)
Ruby:         RGB(224, 17, 95)    - Deep red
Sapphire:     RGB(15, 82, 186)    - Deep blue
Obsidian:     RGB(60, 50, 70)     - Dark purple

// Legendary (glowing)
Quantum Crystal: RGB(150, 50, 255) - Purple glow
Nebulite:        RGB(0, 255, 200)  - Cyan glow
Dark Matter:     RGB(100, 0, 150)  - Dark purple
```

### Step 3: Adjust Visual Style

#### For More Realistic Look:
- Increase **Cel Shading Bands** to 8-10
- Increase **Cel Band Smoothness** to 0.3-0.5
- Lower **Hole Edge Sharpness** to 1-2
- Reduce **Type Color Intensity** to 1.0

#### For More Stylized/Cartoony Look:
- Decrease **Cel Shading Bands** to 3-4
- Decrease **Cel Band Smoothness** to 0.01-0.05
- Increase **Hole Edge Sharpness** to 5-10
- Increase **Type Color Intensity** to 2-3

#### For Subtle Holes:
- Decrease **Hole Density** to 3-5
- Decrease **Hole Size** to 0.2-0.3
- Lower **Type Color Emission** to 0.1

#### For Prominent Holes:
- Increase **Hole Density** to 10-15
- Increase **Hole Size** to 0.4-0.6
- Increase **Type Color Emission** to 0.5-1.0

### Step 4: Add Textures (Optional)

For enhanced realism:

1. **Asteroid Texture**: Use a rocky/stone texture (grey or brown)
   - Recommended resolution: 512x512 or 1024x1024
   - Should be relatively uniform in color
   - Can have subtle variations and cracks

2. **Normal Map**: Use a rock normal map
   - Adds surface detail without extra geometry
   - Enhances the 3D appearance
   - Combine with procedural holes for best effect

### Step 5: Integration with Asteroid System

Update your asteroid spawning code to assign materials based on type:

```csharp
// Example: AsteroidSpawner.cs
public class AsteroidSpawner : MonoBehaviour
{
    [System.Serializable]
    public class AsteroidMaterialSet
    {
        public string asteroidType;
        public Material material;
    }
    
    [SerializeField] private AsteroidMaterialSet[] asteroidMaterials;
    [SerializeField] private GameObject asteroidPrefab;
    
    private void SpawnAsteroid(string asteroidType)
    {
        GameObject asteroid = Instantiate(asteroidPrefab);
        
        // Find matching material
        var materialSet = System.Array.Find(asteroidMaterials, 
            m => m.asteroidType == asteroidType);
        
        if (materialSet != null)
        {
            asteroid.GetComponent<Renderer>().material = materialSet.material;
        }
    }
}
```

## Technical Details

### Performance Considerations

- **Surface Shader**: Uses Unity's surface shader system for compatibility
- **LOD Support**: Includes LOD 200 tag for level-of-detail systems
- **Target 3.0**: Requires shader model 3.0 (most modern platforms)
- **Real-time Calculation**: Holes are calculated per-fragment

**Optimization Tips:**
- Use object pooling for asteroid instances
- Consider LOD models for distant asteroids
- Reduce **Hole Noise Scale** if performance is critical
- Use shared materials when possible (same type = same material instance)

### Procedural Hole Generation

The shader uses two techniques for hole generation:

1. **Voronoi Pattern**: Creates cellular/organic hole distribution
   - Each "cell" has a random center point
   - Distance to nearest point determines hole boundaries
   - Controlled by **Hole Density**

2. **3D Noise**: Adds natural variation
   - Perlin-style noise function
   - Breaks up regular patterns
   - Controlled by **Hole Noise Scale**

### Custom Lighting Model

The `LightingCelShaded` function:
- Calculates standard N·L (normal dot light)
- Quantizes to discrete bands
- Smooths band transitions
- Adds cel-shaded specular highlights
- Maintains directional lighting

## Troubleshooting

### Issue: Holes not visible
**Solution:** 
- Increase **Hole Size** (try 0.5)
- Decrease **Hole Edge Sharpness** (try 2.0)
- Ensure **Type Color** is different from **Base Color**

### Issue: Asteroids too dark
**Solution:**
- Increase **Ambient Intensity**
- Increase **Type Color Intensity**
- Check scene lighting setup
- Increase **Rim Intensity**

### Issue: Type colors too subtle
**Solution:**
- Increase **Type Color Intensity** to 2-3
- Increase **Type Color Emission** to 0.5-1.0
- Decrease **Base Asteroid Color** alpha/brightness

### Issue: Edges look jagged
**Solution:**
- Increase mesh subdivision on asteroid models
- Enable anti-aliasing in project settings
- Slightly increase **Cel Band Smoothness**
- Use higher resolution normal maps

### Issue: Performance problems
**Solution:**
- Reduce **Hole Density**
- Reduce **Hole Noise Scale**
- Use simpler asteroid meshes
- Implement LOD system
- Reduce number of active asteroids

## Advanced Customization

### Creating Animated Holes

Modify the shader to animate holes over time:

```hlsl
// In surf() function, add time to worldPos:
float holeMask = calculateHoles(IN.worldPos + _Time.y * 0.1);
```

### Per-Asteroid Randomization

Use vertex colors to add variation:

```hlsl
// In Input struct, add:
float4 color : COLOR;

// In surf(), multiply hole parameters:
float randomizedDensity = _HoleDensity * IN.color.r;
```

### Multi-Colored Holes

Support multiple type colors:

```hlsl
// Add properties:
_TypeColor2 ("Type Color 2", Color) = (1, 0, 0, 1)

// In surf(), alternate colors:
fixed4 typeColor = lerp(_TypeColor, _TypeColor2, holeMask);
```

## Example Material Presets

### Iron Ore (Common)
- Base Color: (0.35, 0.35, 0.35, 1)
- Type Color: (0.47, 0.47, 0.51, 1)
- Hole Size: 0.3
- Hole Density: 6
- Type Color Emission: 0.1

### Gold (Rare)
- Base Color: (0.4, 0.4, 0.4, 1)
- Type Color: (1, 0.84, 0, 1)
- Hole Size: 0.35
- Hole Density: 8
- Type Color Emission: 0.4

### Quantum Crystal (Legendary)
- Base Color: (0.3, 0.3, 0.3, 1)
- Type Color: (0.59, 0.20, 1, 1)
- Hole Size: 0.4
- Hole Density: 10
- Type Color Emission: 0.8

## Related Files

- Shader: `Assets/Shaders/AsteroidHybridShader.shader`
- Materials: `Assets/Materials/Asteroids/`
- Prefabs: `Assets/Prefabs/Asteroids/`

## Version History

- **v1.0** (Nov 7, 2025): Initial creation
  - Hybrid cel-shading + realistic rendering
  - Procedural swiss cheese holes
  - Type color reveal system
  - Custom lighting model

---

*For questions or suggestions, see the main project documentation.*
