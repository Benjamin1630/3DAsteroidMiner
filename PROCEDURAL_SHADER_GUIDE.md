# Procedural Asteroid Shader Guide

## Overview
The asteroid shader has been completely redesigned to generate all surface details **procedurally** without needing UV mapping or textures. This provides better performance, eliminates texture stretching, and gives complete artistic control through shader parameters.

## Features

### 1. **Swiss Cheese Holes** (Existing Feature - Enhanced)
- Deep holes that reveal the asteroid's mineral type color
- Rotate with the asteroid mesh (object-space calculation)
- Glowing emission effect inside holes
- Configurable density, size, depth, and edge sharpness

### 2. **Procedural Craters** (NEW)
- Impact craters with depth and raised rims
- Natural bowl shape with ambient occlusion
- Rim highlights for realistic lighting
- Configurable density, size, depth, and rim height
- Normals calculated from crater gradient for proper lighting

### 3. **Procedural Rock Piles** (NEW)
- Raised rock formations using fractal Brownian motion (FBM)
- Sharp edges for rocky appearance
- Fine detail layer for micro-surface variation
- Configurable scale, height, and sharpness
- Smooth color variation across rock surfaces

### 4. **Parallax Occlusion Mapping** (NEW)
- Creates convincing depth illusion
- Ray-marches through height field
- Makes craters and rock piles appear 3D
- Configurable strength and quality (step count)
- View-angle dependent depth effect

### 5. **Surface Detail Noise**
- Fine-scale surface roughness
- Multi-octave fractal noise
- Breaks up repetitive patterns
- Configurable scale and strength

## Shader Parameters

### Base Surface
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Base Asteroid Color** | Color | (0.35, 0.35, 0.38) | Base gray rock color |
| **Base Color Variation** | 0-1 | 0.3 | Amount of procedural color variation |
| **Surface Roughness** | 0-1 | 0.85 | How rough/matte the surface is |

### Swiss Cheese Holes
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Hole Density** | 0-20 | 5.0 | Number of holes per unit area |
| **Hole Size** | 0-1 | 0.5 | Size of each hole |
| **Hole Depth** | 0-1 | 0.4 | Visual depth of holes |
| **Hole Edge Sharpness** | 1-10 | 3.0 | How sharp hole edges are |
| **Hole Noise Scale** | 1-50 | 15.0 | Variation in hole shapes |

### Procedural Craters
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Crater Density** | 0-20 | 6.0 | Number of craters per unit area |
| **Crater Size** | 0-1 | 0.45 | Size of crater bowls |
| **Crater Depth** | 0-1 | 0.3 | Depth of crater depression |
| **Crater Rim Height** | 0-1 | 0.15 | Height of raised crater rim |
| **Crater Rim Sharpness** | 1-20 | 8.0 | How pronounced crater rims are |

### Procedural Rock Piles
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Rock Pile Scale** | 1-50 | 12.0 | Size of rock formations |
| **Rock Pile Height** | 0-1 | 0.25 | How tall rock piles are |
| **Rock Pile Sharpness** | 0.5-5 | 2.0 | How angular rocks are |
| **Rock Detail Scale** | 10-100 | 35.0 | Size of fine surface detail |
| **Rock Detail Strength** | 0-0.5 | 0.15 | Intensity of fine detail |

### Surface Detail
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Detail Noise Scale** | 10-100 | 45.0 | Size of micro-surface detail |
| **Detail Noise Strength** | 0-0.5 | 0.2 | Intensity of micro-detail |
| **Overall Normal Strength** | 0-3 | 1.5 | Overall bumpiness multiplier |

### Parallax Depth
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Parallax Strength** | 0-0.1 | 0.03 | How deep the parallax effect is |
| **Parallax Steps** | 4-32 | 16 | Quality of depth (higher=better) |

### Lighting (Unchanged)
| Parameter | Range | Default | Description |
|-----------|-------|---------|-------------|
| **Cel Bands** | 2-10 | 3 | Number of cel-shading bands |
| **Cel Smoothness** | 0-0.5 | 0.15 | Softness of cel bands |
| **Rim Power** | 0.1-8 | 3.0 | Rim light falloff |
| **Rim Intensity** | 0-2 | 0.5 | Rim light strength |

## Technical Details

