using System.Collections.Generic;
using UnityEngine;
using AsteroidMiner.Entities;
using AsteroidMiner.Data;

namespace AsteroidMiner.Systems
{
    /// <summary>
    /// 3D Radar system similar to Star Citizen that tracks nearby asteroids and objects.
    /// Provides data to RadarDisplay for visual representation.
    /// </summary>
    public class RadarSystem : MonoBehaviour
    {
        [Header("Radar Configuration")]
        [Tooltip("Maximum distance to detect objects on radar")]
        [SerializeField] private float radarRange = 1000f;
        
        [Tooltip("How often to update radar (in seconds). Lower = more accurate but more expensive")]
        [SerializeField] private float updateInterval = 0.1f;
        
        [Tooltip("Layers to detect on radar (Asteroids, Hazards, etc.)")]
        [SerializeField] private LayerMask radarLayerMask = -1;
        
        [Header("References")]
        [Tooltip("The ship transform (center of radar)")]
        [SerializeField] private Transform shipTransform;
        
        [Header("Contact Filtering")]
        [Tooltip("Show asteroids on radar")]
        public bool showAsteroids = true;
        
        [Tooltip("Show hazards on radar")]
        public bool showHazards = true;
        
        [Tooltip("Show NPCs on radar")]
        public bool showNPCs = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // ===== Public Properties =====
        public float RadarRange => radarRange;
        public Transform ShipTransform => shipTransform;
        public List<RadarContact> DetectedContacts => detectedContacts;
        
        // ===== Private State =====
        private List<RadarContact> detectedContacts = new List<RadarContact>();
        private float updateTimer = 0f;
        private Collider[] detectionBuffer = new Collider[200]; // Preallocated buffer for performance
        
        // ===== Contact Types =====
        public enum ContactType
        {
            Asteroid,
            Hazard,
            NPC,
            Station,
            Unknown
        }
        
        // ===== Radar Contact Data =====
        public class RadarContact
        {
            public Transform transform;
            public ContactType type;
            public Vector3 relativePosition; // Position relative to ship
            public float distance;
            public string displayName;
            public Color displayColor;
            public Asteroid asteroidComponent; // Optional: for asteroid-specific data
            
            public RadarContact(Transform t, ContactType ct, Vector3 relPos, float dist)
            {
                transform = t;
                type = ct;
                relativePosition = relPos;
                distance = dist;
                displayName = t.name;
                displayColor = GetColorForType(ct);
            }
            
            private Color GetColorForType(ContactType type)
            {
                switch (type)
                {
                    case ContactType.Asteroid:
                        return new Color(0.2f, 0.8f, 0.2f, 1f); // Green
                    case ContactType.Hazard:
                        return new Color(1f, 0.3f, 0.1f, 1f); // Red
                    case ContactType.NPC:
                        return new Color(1f, 0.8f, 0.2f, 1f); // Yellow
                    case ContactType.Station:
                        return new Color(0.2f, 0.5f, 1f, 1f); // Blue
                    default:
                        return Color.white;
                }
            }
        }
        
