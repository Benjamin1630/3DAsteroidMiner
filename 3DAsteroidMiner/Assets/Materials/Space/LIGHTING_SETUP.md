# Space Lighting Setup Guide

## Overview
This setup provides a directional light (distant sun) that illuminates your space scene while keeping the sky completely black. It includes a **visible sun disc in the skybox** that automatically syncs with your directional light direction.

## What Was Created

### 1. SpaceSkybox Material (`Assets/Materials/Space/SpaceSkybox.mat`)
- A black skybox with a **visible sun disc**
- Uses custom shader `Custom/SpaceSkyboxWithSun`
- Sun automatically syncs position with directional light
- Adjustable sun size, color, intensity, and glow
- Optional starfield texture support

### 2. Custom Skybox Shader (`Assets/Shaders/SpaceSkyboxWithSun.shader`)
- Renders a procedural sun disc in the skybox
- Supports sun glow/corona effect
- Maintains pure black space background
- Optional starfield texture overlay
- Highly customizable parameters

### 3. SpaceLightingManager Script (`Assets/Scripts/SpaceLightingManager.cs`)
- Automatically configures lighting on scene start
- Sets ambient light to pure black (no ambient light in space)
- Creates/configures a directional light as the "sun"
- **Syncs skybox sun direction with directional light** in real-time
- Ensures camera background is solid black
- Provides runtime controls for adjusting lighting

## Setup Instructions

### Quick Setup (Recommended)

1. **Add the Lighting Manager to your scene:**
   - Create an empty GameObject in your scene (Right-click in Hierarchy → Create Empty)
   - Name it "LightingManager"
   - Add the `SpaceLightingManager` component to it
   - The script will automatically configure everything on play

2. **Assign the Skybox Material:**
   - In the LightingManager Inspector, check "Use Custom Skybox"
   - Drag `Assets/Materials/Space/SpaceSkybox.mat` to the "Space Skybox Material" field
   - Check "Sync Skybox Sun With Light" (enabled by default) - this makes the sun in the sky match your light direction

3. **Adjust Sun Settings:**
   - **Sun Rotation:** Rotates both the directional light AND the visible sun in the sky
   - **Sun Color:** Changes both light and skybox sun color
   - **Sun Intensity:** Affects directional light brightness

4. **Adjust Skybox Sun Appearance (Optional):**
   - Select the `SpaceSkybox` material in the Project window
   - In the Inspector, you can adjust:
     - **Sun Size:** How large the sun appears (0.01 - 0.5)
     - **Sun Intensity:** Brightness of the sun disc (0 - 5)
     - **Sun Glow:** Amount of glow/corona around sun (0 - 1)
     - **Starfield Texture:** Optional texture for background stars
     - **Starfield Intensity:** Brightness of starfield (0 - 2)

### Manual Setup (If you prefer manual control)

If you don't want to use the script, you can configure manually:

1. **Window → Rendering → Lighting:**
   - Environment tab:
     - Skybox Material: Set to `SpaceSkybox` or None
     - Sun Source: Set to your Directional Light
     - Environment Lighting Source: Color
     - Ambient Color: Black (0, 0, 0)
     - Intensity Multiplier: 0

2. **Create Directional Light:**
   - Right-click in Hierarchy → Light → Directional Light
   - Position: Doesn't matter (directional lights affect entire scene)
   - Rotation: Adjust to set light direction (e.g., X: 50, Y: -30, Z: 0)
   - Color: White or slightly warm (255, 242, 217 for sun-like)
   - Intensity: 1.5
   - Shadows: None (for performance) or Soft Shadows (for realism)

3. **Camera Settings:**
   - Select your Main Camera
   - Clear Flags: Solid Color
   - Background: Black (0, 0, 0, 255)

## Customization

### Skybox Sun Appearance

The skybox material has several parameters you can adjust:

**Sun Size:**
- Small distant star: `0.02`
- Normal sun (default): `0.05`
- Large close sun: `0.1`
- Giant sun: `0.2`

**Sun Intensity:**
- Dim/distant: `1.0`
- Normal (default): `2.0`
- Bright: `3.5`
- Supergiant: `5.0`

**Sun Glow:**
- No glow (sharp edges): `0.0`
- Subtle glow (default): `0.3`
- Strong corona: `0.6`
- Maximum glow: `1.0`

### Sun Light Variations

**Distant Yellow Star (Default):**
```csharp
Sun Color: (1.0, 0.95, 0.85)
Intensity: 1.5
Rotation: (50, -30, 0)
```

**Blue Giant Star:**
```csharp
Sun Color: (0.8, 0.9, 1.0)
Intensity: 2.0
Rotation: (50, -30, 0)
```

**Red Dwarf Star:**
```csharp
Sun Color: (1.0, 0.6, 0.4)
Intensity: 0.8
Rotation: (50, -30, 0)
```

**Multiple Suns (Binary System):**
- Create two directional lights manually
- Set different rotations and colors
- Adjust intensities (e.g., 1.2 and 0.8)
- **Note:** Skybox only shows one sun (synced to the primary light in SpaceLightingManager)
- For multiple skybox suns, you'll need to modify the shader

### Adding a Starfield Background

