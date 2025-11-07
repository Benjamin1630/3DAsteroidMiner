# Asteroid Visual Style Reference

This document describes the expected visual appearance of asteroids using the AsteroidHybridShader.

## Overall Aesthetic

### Core Concept
- **Base Asteroid**: Realistic grey/brown rocky surface (looks like actual space rock)
- **Type Identifier**: Colored "holes" (swiss cheese pattern) revealing the mineral type
- **Lighting Style**: Semi-realistic with cel-shaded bands (hybrid approach)
- **Visual Goal**: Functional clarity (easy to identify types) + aesthetic appeal

---

## Visual Breakdown

### 1. Rocky Exterior (Base Color)
```
Appearance: Uniform grey-brown asteroid surface
Purpose: Makes all asteroids look similar from a distance
Color Range: (0.3 - 0.4, 0.3 - 0.4, 0.3 - 0.4) RGB
Texture: Optional rock texture for added detail
Normal Map: Recommended for surface bumps/cracks
```

**Visual Description:**
- Matte, rocky appearance
- Looks like real asteroid material
- Slightly rough surface (if normal map used)
- Subtle variations in brightness from lighting

### 2. Type Color Holes (Mineral Reveal)
```
Appearance: Procedural "holes" revealing glowing mineral color
Purpose: Visual identification of asteroid type
Pattern: Irregular, organic swiss-cheese pattern
Depth: Appears to be carved into the surface
Glow: Slight emission for visibility
```

**Visual Description:**
- Scattered across the asteroid surface
- Sharp or smooth edges (configurable)
- Slightly recessed (creates depth illusion)
- Emits soft glow in hole color
- Varies in size (mix of small and medium holes)

### 3. Cel-Shaded Lighting
```
Appearance: Stepped lighting bands (3-5 levels)
Purpose: Stylized look, clear depth perception
Style: Anime/toon shader meets realistic lighting
Smoothness: Subtle transitions between bands
```

**Visual Description:**
- Visible "steps" in lighting (not smooth gradient)
- 3-4 discrete brightness levels
- Slightly smoothed transitions (not harsh)
- Maintains directionality of light source
- Specular highlights appear as bright spots

### 4. Rim Lighting
```
Appearance: Bright outline around asteroid edges
Purpose: Separates asteroid from dark space background
Color: Cool blue/white (default)
Intensity: Subtle but noticeable
```

**Visual Description:**
- Visible on silhouette edges
- Brighter when viewed against backlight
- Helps asteroids "pop" visually
- Creates depth in asteroid fields

---

## Rarity Tier Visual Differences

### Common Asteroids (Iron, Copper)
- **Holes**: Fewer, smaller (6 density, 0.3 size)
- **Glow**: Minimal (0.1 emission)
- **Colors**: Muted (grey, brown-orange)
- **Overall**: Subtle, realistic appearance

### Uncommon Asteroids (Nickel, Silver, Titanium)
- **Holes**: Moderate amount (7 density, 0.33 size)
- **Glow**: Low (0.2 emission)
- **Colors**: Metallic (silver, grey-blue)
- **Overall**: Slightly more visible

### Rare Asteroids (Gold, Emerald, Platinum)
- **Holes**: Good amount (8 density, 0.35 size)
- **Glow**: Noticeable (0.35 emission)
- **Colors**: Vibrant (gold, green, white)
- **Overall**: Eye-catching from distance

### Epic Asteroids (Ruby, Sapphire, Obsidian)
- **Holes**: Many (9 density, 0.4 size)
- **Glow**: Strong (0.5 emission)
- **Colors**: Saturated (red, blue, purple)
- **Overall**: Very distinctive, attention-grabbing

### Legendary Asteroids (Quantum Crystal, Nebulite, Dark Matter)
- **Holes**: Maximum (10 density, 0.45 size)
- **Glow**: Very strong (0.8 emission)
- **Colors**: Otherworldly (purple, cyan, dark purple)
- **Overall**: Unmistakable, "magical" appearance

---

## Color Palette Reference

### Common Tier
| Type | RGB Color | Hex Code | Description |
|------|-----------|----------|-------------|
| Iron Ore | (120, 120, 130) | #787882 | Cool grey with blue tint |
| Copper | (184, 115, 51) | #B87333 | Metallic orange-brown |

### Uncommon Tier
| Type | RGB Color | Hex Code | Description |
|------|-----------|----------|-------------|
| Nickel | (200, 200, 180) | #C8C8B4 | Warm silvery grey |
| Silver | (192, 192, 192) | #C0C0C0 | Pure metallic silver |
| Titanium | (150, 150, 160) | #9696A0 | Dark steel blue-grey |

### Rare Tier
| Type | RGB Color | Hex Code | Description |
|------|-----------|----------|-------------|
| Gold | (255, 215, 0) | #FFD700 | Bright metallic gold |
| Emerald | (0, 201, 87) | #00C957 | Vivid green |
| Platinum | (229, 228, 226) | #E5E4E2 | Bright silvery white |

