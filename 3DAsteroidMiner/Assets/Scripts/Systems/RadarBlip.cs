using UnityEngine;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// Individual radar blip representation on the radar display.
    /// Handles visual appearance, color, and fade effects for radar contacts.
    /// Includes elevation indicator line showing vertical position relative to ship.
    /// </summary>
    public class RadarBlip : MonoBehaviour
    {
        // ===== Components =====
        private Renderer blipRenderer;
        private MaterialPropertyBlock propertyBlock;
        private GameObject elevationLineObject;      // Child GameObject for elevation line
        private GameObject elevationCircleObject;    // Child GameObject for elevation circle
        private LineRenderer elevationLine;
        private LineRenderer elevationCircle;
        
        // ===== Cached Properties =====
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
        
        // ===== State =====
        private Color currentColor;
        private float pulseTimer = 0f;
        private bool shouldPulse = false;
        private Vector3 lastLocalPosition;
        private Transform targetTransform; // The actual asteroid/object being tracked
        
        // ===== Elevation Line Settings =====
        private bool showElevationLine = true;
        private float elevationLineWidth = 0.03f;  // Thinner lines
        private float elevationCircleRadius = 0.03f;  // Larger circle
        private int elevationCircleSegments = 20;  // Smoother circle
        private bool showElevationCircle = false;  // Disable the circle/halo
        
        [Header("Debug")]
        [SerializeField] private bool debugElevationLines = true; // Enable for troubleshooting
        
        // ===== Unity Lifecycle =====
        private void Awake()
        {
            blipRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            
            // Create elevation indicator line
            CreateElevationLine();
        }
        
        private void OnEnable()
        {
            // Ensure elevation line is properly initialized when reactivated from pool
            if (elevationLine == null)
            {
                CreateElevationLine();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up elevation line GameObject and its components
            if (elevationLineObject != null)
            {
                if (elevationLine != null && elevationLine.material != null)
                {
                    Destroy(elevationLine.material);
                }
                Destroy(elevationLineObject);
            }
            
            // Clean up elevation circle GameObject and its components
            if (elevationCircleObject != null)
            {
                if (elevationCircle != null && elevationCircle.material != null)
                {
                    Destroy(elevationCircle.material);
                }
                Destroy(elevationCircleObject);
            }
        }
        
        private void Update()
        {
            if (shouldPulse)
            {
                UpdatePulseEffect();
            }
        }
        
        private void LateUpdate()
        {
            // Update elevation lines every frame since the radar rotates
            // The elevation lines need to recalculate based on current world positions
            if (showElevationLine && elevationLine != null && elevationCircle != null && targetTransform != null)
            {
                UpdateElevationLine();
            }
        }
        
        // ===== Blip Update =====
        
        /// <summary>
        /// Update the blip's appearance based on radar contact data
        /// </summary>
        public void UpdateBlip(RadarSystem.RadarContact contact, bool fadeByDistance, float maxRange)
        {
            if (contact == null || blipRenderer == null) return;
            
            // Store the target transform for elevation line calculations
            targetTransform = contact.transform;
            
            // Ensure elevation components exist (safety check for pooled objects)
            if (elevationLine == null || elevationCircle == null)
            {
                CreateElevationLine();
            }
            
            // Set base color
            currentColor = contact.displayColor;
            
            // Apply distance fade
            if (fadeByDistance)
            {
                float distanceRatio = Mathf.Clamp01(contact.distance / maxRange);
                float alpha = Mathf.Lerp(1f, 0.3f, distanceRatio); // Fade from 100% to 30%
                currentColor.a = alpha;
            }
            
            // Apply color to material
            ApplyColor(currentColor);
            
            // Scale based on distance (closer = larger)
            if (fadeByDistance)
            {
                float distanceRatio = Mathf.Clamp01(contact.distance / maxRange);
                float scale = Mathf.Lerp(1.5f, 0.5f, distanceRatio);
                transform.localScale = Vector3.one * scale * 0.005f; // Base scale
            }
            
            // ALWAYS update elevation line when position changes
            // This is critical - the position changed, so the line must be recalculated
            if (transform.localPosition != lastLocalPosition)
            {
                lastLocalPosition = transform.localPosition;
                UpdateElevationLine(); // Update immediately when position changes
            }
        }
        
        /// <summary>
        /// Set the blip color directly
        /// </summary>
        public void SetColor(Color color)
        {
            currentColor = color;
            ApplyColor(color);
        }
        
        /// <summary>
        /// Enable pulse effect for important targets
        /// </summary>
        public void SetPulse(bool pulse)
        {
            shouldPulse = pulse;
            pulseTimer = 0f;
        }
        
        // ===== Visual Effects =====
        
        private void ApplyColor(Color color)
        {
            if (blipRenderer == null) return;
            
            // Use MaterialPropertyBlock for per-instance properties (no material duplication)
            blipRenderer.GetPropertyBlock(propertyBlock);
            
            propertyBlock.SetColor(ColorProperty, color);
            
            // Set emission for holographic glow
            Color emissionColor = color * 2f; // Boost emission intensity
            propertyBlock.SetColor(EmissionColorProperty, emissionColor);
            
            blipRenderer.SetPropertyBlock(propertyBlock);
        }
        
        private void UpdatePulseEffect()
        {
            pulseTimer += Time.deltaTime * 2f; // Pulse speed
            
            float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0 to 1
            
            Color pulseColor = currentColor;
            pulseColor.a = Mathf.Lerp(0.3f, 1f, pulse);
            
            ApplyColor(pulseColor);
        }
        
        /// <summary>
        /// Highlight this blip (for targeting, selection, etc.)
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (highlighted)
            {
                // Brighten and scale up
                Color highlightColor = currentColor * 1.5f;
                highlightColor.a = 1f;
                ApplyColor(highlightColor);
                transform.localScale *= 1.3f;
            }
            else
            {
                // Reset to normal
                ApplyColor(currentColor);
            }
        }
        
        // ===== Elevation Line Management =====
        
        /// <summary>
        /// Create the vertical elevation indicator line and horizontal circle as child GameObjects
        /// Hierarchy: RadarBlip > ElevationLine & ElevationCircle
        /// </summary>
        private void CreateElevationLine()
        {
            // ===== VERTICAL LINE (Child GameObject) =====
            if (elevationLineObject == null)
            {
                elevationLineObject = new GameObject("ElevationLine");
                elevationLineObject.transform.SetParent(transform, false);
                elevationLineObject.transform.localPosition = Vector3.zero;
                elevationLineObject.transform.localRotation = Quaternion.identity;
                elevationLineObject.transform.localScale = Vector3.one;
                elevationLineObject.layer = gameObject.layer;
                
                elevationLine = elevationLineObject.AddComponent<LineRenderer>();
                
                // Configure vertical line
                elevationLine.useWorldSpace = false; // Use local space relative to parent (RadarBlip)
                elevationLine.positionCount = 2;
                elevationLine.startWidth = elevationLineWidth;
                elevationLine.endWidth = elevationLineWidth;
                
                // Create material
                Material lineMaterial = CreateLineMaterial();
                elevationLine.material = lineMaterial;
                
                // Configure rendering
                ConfigureLineRenderer(elevationLine);
                
                if (debugElevationLines)
                {
                    Debug.Log($"RadarBlip: Created elevation line child on {gameObject.name}, useWorldSpace={elevationLine.useWorldSpace}");
                }
            }
            
            // ===== HORIZONTAL CIRCLE (Child GameObject) =====
            if (elevationCircleObject == null)
            {
                elevationCircleObject = new GameObject("ElevationCircle");
                elevationCircleObject.transform.SetParent(transform, false);
                elevationCircleObject.transform.localPosition = Vector3.zero;
                elevationCircleObject.transform.localRotation = Quaternion.identity;
                elevationCircleObject.transform.localScale = Vector3.one;
                elevationCircleObject.layer = gameObject.layer;
                
                elevationCircle = elevationCircleObject.AddComponent<LineRenderer>();
                
                // Configure circle
                elevationCircle.useWorldSpace = false; // Use local space relative to parent (RadarBlip)
                elevationCircle.positionCount = elevationCircleSegments + 1;
                elevationCircle.startWidth = elevationLineWidth;
                elevationCircle.endWidth = elevationLineWidth;
                elevationCircle.loop = true;
                
                // Create material
                Material circleMaterial = CreateLineMaterial();
                elevationCircle.material = circleMaterial;
                
                // Configure rendering
                ConfigureLineRenderer(elevationCircle);
                
                if (debugElevationLines)
                {
                    Debug.Log($"RadarBlip: Created elevation circle child on {gameObject.name}, useWorldSpace={elevationCircle.useWorldSpace}, segments={elevationCircleSegments}");
                }
            }
        }
        
        /// <summary>
        /// Create a material suitable for line rendering
        /// </summary>
        private Material CreateLineMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = Color.white; // Will be tinted by LineRenderer colors
            
            return material;
        }
        
        /// <summary>
        /// Configure common LineRenderer settings
        /// </summary>
        private void ConfigureLineRenderer(LineRenderer lr)
        {
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            lr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            lr.alignment = LineAlignment.View;
            lr.sortingOrder = 1000;
            lr.numCapVertices = 5;
            lr.numCornerVertices = 5;
            lr.textureMode = LineTextureMode.Stretch;
            lr.enabled = true;
        }
        
        /// <summary>
        /// Update the elevation line and circle to show vertical position relative to ship level
        /// </summary>
        private void UpdateElevationLine()
        {
            // Ensure components exist
            if (elevationLine == null || elevationCircle == null)
            {
                if (debugElevationLines)
                    Debug.LogWarning($"RadarBlip: Elevation components missing on {gameObject.name}, creating...");
                CreateElevationLine();
                if (elevationLine == null || elevationCircle == null)
                {
                    Debug.LogError($"RadarBlip: Failed to create elevation components on {gameObject.name}");
                    return;
                }
            }
            
            // CRITICAL: Use LOCAL SPACE relative to the radar display
            // The radar is fixed to dashboard, so we work in radar's local coordinate system
            if (elevationLine.useWorldSpace)
            {
                elevationLine.useWorldSpace = false;
            }
            if (elevationCircle.useWorldSpace)
            {
                elevationCircle.useWorldSpace = false;
            }
            
            if (!showElevationLine)
            {
                elevationLine.enabled = false;
                elevationCircle.enabled = false;
                return;
            }
            
            // If we don't have a target transform, can't draw elevation line
            if (targetTransform == null)
            {
                elevationLine.enabled = false;
                elevationCircle.enabled = false;
                return;
            }
            
            // Calculate ACTUAL vertical distance in world space
            Vector3 asteroidWorldPos = targetTransform.position;
            
            // Get RadarSystem (grandparent) position instead of RadarDisplay (parent)
            Transform radarSystemTransform = transform.parent.parent; // RadarSystem is parent of RadarDisplay
            if (radarSystemTransform == null)
            {
                Debug.LogWarning("RadarBlip: Cannot find RadarSystem (grandparent)");
                elevationLine.enabled = false;
                elevationCircle.enabled = false;
                return;
            }
            
            Vector3 radarWorldPos = radarSystemTransform.position; // RadarSystem position
            float actualVerticalDistance = asteroidWorldPos.y - radarWorldPos.y;
            
            // Only show if there's meaningful vertical displacement
            bool hasElevation = Mathf.Abs(actualVerticalDistance) > 0.001f;
            
            if (!hasElevation)
            {
                elevationLine.enabled = false;
                elevationCircle.enabled = false;
                if (debugElevationLines)
                {
                    Debug.Log($"RadarBlip: No elevation (actualDistance={actualVerticalDistance})");
                }
                return;
            }
            
            // Get the blip's LOCAL position for XZ positioning
            Vector3 blipLocalPos = transform.localPosition;
            
            // CRITICAL: Account for the parent's scale!
            // If the radar display has a scale of 0.01, we need to compensate
            Vector3 parentScale = transform.parent.lossyScale;
            float parentScaleY = Mathf.Abs(parentScale.y);
            
            // Scale the actual vertical distance to radar display units
            // Adjust for parent scale so the line appears correct size
            float verticalScaleFactor = 0.001f / parentScaleY; // Compensate for parent scale
            float scaledVerticalDistance = actualVerticalDistance * verticalScaleFactor;
            
            // Enable components
            elevationLine.enabled = true;
            elevationCircle.enabled = true;
            
            // Update color
            Color lineColor = currentColor;
            lineColor.a = 0.6f;  // Slightly transparent for better visibility
            
            // === VERTICAL LINE (LOCAL SPACE RELATIVE TO RADAR DISPLAY) ===
            // GOAL: Draw a line from the blip toward the radar center
            //       Using the radar display's LOCAL coordinate system
            //       Reference point is the RadarSystem (grandparent) position
            //       The line should be STRICTLY VERTICAL in radar's local Y-axis
            //
            // Hierarchy:
            //   RadarSystem (grandparent, reference position)
            //     └─ RadarDisplay (parent, FIXED to dashboard)
            //        └─ BlipContainer (local 0,0,0 relative to RadarDisplay)
            //           └─ RadarBlip (at blipLocalPos relative to RadarDisplay)
            //              └─ ElevationLine (child of RadarBlip, uses local space)
            
            // The blip's local position relative to the radar display (parent)
            // This was already calculated earlier - blipLocalPos
            
            // In the radar's coordinate system:
            // - Radar center is at (0, 0, 0)
            // - Blip is at blipLocalPos
            // - We want a vertical line from blip toward center (only Y changes)
            
            // In the ElevationLine's local space (child of RadarBlip):
            // - Blip center is at (0, 0, 0)
            // - Radar center is at -blipLocalPos
            // - We only want the Y component for a vertical line
            
            float verticalOffsetToRadarCenter = -blipLocalPos.y;
            
            // Calculate line length based on ACTUAL world distance
            // Use a scale factor so lines are visible on the radar
            float visualScaleFactor = 100f;
            float lineLength = Mathf.Abs(actualVerticalDistance) * 0.001f * visualScaleFactor;
            
            // Clamp to reasonable range for radar display
            lineLength = Mathf.Clamp(lineLength, 0.1f, 5.0f);
            
            // Clamp line length to not exceed the distance to radar center
            // This ensures the line stops at the radar plane, not beyond it
            float maxLineLength = Mathf.Abs(verticalOffsetToRadarCenter);
            if (lineLength > maxLineLength)
            {
                lineLength = maxLineLength;
            }
            
            // The line direction points toward radar center
            // If blip is above center (positive Y), line goes down (negative)
            // If blip is below center (negative Y), line goes up (positive)
            float lineDirection = blipLocalPos.y > 0 ? -1f : 1f;
            
            if (debugElevationLines)
            {
                Debug.Log($"=== ELEVATION LINE CALCULATION ===");
                Debug.Log($"  Asteroid world Y: {asteroidWorldPos.y}m");
                Debug.Log($"  Radar world Y: {radarWorldPos.y}m");
                Debug.Log($"  ACTUAL vertical distance: {actualVerticalDistance}m");
                Debug.Log($"  Blip local pos (in radar space): {blipLocalPos}");
                Debug.Log($"  Vertical offset to radar center: {verticalOffsetToRadarCenter}");
                Debug.Log($"  Line direction (1=up, -1=down): {lineDirection}");
                Debug.Log($"  Visual scale factor: {visualScaleFactor}");
                Debug.Log($"  Calculated line length: {Mathf.Abs(actualVerticalDistance) * 0.001f * visualScaleFactor}");
                Debug.Log($"  Max line length (to radar center): {maxLineLength}");
                Debug.Log($"  Final clamped line length: {lineLength}");
            }
            
            // Draw STRICTLY VERTICAL line in LOCAL SPACE (relative to RadarBlip)
            // Start: At blip center in ElevationLine's local space (0, 0, 0)
            // End: Move ONLY in Y direction toward the radar center
            Vector3 lineStart = Vector3.zero;
            Vector3 lineEnd = new Vector3(0f, lineDirection * lineLength, 0f);
            
            if (debugElevationLines)
            {
                Debug.Log($"  Line start (local): {lineStart}");
                Debug.Log($"  Line end (local): {lineEnd}");
                Debug.Log($"  Final line length: {Vector3.Distance(lineStart, lineEnd)}");
            }
            
            // Set positions in LOCAL SPACE (relative to RadarBlip, which is relative to RadarDisplay)
            elevationLine.SetPosition(0, lineStart);
            elevationLine.SetPosition(1, lineEnd);
            
            // Color gradient - start faded (at radar plane), end bright (at blip)
            Color fadedColor = lineColor;
            fadedColor.a = 0.3f;
            elevationLine.startColor = fadedColor;
            elevationLine.endColor = lineColor;
            
            // === HORIZONTAL CIRCLE (LOCAL SPACE) ===
            // Circle at the end of the line showing where it intersects the radar plane
            // Only show if enabled
            if (showElevationCircle && elevationCircle != null)
            {
                for (int i = 0; i <= elevationCircleSegments; i++)
                {
                    float angle = (float)i / elevationCircleSegments * Mathf.PI * 2f;
                    
                    // Calculate circle point around the line's end point
                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle) * elevationCircleRadius,
                        0f,
                        Mathf.Sin(angle) * elevationCircleRadius
                    );
                    
                    // Circle at the end of the elevation line
                    Vector3 circlePoint = lineEnd + offset;
                    
                    elevationCircle.SetPosition(i, circlePoint);
                }
                
                elevationCircle.startColor = lineColor;
                elevationCircle.endColor = lineColor;
                elevationCircle.enabled = true;
            }
            else if (elevationCircle != null)
            {
                elevationCircle.enabled = false;
            }
            
            if (debugElevationLines)
            {
                Debug.Log($"=== RadarBlip {gameObject.name} ELEVATION UPDATE ===");
                Debug.Log($"  actualVerticalDistance={actualVerticalDistance}m");
                Debug.Log($"  scaledVerticalDistance={scaledVerticalDistance}");
                Debug.Log($"  lineStart={lineStart}, lineEnd={lineEnd}");
                Debug.Log($"  lineEnabled={elevationLine.enabled}, lineUseWorldSpace={elevationLine.useWorldSpace}");
                Debug.Log($"  linePositions: 0={elevationLine.GetPosition(0)}, 1={elevationLine.GetPosition(1)}");
                Debug.Log($"  lineWidth: start={elevationLine.startWidth}, end={elevationLine.endWidth}");
                Debug.Log($"  lineMaterial: {(elevationLine.material != null ? elevationLine.material.name : "NULL")}");
                Debug.Log($"  circleEnabled={elevationCircle.enabled}");
            }
        }
        
        /// <summary>
        /// Toggle elevation line visibility
        /// </summary>
        public void SetElevationLineVisible(bool visible)
        {
            showElevationLine = visible;
            
            if (debugElevationLines)
            {
                Debug.Log($"RadarBlip {gameObject.name}: SetElevationLineVisible({visible})");
            }
            
            if (elevationLine == null || elevationCircle == null)
            {
                if (debugElevationLines)
                    Debug.LogWarning($"RadarBlip {gameObject.name}: Elevation components missing, creating...");
                CreateElevationLine();
            }
            
            if (elevationLine != null)
            {
                if (!visible)
                {
                    elevationLine.enabled = false;
                }
                else
                {
                    UpdateElevationLine();
                }
            }
            
            if (elevationCircle != null)
            {
                if (!visible)
                {
                    elevationCircle.enabled = false;
                }
                else
                {
                    UpdateElevationLine();
                }
            }
        }
        
        // ===== Diagnostic Methods =====
        
        /// <summary>
        /// Validate the elevation line setup for debugging
        /// </summary>
        public void ValidateElevationSetup()
        {
            Debug.Log("=== ELEVATION LINE DIAGNOSTIC ===");
            Debug.Log($"Blip: {gameObject.name}");
            Debug.Log($"  Position: local={transform.localPosition}, world={transform.position}");
            Debug.Log($"  Parent: {(transform.parent != null ? transform.parent.name : "NULL")}");
            Debug.Log($"  Show Elevation Lines: {showElevationLine}");
            
            Debug.Log($"\nVertical Line:");
            if (elevationLine == null)
            {
                Debug.LogError("  elevationLine LineRenderer is NULL!");
            }
            else
            {
                Debug.Log($"  LineRenderer: attached to {elevationLine.gameObject.name}, enabled={elevationLine.enabled}");
                Debug.Log($"  Positions: count={elevationLine.positionCount}");
                if (elevationLine.positionCount >= 2)
                {
                    Vector3 p0 = elevationLine.GetPosition(0);
                    Vector3 p1 = elevationLine.GetPosition(1);
                    Debug.Log($"    [0] world space={p0}");
                    Debug.Log($"    [1] world space={p1}");
                    Debug.Log($"    Distance: {Vector3.Distance(p0, p1)}");
                }
                Debug.Log($"  Width: start={elevationLine.startWidth}, end={elevationLine.endWidth}");
                Debug.Log($"  World Space: {elevationLine.useWorldSpace}");
                Debug.Log($"  Material: {(elevationLine.material != null ? elevationLine.material.name : "NULL")}");
                Debug.Log($"  Shader: {(elevationLine.material != null && elevationLine.material.shader != null ? elevationLine.material.shader.name : "NULL")}");
            }
            
            Debug.Log($"\nHorizontal Circle:");
            if (elevationCircle == null)
            {
                Debug.LogError("  elevationCircle LineRenderer is NULL!");
            }
            else
            {
                Debug.Log($"  LineRenderer: attached to {elevationCircle.gameObject.name}, enabled={elevationCircle.enabled}");
                Debug.Log($"  Positions: count={elevationCircle.positionCount}");
                Debug.Log($"  Width: start={elevationCircle.startWidth}, end={elevationCircle.endWidth}");
                Debug.Log($"  Loop: {elevationCircle.loop}");
                Debug.Log($"  World Space: {elevationCircle.useWorldSpace}");
            }
            
            Debug.Log("=================================\n");
        }
        
        // ===== Debug Visualization =====
        private void OnDrawGizmos()
        {
            if (!debugElevationLines) return;
            
            // Draw elevation line (already in world space)
            if (elevationLine != null && elevationLine.positionCount >= 2)
            {
                Vector3 worldStart = elevationLine.GetPosition(0);
                Vector3 worldEnd = elevationLine.GetPosition(1);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(worldStart, worldEnd);
                Gizmos.DrawWireSphere(worldStart, 0.01f);
                Gizmos.DrawWireSphere(worldEnd, 0.01f);
            }
            
            // Draw elevation circle (already in world space)
            if (elevationCircle != null && elevationCircle.positionCount > 2)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < elevationCircle.positionCount - 1; i++)
                {
                    Vector3 worldStart = elevationCircle.GetPosition(i);
                    Vector3 worldEnd = elevationCircle.GetPosition(i + 1);
                    Gizmos.DrawLine(worldStart, worldEnd);
                }
            }
        }
    }
}
