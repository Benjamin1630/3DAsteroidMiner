using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Utility to generate particle textures programmatically.
/// Access via Window > Generate Particle Textures
/// </summary>
public class ParticleTextureGenerator : EditorWindow
{
    private int textureSize = 256;
    private ParticleShape selectedShape = ParticleShape.SoftCircle;
    private Color particleColor = Color.white;
    private float softness = 0.5f;
    private int starPoints = 5;
    
    private enum ParticleShape
    {
        SoftCircle,
        HardCircle,
        Star,
        Sparkle,
        Glow,
        Diamond
    }
    
    [MenuItem("Window/Generate Particle Textures")]
    public static void ShowWindow()
    {
        GetWindow<ParticleTextureGenerator>("Particle Texture Generator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Particle Texture Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        GUILayout.Label("Texture Settings", EditorStyles.boldLabel);
        textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 64, 512);
        selectedShape = (ParticleShape)EditorGUILayout.EnumPopup("Shape", selectedShape);
        particleColor = EditorGUILayout.ColorField("Color", particleColor);
        
        EditorGUILayout.Space();
        
        if (selectedShape == ParticleShape.SoftCircle || selectedShape == ParticleShape.Glow)
        {
            softness = EditorGUILayout.Slider("Softness", softness, 0.1f, 1f);
        }
        
        if (selectedShape == ParticleShape.Star || selectedShape == ParticleShape.Sparkle)
        {
            starPoints = EditorGUILayout.IntSlider("Star Points", starPoints, 4, 8);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will generate a texture with the selected shape.\n" +
            "The texture will have transparency (alpha channel) for proper blending.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Texture", GUILayout.Height(40)))
        {
            GenerateTexture();
        }
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Quick Presets", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Soft White Dust"))
        {
            selectedShape = ParticleShape.SoftCircle;
            particleColor = Color.white;
            softness = 0.7f;
            textureSize = 128;
        }
        
        if (GUILayout.Button("Twinkling Star"))
        {
            selectedShape = ParticleShape.Star;
            particleColor = Color.white;
            starPoints = 4;
            textureSize = 128;
        }
        
        if (GUILayout.Button("Sparkle Effect"))
        {
            selectedShape = ParticleShape.Sparkle;
            particleColor = new Color(0.9f, 0.95f, 1f);
            starPoints = 4;
            textureSize = 256;
        }
        
        if (GUILayout.Button("Glowing Orb"))
        {
            selectedShape = ParticleShape.Glow;
            particleColor = new Color(0.7f, 0.85f, 1f);
            softness = 0.9f;
            textureSize = 256;
        }
    }
    
    private void GenerateTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureSize * textureSize];
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f;
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int index = y * textureSize + x;
                Vector2 pos = new Vector2(x, y);
                
                float alpha = CalculateAlpha(pos, center, radius);
                pixels[index] = new Color(particleColor.r, particleColor.g, particleColor.b, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Save texture
        string shapeName = selectedShape.ToString();
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Particle Texture",
            $"Particle_{shapeName}_{textureSize}",
            "png",
            "Choose location to save texture"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(path, pngData);
        
        AssetDatabase.Refresh();
        
        // Configure texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        
        EditorUtility.DisplayDialog("Success", $"Particle texture created at:\n{path}", "OK");
        
        // Ping the texture in the project
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
    }
    
    private float CalculateAlpha(Vector2 pos, Vector2 center, float radius)
    {
        Vector2 dir = pos - center;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x);
        
        switch (selectedShape)
        {
            case ParticleShape.SoftCircle:
                return CalculateSoftCircle(distance, radius);
                
            case ParticleShape.HardCircle:
                return distance <= radius ? 1f : 0f;
                
            case ParticleShape.Star:
                return CalculateStar(distance, angle, radius);
                
            case ParticleShape.Sparkle:
                return CalculateSparkle(distance, angle, radius);
                
            case ParticleShape.Glow:
                return CalculateGlow(distance, radius);
                
            case ParticleShape.Diamond:
                return CalculateDiamond(pos, center, radius);
                
            default:
                return 0f;
        }
    }
    
    private float CalculateSoftCircle(float distance, float radius)
    {
        if (distance >= radius) return 0f;
        
        float normalizedDist = distance / radius;
        float falloff = 1f - Mathf.Pow(normalizedDist, 1f / softness);
        return Mathf.Clamp01(falloff);
    }
    
    private float CalculateGlow(float distance, float radius)
    {
        if (distance >= radius) return 0f;
        
        float normalizedDist = distance / radius;
        float glow = Mathf.Exp(-normalizedDist * normalizedDist / (softness * softness));
        return Mathf.Clamp01(glow);
    }
    
    private float CalculateStar(float distance, float angle, float radius)
    {
        // Create star shape with variable points
        float angleStep = Mathf.PI * 2f / starPoints;
        float localAngle = (angle % angleStep) / angleStep;
        
        // Create spikes
        float spikeInfluence = Mathf.Abs(Mathf.Cos(localAngle * starPoints));
        float adjustedRadius = radius * (0.5f + 0.5f * spikeInfluence);
        
        if (distance >= adjustedRadius) return 0f;
        
        float normalizedDist = distance / adjustedRadius;
        return Mathf.Clamp01(1f - normalizedDist);
    }
    
    private float CalculateSparkle(float distance, float angle, float radius)
    {
        // Create cross/sparkle pattern
        float horizontal = Mathf.Abs(Mathf.Cos(angle));
        float vertical = Mathf.Abs(Mathf.Sin(angle));
        float cross = Mathf.Max(horizontal, vertical);
        
        // Combine with circular falloff
        float circleFalloff = 1f - (distance / radius);
        float sparkle = cross * 0.7f + 0.3f;
        
        return Mathf.Clamp01(circleFalloff * sparkle);
    }
    
    private float CalculateDiamond(Vector2 pos, Vector2 center, float radius)
    {
        // Manhattan distance for diamond shape
        float dx = Mathf.Abs(pos.x - center.x);
        float dy = Mathf.Abs(pos.y - center.y);
        float manhattanDist = dx + dy;
        
        if (manhattanDist >= radius * 1.5f) return 0f;
        
        float normalizedDist = manhattanDist / (radius * 1.5f);
        return Mathf.Clamp01(1f - normalizedDist);
    }
}
