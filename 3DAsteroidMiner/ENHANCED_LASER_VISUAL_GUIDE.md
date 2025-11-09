# Enhanced Mining Laser - Visual Reference Guide

## Color Palette

### Default Hot Mining Laser Colors

```
╔══════════════════════════════════════════════════════════════╗
║                    COLOR BREAKDOWN                            ║
╠══════════════════════════════════════════════════════════════╣
║  Base Color (Outer Beam):                                    ║
║  ████████ RGB(255, 102, 0) - Hot Orange                      ║
║  Used for: Beam edges, outer glow                            ║
║                                                               ║
║  Hot Color (Core):                                           ║
║  ████████ RGB(255, 230, 102) - Yellow-White Hot              ║
║  Used for: Beam center, intense heat areas                   ║
║                                                               ║
║  Impact Hot Color:                                           ║
║  ████████ RGB(255, 153, 26) - Molten Orange                  ║
║  Used for: Initial impact glow                               ║
║                                                               ║
║  Impact Cool Color:                                          ║
║  ████████ RGB(51, 25, 13) - Dark Scorch                      ║
║  Used for: Faded heat mark                                   ║
╚══════════════════════════════════════════════════════════════╝
```

### Temperature-Based Color Theory

```
Heat Scale (Laser Mining):
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1500°C+  ████ Yellow-White  (Core of beam)
1200°C   ████ Bright Yellow  (Inner beam)
1000°C   ████ Orange-Yellow  (Mid beam)
 800°C   ████ Hot Orange     (Outer beam)
 500°C   ████ Deep Orange    (Particles)
 300°C   ████ Red-Orange     (Cooling)
 100°C   ████ Dark Red       (Scorch mark)
  20°C   ████ Dark Brown     (Faded mark)
```

---

## Laser Beam Gradient

### Cross-Section View

```
         WIDTH
    ◄───────────►
    
    ░░░░░░░░░░░    ← Alpha fade (transparent edges)
    ▒▒▒▒▒▒▒▒▒▒▒    ← Orange (base color)
    ▓▓▓▓▓▓▓▓▓▓▓    ← Orange-yellow blend
    ███████████    ← Yellow-white (hot color, core)
    ▓▓▓▓▓▓▓▓▓▓▓    ← Orange-yellow blend
    ▒▒▒▒▒▒▒▒▒▒▒    ← Orange (base color)
    ░░░░░░░░░░░    ← Alpha fade
```

### Length View

```
    SHIP                    ASTEROID
     ║                         ◎
     ║                         ║
     ╠═══════════════════════⚡
     ║    ^         ^        ^
     ║    │         │        └─ Impact point (sparks + heat)
     ║    │         └────────── Mid-beam particles
     ║    └──────────────────── Origin particles
     ║
   Laser
   Origin
```

---

## Animation Phases

### Phase 1: Inactive
```
[Ship]     (nothing)      [Asteroid]

State: Laser off
Visual: No line renderer, no particles
Duration: Indefinite
```

### Phase 2: Starting Up (0.0s → 0.3s)
```
Frame 1 (0.0s):
[Ship] ∙                  [Asteroid]
       ↑
    Startup particles burst

Frame 2 (0.1s):
[Ship] ∙∙∙░──────────→   [Asteroid]
          ↑
    Thin beam appears, 30% width

Frame 3 (0.2s):
[Ship] ∙∙∙▒▓████████→    [Asteroid]
            ↑
    Beam 70% width, brightening

Frame 4 (0.3s):
[Ship] ══════════════⚡→ ⊕ [Asteroid]
       ↑             ↑   ↑
    Full width    Sparks Impact
    + looping particles appear
```

### Phase 3: Active Mining (steady state)
```
Pulse Cycle (0.125s per cycle at speed 8):

High Intensity:
[Ship] ═══════════════⚡⚡→ ⊕ [Asteroid]
       ████████████████    ⊗
    0.25 width, bright    Glowing
                          heat mark

Normal Intensity:
[Ship] ══════════════⚡→ ⊕ [Asteroid]
       ██████████████     ⊙
    0.20 width           Medium
                         glow

Low Intensity:
[Ship] ════════════⚡→  ⊕ [Asteroid]
       ████████████      ∘
    0.15 width          Dim
                        glow

(Repeats continuously with smooth interpolation)
```

