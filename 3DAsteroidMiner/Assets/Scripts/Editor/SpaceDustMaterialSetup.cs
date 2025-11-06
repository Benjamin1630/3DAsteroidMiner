using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to quickly configure Space Dust particle materials.
/// Access via Window > Space Dust Material Setup
/// NOTE: This file MUST be in an Editor folder to work properly.
/// </summary>
public class SpaceDustMaterialSetup : EditorWindow
{
    private Material particleMaterial;
    private Color dustColor = new Color(0.8f, 0.9f, 1f, 0.8f);
    private float brightness = 1f;
    private bool useAdditiveBlending = false;
    private bool useSoftParticles = true;
    private bool useGPUInstancing = true;
    
    [MenuItem("Window/Space Dust Material Setup")]
    public static void ShowWindow()
    {
        GetWindow<SpaceDustMaterialSetup>("Dust Material Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Space Dust Material Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        particleMaterial = (Material)EditorGUILayout.ObjectField("Particle Material", particleMaterial, typeof(Material), false);
        
        EditorGUILayout.Space();
        GUILayout.Label("Color Settings", EditorStyles.boldLabel);
        dustColor = EditorGUILayout.ColorField("Dust Color", dustColor);
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0f, 3f);
        
        EditorGUILayout.Space();
        GUILayout.Label("Rendering Options", EditorStyles.boldLabel);
        useAdditiveBlending = EditorGUILayout.Toggle("Additive Blending", useAdditiveBlending);
        useSoftParticles = EditorGUILayout.Toggle("Soft Particles", useSoftParticles);
        useGPUInstancing = EditorGUILayout.Toggle("GPU Instancing", useGPUInstancing);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Additive Blending: Creates glowing particles (recommended for bright space dust)\n" +
            "Soft Particles: Smooth blending with geometry (recommended ON)\n" +
            "GPU Instancing: Performance optimization (recommended ON)",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        GUI.enabled = particleMaterial != null;
        if (GUILayout.Button("Apply Settings to Material", GUILayout.Height(30)))
        {
            ApplySettingsToMaterial();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create New Particle Material", GUILayout.Height(30)))
        {
            CreateNewMaterial();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Quick Setup:\n" +
            "1. Click 'Create New Particle Material' to generate a new material\n" +
            "2. Adjust color and rendering settings above\n" +
            "3. Click 'Apply Settings to Material'\n" +
            "4. Assign the material to your Particle System Renderer",
            MessageType.None
        );
    }
    
    private void ApplySettingsToMaterial()
    {
        if (particleMaterial == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a material first!", "OK");
            return;
        }
        
        Undo.RecordObject(particleMaterial, "Apply Space Dust Material Settings");
        
        // Set color with brightness
        Color finalColor = dustColor * brightness;
        
        // Try different color property names depending on shader
        if (particleMaterial.HasProperty("_TintColor"))
        {
            particleMaterial.SetColor("_TintColor", finalColor);
        }
        if (particleMaterial.HasProperty("_Color"))
        {
            particleMaterial.SetColor("_Color", finalColor);
        }
        if (particleMaterial.HasProperty("_BaseColor"))
        {
            particleMaterial.SetColor("_BaseColor", finalColor);
        }
        
        // Set blending mode
        if (particleMaterial.HasProperty("_SrcBlend") && particleMaterial.HasProperty("_DstBlend"))
        {
            if (useAdditiveBlending)
            {
                particleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            }
            else
            {
                particleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
        }
        
        // Enable soft particles if supported
        if (particleMaterial.HasProperty("_SoftParticlesEnabled"))
        {
            particleMaterial.SetFloat("_SoftParticlesEnabled", useSoftParticles ? 1f : 0f);
        }
        
        // Enable GPU instancing
        particleMaterial.enableInstancing = useGPUInstancing;
        
        EditorUtility.SetDirty(particleMaterial);
        AssetDatabase.SaveAssets();
        Debug.Log("Material settings applied successfully!");
    }
    
    private void CreateNewMaterial()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Particle Material",
            "SpaceDustMaterial",
            "mat",
            "Choose location to save material"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        // Create material with Particles/Standard Unlit shader
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        }
        
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find a suitable particle shader! Please create a material manually.", "OK");
            return;
        }
        
        Material newMaterial = new Material(shader);
        newMaterial.name = "SpaceDustMaterial";
        
        // Apply default settings
        particleMaterial = newMaterial;
        ApplySettingsToMaterial();
        
        // Save material
        AssetDatabase.CreateAsset(newMaterial, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", "Material created at: " + path, "OK");
        EditorGUIUtility.PingObject(newMaterial);
        Selection.activeObject = newMaterial;
    }
}
