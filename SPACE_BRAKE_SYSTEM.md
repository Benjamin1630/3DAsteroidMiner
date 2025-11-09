# Space Brake System

## Overview
The Space Brake system provides players with a toggleable braking mechanism that applies counter-force to slow down the ship in space. This feature is inspired by space flight simulators like Elite Dangerous and Star Citizen.

**Key Behavior**: The space brake is intelligent - it only applies counter-force in directions where you're **NOT** actively thrusting. This allows you to accelerate in one direction while the brake automatically stops drift in other directions.

## Features
- **Toggle Activation**: Press X (keyboard) or Y button (gamepad) to toggle space brake on/off
- **Selective Counter-Force**: Only applies braking force in axes where you're NOT thrusting:
  - Forward/Back: Brakes only if not pressing W/S
  - Left/Right: Brakes only if not pressing A/D
  - Up/Down: Brakes only if not pressing Space/Ctrl
  - **Allows simultaneous acceleration and braking in different directions!**
- **Dynamic Fuel Consumption**: Fuel usage scales with:
  - Actual brake force applied (only what's needed)
  - Current ship velocity in non-input directions
  - Fuel efficiency upgrade level (higher upgrades = less fuel consumption)
- **Smart Behavior**: Only activates when:
  - Space brake is toggled on
  - Ship has fuel remaining
  - Ship is moving in directions without thrust input

## Input Bindings
- **Keyboard**: X key
- **Gamepad**: Y button (buttonNorth)

## Implementation Details

### PlayerInputHandler.cs Changes
1. **Added Event**: `OnSpaceBrakeToggled` - Fires when space brake is toggled (passes bool state)
2. **Added State**: `spaceBrakeActive` - Tracks current space brake state
3. **Added Callback**: `OnSpaceBrakeCallback` - Handles input and toggles state
4. **Added Method**: `IsSpaceBrakeActive()` - Public method to check if space brake is active

### PlayerController.cs Changes
1. **Added Settings**:
   - `spaceBrakeForce` (default: 30f) - Force multiplier for counter-force
   - `spaceBrakeFuelRate` (default: 0.25f) - Fuel consumption rate per second
   
2. **Added State**: `spaceBrakeActive` - Tracks space brake state from input

3. **Added Method**: `ApplySpaceBrake()` - Core braking logic that:
   - Calculates counter-force opposite to velocity
   - Applies force using `ForceMode.Acceleration`
   - Consumes fuel proportional to time

4. **Added Property**: `IsSpaceBrakeActive` - Public property to check brake state

## Physics Behavior
The space brake intelligently applies counter-force only in directions where the player is not providing thrust input:

```csharp
// Convert velocity to local space
Vector3 velocityLocal = transform.InverseTransformDirection(rb.linearVelocity);

// Only brake in axes without input (threshold: 0.1)
if (Abs(thrustStrafeInput) < 0.1f) brakeVector.x = -velocityLocal.x;
if (Abs(thrustVerticalInput) < 0.1f) brakeVector.y = -velocityLocal.y;
if (Abs(thrustForwardInput) < 0.1f) brakeVector.z = -velocityLocal.z;

// Apply selective brake force
Vector3 brakeForce = brakeVector.normalized * spaceBrakeForce * ratio;
rb.AddForce(transform.TransformDirection(brakeForce), ForceMode.Acceleration);
```

**This creates intelligent flight assist behavior:**
- **Example 1**: Holding W (forward) while drifting right → Brake only stops rightward drift
- **Example 2**: Holding D (strafe right) while moving forward → Brake only stops forward motion
- **Example 3**: No input while tumbling → Brake stops all motion in all directions
- **Example 4**: Full 6DoF control maintained - accelerate where you want, brake where you don't

### Fuel Consumption Formula
Fuel consumption is dynamically calculated based on the actual brake force being applied:

```csharp
float normalizedVelocity = Mathf.Clamp01(velocityMagnitude / 50f);
float normalizedForce = Mathf.Clamp01(actualBrakeMagnitude / spaceBrakeForce);
float fuelConsumption = spaceBrakeFuelRate * normalizedForce * normalizedVelocity * fuelEfficiency * Time.fixedDeltaTime;
```

**Factors:**
- **Base Rate**: `spaceBrakeFuelRate` (default 0.25)
- **Force Ratio**: Actual brake force applied / max force (0-1, scales with what's actually being braked)
- **Velocity Factor**: Normalized velocity (0-1, based on speed up to ~50 units/sec)
- **Fuel Efficiency**: From upgrade system (1.0 - level * 0.08, so level 10 = 0.2x consumption)

**Important**: Fuel is only consumed proportional to the actual braking being done. If you're thrusting forward while the brake only stops lateral drift, fuel consumption is minimal!

This means:
- **Slow speeds** (< 5 units/sec): ~10% fuel consumption
- **Medium speeds** (25 units/sec): ~50% fuel consumption  
- **High speeds** (50+ units/sec): 100% fuel consumption
- **Fuel Efficiency Level 1**: 92% consumption (0.92x)
- **Fuel Efficiency Level 10**: 20% consumption (0.20x)

## Configuration
You can adjust the space brake behavior in the Unity Inspector on the PlayerController component:

- **Space Brake Force**: Higher values = faster deceleration (default: 30)
- **Space Brake Fuel Rate**: Base fuel consumption rate (default: 0.25/second max)
  - Note: Actual consumption scales with velocity and fuel efficiency upgrades

## Usage Tips
1. **Flight Assist Mode**: Leave space brake ON for easier flight control - it acts like flight assist in Elite Dangerous
2. **Precision Maneuvering**: Thrust in one direction while drift is automatically cancelled in others
3. **Strafing Combat**: Strafe sideways while the brake keeps you from drifting forward/back
4. **Quick Direction Changes**: Reverse thrust while brake handles residual drift
5. **Fuel Efficient**: Only uses fuel for actual braking, not when you're actively thrusting
6. **Landing Approaches**: Perfect for station docking - thrust toward station while brake kills all lateral drift
7. **Traditional 6DoF**: Turn it OFF for pure Newtonian physics with no automatic stabilization

## Technical Notes
- Uses Unity's new Input System (not legacy Input)
- Toggle-based rather than hold-based for easier gameplay
- Integrates with existing fuel system via ShipStats component
- Only active during flight (disabled when docked)
- Frame-independent using Time.fixedDeltaTime for fuel consumption

## Future Enhancements
Possible improvements for the space brake system:
- Visual feedback (brake indicator light in UI)
- Audio feedback (thruster sound when active)
- Particle effects showing counter-thrust
- Different brake modes (soft/hard braking)
- Upgrade system to improve brake force efficiency
