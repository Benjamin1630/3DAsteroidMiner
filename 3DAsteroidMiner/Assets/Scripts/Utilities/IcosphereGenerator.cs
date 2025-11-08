using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates an icosphere mesh with uniform triangle distribution.
/// Better than Unity's default sphere for effects that need equal spacing.
/// </summary>
public static class IcosphereGenerator
{
        private static Dictionary<long, int> middlePointIndexCache;
        
        /// <summary>
        /// Creates an icosphere mesh with the specified subdivision level.
        /// </summary>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="subdivisions">Number of subdivisions (0-4 recommended, higher = more triangles)</param>
        /// <returns>Generated mesh</returns>
        public static Mesh Create(float radius = 1f, int subdivisions = 2)
        {
            middlePointIndexCache = new Dictionary<long, int>();
            
            Mesh mesh = new Mesh();
            mesh.name = "Icosphere";
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            
            // Create 12 vertices of icosahedron
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            
            vertices.Add(new Vector3(-1, t, 0).normalized * radius);
            vertices.Add(new Vector3(1, t, 0).normalized * radius);
            vertices.Add(new Vector3(-1, -t, 0).normalized * radius);
            vertices.Add(new Vector3(1, -t, 0).normalized * radius);
            
            vertices.Add(new Vector3(0, -1, t).normalized * radius);
            vertices.Add(new Vector3(0, 1, t).normalized * radius);
            vertices.Add(new Vector3(0, -1, -t).normalized * radius);
            vertices.Add(new Vector3(0, 1, -t).normalized * radius);
            
            vertices.Add(new Vector3(t, 0, -1).normalized * radius);
            vertices.Add(new Vector3(t, 0, 1).normalized * radius);
            vertices.Add(new Vector3(-t, 0, -1).normalized * radius);
            vertices.Add(new Vector3(-t, 0, 1).normalized * radius);
            
            // Create 20 triangles of icosahedron
            List<TriangleIndices> faces = new List<TriangleIndices>();
            
            // 5 faces around point 0
            faces.Add(new TriangleIndices(0, 11, 5));
            faces.Add(new TriangleIndices(0, 5, 1));
            faces.Add(new TriangleIndices(0, 1, 7));
            faces.Add(new TriangleIndices(0, 7, 10));
            faces.Add(new TriangleIndices(0, 10, 11));
            
            // 5 adjacent faces
            faces.Add(new TriangleIndices(1, 5, 9));
            faces.Add(new TriangleIndices(5, 11, 4));
            faces.Add(new TriangleIndices(11, 10, 2));
            faces.Add(new TriangleIndices(10, 7, 6));
            faces.Add(new TriangleIndices(7, 1, 8));
            
            // 5 faces around point 3
            faces.Add(new TriangleIndices(3, 9, 4));
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(3, 2, 6));
            faces.Add(new TriangleIndices(3, 6, 8));
            faces.Add(new TriangleIndices(3, 8, 9));
            
            // 5 adjacent faces
            faces.Add(new TriangleIndices(4, 9, 5));
            faces.Add(new TriangleIndices(2, 4, 11));
            faces.Add(new TriangleIndices(6, 2, 10));
            faces.Add(new TriangleIndices(8, 6, 7));
            faces.Add(new TriangleIndices(9, 8, 1));
            
            // Subdivide faces
            for (int i = 0; i < subdivisions; i++)
            {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (var tri in faces)
                {
                    int a = GetMiddlePoint(tri.v1, tri.v2, vertices, radius);
                    int b = GetMiddlePoint(tri.v2, tri.v3, vertices, radius);
                    int c = GetMiddlePoint(tri.v3, tri.v1, vertices, radius);
                    
                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }
            
            // Convert faces to triangle list
            foreach (var tri in faces)
            {
                triangles.Add(tri.v1);
                triangles.Add(tri.v2);
                triangles.Add(tri.v3);
            }
            
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            
            // Calculate normals only (UVs not needed - shader uses triplanar mapping)
            Vector3[] normals = new Vector3[vertices.Count];
            
            for (int i = 0; i < vertices.Count; i++)
            {
                normals[i] = vertices[i].normalized;
            }
            
            mesh.normals = normals;
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private static int GetMiddlePoint(int p1, int p2, List<Vector3> vertices, float radius)
        {
            // Check if we've already created this vertex
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;
            
            if (middlePointIndexCache.TryGetValue(key, out int ret))
            {
                return ret;
            }
            
            // Not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = ((point1 + point2) / 2f).normalized * radius;
            
            // Add vertex to mesh
            int i = vertices.Count;
            vertices.Add(middle);
            
            // Store in cache
            middlePointIndexCache.Add(key, i);
            
            return i;
        }
        
        private struct TriangleIndices
        {
            public int v1;
            public int v2;
            public int v3;
            
            public TriangleIndices(int v1, int v2, int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }
    }
