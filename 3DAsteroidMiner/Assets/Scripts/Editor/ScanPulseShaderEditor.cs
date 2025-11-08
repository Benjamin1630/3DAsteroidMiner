using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the ScanPulse shader to handle double-sided rendering toggle
/// </summary>
public class ScanPulseShaderEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Find the double-sided property
        MaterialProperty doubleSidedProp = FindProperty("_DoubleSided", properties);
        MaterialProperty cullProp = FindProperty("_Cull", properties);
        
        // Draw all properties
        base.OnGUI(materialEditor, properties);
        
        // Update cull mode based on double-sided toggle
        Material targetMat = materialEditor.target as Material;
        if (targetMat != null)
        {
            bool isDoubleSided = doubleSidedProp.floatValue > 0.5f;
            cullProp.floatValue = isDoubleSided ? 0 : 1; // 0 = Off (both sides), 1 = Front (back only)
        }
    }
    
    // Enable material preview
    public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
    {
        base.OnMaterialPreviewGUI(materialEditor, r, background);
    }
    
    public override void OnMaterialPreviewSettingsGUI(MaterialEditor materialEditor)
    {
        base.OnMaterialPreviewSettingsGUI(materialEditor);
    }
}