You can add stars to the background:

1. **Create or find a starfield texture:**
   - Black background with white/colored dots for stars
   - Tileable texture works best
   - 2048x1024 or larger recommended

2. **Assign to skybox material:**
   - Select `SpaceSkybox` material
   - Drag texture to "Starfield Texture" slot
   - Adjust "Starfield Intensity" (0.3 - 0.8 recommended)

3. **Alternative - Procedural starfield:**
   - You can modify the shader to generate stars procedurally
   - Or use a particle system for 3D volumetric stars

### Adding Rim Lighting for Space Objects

For better visibility in space, you can add rim lighting to your asteroids/ships:

```csharp
// Add to your asteroid/ship material shader
// Or use Unity's Standard Shader with emission
// Set emission color to match sun color but very dim
```

## Performance Considerations

- **Shadows:** Disabled by default for performance
  - Enable if you want asteroids to cast shadows on each other
  - Set to "Soft Shadows" for best quality
  - Adjust shadow distance in Quality Settings

- **Reflection Probes:** Set to minimal (0.3 intensity)
  - Space has minimal reflections
  - Saves GPU performance

## Troubleshooting

**Problem:** Scene still has blue/gray tint
- **Solution:** Ensure ambient intensity is 0 and ambient mode is "Color" (not Skybox)

**Problem:** Objects are too dark/can't see anything
- **Solution:** Increase sun intensity or add point lights near important objects

**Problem:** Lighting looks flat
- **Solution:** Enable shadows on the directional light or add rim lighting to materials

**Problem:** Camera shows blue sky instead of black with sun
- **Solution:** Make sure the skybox material is assigned in the LightingManager or in Window → Rendering → Lighting → Skybox Material

**Problem:** Sun in skybox doesn't move when I rotate the light
- **Solution:** Make sure "Sync Skybox Sun With Light" is checked in the SpaceLightingManager

**Problem:** Sun appears in wrong position
- **Solution:** The sun direction is based on the light's forward direction (negative Z axis). Rotate the light transform to aim it correctly.

**Problem:** Sun is too small/large
- **Solution:** Adjust the "Sun Size" parameter on the SpaceSkybox material (0.01 - 0.5)

**Problem:** Can't see the sun at all
- **Solution:** 
  1. Check that SpaceSkybox material shader is set to "Custom/SpaceSkyboxWithSun"
  2. Increase "Sun Intensity" on the material
  3. Verify light is not pointing directly down or in extreme angles

## Runtime Controls

The SpaceLightingManager provides methods you can call from other scripts:

```csharp
// Get reference
SpaceLightingManager lightingManager = FindObjectOfType<SpaceLightingManager>();

// Change sun intensity (directional light)
lightingManager.SetSunIntensity(2.0f);

// Change sun color (affects both light and skybox)
lightingManager.SetSunColor(new Color(1f, 0.6f, 0.4f)); // Red star

// Enable/disable sun light
lightingManager.SetSunLightEnabled(false);

// Adjust skybox sun appearance
lightingManager.SetSkyboxSunSize(0.08f); // Make sun appear larger
lightingManager.SetSkyboxSunIntensity(3.0f); // Make sun brighter in sky
```

### Rotating the Sun During Gameplay

To create a day/night cycle or moving sun:

```csharp
public class SunRotation : MonoBehaviour
{
    public Light sunLight;
    public float rotationSpeed = 5f;
    
    void Update()
    {
        // Rotate sun over time
        sunLight.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        
        // The skybox sun will automatically sync if "Sync Skybox Sun With Light" is enabled
    }
}
```

## Additional Enhancements

### Add Point Lights for Stations/Ships
```csharp
// Example: Add glowing lights to space station
GameObject stationLight = new GameObject("Station Light");
Light light = stationLight.AddComponent<Light>();
light.type = LightType.Point;
light.color = Color.cyan;
light.intensity = 3f;
light.range = 50f;
```

### Add Starfield Background
- Create a particle system with small white particles
- Place very far from camera
- Set to world space
- Slow/no movement for distant stars

### Add Lens Flare to Sun
- Add Lens Flare component to sun light GameObject
- Assign a Flare asset from Unity's Standard Assets

---

**Note:** This setup provides a realistic space environment with a visible sun disc in the skybox that automatically syncs with your directional light. The sun will appear to be in the same direction as the light is shining from, creating a cohesive lighting experience.

## Quick Settings Reference

| Parameter | Location | Default | Description |
|-----------|----------|---------|-------------|
| Sun Rotation | LightingManager | (50, -30, 0) | Direction of light & skybox sun |
| Sun Color | LightingManager | Warm white | Color of light & skybox sun |
| Sun Intensity | LightingManager | 1.5 | Directional light brightness |
| Sun Size | SpaceSkybox Material | 0.05 | Visual size of sun disc |
| Sun Intensity (Skybox) | SpaceSkybox Material | 2.0 | Brightness of sun in sky |
| Sun Glow | SpaceSkybox Material | 0.3 | Corona/glow around sun |
| Sync Skybox | LightingManager | ✓ Enabled | Auto-sync sun direction |

---

*Created for Asteroid Miner: Deep Space Operations - Realistic space lighting with visible sun*