### Phase 4: Shutting Down (0.0s → 0.2s)
```
Frame 1 (0.0s):
[Ship] ══════════════→   ⊕ [Asteroid]
       ↓                  ↓
    Stop looping      Start cool-
    particles         ing effect

Frame 2 (0.1s):
[Ship] ▓▓▓▓▓▓▓──→  ∙∙∙   ⊙ [Asteroid]
       ↑                  ↓
    70% width         Fading
    Shutdown          heat mark
    particles

Frame 3 (0.2s):
[Ship]  ∙  ∙  ∙           ○ [Asteroid]
        ↑                  ↓
    Beam gone          Heat mark
    Final particles    cooling

Frame 4 (1.5s later):
[Ship]                      [Asteroid]
                            ↑
                        Heat mark
                        fully faded
```

---

## Particle Effects Visualization

### 1. Startup Particles
```
    Burst Pattern:
    
       ∙ ∙ ∙
      ∙∙∙∙∙∙∙
     ∙∙∙[○]∙∙∙  ← Ship laser mount
      ∙∙∙∙∙∙∙
       ∙ ∙ ∙
       
    Direction: Cone forward (15° angle)
    Speed: 5 units/sec
    Lifetime: 0.3 seconds
    Color: Orange (255, 153, 51)
    Count: ~15 particles
```

### 2. Looping Beam Particles
```
    Along Beam:
    
    [Ship] ∙ ∙ ∙ ∙ ∙ ∙ ∙ ∙ → [Asteroid]
           ↑ ↑ ↑ ↑ ↑ ↑ ↑ ↑
        Continuous particle stream
        
    Direction: Narrow cone (5° angle)
    Speed: 2 units/sec
    Lifetime: 0.5 seconds
    Color: Hot yellow (255, 204, 77)
    Rate: 30 particles/sec
```

### 3. Shutdown Particles
```
    Dissipation:
    
         ∙     ∙
       ∙   ∙ ∙   ∙
      ∙   [○]   ∙  ← Laser fading
       ∙   ∙ ∙   ∙
         ∙     ∙
         
    Direction: Expanding sphere
    Speed: 3 units/sec
    Lifetime: 0.4 seconds
    Color: Fading orange
    Count: ~12 particles
```

### 4. Spark Particles
```
    Impact Sparks:
    
                    ∙  ∙
                   ∙ ⊕ ∙  ← Impact point
                  ∙  ∙  ∙
                 ∙   ↓   ∙
                   (falling)
                   
    Direction: Random sphere, affected by gravity
    Speed: 5-15 units/sec (random)
    Lifetime: 0.2-0.5 seconds (random)
    Color: Bright yellow-white
    Rate: 20 particles/sec
```

### 5. Heat Distortion Particles
```
    Heat Waves:
    
              ≈≈≈≈≈
             ≈≈≈⊕≈≈≈  ← Rising heat
            ≈≈≈ ↑ ≈≈≈
           ≈≈≈  │  ≈≈≈
          
    Direction: Upward cone (10° angle)
    Speed: 1 unit/sec
    Lifetime: 1.0 second
    Color: Semi-transparent orange
    Size: Grows 50% → 150% over lifetime
    Rate: 15 particles/sec
```

---

## Heat Mark Evolution

### Timeline View

```
T = 0.0s (Impact Start):
    ∙           ← Small point appears
    
T = 0.15s:
    ⊕           ← Growing, bright glow
    Size: 0.75  Intensity: 100%
    Color: Molten orange
    
T = 0.3s:
    ⊗           ← Maximum size reached
    Size: 1.2   Intensity: 100%
    Color: Orange-yellow
    Light: Full intensity
    
T = 0.3s - 1.8s (Active):
    ⊕           ← Pulsing glow
    Pulsing between 80%-100% intensity
    Color: Hot orange
    
T = 1.8s (Mining stops):
    ⊙           ← Begin cooling
    Intensity: Fading
    Color: Orange → Red-orange
    
T = 2.3s:
    ○           ← Cooling down
    Intensity: 50%
    Color: Dark red
    Light: Fading
    
T = 3.0s:
    ∘           ← Nearly gone
    Intensity: 20%
    Color: Dark brown
    
T = 3.3s:
                ← Completely faded
```

### Size Progression

```
Time  Size    Visual
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
0.0s  0.5     ∙
0.1s  0.7     •
0.2s  0.9     ●
0.3s  1.2     ⊕ (max)
1.8s  1.2     ⊕ (stable)
2.3s  1.2     ⊙ (cooling)
3.0s  1.2     ○ (fading)
3.3s  0.0     (gone)
```

---

## Width Progression

### Startup Phase

```
Time    Width   Visual Representation
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
0.00s   0.00    (nothing)
0.05s   0.05    ─
0.10s   0.08    ═
0.15s   0.11    ═
0.20s   0.13    ══
0.25s   0.14    ══
0.30s   0.15    ═══ (base width)
```

### Active Phase (Pulsing)

