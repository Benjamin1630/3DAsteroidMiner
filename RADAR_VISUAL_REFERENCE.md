# Radar System Visual Reference

## 🎨 What It Should Look Like

### Overall Appearance
Imagine a **holographic circular display** mounted on your ship's dashboard, similar to what you'd see in Star Citizen. The display glows with a cyan/blue holographic effect and shows a top-down (or 3D) view of space around your ship.

---

## 📐 Layout Diagram (ASCII Art)

### Top View (Circular2D Mode):
```
     ╔═══════════════════════════════╗
     ║    HOLOGRAPHIC RADAR SCREEN   ║
     ║                               ║
     ║         · (Asteroid)          ║
     ║      ╭─────────────╮          ║
     ║   ·  │   ┌───┐     │  ·       ║
     ║      │   │ ▲ │     │          ║
     ║      │   └───┘     │   ·      ║
     ║   ·  │    SHIP     │          ║
     ║      ╰─────────────╯  ·       ║
     ║         (Grid Ring)           ║
     ║    ·                    ·     ║
     ║                               ║
     ╚═══════════════════════════════╝
     
Legend:
▲ = Ship (always at center)
· = Asteroid blips (green dots)
╭─╮ = Grid rings (cyan circles)
│ = Radial lines (cyan spokes)
```

### Side View (showing 3D elevation):
```
        Blips above ship (↑)
           ·   ·
    ━━━━━━━━━━━━━━━━━━━  ← Display plane
         ·  ▲  ·         ← Ship level
    ━━━━━━━━━━━━━━━━━━━
           ·   ·
        Blips below ship (↓)
```

---

## 🎨 Visual Elements Breakdown

### 1. Radar Screen (Background)
```
Material: HolographicRadar shader
Appearance: 
  - Semi-transparent cyan glass
  - Animated horizontal scanlines (moving up/down)
  - Bright edges (fresnel rim lighting)
  - Subtle flicker/shimmer effect
  - Faint grid pattern overlay
  
Color: RGB(50, 200, 255) with 70% opacity
Glow: Cyan emission at intensity 2.0
```

**Think**: Holographic projection from sci-fi movies, like displays in Iron Man's suit or Star Wars holograms.

### 2. Grid System
```
Structure:
  - 4 concentric circles (rings)
  - 8 radial lines (spokes)
  - Thin glowing cyan lines
  - All lines ~0.001 units thick
  
Purpose: Distance reference
  - Inner ring = 25% of radar range
  - Ring 2 = 50% of radar range
  - Ring 3 = 75% of radar range
  - Outer ring = 100% of radar range
```

**Think**: Like a target or bullseye, but with radial spokes like a compass rose.

### 3. Ship Indicator (Center)
```
Shape: Small cube or pyramid
Position: Exact center (0, 0, 0 local space)
Color: White or cyan with bright emission
Size: ~0.01 units (visible but not blocking view)
Rotation: Can rotate to show ship heading

Alternative: Use arrow/triangle pointing forward
```

**Think**: "You are here" marker, like Google Maps blue dot.

### 4. Radar Blips
```
Shape: Small cubes (0.003-0.008 units)
Behavior:
  - Glow with emission
  - Pulse slowly (brightness varies)
  - Scale based on distance:
    * Close = larger
    * Far = smaller
  - Fade based on distance:
    * Close = 100% opacity
    * Far = 30% opacity

Colors (by type):
  Asteroids:
    - Common: Gray
    - Uncommon: Green
    - Rare: Blue
    - Epic: Purple
    - Legendary: Orange
  
  Hazards: Red
  NPCs: Yellow
  Stations: Bright blue
```

**Think**: Glowing dots/pixels on a radar screen, like submarine sonar blips.

### 5. Visual Effects

#### Scanlines:
```
Pattern: Horizontal lines moving vertically
Speed: Slow, continuous scroll
Frequency: 20-30 lines across display
Intensity: Subtle (30% brightness increase)
```
**Think**: Old CRT monitor scan effect or Matrix rain.

#### Fresnel Glow:
```
Location: Edges of display
Effect: Brighter rim around perimeter
Power: Medium (value 2-3)
Color: Same as base (cyan)
```
**Think**: Backlit glass edges, like LED monitor glow.

#### Flickering:
```
Type: Random subtle brightness variation
Speed: Slow (5-10 Hz)
Intensity: Very subtle (10% max)
Purpose: Adds realism, makes it feel "powered"
```
**Think**: Slight power fluctuation in hologram.

---

## 📏 Size and Placement

### Physical Display Size (in 3D space):
```
Recommended Scale:
  - Width: 0.15-0.25 units
  - Height: 0.15-0.25 units
  - Depth: 0.01-0.05 units (if 3D mode)

For reference:
  - If ship is ~10 units long
  - Display should be ~2% of ship size
  - Small enough to fit on dashboard
  - Large enough to see clearly in VR/monitor
```

### Dashboard Position:
```
Typical Cockpit Layout:

        [Windshield]
            /     \
           /       \
    ┌─────┴─────────┴─────┐
    │  ╭───╮    ╭───╮     │
    │  │HUD│    │NAV│     │  ← Secondary displays
    │  ╰───╯    ╰───╯     │
    │                      │
    │     ╭─────────╮      │
    │     │  RADAR  │      │  ← YOUR RADAR HERE
    │     ╰─────────╯      │
    │                      │
    └──────────────────────┘
          [Pilot Seat]
```

