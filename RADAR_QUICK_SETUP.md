# Radar System - Quick Setup Guide

## 🚀 Quick Start (30 seconds)

### Option 1: Automatic Setup (Recommended)
1. Tag your ship GameObject with "Player" tag
2. Menu: `Tools > Asteroid Miner > Setup Radar System`
3. Adjust position in Scene view (should be on dashboard)
4. Done! Press Play to test

### Option 2: Manual Setup
1. Create empty GameObject → Name: "RadarSystem"
2. Add `RadarSystem.cs` component
3. Create Plane as child → Name: "RadarDisplay"
4. Add `RadarDisplay.cs` component
5. Assign RadarSystem reference in RadarDisplay
6. Create materials with HolographicRadar and RadarBlip shaders
7. Assign materials to display

---

## 📍 Positioning on Dashboard

### Recommended Local Position (inside cockpit):
```
Position: (0.3, 0.5, 0.8)  // Right side of dashboard
Rotation: (45, 0, 0)        // Tilted toward pilot
Scale: (0.02, 1, 0.02)      // Small display size
```

### Adjust to Your Ship:
- Position in front of pilot seat
- Tilt ~45° so it faces pilot
- Scale 0.15-0.3 units for good visibility
- Make it child of ship so it moves with ship

---

## ⚙️ Essential Settings

### RadarSystem Component:
| Setting | Default | Recommended | Notes |
|---------|---------|-------------|-------|
| Radar Range | 1000m | 500-2000m | Detection distance |
| Update Interval | 0.1s | 0.1-0.2s | Lower = smoother |
| Layer Mask | All | Asteroids + Hazards | What to detect |

### RadarDisplay Component:
| Setting | Default | Recommended | Notes |
|---------|---------|-------------|-------|
| Display Mode | Circular3D | Circular3D | 3D holographic |
| Display Radius | 0.15 | 0.15-0.25 | Physical size |
| Display Height | 0.05 | 0.03-0.08 | Vertical range |
| Blip Scale | 0.005 | 0.003-0.008 | Blip size |
| Max Blips | 100 | 50-150 | Performance limit |
| Object Pooling | True | True | Better performance |

---

## 🎨 Material Setup

### RadarScreen Material:
```
Shader: Custom/HolographicRadar
Base Color: RGB(50, 200, 255) Alpha(180)
Emission: RGB(50, 200, 255) Intensity(2)
Opacity: 0.7
Scanline Speed: 2
```

### RadarBlip Material:
```
Shader: Custom/RadarBlip
Color: RGB(50, 255, 50) - Green for asteroids
Emission: RGB(50, 255, 50) Intensity(2)
Glow Intensity: 3
Pulse Speed: 2
```

---

## 🎯 Testing Checklist

- [ ] RadarSystem has ship transform assigned
- [ ] Asteroids appear as green blips when spawned
- [ ] Blips scale/fade with distance
- [ ] Grid shows 4 rings and 8 radial lines
- [ ] Ship indicator at center
- [ ] Display rotates with ship
- [ ] Holographic scanline effect visible
- [ ] Blips pulse/glow correctly

---

## 🐛 Common Issues

**No blips appearing?**
→ Check Layer Mask includes asteroids
→ Verify asteroids have colliders
→ Check radar range is sufficient

**Blips in wrong position?**
→ Ensure RadarDisplay is child of ship
→ Check displayRadius scale
→ Verify display mode setting

**Performance slow?**
→ Reduce maxBlips to 50
→ Increase updateInterval to 0.2s
→ Enable object pooling

**Shaders not working?**
→ Ensure shaders in Assets/Shaders/
→ Check material shader assignment
→ Try Standard shader as fallback

---

## 🔧 Quick Tweaks

### Make radar bigger:
```csharp
radarDisplay.SetDisplayRadius(0.3f);
```

### Change detection range:
```csharp
radarSystem.SetRadarRange(2000f);
```

### Hide hazards:
```csharp
radarSystem.ToggleHazards(false);
```

### Switch to flat 2D mode:
```csharp
radarDisplay.SetDisplayMode(RadarDisplay.RadarDisplayMode.Circular2D);
```

---

## 📚 Full Documentation
See `RADAR_SYSTEM_README.md` for complete details.

---

**Version**: 1.0 | **Updated**: November 13, 2025
