using UnityEngine;
using UnityEditor;
using AsteroidMiner.Systems;

namespace AsteroidMiner.Editor
{
    /// <summary>
    /// Editor utility to quickly set up a radar system in the scene.
    /// Menu: Tools > Asteroid Miner > Setup Radar System
    /// </summary>
    public class RadarSetupUtility : MonoBehaviour
    {
        [MenuItem("Tools/Asteroid Miner/Setup Radar System")]
        public static void SetupRadarSystem()
        {
            // Find player ship
            GameObject playerShip = GameObject.FindGameObjectWithTag("Player");
            if (playerShip == null)
            {
                Debug.LogError("RadarSetup: No GameObject with 'Player' tag found! Please tag your ship first.");
                return;
            }
            
            // Create radar system root
            GameObject radarSystemObj = new GameObject("RadarSystem");
            radarSystemObj.transform.SetParent(playerShip.transform);
            radarSystemObj.transform.localPosition = Vector3.zero;
            
            RadarSystem radarSystem = radarSystemObj.AddComponent<RadarSystem>();
            
            // Create radar display
            GameObject radarDisplayObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            radarDisplayObj.name = "RadarDisplay";
            radarDisplayObj.transform.SetParent(radarSystemObj.transform);
            
            // Position on dashboard (adjust as needed)
            radarDisplayObj.transform.localPosition = new Vector3(0.3f, 0.5f, 0.8f);
            radarDisplayObj.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            radarDisplayObj.transform.localScale = new Vector3(0.02f, 1f, 0.02f); // Small dashboard display
            
            // Remove collider (not needed for UI)
            Collider col = radarDisplayObj.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            
            // Add radar display component
            RadarDisplay radarDisplay = radarDisplayObj.AddComponent<RadarDisplay>();
            
            // Create materials
            Material radarScreenMat = CreateRadarScreenMaterial();
            Material radarBlipMat = CreateRadarBlipMaterial();
            
            // Assign materials
            radarDisplayObj.GetComponent<Renderer>().material = radarScreenMat;
            
            // Configure radar display via reflection (since fields are serialized)
            SerializedObject serializedDisplay = new SerializedObject(radarDisplay);
            serializedDisplay.FindProperty("radarSystem").objectReferenceValue = radarSystem;
            serializedDisplay.FindProperty("blipMaterial").objectReferenceValue = radarBlipMat;
            serializedDisplay.FindProperty("radarScreenMaterial").objectReferenceValue = radarScreenMat;
            serializedDisplay.FindProperty("displayRadius").floatValue = 0.15f;
            serializedDisplay.FindProperty("displayHeight").floatValue = 0.05f;
            serializedDisplay.FindProperty("blipScale").floatValue = 0.005f;
            serializedDisplay.ApplyModifiedProperties();
            
            // Configure radar system
            SerializedObject serializedSystem = new SerializedObject(radarSystem);
            serializedSystem.FindProperty("radarRange").floatValue = 1000f;
            serializedSystem.FindProperty("updateInterval").floatValue = 0.1f;
            serializedSystem.ApplyModifiedProperties();
            
            // Select the created radar system
            Selection.activeGameObject = radarSystemObj;
            
            Debug.Log("<color=green>Radar System created successfully!</color>\n" +
                      "- RadarSystem attached to player ship\n" +
                      "- RadarDisplay created as child\n" +
                      "- Materials created in Assets/Materials/Radar/\n" +
                      "- Adjust position on dashboard as needed\n" +
                      "- Configure range and display settings in Inspector");
        }
        
        private static Material CreateRadarScreenMaterial()
        {
            // Check if shader exists
            Shader shader = Shader.Find("Custom/HolographicRadar");
            if (shader == null)
            {
                Debug.LogWarning("HolographicRadar shader not found, using Standard shader");
                shader = Shader.Find("Standard");
            }
            
            Material mat = new Material(shader);
            mat.name = "RadarScreen_Mat";
            
            // Set properties if HolographicRadar shader
            if (shader.name == "Custom/HolographicRadar")
            {
                mat.SetColor("_Color", new Color(0.2f, 0.8f, 1f, 0.7f));
                mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f, 2f));
                mat.SetFloat("_FresnelPower", 3f);
                mat.SetFloat("_FresnelIntensity", 2f);
                mat.SetFloat("_ScanlineSpeed", 2f);
                mat.SetFloat("_ScanlineFrequency", 20f);
                mat.SetFloat("_Opacity", 0.7f);
            }
            else
            {
                // Fallback for Standard shader
                mat.SetColor("_Color", new Color(0.2f, 0.8f, 1f, 0.5f));
                mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f, 1f));
                mat.EnableKeyword("_EMISSION");
            }
            
            // Save material
            string path = "Assets/Materials/Radar";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "Radar");
            }
            
            AssetDatabase.CreateAsset(mat, $"{path}/RadarScreen_Mat.mat");
            AssetDatabase.SaveAssets();
            
            return mat;
        }
        
        private static Material CreateRadarBlipMaterial()
        {
            // Check if shader exists
            Shader shader = Shader.Find("Custom/RadarBlip");
            if (shader == null)
            {
                Debug.LogWarning("RadarBlip shader not found, using Standard shader");
                shader = Shader.Find("Standard");
            }
            
            Material mat = new Material(shader);
            mat.name = "RadarBlip_Mat";
            
            // Set properties if RadarBlip shader
            if (shader.name == "Custom/RadarBlip")
            {
                mat.SetColor("_Color", new Color(0.2f, 1f, 0.2f, 1f));
                mat.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.2f, 2f));
                mat.SetFloat("_GlowIntensity", 3f);
                mat.SetFloat("_PulseSpeed", 2f);
                mat.SetFloat("_FresnelPower", 2f);
            }
            else
            {
                // Fallback for Standard shader
                mat.SetColor("_Color", new Color(0.2f, 1f, 0.2f, 1f));
                mat.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.2f, 1f));
                mat.EnableKeyword("_EMISSION");
            }
            
            // Save material
            string path = "Assets/Materials/Radar";
            AssetDatabase.CreateAsset(mat, $"{path}/RadarBlip_Mat.mat");
            AssetDatabase.SaveAssets();
            
            return mat;
        }
        
        [MenuItem("Tools/Asteroid Miner/Setup Radar System", true)]
        public static bool ValidateSetupRadarSystem()
        {
            // Only enable menu item if in scene (not prefab mode)
            return !EditorApplication.isPlaying;
        }
    }
}