**Placement Tips**:
- Right side of center console (so left hand can reach)
- Angled ~45° toward pilot (easier to see)
- Below eye level but in peripheral vision
- Not blocking critical flight instruments

---

## 🎭 Comparison to Star Citizen

### Star Citizen Radar:
```
Your Implementation:
✅ Circular/spherical display
✅ Ship at center
✅ Color-coded contacts
✅ 3D elevation visualization
✅ Holographic appearance
✅ Grid reference lines

Differences:
❌ No contact icons (yet) - just colored cubes
❌ No information panel (yet)
❌ No threat assessment (yet)
❌ No zoom levels (yet)
```

**Your radar is a simplified but functional version** that captures the core aesthetic and functionality.

---

## 🌈 Color Palette Reference

### Primary Colors:
```
Display Base:    RGB(50, 200, 255)   #32C8FF  Cyan
Grid Lines:      RGB(50, 200, 255)   #32C8FF  Cyan (50% opacity)
Ship Indicator:  RGB(100, 255, 255)  #64FFFF  Bright Cyan

Blip Colors:
Common Asteroid:    RGB(150, 150, 150)  #969696  Gray
Uncommon Asteroid:  RGB(80, 255, 80)    #50FF50  Green
Rare Asteroid:      RGB(80, 150, 255)   #5096FF  Blue
Epic Asteroid:      RGB(200, 80, 255)   #C850FF  Purple
Legendary Asteroid: RGB(255, 150, 50)   #FF9632  Orange

Hazard:        RGB(255, 80, 30)    #FF501E  Red
NPC:           RGB(255, 200, 50)   #FFC832  Yellow
Station:       RGB(50, 150, 255)   #3296FF  Blue
```

### Hex Codes (for external tools):
- Cyan: `#32C8FF`
- Green: `#50FF50`
- Blue: `#5096FF`
- Purple: `#C850FF`
- Orange: `#FF9632`
- Red: `#FF501E`
- Yellow: `#FFC832`

---

## 🎬 Animation Reference

### Scanline Animation:
```
Frame 1:  ═══════  ← Scanlines at top
Frame 2:   ═══════
Frame 3:    ═══════
Frame 4:     ═══════
Frame 5:      ═══════ ← Moving down
...repeat...
```
Speed: 2 units per second

### Blip Pulse:
```
Time 0s:  ● (100% brightness)
Time 0.5s: ○ (70% brightness)
Time 1s:  ● (100% brightness)
...repeat...
```
Period: 1-2 seconds per pulse

### Flicker:
```
Random brightness: 90-110% of base
Very subtle, barely noticeable
Adds "living" quality to display
```

---

## 🖼️ Real-World Visual References

### Similar To:
1. **Star Citizen Radar** - Main inspiration
2. **Elite Dangerous Scanner** - Grid style
3. **Subnautica Sonar** - Pulse effect
4. **Alien Isolation Motion Tracker** - Aesthetic
5. **The Expanse Ship Displays** - Color scheme
6. **Halo VISR Mode** - Holographic overlay
7. **Mass Effect Galaxy Map** - Holographic style

### Google Image Search Terms:
- "star citizen radar display"
- "holographic interface sci-fi"
- "ship radar hologram"
- "tactical display holographic"
- "sci-fi dashboard UI"

---

## 💡 Visualization Tips

### In Unity Editor:
1. **Scene View**: Use to position on dashboard
2. **Game View**: Check from pilot perspective
3. **Frame Selected (F)**: Focus camera on radar
4. **Isolate Selection**: View radar alone

### Testing Views:
1. **Close-up**: See detail of blips and grid
2. **Pilot POV**: Check visibility from seat
3. **VR Mode**: Test in VR if applicable
4. **Different Lighting**: Test in bright/dark space

### Adjustment Tips:
- **Too dim?** → Increase emission intensity
- **Too bright?** → Reduce emission, increase opacity
- **Blips invisible?** → Increase blip scale
- **Grid cluttered?** → Reduce ring/line count
- **Effects distracting?** → Reduce scanline intensity

---

## 🎨 Material Settings Quick Reference

### RadarScreen Material:
```
Shader: Custom/HolographicRadar

Base Color: (50, 200, 255, 180)
Emission: (50, 200, 255) × 2.0
Fresnel Power: 3.0
Fresnel Intensity: 2.0
Scanline Speed: 2.0
Scanline Frequency: 20.0
Opacity: 0.7
Flicker Speed: 5.0
Flicker Intensity: 0.1
```

### RadarBlip Material:
```
Shader: Custom/RadarBlip

Color: (50, 255, 50, 255) [green default]
Emission: (50, 255, 50) × 2.0
Glow Intensity: 3.0
Pulse Speed: 2.0
Fresnel Power: 2.0
```

---

## 📸 Screenshot Checklist

When your radar is working, it should show:
- [ ] Cyan glowing circular display
- [ ] Grid with rings and radial lines
- [ ] White/cyan ship indicator at center
- [ ] Colored blips for nearby asteroids
- [ ] Blips correctly positioned relative to ship
- [ ] Scanlines animating smoothly
- [ ] Rim glow around edges
- [ ] Blips pulsing gently
- [ ] Larger blips closer, smaller blips farther
- [ ] Professional sci-fi aesthetic

---

**If your radar looks like this, you've nailed it!** 🎯

*For questions about the visual appearance, refer to Star Citizen gameplay videos showing the radar system.*