```
Time    Width   Intensity   Visual
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
0.00s   0.15    100%       ═══
0.125s  0.18    120%       ════
0.25s   0.15    100%       ═══
0.375s  0.12    80%        ══
0.50s   0.15    100%       ═══
(cycle repeats)
```

### Shutdown Phase

```
Time    Width   Visual Representation
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
0.00s   0.15    ═══
0.05s   0.12    ══
0.10s   0.08    ═
0.15s   0.04    ─
0.20s   0.00    (nothing)
```

---

## Layer Composition

### What the player sees (composite):

```
Layer 4: Heat Distortion Particles (≈≈≈) - Transparent orange
         ↓
Layer 3: Spark Particles (∙∙∙) - Bright yellow-white
         ↓
Layer 2: Looping Beam Particles (•••) - Yellow
         ↓
Layer 1: LineRenderer Beam (═══) - Orange-yellow gradient
         ↓
Layer 0: Impact Heat Mark (⊕) - Glowing orange on asteroid

Combined Effect: Intense, hot, powerful mining laser!
```

---

## Recommended Unity Settings

### LineRenderer Settings
```
Positions: 2 (start, end)
Width Curve: Constant (adjusted via script)
Color Gradient: Custom (set by script)
Corner Vertices: 4
End Cap Vertices: 4
Alignment: View
Texture Mode: Tile
Shadow Casting: Off
Receive Shadows: Off
Sorting Order: 100
```

### Material Settings
```
Shader: Standard
Rendering Mode: Transparent (for blend mode)
Albedo: Hot orange
Metallic: 0
Smoothness: 0.8
Emission: Enabled
  Color: Yellow-white
  Intensity: 3.0 (HDR)
```

### Particle System Settings (Looping)
```
Main Module:
  Duration: 1.0
  Looping: true
  Start Lifetime: 0.5
  Start Speed: 2
  Start Size: 0.15
  Start Color: Yellow
  Simulation Space: World
  Max Particles: 20

Emission:
  Rate over Time: 30

Shape:
  Shape: Cone
  Angle: 5
  Radius: 0.05
```

### Light Component (Impact)
```
Type: Point
Color: Hot orange
Intensity: 2.0
Range: 2.4 (maxSize * 2)
Render Mode: Force Pixel
Shadows: None
```

---

## Performance Budget

### Per Laser
```
Component               Cost (ms)   Memory
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
LineRenderer            0.05        50 KB
Startup Particles       0.02        20 KB
Looping Particles       0.08        30 KB
Shutdown Particles      0.02        20 KB
Spark Particles         0.10        40 KB
Heat Distortion         0.08        40 KB
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Total                   0.35 ms     200 KB
```

### 6 Lasers + 10 Impact Effects
```
6 Lasers:              2.1 ms      1.2 MB
10 Impact Effects:     0.5 ms      0.5 MB
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Total:                 2.6 ms      1.7 MB

Target: < 16.67ms per frame (60 FPS)
Actual: 13.07ms remaining ✅
```

---

## Visual Comparison Reference

### Before (Old System)
```
Simple green line:
    
    [Ship]───────────────→[Asteroid]
    
    • Instant on/off
    • Static appearance
    • No effects
    • Minimal impact
```

### After (Enhanced System)
```
Realistic mining laser:

    [Ship]═══∙∙∙══════⚡⚡→⊕≈[Asteroid]
          ↑  ↑  ↑    ↑↑ ↑
          │  │  │    │└ Heat mark
          │  │  │    └─ Sparks
          │  │  └────── Heat distortion
          │  └───────── Beam particles
          └──────────── Animated beam
          
    • Smooth startup/shutdown
    • Dynamic pulsing
    • Multiple particle systems
    • Impactful visual feedback
```

---

## Testing Checklist

### Visual Quality
- [ ] Beam color gradient visible (orange → yellow)
- [ ] Smooth startup animation (0.3s)
- [ ] Smooth shutdown animation (0.2s)
- [ ] Pulsing effect clearly visible
- [ ] Particles emit correctly
- [ ] Sparks fly from impact point
- [ ] Heat distortion visible
- [ ] Heat mark appears on asteroid
- [ ] Heat mark glows initially
- [ ] Heat mark fades correctly

### Performance
- [ ] Maintains 60 FPS with 1 laser
- [ ] Maintains 60 FPS with 6 lasers
- [ ] No memory leaks after 5 minutes
- [ ] Particles don't accumulate indefinitely
- [ ] Impact effects clean up properly

### Functionality
- [ ] Laser tracks target correctly
- [ ] Multiple lasers don't interfere
- [ ] Works with all upgrade levels
- [ ] Handles rapid on/off correctly
- [ ] No errors in console

---

**Visual Reference Complete!**  
Use this guide to verify your laser looks and performs as intended. 🎯✨