### Object-Space Coordinates
All procedural features use **object-space coordinates** (`input.positionOS`) instead of world-space or UV coordinates. This means:
- ✅ Features rotate perfectly with the asteroid
- ✅ No UV unwrapping needed
- ✅ No texture stretching issues
- ✅ Consistent detail density across surface
- ✅ Works perfectly with procedural meshes

### Noise Functions
The shader implements several noise algorithms:

1. **Hash Function**: Pseudo-random number generation from 3D position
2. **3D Perlin Noise**: Smooth gradient noise
3. **Fractal Brownian Motion (FBM)**: Multi-octave layered noise for natural patterns
4. **Voronoi Cells**: For hole and crater placement

### Normal Calculation
Surface normals are built procedurally by:
1. Starting with mesh vertex normals
2. Adding crater bowl normals (inward depression)
3. Adding crater rim normals (outward bulge)
4. Adding rock pile normals (based on height gradient)
5. Adding fine detail from surface noise
6. Transforming to world space for lighting
7. Blending with original normals based on strength

### Performance Considerations

**Parallax Quality vs Performance:**
- **Low-end**: `Parallax Steps = 8`, `Parallax Strength = 0.02`
- **Mid-range**: `Parallax Steps = 16`, `Parallax Strength = 0.03` (default)
- **High-end**: `Parallax Steps = 24`, `Parallax Strength = 0.04`

**Optimization Tips:**
- Reduce `Parallax Steps` for mobile/low-end hardware
- Lower `Detail Noise Scale` reduces noise calculations
- Disable parallax (`Parallax Strength = 0`) for distant asteroids
- Use LOD system to reduce subdivisions on distant asteroids

## Usage Instructions

### Setting Up Materials

1. **Create New Material:**
   - Right-click in Project → Create → Material
   - Name it `AsteroidMaterial_Procedural`
   - Set Shader to `Custom/AsteroidHybridShader_URP`

2. **Configure Base Appearance:**
   - Set **Base Asteroid Color** to desired rock color
   - Adjust **Base Color Variation** for color diversity
   - Set **Surface Roughness** high (0.8-0.9) for rocky look

3. **Configure Craters:**
   - Increase **Crater Density** for heavily impacted look
   - Balance **Crater Depth** and **Crater Rim Height**
   - Higher **Crater Rim Sharpness** = more defined impacts

4. **Configure Rock Piles:**
   - Lower **Rock Pile Scale** = more frequent, smaller rocks
   - Higher **Rock Pile Sharpness** = more angular/chunky
   - Adjust **Rock Pile Height** for prominence

5. **Fine-Tune Details:**
   - Adjust **Detail Noise Scale** for micro-bumps
   - Set **Overall Normal Strength** to control bumpiness
   - Tweak **Parallax Strength** for depth effect

6. **Configure Holes:**
   - **Hole Density** controls how "swiss-cheese" it looks
   - **Hole Size** determines hole diameter
   - **Hole Depth** affects visual depth and emission

### Per-Type Color Configuration

The shader still supports **per-asteroid-type colors** via `MaterialPropertyBlock`:

```csharp
// In AsteroidVisualController.cs
public void SetAsteroidType(AsteroidTypeVisualData data)
{
    propertyBlock.SetColor(typeColorID, data.typeColor);
    propertyBlock.SetFloat(typeColorIntensityID, data.typeColorIntensity);
    propertyBlock.SetFloat(typeColorEmissionID, data.typeColorEmission);
    renderer.SetPropertyBlock(propertyBlock);
}
```

This allows:
- **One shared material** for all asteroids (performance)
- **Unique colors** per asteroid type (Gold=yellow, Iron=gray, etc.)
- **Type colors revealed in holes** for visual feedback

### Asteroid Prefab Setup

1. **Remove Old UV Dependencies:**
   - `ProceduralAsteroidMesh` component no longer has UV mapping options
   - No need to assign base textures to materials
   - Normal maps are not used (all generated procedurally)

2. **Assign Material:**
   - Set `ProceduralAsteroidMesh.defaultAsteroidMaterial` to your procedural material
   - Set `AsteroidVisualController.asteroidMaterial` to the same material

3. **Configure Type Data:**
   - Create `AsteroidTypeVisualData` assets for each type
   - Set unique `typeColor` for each (Gold, Silver, Iron, etc.)
   - Adjust `typeColorIntensity` (1.0-2.0 typical)
   - Set `typeColorEmission` (0.2-0.5 for subtle glow)

## Visual Styles

### Style Presets

