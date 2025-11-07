using UnityEngine;
using UnityEditor;
using System.IO;
using AsteroidMiner.Data;

/// <summary>
/// Editor utility to create all 16 asteroid type visual data assets.
/// Use: Tools > Asteroid Miner > Create All Asteroid Visual Presets
/// </summary>
public class AsteroidVisualPresetCreator : Editor
{
    [MenuItem("Tools/Asteroid Miner/Create All Asteroid Visual Presets")]
    public static void CreateAllPresets()
    {
        string folderPath = "Assets/Data/AsteroidTypes";
        
        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Data", "AsteroidTypes");
        }
        
        int created = 0;
        
        // Common asteroids (60% spawn rate)
        created += CreatePreset(folderPath, "IronOre", new Color(0.47f, 0.47f, 0.51f), AsteroidRarity.Common, 2);
        created += CreatePreset(folderPath, "Copper", new Color(0.72f, 0.45f, 0.20f), AsteroidRarity.Common, 5);
        
        // Uncommon asteroids (20% spawn rate)
        created += CreatePreset(folderPath, "Nickel", new Color(0.78f, 0.78f, 0.71f), AsteroidRarity.Uncommon, 12);
        created += CreatePreset(folderPath, "Silver", new Color(0.75f, 0.75f, 0.75f), AsteroidRarity.Uncommon, 18);
        created += CreatePreset(folderPath, "Titanium", new Color(0.59f, 0.59f, 0.63f), AsteroidRarity.Uncommon, 25);
        
        // Rare asteroids (12% spawn rate)
        created += CreatePreset(folderPath, "Gold", new Color(1.0f, 0.84f, 0.0f), AsteroidRarity.Rare, 40);
        created += CreatePreset(folderPath, "Emerald", new Color(0.0f, 0.79f, 0.34f), AsteroidRarity.Rare, 55);
        created += CreatePreset(folderPath, "Platinum", new Color(0.90f, 0.89f, 0.89f), AsteroidRarity.Rare, 70);
        
        // Epic asteroids (6% spawn rate)
        created += CreatePreset(folderPath, "Ruby", new Color(0.88f, 0.07f, 0.37f), AsteroidRarity.Epic, 100);
        created += CreatePreset(folderPath, "Sapphire", new Color(0.06f, 0.32f, 0.73f), AsteroidRarity.Epic, 120);
        created += CreatePreset(folderPath, "Obsidian", new Color(0.24f, 0.20f, 0.27f), AsteroidRarity.Epic, 140);
        
        // Legendary asteroids (2% spawn rate)
        created += CreatePreset(folderPath, "QuantumCrystal", new Color(0.59f, 0.20f, 1.0f), AsteroidRarity.Legendary, 200);
        created += CreatePreset(folderPath, "Nebulite", new Color(0.0f, 1.0f, 0.78f), AsteroidRarity.Legendary, 250);
        created += CreatePreset(folderPath, "DarkMatter", new Color(0.39f, 0.0f, 0.59f), AsteroidRarity.Legendary, 350);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created {created} asteroid visual preset assets in {folderPath}");
        EditorUtility.DisplayDialog("Success", 
            $"Successfully created {created} asteroid visual preset assets!\n\nLocation: {folderPath}", 
            "OK");
    }
    
    private static int CreatePreset(string folderPath, string typeName, Color typeColor, 
                                    AsteroidRarity rarity, int value)
    {
        string assetPath = $"{folderPath}/AsteroidTypeVisual_{typeName}.asset";
        
        // Check if already exists
        if (File.Exists(assetPath))
        {
            Debug.Log($"Skipping {typeName} - already exists");
            return 0;
        }
        
        // Create preset
        var data = AsteroidTypeVisualData.CreatePreset(typeName, typeColor, rarity, value);
        
        // Set display name with spaces
        data.typeName = AddSpacesToCamelCase(typeName);
        data.resourceName = data.typeName;
        
        // Save asset
        AssetDatabase.CreateAsset(data, assetPath);
        
        Debug.Log($"Created: {data.typeName} ({rarity})");
        return 1;
    }
    
    private static string AddSpacesToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        System.Text.StringBuilder result = new System.Text.StringBuilder(text.Length * 2);
        result.Append(text[0]);
        
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                result.Append(' ');
            result.Append(text[i]);
        }
        
        return result.ToString();
    }
    
    [MenuItem("Tools/Asteroid Miner/Create Material Template")]
    public static void CreateMaterialTemplate()
    {
        string folderPath = "Assets/Materials/Asteroids";
        
        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Materials", "Asteroids");
        }
        
        // Try URP shader first, fallback to built-in
        Shader shader = Shader.Find("Custom/AsteroidHybridShader_URP");
        if (shader == null)
        {
            shader = Shader.Find("Custom/AsteroidHybridShader");
        }
        
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find asteroid shader. Make sure AsteroidHybridShader_URP.shader or AsteroidHybridShader.shader is compiled.", 
                "OK");
            return;
        }
        
        // Create material
        Material mat = new Material(shader);
        mat.name = "AsteroidMaterial_Template";
        
        string assetPath = $"{folderPath}/AsteroidMaterial_Template.mat";
        AssetDatabase.CreateAsset(mat, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select and ping the material
        Selection.activeObject = mat;
        EditorGUIUtility.PingObject(mat);
        
        string shaderName = shader.name;
        Debug.Log($"Created material template using shader: {shaderName} at: {assetPath}");
        EditorUtility.DisplayDialog("Success", 
            $"Created material template using {shaderName}!\n\nLocation: {assetPath}\n\nAssign this to your asteroid prefabs.", 
            "OK");
    }
}