        // ===== Unity Lifecycle =====
        private void Awake()
        {
            // Auto-find ship if not assigned
            if (shipTransform == null)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    shipTransform = player.transform;
                }
                else
                {
                    Debug.LogWarning("RadarSystem: No ship transform assigned and couldn't find PlayerController!");
                }
            }
        }
        
        private void Update()
        {
            if (shipTransform == null) return;
            
            updateTimer += Time.deltaTime;
            
            if (updateTimer >= updateInterval)
            {
                UpdateRadarContacts();
                updateTimer = 0f;
            }
        }
        
        // ===== Radar Update =====
        private void UpdateRadarContacts()
        {
            detectedContacts.Clear();
            
            // Use OverlapSphereNonAlloc for performance (no allocations)
            int hitCount = Physics.OverlapSphereNonAlloc(
                shipTransform.position,
                radarRange,
                detectionBuffer,
                radarLayerMask,
                QueryTriggerInteraction.Ignore
            );
            
            if (showDebugInfo)
            {
                Debug.Log($"RadarSystem: Detected {hitCount} objects within {radarRange}m");
            }
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = detectionBuffer[i];
                
                // Skip if it's the ship itself
                if (col.transform == shipTransform || col.transform.IsChildOf(shipTransform))
                    continue;
                
                // Determine contact type
                ContactType contactType = DetermineContactType(col.gameObject);
                
                // Filter based on settings
                if (!ShouldShowContact(contactType))
                    continue;
                
                // Calculate relative position and distance
                Vector3 relativePosition = col.transform.position - shipTransform.position;
                float distance = relativePosition.magnitude;
                
                // Create radar contact
                RadarContact contact = new RadarContact(col.transform, contactType, relativePosition, distance);
                
                // Add asteroid-specific data
                if (contactType == ContactType.Asteroid)
                {
                    Asteroid asteroid = col.GetComponentInParent<Asteroid>();
                    if (asteroid != null)
                    {
                        contact.asteroidComponent = asteroid;
                        contact.displayName = asteroid.Type != null ? asteroid.Type.resourceName : "Asteroid";
                        
                        // Color based on asteroid rarity
                        if (asteroid.Type != null)
                        {
                            contact.displayColor = GetColorForRarity(asteroid.Type.rarity);
                        }
                    }
                }
                
                detectedContacts.Add(contact);
            }
        }
        
        // ===== Contact Classification =====
        private ContactType DetermineContactType(GameObject obj)
        {
            // Check for components
            if (obj.GetComponentInParent<Asteroid>() != null)
                return ContactType.Asteroid;
            
            // Check by tag
            if (obj.CompareTag("Hazard"))
                return ContactType.Hazard;
            
            if (obj.CompareTag("NPC"))
                return ContactType.NPC;
            
            if (obj.CompareTag("Station"))
                return ContactType.Station;
            
            // Check by layer
            string layerName = LayerMask.LayerToName(obj.layer);
            if (layerName.Contains("Asteroid"))
                return ContactType.Asteroid;
            
            if (layerName.Contains("Hazard"))
                return ContactType.Hazard;
            
            return ContactType.Unknown;
        }
        
        private bool ShouldShowContact(ContactType type)
        {
            switch (type)
            {
                case ContactType.Asteroid:
                    return showAsteroids;
                case ContactType.Hazard:
                    return showHazards;
                case ContactType.NPC:
                    return showNPCs;
                case ContactType.Station:
                    return true; // Always show stations
                default:
                    return false;
            }
        }
        
        private Color GetColorForRarity(AsteroidRarity rarity)
        {
            switch (rarity)
            {
                case AsteroidRarity.Common:
                    return new Color(0.6f, 0.6f, 0.6f, 1f); // Gray
                case AsteroidRarity.Uncommon:
                    return new Color(0.3f, 1f, 0.3f, 1f); // Green
                case AsteroidRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f, 1f); // Blue
                case AsteroidRarity.Epic:
                    return new Color(0.8f, 0.3f, 1f, 1f); // Purple
                case AsteroidRarity.Legendary:
                    return new Color(1f, 0.6f, 0.1f, 1f); // Orange
                default:
                    return Color.white;
            }
        }
        
        // ===== Public API =====
        
        /// <summary>
        /// Set radar range dynamically (useful for upgrades)
        /// </summary>
        public void SetRadarRange(float range)
        {
            radarRange = Mathf.Max(100f, range);
        }
        
        /// <summary>
        /// Get contacts of a specific type
        /// </summary>
        public List<RadarContact> GetContactsByType(ContactType type)
        {
            List<RadarContact> filteredContacts = new List<RadarContact>();
            
            foreach (RadarContact contact in detectedContacts)
            {
                if (contact.type == type)
                {
                    filteredContacts.Add(contact);
                }
            }
            
            return filteredContacts;
        }
        
        /// <summary>
        /// Get the closest contact of a specific type
        /// </summary>
        public RadarContact GetClosestContact(ContactType type)
        {
            RadarContact closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (RadarContact contact in detectedContacts)
            {
                if (contact.type == type && contact.distance < closestDistance)
                {
                    closest = contact;
                    closestDistance = contact.distance;
                }
            }
            
            return closest;
        }
        
        // ===== Debug Visualization =====
        private void OnDrawGizmosSelected()
        {
            if (shipTransform == null) return;
            
            // Draw radar range sphere
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(shipTransform.position, radarRange);
            
            // Draw detected contacts
            if (Application.isPlaying && detectedContacts != null)
            {
                foreach (RadarContact contact in detectedContacts)
                {
                    if (contact.transform == null) continue;
                    
                    Gizmos.color = contact.displayColor;
                    Gizmos.DrawLine(shipTransform.position, contact.transform.position);
                    Gizmos.DrawWireSphere(contact.transform.position, 5f);
                }
            }
        }
    }
}
