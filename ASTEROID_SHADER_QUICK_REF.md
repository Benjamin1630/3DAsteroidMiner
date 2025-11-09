# 🎨 Asteroid Shader Quick Reference Card

## 📋 One-Page Setup & Reference

---

## ⚡ 60-Second Setup

```
1. Tools > Asteroid Miner > Create All Asteroid Visual Presets
2. Tools > Asteroid Miner > Create Material Template  
3. Add AsteroidVisualController to asteroid prefab
4. Assign material to prefab's MeshRenderer
5. Done!
```

---

## 💻 Code Snippet (Copy & Use)

```csharp
// In your asteroid spawner:
AsteroidVisualController visual = asteroid.GetComponent<AsteroidVisualController>();
visual.SetAsteroidType(asteroidTypeData); // Assign type
visual.RandomizeHolePattern(0.15f);       // Add variation
```

---

## 🎨 16 Asteroid Colors (RGB Hex)

| Type | Color | Hex | Rarity |
|------|-------|-----|--------|
| Iron Ore | Blue-grey | #787882 | Common |
| Copper | Orange | #B87333 | Common |
| Nickel | Silver | #C8C8B4 | Uncommon |
| Silver | Pure silver | #C0C0C0 | Uncommon |
| Titanium | Steel | #9696A0 | Uncommon |
| Gold | Gold | #FFD700 | Rare |
| Emerald | Green | #00C957 | Rare |
| Platinum | White | #E5E4E2 | Rare |
| Ruby | Red | #E0115F | Epic |
| Sapphire | Blue | #0F52BA | Epic |
| Obsidian | Purple | #3C3246 | Epic |
| Quantum Crystal | Purple | #9632FF | Legendary |
| Nebulite | Cyan | #00FFC8 | Legendary |
| Dark Matter | Purple | #640096 | Legendary |

---

## 🔧 Quick Material Tweaks

### Bigger Holes
```
Hole Size: 0.45
Type Color Intensity: 2.0
Type Color Emission: 0.6
```

### More Realistic
```
Cel Bands: 8
Cel Smoothness: 0.3
Hole Edge Sharpness: 2.0
```

### More Cartoony
```
Cel Bands: 3
Cel Smoothness: 0.01
Hole Edge Sharpness: 8.0
```

### Performance Mode
```
Hole Density: 5
Hole Noise Scale: 10
Normal Strength: 0.5
```

---

## 🐛 Quick Fixes

| Problem | Solution |
|---------|----------|
| Asteroids black | Add Directional Light, increase Ambient Intensity |
| No holes visible | Increase Hole Size to 0.5, check Type Color |
| Colors too similar | Increase Type Color Intensity to 2.5 |
| Performance issues | Reduce Hole Density to 5-6 |
| Shader not found | Check Assets/Shaders/, wait for compile |

---

## 📊 Key Properties

### Most Important
- **Type Color** - The asteroid's mineral color
- **Hole Size** - How big the holes are (0.35 default)
- **Hole Density** - How many holes (8 default)
- **Type Color Emission** - Hole glow (0.3 default)

### For Style
- **Cel Bands** - Lighting steps (4 default)
- **Cel Smoothness** - Band softness (0.05 default)
- **Hole Edge Sharpness** - Hole edges (3 default)

### For Polish
- **Rim Intensity** - Edge highlight (0.5 default)
- **Normal Strength** - Surface detail (1.0 default)
- **Ambient Intensity** - Overall brightness (0.3 default)

---

## 📁 File Locations

```
Shader:     Assets/Shaders/AsteroidHybridShader.shader
Controller: Assets/Scripts/AsteroidVisualController.cs
Data:       Assets/Scripts/AsteroidTypeVisualData.cs
Tools:      Assets/Scripts/Editor/AsteroidVisualPresetCreator.cs
Example:    Assets/Scripts/Examples/AsteroidSpawnerWithVisuals.cs
```

---

## 📚 Documentation

- **Quick Setup** → ASTEROID_SHADER_SETUP.md (10 min)
- **Full Reference** → ASTEROID_SHADER_GUIDE.md (complete)
- **Style Guide** → ASTEROID_VISUAL_REFERENCE.md (colors)
- **Overview** → README_ASTEROID_VISUALS.md (summary)

---

## 🎯 Rarity Visual Scale

```
Common      → Few small holes, minimal glow
Uncommon    → Moderate holes, low glow
Rare        → Many holes, noticeable glow
Epic        → Lots of holes, strong glow  
Legendary   → Maximum holes, very strong glow
```

---

## 🚀 Integration Checklist

- [ ] Run both Tools menu commands
- [ ] Add AsteroidVisualController to prefab
- [ ] Assign material to prefab
- [ ] Update spawner code
- [ ] Map 16 type data assets in Inspector
- [ ] Test all asteroid types
- [ ] Adjust material properties
- [ ] Verify performance (60 FPS)

---

## 💡 Pro Tips

1. **Variation**: Always call `RandomizeHolePattern()` for uniqueness
2. **Performance**: Use MaterialPropertyBlock (built-in)
3. **Testing**: Use context menu "Spawn Test Asteroids"
4. **Tweaking**: Adjust in Play Mode, copy values when happy
5. **Sharing**: All asteroids can share one material

---

## ⚙️ Default Values (If Unsure)

```
Base Color: (0.4, 0.4, 0.4)
Type Color Intensity: 1.5
Type Color Emission: 0.3
Hole Density: 8.0
Hole Size: 0.35
Hole Edge Sharpness: 3.0
Cel Bands: 4
Cel Smoothness: 0.05
Rim Intensity: 0.5
```

---

## 🎮 Runtime API

```csharp
// Set full type configuration
visual.SetAsteroidType(typeData);

// Quick color change
visual.SetTypeColor(Color.red, intensity, emission);

// Add variation (0.0-1.0)
visual.RandomizeHolePattern(0.2f);

// Adjust holes
visual.SetHoleProperties(density, size, sharpness);

// Change base color
visual.SetBaseColor(Color.grey);
```

---

## 📐 Material Property Ranges

| Property | Min | Max | Default | Use |
|----------|-----|-----|---------|-----|
| Hole Density | 3 | 15 | 8 | More/less holes |
| Hole Size | 0.1 | 0.8 | 0.35 | Bigger/smaller |
| Type Color Intensity | 0 | 3 | 1.5 | Brighter color |
| Type Color Emission | 0 | 2 | 0.3 | Glow strength |
| Cel Bands | 2 | 10 | 4 | More/less steps |
| Cel Smoothness | 0 | 0.5 | 0.05 | Softer bands |
| Hole Edge Sharpness | 1 | 10 | 3 | Sharp/soft edges |

---

## 🏆 Quality Checklist

✅ All 16 types have distinct colors  
✅ Holes visible from gameplay distance  
✅ Cel-shading looks good (not harsh)  
✅ Rim lighting visible in dark space  
✅ Performance is 60+ FPS  
✅ No identical-looking asteroids  
✅ Colors match reference palette  
✅ Easy to identify types quickly  

---

## 🆘 Support

**Issue?** Check ASTEROID_SHADER_GUIDE.md troubleshooting section  
**Customizing?** See ASTEROID_VISUAL_REFERENCE.md for presets  
**Integrating?** Study AsteroidSpawnerWithVisuals.cs example  

---

*Keep this card handy during development! 📌*

**Version 1.0** | November 7, 2025 | Production Ready ✅
