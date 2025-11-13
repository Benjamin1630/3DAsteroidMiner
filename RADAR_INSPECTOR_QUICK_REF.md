# Radar Inspector Quick Reference

## RadarSystem Inspector Layout

```
┌─────────────────────────────────────────┐
│ Radar System (Script)                   │
├─────────────────────────────────────────┤
│ Radar Configuration                     │
│   Radar Range           [1000    ]      │
│   Update Interval       [0.1     ]      │
│   Radar Layer Mask      [Everything]    │
├─────────────────────────────────────────┤
│ References                              │
│   Ship Transform        [None    ]      │
├─────────────────────────────────────────┤
│ Contact Filtering                       │
│   ☑ Show Asteroids                      │
│   ☑ Show Hazards                        │
│   ☑ Show NPCs                           │
├─────────────────────────────────────────┤
│ Debug                                   │
│   ☐ Show Debug Info                     │
└─────────────────────────────────────────┘
```

**Quick Actions:**
- Uncheck **Show Asteroids** to hide all asteroids from radar
- Uncheck **Show Hazards** to hide space debris/mines
- Uncheck **Show NPCs** to hide other ships
- Adjust **Radar Range** to change detection distance

---

## RadarDisplay Inspector Layout

```
┌─────────────────────────────────────────┐
│ Radar Display (Script)                  │
├─────────────────────────────────────────┤
│ References                              │
│   Radar System          [RadarSys]      │
│   Blip Container        [BlipCont]      │
├─────────────────────────────────────────┤
│ Display Configuration                   │
│   Display Mode          [Circular3D ▼]  │
│   Display Radius        [0.15    ]      │
│   Display Height        [0.05    ]      │
│   Blip Scale            [0.005   ]      │
│   ☑ Fade By Distance                    │
├─────────────────────────────────────────┤
│ Elevation Indicators                    │
│   ☑ Show Elevation Lines                │
├─────────────────────────────────────────┤
│ Visual Settings                         │
│   ☐ Show Radar Screen                   │
│   ☑ Show Grid                           │
│   Blip Material         [BlipMat ]      │
│   Radar Screen Material [ScreenMt]      │
│   Grid Color            [▓ Cyan  ]      │
│   Grid Ring Count       [4       ]      │
│   Grid Radial Lines     [8       ]      │
├─────────────────────────────────────────┤
│ Ship Indicator                          │
│   Ship Indicator Prefab [None    ]      │
│   Ship Indicator Scale  [0.01    ]      │
├─────────────────────────────────────────┤
│ Performance                             │
│   Max Blips             [100     ]      │
│   ☑ Use Object Pooling                  │
├─────────────────────────────────────────┤
│ Debug                                   │
│   ☐ Show Debug Info                     │
└─────────────────────────────────────────┘
```

**Quick Actions:**
- Change **Display Mode** dropdown:
  - **Circular2D** = Flat top-down radar
  - **Circular3D** = Full 3D with elevation (recommended)
  - **Cylindrical** = Height-aware projection
- Toggle **Show Elevation Lines** for vertical position indicators
- Toggle **Show Grid** to hide/show grid lines
- Toggle **Show Radar Screen** for holographic background effect

---

## Display Mode Comparison

### Circular2D
```
        ●  ●
      ●  ⊕  ●
        ●  ●
```
- Flat 2D view
- No height information
- Like traditional radar
- Best for: Top-down gameplay

### Circular3D (Recommended)
```
      ●     ●
        ⊕
    ●   |   ●
        ●
```
- Full 3D positioning
- Elevation shown by Y position
- Like Star Citizen radar
- Best for: 6DOF space flight

### Cylindrical
```
    ●       ●
    |       |
    ⊕───────
    |       |
    ●       ●
```
- Ring for horizontal position
- Height for elevation
- Good vertical awareness
- Best for: Vertical mining

---

## Toggle Effects

### Show Elevation Lines
**ON (☑):**
```
    ●
    |← Vertical line to ship level
    |
────⊕────
    |
    ●
    |
```

**OFF (☐):**
```
    ●


────⊕────


    ●

```

