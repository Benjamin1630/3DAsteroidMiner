using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// 3D holographic radar display similar to Star Citizen.
    /// Renders radar contacts on a circular/spherical display with the ship at center.
    /// Designed to be placed on a ship dashboard as a 3D UI element.
    /// </summary>
    public class RadarDisplay : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The RadarSystem that provides contact data")]
        [SerializeField] private RadarSystem radarSystem;
        
        [Tooltip("Parent transform for radar blips (will contain all spawned blip objects)")]
        [SerializeField] private Transform blipContainer;
        
        [Header("Display Configuration")]
        [Tooltip("Type of radar display")]
        public RadarDisplayMode displayMode = RadarDisplayMode.Circular3D;
        
        [Tooltip("Scale of the radar display in world units")]
        [SerializeField] private float displayRadius = 0.15f;
        
        [Tooltip("Height range for 3D radar (for elevation visualization). Increase this to make elevation lines longer.")]
        [SerializeField] private float displayHeight = 0.15f;  // Increased to match radius for better visibility
        
        [Tooltip("Scale factor for blip sizes")]
        [SerializeField] private float blipScale = 0.005f;
        
        [Tooltip("Fade blips based on distance")]
        [SerializeField] private bool fadeByDistance = true;
        
        [Header("Elevation Indicators")]
        [Tooltip("Show vertical lines indicating asteroid elevation relative to ship")]
        public bool showElevationLines = true;
        
        [Header("Visual Settings")]
        [Tooltip("Show the radar screen background (disable to show only grid and blips)")]
        public bool showRadarScreen = false;
        
        [Tooltip("Material for radar blips")]
        [SerializeField] private Material blipMaterial;
        
        [Tooltip("Material for the radar grid/screen")]
        [SerializeField] private Material radarScreenMaterial;
        
        [Tooltip("Color for radar grid lines")]
        [SerializeField] private Color gridColor = new Color(0.2f, 0.8f, 1f, 0.5f);
        
        [Tooltip("Number of grid rings to draw")]
        [SerializeField] private int gridRingCount = 4;
        
        [Tooltip("Number of grid radial lines")]
        [SerializeField] private int gridRadialLines = 8;
        
        [Tooltip("Show grid lines on radar display")]
        public bool showGrid = true;
        
        [Header("Ship Indicator")]
        [Tooltip("Prefab for ship center indicator")]
        [SerializeField] private GameObject shipIndicatorPrefab;
        
        [Tooltip("Scale of ship indicator")]
        [SerializeField] private float shipIndicatorScale = 0.01f;
        
        [Header("Performance")]
        [Tooltip("Maximum number of blips to display")]
        [SerializeField] private int maxBlips = 100;
        
        [Tooltip("Use object pooling for blips")]
        [SerializeField] private bool useObjectPooling = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // ===== Display Modes =====
        public enum RadarDisplayMode
        {
            Circular2D,      // Flat circular radar (top-down)
            Circular3D,      // 3D spherical radar with elevation
            Cylindrical      // Cylindrical projection
        }
        
        // ===== Private State =====
        private List<RadarBlip> activeBlips = new List<RadarBlip>();
        private Queue<RadarBlip> blipPool = new Queue<RadarBlip>();
        private LineRenderer[] gridLines;
        private GameObject shipIndicator;
        private bool lastElevationLinesState = true;
        
        // ===== Unity Lifecycle =====
        private void Awake()
        {
            // Auto-find radar system if not assigned
            if (radarSystem == null)
            {
                radarSystem = FindFirstObjectByType<RadarSystem>();
                if (radarSystem == null)
                {
                    Debug.LogError("RadarDisplay: No RadarSystem found! Please assign or create one.");
                }
            }
            
            // Handle radar screen background visibility
            MeshRenderer screenRenderer = GetComponent<MeshRenderer>();
            if (screenRenderer != null)
            {
                screenRenderer.enabled = showRadarScreen;
            }
            
            // Create blip container if not assigned
            if (blipContainer == null)
            {
                GameObject container = new GameObject("BlipContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                container.transform.localRotation = Quaternion.identity;
                container.transform.localScale = Vector3.one;
                blipContainer = container.transform;
            }
            else
            {
                // Ensure existing container has correct local transform
                blipContainer.localPosition = Vector3.zero;
                blipContainer.localRotation = Quaternion.identity;
                blipContainer.localScale = Vector3.one;
            }
            
            // Initialize grid
            InitializeRadarGrid();
            
            // Create ship indicator
            CreateShipIndicator();
        }
        
        private void OnEnable()
        {
            // Pre-populate blip pool if using pooling
            if (useObjectPooling)
            {
                for (int i = 0; i < 20; i++)
                {
                    RadarBlip blip = CreateBlipInstance();
                    blip.gameObject.SetActive(false);
                    blipPool.Enqueue(blip);
                }
            }
        }
        
        private void Update()
        {
            if (radarSystem == null) return;
            
            // Keep blip container in local space (rotates with radar display)
            // Individual blips are positioned using ship-relative coordinates
            // Elevation lines use world space to stay vertical
            if (blipContainer != null)
            {
                blipContainer.localRotation = Quaternion.identity;
            }
            
            // Update ship indicator rotation to show forward direction
            // The ship indicator should point "up" in local radar space when ship faces forward
            if (shipIndicator != null && radarSystem.ShipTransform != null)
            {
                // Keep ship indicator at center, no rotation needed since radar rotates with ship
                shipIndicator.transform.localPosition = Vector3.zero;
                shipIndicator.transform.localRotation = Quaternion.identity;
            }
            
            // Handle grid visibility (only check when it changes)
            if (gridLines != null)
            {
                foreach (LineRenderer lr in gridLines)
                {
                    if (lr != null && lr.enabled != showGrid)
                        lr.enabled = showGrid;
                }
            }
            
            // Handle radar screen visibility (only check when it changes)
            MeshRenderer screenRenderer = GetComponent<MeshRenderer>();
            if (screenRenderer != null && screenRenderer.enabled != showRadarScreen)
            {
                screenRenderer.enabled = showRadarScreen;
            }
            
            // Update radar display every frame (but efficiently)
            UpdateRadarDisplay();
        }
        
        // ===== Display Update =====
        private void UpdateRadarDisplay()
        {
            List<RadarSystem.RadarContact> contacts = radarSystem.DetectedContacts;
            
            if (contacts == null || contacts.Count == 0)
            {
                // Hide all active blips when no contacts
                if (activeBlips.Count > 0)
                {
                    ReturnAllBlipsToPool();
                }
                return;
            }
            
            // Limit to max blips
            int blipsNeeded = Mathf.Min(contacts.Count, maxBlips);
            
            // Return excess blips to pool
            while (activeBlips.Count > blipsNeeded)
            {
                int lastIndex = activeBlips.Count - 1;
                RadarBlip blip = activeBlips[lastIndex];
                activeBlips.RemoveAt(lastIndex);
                
                if (useObjectPooling)
                {
                    blip.gameObject.SetActive(false);
                    blipPool.Enqueue(blip);
                }
                else
                {
                    Destroy(blip.gameObject);
                }
            }
            
            // Get more blips if needed
            while (activeBlips.Count < blipsNeeded)
            {
                RadarBlip blip = GetBlipFromPool();
                blip.SetElevationLineVisible(showElevationLines); // Initialize new blip
                activeBlips.Add(blip);
            }
            
            // Update existing blips with new contact data
            for (int i = 0; i < blipsNeeded; i++)
            {
                RadarSystem.RadarContact contact = contacts[i];
                
                if (contact.transform == null) continue;
                
                RadarBlip blip = activeBlips[i];
                
                // Calculate radar position
                Vector3 radarPosition = CalculateRadarPosition(contact);
                
                // Update blip
                blip.transform.localPosition = radarPosition;
                blip.UpdateBlip(contact, fadeByDistance, radarSystem.RadarRange);
            }
            
            // Only update elevation line visibility when it changes
            if (showElevationLines != lastElevationLinesState)
            {
                lastElevationLinesState = showElevationLines;
                foreach (RadarBlip blip in activeBlips)
                {
                    blip.SetElevationLineVisible(showElevationLines);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"RadarDisplay: Showing {activeBlips.Count} blips (pool: {blipPool.Count})");
            }
        }
        
        // ===== Position Calculation =====
        private Vector3 CalculateRadarPosition(RadarSystem.RadarContact contact)
        {
            // Get relative position to ship in world space
            Vector3 worldRelativePos = contact.relativePosition;
            
            // Transform to radar display's local space (handles both position and rotation)
            Vector3 relativePos = worldRelativePos;
            if (radarSystem.ShipTransform != null)
            {
                // Convert world direction to ship's local space
                // This makes the radar rotate with the ship
                relativePos = radarSystem.ShipTransform.InverseTransformDirection(worldRelativePos);
            }
            
            // Normalize to radar range
            float normalizedDistance = Mathf.Clamp01(contact.distance / radarSystem.RadarRange);
            
            Vector3 radarPos = Vector3.zero;
            
            switch (displayMode)
            {
                case RadarDisplayMode.Circular2D:
                    // Flat top-down view
                    radarPos.x = relativePos.x / radarSystem.RadarRange * displayRadius;
                    radarPos.z = relativePos.z / radarSystem.RadarRange * displayRadius;
                    radarPos.y = 0f;
                    break;
                
                case RadarDisplayMode.Circular3D:
                    // 3D spherical view with elevation
                    radarPos.x = relativePos.x / radarSystem.RadarRange * displayRadius;
                    radarPos.z = relativePos.z / radarSystem.RadarRange * displayRadius;
                    radarPos.y = relativePos.y / radarSystem.RadarRange * displayHeight;
                    break;
                
                case RadarDisplayMode.Cylindrical:
                    // Cylindrical projection (distance as radius, height as height)
                    Vector2 horizontalDir = new Vector2(relativePos.x, relativePos.z).normalized;
                    radarPos.x = horizontalDir.x * normalizedDistance * displayRadius;
                    radarPos.z = horizontalDir.y * normalizedDistance * displayRadius;
                    radarPos.y = relativePos.y / radarSystem.RadarRange * displayHeight;
                    break;
            }
            
            return radarPos;
        }
        
        // ===== Blip Management =====
        private RadarBlip GetBlipFromPool()
        {
            RadarBlip blip;
            
            if (useObjectPooling && blipPool.Count > 0)
            {
                blip = blipPool.Dequeue();
                blip.gameObject.SetActive(true);
            }
            else
            {
                blip = CreateBlipInstance();
            }
            
            return blip;
        }
        
        private void ReturnAllBlipsToPool()
        {
            foreach (RadarBlip blip in activeBlips)
            {
                if (useObjectPooling)
                {
                    blip.gameObject.SetActive(false);
                    blipPool.Enqueue(blip);
                }
                else
                {
                    Destroy(blip.gameObject);
                }
            }
            
            activeBlips.Clear();
        }
        
        private RadarBlip CreateBlipInstance()
        {
            // Create simple cube for blip
            GameObject blipObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blipObj.name = "RadarBlip";
            blipObj.transform.SetParent(blipContainer);
            blipObj.transform.localScale = Vector3.one * blipScale;
            
            // Remove collider (not needed for UI)
            Collider col = blipObj.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Set material
            if (blipMaterial != null)
            {
                blipObj.GetComponent<Renderer>().material = blipMaterial;
            }
            else
            {
                // Create simple emissive material
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_EmissionColor", Color.green);
                mat.EnableKeyword("_EMISSION");
                blipObj.GetComponent<Renderer>().material = mat;
            }
            
            // Add RadarBlip component
            RadarBlip blip = blipObj.AddComponent<RadarBlip>();
            
            return blip;
        }
        
        // ===== Grid Visualization =====
        private void InitializeRadarGrid()
        {
            // Create grid container
            GameObject gridContainer = new GameObject("RadarGrid");
            gridContainer.transform.SetParent(transform);
            gridContainer.transform.localPosition = Vector3.zero;
            gridContainer.transform.localRotation = Quaternion.identity;
            
            // Create grid lines (rings + radial lines)
            int totalLines = gridRingCount + gridRadialLines;
            gridLines = new LineRenderer[totalLines];
            
            // Create ring lines
            for (int i = 0; i < gridRingCount; i++)
            {
                GameObject lineObj = new GameObject($"GridRing_{i}");
                lineObj.transform.SetParent(gridContainer.transform);
                lineObj.transform.localPosition = Vector3.zero;
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = true;
                lr.widthMultiplier = 0.001f;
                lr.material = new Material(Shader.Find("Unlit/Color"));
                lr.material.color = gridColor;
                lr.startColor = gridColor;
                lr.endColor = gridColor;
                
                // Create circle points
                int segments = 32;
                lr.positionCount = segments;
                float ringRadius = displayRadius * (i + 1) / (float)gridRingCount;
                
                for (int j = 0; j < segments; j++)
                {
                    float angle = j * 2f * Mathf.PI / segments;
                    Vector3 pos = new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        0f,
                        Mathf.Sin(angle) * ringRadius
                    );
                    lr.SetPosition(j, pos);
                }
                
                gridLines[i] = lr;
            }
            
            // Create radial lines
            for (int i = 0; i < gridRadialLines; i++)
            {
                GameObject lineObj = new GameObject($"GridRadial_{i}");
                lineObj.transform.SetParent(gridContainer.transform);
                lineObj.transform.localPosition = Vector3.zero;
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.widthMultiplier = 0.001f;
                lr.material = new Material(Shader.Find("Unlit/Color"));
                lr.material.color = gridColor;
                lr.startColor = gridColor;
                lr.endColor = gridColor;
                lr.positionCount = 2;
                
                float angle = i * 2f * Mathf.PI / gridRadialLines;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, direction * displayRadius);
                
                gridLines[gridRingCount + i] = lr;
            }
        }
        
        private void CreateShipIndicator()
        {
            if (shipIndicatorPrefab != null)
            {
                shipIndicator = Instantiate(shipIndicatorPrefab, transform);
                shipIndicator.transform.localPosition = Vector3.zero;
                shipIndicator.transform.localScale = Vector3.one * shipIndicatorScale;
            }
            else
            {
                // Create simple ship indicator (pyramid)
                shipIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shipIndicator.name = "ShipIndicator";
                shipIndicator.transform.SetParent(transform);
                shipIndicator.transform.localPosition = Vector3.zero;
                shipIndicator.transform.localScale = Vector3.one * shipIndicatorScale;
                
                // Remove collider
                Collider col = shipIndicator.GetComponent<Collider>();
                if (col != null) Destroy(col);
                
                // Set material
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
                mat.SetColor("_EmissionColor", Color.cyan * 2f);
                mat.EnableKeyword("_EMISSION");
                shipIndicator.GetComponent<Renderer>().material = mat;
            }
        }
        
        // ===== Public API =====
        
        /// <summary>
        /// Set the display mode (2D, 3D, Cylindrical)
        /// </summary>
        public void SetDisplayMode(RadarDisplayMode mode)
        {
            displayMode = mode;
        }
        
        /// <summary>
        /// Set radar display radius
        /// </summary>
        public void SetDisplayRadius(float radius)
        {
            displayRadius = Mathf.Max(0.05f, radius);
            InitializeRadarGrid(); // Rebuild grid with new size
        }
        
        // ===== Debug =====
        private void OnDrawGizmosSelected()
        {
            // Draw radar display bounds
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, displayRadius);
            
            if (displayMode == RadarDisplayMode.Circular3D || displayMode == RadarDisplayMode.Cylindrical)
            {
                Gizmos.DrawWireCube(transform.position + Vector3.up * displayHeight * 0.5f, 
                    new Vector3(displayRadius * 2f, displayHeight, displayRadius * 2f));
            }
        }
    }
}