### Epic Tier
| Type | RGB Color | Hex Code | Description |
|------|-----------|----------|-------------|
| Ruby | (224, 17, 95) | #E0115F | Deep crimson red |
| Sapphire | (15, 82, 186) | #0F52BA | Royal blue |
| Obsidian | (60, 50, 70) | #3C3246 | Dark purple-grey |

### Legendary Tier
| Type | RGB Color | Hex Code | Description |
|------|-----------|----------|-------------|
| Quantum Crystal | (150, 50, 255) | #9632FF | Vibrant purple (high glow) |
| Nebulite | (0, 255, 200) | #00FFC8 | Bright cyan (high glow) |
| Dark Matter | (100, 0, 150) | #640096 | Deep purple (moderate glow) |

---

## Lighting Scenarios

### Bright Scene (Near Sun/Station)
- Cel bands very visible (3-4 distinct levels)
- Type colors vibrant and saturated
- Rim lighting subtle (not needed)
- Specular highlights prominent

### Dark Scene (Deep Space)
- Cel bands still visible but softer
- Type colors provide main visibility
- Rim lighting crucial for silhouette
- Emission from holes more noticeable

### Medium Lighting (Most Common)
- Balanced cel-shading effect
- Type colors clear and readable
- Rim lighting enhances depth
- Holes create nice contrast

---

## Animation Possibilities

### Static (Default)
- Holes are fixed in world space
- Asteroid rotates, holes stay positioned
- Pattern unique per asteroid instance

### Dynamic (Advanced)
- Holes can slowly "pulse" (scale emission)
- Pattern can slowly shift (animate worldPos in shader)
- Rare types can have animated glow

---

## Material Property Quick Reference

### For Close-Up Detail
```
Normal Strength: 1.5 - 2.0
Hole Density: 10 - 12
Hole Size: 0.4 - 0.5
Cel Bands: 5 - 8
```

### For Distant Visibility
```
Type Color Intensity: 2.0 - 2.5
Type Color Emission: 0.5 - 1.0
Rim Intensity: 0.8 - 1.2
Hole Size: 0.45 - 0.55
```

### For Performance
```
Hole Density: 5 - 6
Hole Noise Scale: 10 - 12
Normal Strength: 0.5 - 1.0
Cel Bands: 3 - 4
```

### For Stylized Look
```
Cel Bands: 3
Cel Smoothness: 0.01
Hole Edge Sharpness: 6 - 8
Type Color Intensity: 2.5
```

### For Realistic Look
```
Cel Bands: 8 - 10
Cel Smoothness: 0.4
Hole Edge Sharpness: 1 - 2
Type Color Intensity: 1.0
```

---

## Comparison with Original Web Game

### Original (2D Canvas)
- Flat colored shapes
- Simple solid colors
- No depth or lighting
- Type identified by pure color

### Unity 3D (This Shader)
- Full 3D with lighting
- Realistic rocky base + color identifier
- Depth from cel-shading
- Type identified by hole colors + intensity

### Design Philosophy Match
✅ **Visual Clarity**: Easy to identify types at a glance
✅ **Aesthetic Appeal**: More visually interesting than flat colors
✅ **Performance**: Efficient shader, single material per type
✅ **Scalability**: Works at any distance or zoom level

---

## Implementation Notes

### Material Instancing
- Use MaterialPropertyBlock for runtime changes
- Avoids creating unique materials per asteroid
- Better performance in large asteroid fields

### Variation Within Type
- RandomizeHolePattern() adds uniqueness
- Same type = same color, different pattern
- Prevents "copy-paste" look

### LOD Considerations
- Shader performs well at any distance
- For extreme distances, consider:
  - Simplified mesh LODs
  - Reduced hole calculations
  - Billboard imposters (very far)

---

## Testing Checklist

When implementing, verify:
- [ ] All 16 types have distinct, recognizable colors
- [ ] Holes are visible from typical gameplay distance
- [ ] Cel-shading doesn't create harsh artifacts
- [ ] Rim lighting works against dark backgrounds
- [ ] Performance is acceptable with 100+ asteroids
- [ ] Colors match the reference palette
- [ ] Emission doesn't bloom excessively
- [ ] Normal maps (if used) enhance appearance

---

## Future Enhancements

### Potential Additions
1. **Animated Holes**: Pulsing glow effect for rare types
2. **Damage States**: Cracks appear as asteroid takes damage
3. **Heat Glow**: Asteroid "heats up" during mining
4. **Particle Effects**: Dust/sparkles from holes
5. **Custom Textures**: Hand-painted rock textures
6. **Normal Map Generation**: Procedural normal detail
7. **Rotation Animation**: Slow rotation on spawn

---

*This visual reference ensures consistency across all asteroid implementations.*
