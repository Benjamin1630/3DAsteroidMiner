# ScanPulse Shader - Parameter Guide

## Overview
Enhanced ScanPulse shader with extensive customization options for dramatic visual effects.

## Rendering Options

### Double Sided
- **Type:** Toggle
- **Default:** Off
- **Description:** When enabled, the scan pulse renders on both sides (inside and outside). Toggle this on to see changes from both perspectives in the inspector.

---

## Main Properties

### Pulse Color
- **Type:** Color
- **Default:** Cyan (0.2, 0.8, 1, 1)
- **Description:** Primary color of the scan pulse effect

### Progress
- **Type:** Range (0-1)
- **Default:** 0
- **Description:** Controls the fade-out progression of the pulse (0 = full intensity, 1 = faded out)

### Radius
- **Type:** Float
- **Default:** 100
- **Description:** Current radius of the scan pulse sphere

---

## Edge Properties

### Edge Thickness
- **Type:** Range (0.01-2.0)
- **Default:** 0.15
- **Description:** Thickness of the pulse edge ring. Lower values = thinner edge, higher = thicker glow

### Edge Sharpness
- **Type:** Range (0.1-10.0)
- **Default:** 1.0
- **Description:** Controls how sharp or soft the edge appears. Higher values = sharper, more defined edge

### Edge Glow Intensity
- **Type:** Range (0-10)
- **Default:** 2.0
- **Description:** Intensity multiplier for the edge glow effect. Higher values = more intense bright edge

---

## Brightness & Intensity

### Brightness
- **Type:** Range (0-10)
- **Default:** 2
- **Description:** Overall brightness multiplier for the entire effect

### Alpha Multiplier
- **Type:** Range (0-2)
- **Default:** 1.0
- **Description:** Controls the transparency/opacity of the pulse. Higher = more opaque

### Fade Curve Exponent
- **Type:** Range (0.1-5.0)
- **Default:** 1.0
- **Description:** Controls the fade curve shape. Values < 1 = slow start/fast end, Values > 1 = fast start/slow end

---

## Scan Lines

### Scan Line Frequency
- **Type:** Range (0-100)
- **Default:** 20.0
- **Description:** Density of horizontal scan lines. Higher = more lines

### Scan Line Speed
- **Type:** Range (-20 to 20)
- **Default:** 5.0
- **Description:** Speed and direction of scan line animation. Negative values reverse direction

### Scan Line Intensity
- **Type:** Range (0-1)
- **Default:** 0.15
- **Description:** Contrast/visibility of scan lines. 0 = smooth, 1 = high contrast

### Scan Line Offset
- **Type:** Range (0.5-1.5)
- **Default:** 0.85
- **Description:** Base brightness level for scan lines. Adjust for different looks

---

## Grid Pattern

### Grid Scale
- **Type:** Range (1-50)
- **Default:** 15.0
- **Description:** Size/density of the grid overlay pattern. Higher = smaller grid cells

### Grid Intensity
- **Type:** Range (0-1)
- **Default:** 0.3
- **Description:** Visibility/brightness of the grid pattern. 0 = invisible, 1 = very bright

### Grid Smoothness
- **Type:** Range (0-1)
- **Default:** 0.5
- **Description:** Softness of grid lines. 0 = sharp grid, 1 = soft/blurred grid

---

## Wave/Distortion Effects

### Wave Amplitude
- **Type:** Range (0-0.5)
- **Default:** 0.0
- **Description:** Strength of wave distortion effect. 0 = no distortion, higher = more wavy

### Wave Frequency
- **Type:** Range (0-20)
- **Default:** 5.0
- **Description:** Number of wave cycles. Higher = more waves/ripples

### Wave Speed
- **Type:** Range (-10 to 10)
- **Default:** 1.0
- **Description:** Speed of wave animation. Negative values reverse direction

---

## Pulse Animation

### Pulse Intensity
- **Type:** Range (0-2)
- **Default:** 0.0
- **Description:** Strength of pulsating brightness effect. 0 = no pulse, higher = more pulsation

### Pulse Speed
- **Type:** Range (0-20)
- **Default:** 1.0
- **Description:** Speed of pulse animation. Higher = faster pulsing

---

## Color Variation

### Secondary Color
- **Type:** Color
- **Default:** Orange (1, 0.5, 0.2, 1)
- **Description:** Second color to blend with primary color for gradients

### Color Mix Amount
- **Type:** Range (0-1)
- **Default:** 0.0
- **Description:** Blending between primary and secondary colors. 0 = primary only, 1 = gradient effect

### Color Gradient Power
- **Type:** Range (0.1-5.0)
- **Default:** 1.0
- **Description:** Controls gradient distribution. Lower = gradual, higher = more concentrated

---

## Usage Tips

### Creating Dramatic Effects:

1. **Intense Sci-Fi Pulse:**
   - Edge Sharpness: 5.0
   - Edge Glow: 8.0
   - Brightness: 6.0
   - Scan Line Frequency: 50
   - Grid Intensity: 0.6

2. **Soft Energy Wave:**
   - Edge Sharpness: 0.5
   - Edge Glow: 3.0
   - Wave Amplitude: 0.2
   - Wave Frequency: 10
   - Pulse Intensity: 0.5

3. **Distorted Anomaly:**
   - Wave Amplitude: 0.4
   - Wave Frequency: 15
   - Wave Speed: 5.0
   - Scan Line Speed: -10
   - Edge Thickness: 0.5

4. **Color Shifting Scan:**
   - Color Mix Amount: 0.8
   - Color Gradient Power: 2.0
   - Secondary Color: Set to contrasting color
   - Pulse Intensity: 1.0
   - Pulse Speed: 8.0

5. **Retro Grid Scanner:**
   - Grid Scale: 30
   - Grid Intensity: 0.8
   - Grid Smoothness: 0.2
   - Scan Line Frequency: 40
   - Brightness: 4.0

---

## Technical Notes

- All animations are time-based using Unity's `_Time` built-in variable
- Double-sided rendering automatically adjusts culling mode
- Wave distortion is applied to the radial distance calculation
- Grid pattern uses UV coordinates for consistent texturing
- Edge detection uses normalized distance from sphere center
- All effects are performance-optimized for real-time use

---

**Created:** November 8, 2025  
**Shader Version:** 2.0 (Enhanced)