### Show Grid
**ON (☑):**
```
    ●╱───╲●
   ╱   ⊕   ╲
  ●─────────●
   ╲       ╱
    ●╲───╱●
```

**OFF (☐):**
```
    ●     ●
        ⊕
    ●       ●

    ●     ●
```

### Show Radar Screen
**ON (☑):**
```
┌─────────────┐
│  ●╱───╲●   │← Holographic background
│ ╱   ⊕   ╲  │
│●─────────● │
│ ╲       ╱  │
│  ●╲───╱●   │
└─────────────┘
```

**OFF (☐):** (Default - Better Visibility)
```
    ●╱───╲●
   ╱   ⊕   ╲
  ●─────────●
   ╲       ╱
    ●╲───╱●
```

---

## Contact Filtering Examples

### All Contacts (Default)
```
Radar shows:
  ● Asteroids (green/colored by rarity)
  ● Hazards (red)
  ● NPCs (yellow)
```

### Asteroids Only
```
Uncheck: Show Hazards, Show NPCs
Radar shows:
  ● Asteroids only
```

### Mining Mode
```
Check: Show Asteroids, Show Hazards
Uncheck: Show NPCs
Radar shows:
  ● Asteroids (for mining)
  ● Hazards (to avoid)
```

### Combat Mode
```
Check: Show NPCs, Show Hazards
Uncheck: Show Asteroids
Radar shows:
  ● Enemy ships
  ● Dangerous obstacles
```

---

## Common Configurations

### 1. Clean Minimal Display
```
Display Mode:        Circular3D
Show Elevation Lines: OFF
Show Radar Screen:   OFF
Show Grid:           ON (minimal - 3 rings, 6 lines)
Grid Ring Count:     3
Grid Radial Lines:   6
```

### 2. Maximum Information
```
Display Mode:        Circular3D
Show Elevation Lines: ON
Show Radar Screen:   OFF
Show Grid:           ON
Grid Ring Count:     5
Grid Radial Lines:   12
```

### 3. Star Citizen Style
```
Display Mode:        Circular3D
Show Elevation Lines: ON
Show Radar Screen:   ON (with transparent material)
Show Grid:           ON
Grid Ring Count:     4
Grid Radial Lines:   8
Grid Color:          Cyan (0.2, 0.8, 1, 0.5)
```

### 4. Performance Mode
```
Display Mode:        Circular2D
Show Elevation Lines: OFF
Show Radar Screen:   OFF
Show Grid:           ON (minimal)
Max Blips:           50
Use Object Pooling:  ON
```

---

## Troubleshooting

### "I don't see any blips"
Check:
1. ☑ Show Asteroids/Hazards/NPCs are checked
2. Radar Range is large enough (try 1000+)
3. Radar Layer Mask includes your objects
4. Objects are within range

### "Elevation lines don't show"
Check:
1. ☑ Show Elevation Lines is checked
2. Display Mode is Circular3D or Cylindrical
3. Objects are above/below ship level

### "Grid is too cluttered"
Adjust:
- Grid Ring Count: 3-4 (instead of 5+)
- Grid Radial Lines: 6-8 (instead of 12+)
- Or toggle ☐ Show Grid OFF completely

### "Can't see blips behind screen"
Solution:
- Toggle ☐ Show Radar Screen OFF
- Or make Radar Screen Material more transparent

---

## Legend

```
⊕  = Your Ship (center)
●  = Radar Contact (asteroid/hazard/NPC)
|  = Elevation Line (shows height difference)
╱╲ = Grid Lines
▓  = Color picker
☑  = Checkbox ON
☐  = Checkbox OFF
▼  = Dropdown menu
```

---

## Quick Tips

💡 **Want clearer view?** Turn off Radar Screen (it's off by default)

💡 **Too many contacts?** Lower Max Blips or filter by type

💡 **Need height info?** Enable Elevation Lines in 3D mode

💡 **Performance issue?** Use Circular2D mode and enable Object Pooling

💡 **Testing?** Toggle Show Debug Info to see contact counts

💡 **Real-time tweaking?** All Inspector changes apply instantly in Play mode!

---

This reference card shows all Inspector controls and their effects at a glance!
