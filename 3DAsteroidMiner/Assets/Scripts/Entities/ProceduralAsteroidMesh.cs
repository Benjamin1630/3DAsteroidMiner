using UnityEngine;

namespace AsteroidMiner.Entities
{
    /// <summary>
    /// Generates procedural asteroid meshes with random vertex displacement.
    /// Supports dynamic shrinking for mining effects.
    /// No UV mapping required - shader uses object-space procedural generation.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ProceduralAsteroidMesh : MonoBehaviour
    {
        [Header("Mesh Generation Settings")]
        [SerializeField] private int subdivisions = 2; // Icosphere subdivision level (0-4)
        [SerializeField] private float baseRadius = 1f;
        [SerializeField] private float displacementAmount = 0.3f; // How much vertices can be displaced
        [SerializeField] private float noiseScale = 2.5f; // Perlin noise scale for randomness
        [SerializeField] private int seed = 0;
        
        [Header("Material Settings")]
        [Tooltip("Default material to use if none is assigned. Should be set in prefab.")]
        [SerializeField] private Material defaultAsteroidMaterial;
        
        [Header("Mining Shrink Effect")]
        [SerializeField] private bool enableShrinkEffect = true;
        [SerializeField] private float minShrinkScale = 0.3f; // Minimum size when fully mined (30%)
        
        [Header("Collision Settings")]
        [SerializeField] private bool updateCollisionMesh = true; // Update collider to match visual mesh
        
        // ===== Components =====
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Mesh originalMesh;
        private Vector3[] originalVertices;
        private Vector3[] displacedVertices;
        private float currentShrinkAmount = 0f; // 0 = full size, 1 = minimum size
        
        // ===== Lifecycle =====
        
        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            
            // Configure mesh collider
            if (meshCollider != null)
            {
                meshCollider.convex = true; // Required for non-static colliders
            }
            
            // Ensure material is assigned from the start
            if (meshRenderer != null && defaultAsteroidMaterial != null)
            {
                if (meshRenderer.sharedMaterial == null)
                {
                    meshRenderer.sharedMaterial = defaultAsteroidMaterial;
                    Debug.Log($"ProceduralAsteroidMesh: Assigned default material in Awake on {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Generate a new random asteroid mesh with a specific seed.
        /// </summary>
        /// <param name="randomSeed">Random seed for unique asteroid generation</param>
        /// <param name="asteroidHealth">Optional: Health of asteroid type to scale base radius</param>
        public void GenerateMesh(int randomSeed, float asteroidHealth = 0f)
        {
            seed = randomSeed;
            Random.InitState(seed);
            
            // Scale base radius based on asteroid health (if provided)
            // Lower health = smaller asteroid, Higher health = larger asteroid
            // Health values typically range from 2 (legendary) to 10 (common)
            float effectiveRadius = baseRadius;
            if (asteroidHealth > 0f)
            {
                // Map health to radius multiplier
                // Health 2 (legendary, weak) = 0.6x radius (smallest)
                // Health 5-6 (mid) = 0.85x-0.95x radius (medium)
                // Health 10 (common, strong) = 1.3x radius (largest)
                float normalizedHealth = Mathf.Clamp01((asteroidHealth - 2f) / 8f); // Maps 2-10 to 0-1
                effectiveRadius = baseRadius * Mathf.Lerp(0.6f, 1.3f, normalizedHealth);
            }
            
            // Create base icosphere mesh with scaled radius
            Mesh baseMesh = CreateIcosphere(subdivisions, effectiveRadius);
            
            // Store original vertices
            originalVertices = baseMesh.vertices;
            displacedVertices = new Vector3[originalVertices.Length];
            
            // Apply random displacement to vertices
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                
                // Use 3D Perlin noise for natural-looking displacement
                float noise = GetPerlinNoise3D(vertex * noiseScale);
                
                // Displace vertex along its normal (radially from center)
                float displacement = Mathf.Lerp(-displacementAmount, displacementAmount, noise);
                displacedVertices[i] = vertex.normalized * (effectiveRadius + displacement);
            }
            
            // Apply displaced vertices to mesh
            baseMesh.vertices = displacedVertices;
            baseMesh.RecalculateBounds();
            baseMesh.RecalculateNormals();
            baseMesh.RecalculateTangents();
            
            // Verify UVs were generated (should always succeed now)
            #if UNITY_EDITOR
            if (baseMesh.uv == null || baseMesh.uv.Length == 0)
            {
                Debug.LogError($"ProceduralAsteroidMesh: UVs not generated for {gameObject.name}!");
            }
            else
            {
                Debug.Log($"ProceduralAsteroidMesh: Generated {baseMesh.uv.Length} UVs for {baseMesh.vertexCount} vertices on {gameObject.name}");
            }
            #endif
            
            originalMesh = baseMesh;
            
            // CRITICAL: Preserve material before and after mesh assignment
            Material preservedMaterial = null;
            if (meshRenderer != null)
            {
                preservedMaterial = meshRenderer.sharedMaterial;
            }
            
            // If no material is assigned, use default
            if (preservedMaterial == null && defaultAsteroidMaterial != null)
            {
                preservedMaterial = defaultAsteroidMaterial;
                Debug.Log($"ProceduralAsteroidMesh: Using default material for {gameObject.name}");
            }
            
            meshFilter.mesh = originalMesh;
            
            // Restore/assign material after mesh assignment
            if (preservedMaterial != null && meshRenderer != null)
            {
                meshRenderer.sharedMaterial = preservedMaterial;
                Debug.Log($"ProceduralAsteroidMesh: Material '{preservedMaterial.name}' assigned to {gameObject.name}");
            }
            else if (meshRenderer != null)
            {
                Debug.LogError($"ProceduralAsteroidMesh: No material available for {gameObject.name}! Assign defaultAsteroidMaterial in Inspector.");
            }
            
            // Update collision mesh to match visual mesh
            if (updateCollisionMesh && meshCollider != null)
            {
                meshCollider.sharedMesh = null; // Clear old mesh
                meshCollider.sharedMesh = originalMesh; // Assign new mesh
            }
            
            // Reset shrink amount
            currentShrinkAmount = 0f;
        }
        
        /// <summary>
        /// Update the mesh shrink effect based on mining progress (0-1).
        /// 0 = full size, 1 = fully mined (smallest size)
        /// </summary>
        public void UpdateShrinkEffect(float miningProgress)
        {
            if (!enableShrinkEffect || originalMesh == null)
                return;
            
            float newShrinkAmount = Mathf.Clamp01(miningProgress);
            
            // Only update if change is significant (>1%) to reduce mesh recalculations
            if (Mathf.Abs(newShrinkAmount - currentShrinkAmount) < 0.01f)
                return;
            
            currentShrinkAmount = newShrinkAmount;
            
            // Calculate scale based on mining progress
            // Shrinks from 1.0 to minShrinkScale as mining progresses
            float scale = Mathf.Lerp(1f, minShrinkScale, currentShrinkAmount);
            
            // Apply scale to vertices (shrink toward center)
            Vector3[] shrunkVertices = new Vector3[displacedVertices.Length];
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                shrunkVertices[i] = displacedVertices[i] * scale;
            }
            
            // Update mesh - OPTIMIZATION: Only recalculate normals every 10% mining progress
            originalMesh.vertices = shrunkVertices;
            originalMesh.RecalculateBounds();
            
            // Only recalculate normals at 25%, 50%, 75%, and 100% mining
            if (currentShrinkAmount % 0.25f < 0.02f || currentShrinkAmount > 0.98f)
            {
                originalMesh.RecalculateNormals();
            }
            
            // Update collision mesh less frequently (every 20% or at destruction)
            if (updateCollisionMesh && meshCollider != null)
            {
                if (currentShrinkAmount % 0.2f < 0.02f || currentShrinkAmount > 0.98f)
                {
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = originalMesh;
                }
            }
        }
        
        /// <summary>
        /// Reset the asteroid to full size (for object pooling).
        /// </summary>
        public void ResetMesh()
        {
            if (originalMesh != null && displacedVertices != null)
            {
                originalMesh.vertices = displacedVertices;
                originalMesh.RecalculateBounds();
                originalMesh.RecalculateNormals();
                currentShrinkAmount = 0f;
                
                // Reset collision mesh
                if (updateCollisionMesh && meshCollider != null)
                {
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = originalMesh;
                }
            }
        }
        
        /// <summary>
        /// Explicitly set the asteroid material. Use this before calling GenerateMesh.
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null && material != null)
            {
                meshRenderer.sharedMaterial = material;
                Debug.Log($"ProceduralAsteroidMesh: Material explicitly set to '{material.name}' on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Get the current material on the renderer.
        /// </summary>
        public Material GetMaterial()
        {
            return meshRenderer != null ? meshRenderer.sharedMaterial : null;
        }
        
        // ===== Mesh Generation =====
        
        /// <summary>
        /// Create an icosphere mesh (geodesic sphere with evenly distributed vertices).
        /// Better than UV sphere for deformation.
        /// </summary>
        /// <param name="subdivisionLevel">Number of subdivisions (0-4)</param>
        /// <param name="radius">Radius of the sphere</param>
        private Mesh CreateIcosphere(int subdivisionLevel, float radius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Procedural Asteroid";
            
            // Start with icosahedron (20-sided polyhedron)
            float t = (1f + Mathf.Sqrt(5f)) / 2f; // Golden ratio
            
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-1,  t,  0).normalized * radius,
                new Vector3( 1,  t,  0).normalized * radius,
                new Vector3(-1, -t,  0).normalized * radius,
                new Vector3( 1, -t,  0).normalized * radius,
                new Vector3( 0, -1,  t).normalized * radius,
                new Vector3( 0,  1,  t).normalized * radius,
                new Vector3( 0, -1, -t).normalized * radius,
                new Vector3( 0,  1, -t).normalized * radius,
                new Vector3( t,  0, -1).normalized * radius,
                new Vector3( t,  0,  1).normalized * radius,
                new Vector3(-t,  0, -1).normalized * radius,
                new Vector3(-t,  0,  1).normalized * radius
            };
            
            int[] triangles = new int[]
            {
                0, 11, 5,   0, 5, 1,    0, 1, 7,    0, 7, 10,   0, 10, 11,
                1, 5, 9,    5, 11, 4,   11, 10, 2,  10, 7, 6,   7, 1, 8,
                3, 9, 4,    3, 4, 2,    3, 2, 6,    3, 6, 8,    3, 8, 9,
                4, 9, 5,    2, 4, 11,   6, 2, 10,   8, 6, 7,    9, 8, 1
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            
            // Generate basic UVs (required by Unity even though shader uses object-space coordinates)
            // Use spherical mapping: UV = (atan2(z,x) / 2π, asin(y/r) / π + 0.5)
            GenerateSphericalUVs(mesh);
            
            // Subdivide for smoother sphere
            for (int i = 0; i < subdivisionLevel; i++)
            {
                mesh = SubdivideMesh(mesh, radius);
            }
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Subdivide mesh triangles for higher detail.
        /// </summary>
        /// <param name="mesh">Mesh to subdivide</param>
        /// <param name="radius">Sphere radius to project midpoints onto</param>
        private Mesh SubdivideMesh(Mesh mesh, float radius)
        {
            Vector3[] oldVertices = mesh.vertices;
            int[] oldTriangles = mesh.triangles;
            
            int oldTriCount = oldTriangles.Length / 3;
            
            // Use dictionary to track and reuse midpoint vertices (avoid duplicates on shared edges)
            System.Collections.Generic.Dictionary<long, int> midpointCache = new System.Collections.Generic.Dictionary<long, int>();
            System.Collections.Generic.List<Vector3> newVerticesList = new System.Collections.Generic.List<Vector3>(oldVertices);
            System.Collections.Generic.List<int> newTrianglesList = new System.Collections.Generic.List<int>(oldTriangles.Length * 4);
            
            for (int i = 0; i < oldTriangles.Length; i += 3)
            {
                int v0 = oldTriangles[i];
                int v1 = oldTriangles[i + 1];
                int v2 = oldTriangles[i + 2];
                
                // Get or create midpoint vertices
                int m0 = GetMidpointIndex(v0, v1, midpointCache, newVerticesList, oldVertices, radius);
                int m1 = GetMidpointIndex(v1, v2, midpointCache, newVerticesList, oldVertices, radius);
                int m2 = GetMidpointIndex(v2, v0, midpointCache, newVerticesList, oldVertices, radius);
                
                // Create 4 new triangles from 1 old triangle
                // Triangle 1: v0, m0, m2
                newTrianglesList.Add(v0);
                newTrianglesList.Add(m0);
                newTrianglesList.Add(m2);
                
                // Triangle 2: v1, m1, m0
                newTrianglesList.Add(v1);
                newTrianglesList.Add(m1);
                newTrianglesList.Add(m0);
                
                // Triangle 3: v2, m2, m1
                newTrianglesList.Add(v2);
                newTrianglesList.Add(m2);
                newTrianglesList.Add(m1);
                
                // Triangle 4: m0, m1, m2 (center)
                newTrianglesList.Add(m0);
                newTrianglesList.Add(m1);
                newTrianglesList.Add(m2);
            }
            
            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerticesList.ToArray();
            newMesh.triangles = newTrianglesList.ToArray();
            
            // Generate UVs for subdivided mesh
            GenerateSphericalUVs(newMesh);
            
            return newMesh;
        }
        
        /// <summary>
        /// Generate spherical UV mapping for a mesh.
        /// Required by Unity even though our shader uses object-space coordinates.
        /// </summary>
        private void GenerateSphericalUVs(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 normalized = vertices[i].normalized;
                
                // Spherical mapping: UV = (atan2(z,x) / 2π, asin(y) / π + 0.5)
                float u = 0.5f + Mathf.Atan2(normalized.z, normalized.x) / (2f * Mathf.PI);
                float v = 0.5f + Mathf.Asin(normalized.y) / Mathf.PI;
                
                uvs[i] = new Vector2(u, v);
            }
            
            mesh.uv = uvs;
        }
        
        /// <summary>
        /// Get or create a midpoint vertex between two vertices.
        /// Uses caching to reuse vertices on shared edges.
        /// </summary>
        private int GetMidpointIndex(int v0, int v1, System.Collections.Generic.Dictionary<long, int> cache, 
            System.Collections.Generic.List<Vector3> vertices, Vector3[] originalVertices, float radius)
        {
            // Create unique key for this edge (order independent)
            long key = ((long)Mathf.Min(v0, v1) << 32) | (long)Mathf.Max(v0, v1);
            
            // Check if midpoint already exists
            if (cache.TryGetValue(key, out int cachedIndex))
            {
                return cachedIndex;
            }
            
            // Create new midpoint vertex
            Vector3 midpoint = ((originalVertices[v0] + originalVertices[v1]) / 2f).normalized * radius;
            int newIndex = vertices.Count;
            vertices.Add(midpoint);
            cache[key] = newIndex;
            
            return newIndex;
        }
        
        /// <summary>
        /// 3D Perlin noise function for natural displacement.
        /// </summary>
        private float GetPerlinNoise3D(Vector3 position)
        {
            // Combine 3 2D Perlin noise samples for 3D effect
            float xy = Mathf.PerlinNoise(position.x + seed, position.y + seed);
            float xz = Mathf.PerlinNoise(position.x + seed, position.z + seed);
            float yz = Mathf.PerlinNoise(position.y + seed, position.z + seed);
            
            return (xy + xz + yz) / 3f;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            subdivisions = Mathf.Clamp(subdivisions, 0, 4); // Limit to prevent excessive vertices
            displacementAmount = Mathf.Max(0f, displacementAmount);
            minShrinkScale = Mathf.Clamp(minShrinkScale, 0.1f, 1f);
        }
#endif
    }
}