**1. Heavily Cratered (Moon-like)**
```
Crater Density: 8.0
Crater Size: 0.5
Crater Depth: 0.4
Crater Rim Height: 0.2
Rock Pile Height: 0.1
```

**2. Rocky/Chunky**
```
Crater Density: 3.0
Crater Depth: 0.2
Rock Pile Scale: 8.0
Rock Pile Height: 0.4
Rock Pile Sharpness: 3.0
```

**3. Smooth with Detail**
```
Crater Density: 2.0
Crater Depth: 0.15
Rock Pile Height: 0.1
Detail Noise Strength: 0.3
Overall Normal Strength: 1.0
```

**4. Heavily Damaged (Lots of Holes)**
```
Hole Density: 8.0
Hole Size: 0.6
Hole Depth: 0.5
Crater Density: 6.0
```

### Artistic Guidelines

**For Small Asteroids (<1m):**
- Lower detail scales (higher numbers = smaller features)
- Reduce parallax strength (less noticeable at small size)
- Fewer, larger features

**For Large Asteroids (>3m):**
- Higher detail scales for visible micro-detail
- Increase parallax strength for depth
- More, smaller features for realistic scale

**For Different Mineral Types:**
- **Metallic** (Gold, Platinum): Lower roughness (0.3-0.5), subtle rim light
- **Rocky** (Iron, Copper): High roughness (0.8-0.9), strong detail
- **Crystalline** (Emerald, Ruby): Lower roughness (0.4-0.6), higher emission

## Troubleshooting

### Problem: Asteroids Look Too Flat
**Solution:** Increase `Parallax Strength` and `Overall Normal Strength`

### Problem: Asteroids Too Noisy/Busy
**Solution:** Reduce `Detail Noise Strength` and lower `Crater Density`

### Problem: Holes Not Visible
**Solution:** Increase `Hole Size` and ensure `Type Color` contrasts with `Base Color`

### Problem: Performance Issues
**Solution:** 
- Reduce `Parallax Steps` (16 → 8)
- Lower mesh `subdivisions` in `ProceduralAsteroidMesh`
- Simplify far asteroids with LOD

### Problem: Craters Look Fake
**Solution:**
- Balance `Crater Depth` with `Crater Rim Height`
- Increase `Crater Rim Sharpness` for defined edges
- Add variety by slightly randomizing crater parameters

### Problem: Color Too Uniform
**Solution:** Increase `Base Color Variation` (0.3-0.5)

## Migration from Old Shader

### What Changed
- ❌ **Removed**: UV mapping (spherical/triplanar)
- ❌ **Removed**: Texture sampling (`_MainTex`, `_NormalMap`)
- ✅ **Added**: Procedural craters
- ✅ **Added**: Procedural rock piles
- ✅ **Added**: Parallax occlusion mapping
- ✅ **Added**: Advanced procedural normal generation
- ✅ **Kept**: Swiss cheese holes
- ✅ **Kept**: Cel-shading
- ✅ **Kept**: Per-type colors via MaterialPropertyBlock

### Migration Steps
1. **Update Shader**: Material automatically updates when shader changes
2. **Remove Textures**: No longer needed (can delete from material slots)
3. **Adjust Parameters**: Start with defaults, then fine-tune
4. **Test Performance**: Should be equal or better (no texture sampling)
5. **Update Documentation**: Remove references to UV mapping

### Code Changes
- `ProceduralAsteroidMesh.cs`: Removed all UV generation methods
- `AsteroidVisualController.cs`: No changes needed (MaterialPropertyBlock still works)
- `Asteroid.cs`: No changes needed

## Performance Profile

**Shader Complexity:**
- Vertex Shader: Simple (just transforms)
- Fragment Shader: Medium-High (multiple noise calls + parallax)
- Estimated Cost: ~150-200 ALU instructions per fragment

**Compared to Texture-Based:**
- ✅ **Pros**: No texture memory, no UV unwrapping, perfect detail at any scale
- ⚠️ **Cons**: More fragment shader work (offset by no texture fetches)
- 📊 **Net Performance**: Similar or slightly better on modern GPUs

**Optimization Opportunities:**
- Use shader LOD (different quality for distance)
- Disable parallax for far objects
- Reduce noise octaves for distant asteroids
- Use compute shaders for noise precalculation (advanced)

---

**Last Updated:** November 8, 2025  
**Shader Version:** 2.0 (Fully Procedural)  
**Compatible With:** Unity 2022.3+ URP
