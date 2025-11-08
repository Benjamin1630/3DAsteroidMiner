Shader "Custom/ScanPulse"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Dot Color", Color) = (0.2, 0.8, 1, 1)
        _BackgroundAlpha ("Background Alpha", Range(0, 1)) = 0.0
        
        [Header(Scan Pulse)]
        _Progress ("Progress", Range(0, 1)) = 0.0
        _EdgeThickness ("Edge Thickness", Range(0.1, 5.0)) = 0.5
        
        [Header(Grid Settings)]
        _GridScale ("Dot Spacing", Range(0.1, 20)) = 1.0
        _DotSize ("Dot Size", Range(0.01, 0.5)) = 0.15
        _DotSharpness ("Dot Sharpness", Range(0.1, 10)) = 3.0
        _DotBrightness ("Dot Brightness", Range(0, 5)) = 2.0
        
        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 0.0
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 0.0
        
        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
        [Toggle] _PreviewMode ("Preview Mode (Inspector Only)", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        LOD 100
        
        Pass
        {
            Name "DOTGRID"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 objectPos : TEXCOORD1;
                float3 objectNormal : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };
            
            uniform float4 _Color;
            uniform float _BackgroundAlpha;
            uniform float _Progress;
            uniform float _EdgeThickness;
            uniform float _GridScale;
            uniform float _DotSize;
            uniform float _DotSharpness;
            uniform float _DotBrightness;
            uniform float _PulseSpeed;
            uniform float _PulseIntensity;
            uniform float _PreviewMode;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.objectPos = v.vertex.xyz;
                o.objectNormal = v.normal;
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Get sphere center in world space
                float3 sphereCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Calculate distance from center in LOCAL space (accounts for scale)
                float3 localPos = i.worldPos - sphereCenter;
                float distFromCenter = length(localPos);
                
                // The mesh is a unit sphere (radius 1) that gets scaled
                // So we compare against the scaled radius (which is the mesh size)
                // Get the scale from the transform
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                float meshRadius = scale.x; // Assume uniform scale
                
                // Normalize distance based on actual mesh radius
                float normalizedDist = distFromCenter / max(meshRadius, 0.001);
                
                // Calculate edge mask - only show near the edge (normalized distance ~1.0)
                float edgeDist = abs(normalizedDist - 1.0);
                float edgeMask = 1.0;
                float progressFade = 1.0;
                
                // Only apply masking in non-preview mode
                if (_PreviewMode < 0.5)
                {
                    edgeMask = 1.0 - smoothstep(0.0, _EdgeThickness, edgeDist);
                    progressFade = 1.0 - _Progress;
                }
                
                // Calculate grid UV coordinates
                float2 gridUV;
                if (_PreviewMode > 0.5)
                {
                    // Preview mode: simple UV mapping
                    gridUV = i.uv * 20.0;
                }
                else
                {
                    // Runtime mode: Use cubic projection for perfectly uniform dot spacing
                    // This maps the sphere to a cube, avoiding all pole distortion
                    float3 normalized = normalize(i.objectPos);
                    float3 absPos = abs(normalized);
                    
                    // Find dominant axis
                    float maxAxis = max(absPos.x, max(absPos.y, absPos.z));
                    
                    // Project onto cube face based on dominant axis
                    float2 cubeUV;
                    if (absPos.x >= maxAxis)
                    {
                        // X face
                        cubeUV = float2(normalized.z / absPos.x, normalized.y / absPos.x);
                    }
                    else if (absPos.y >= maxAxis)
                    {
                        // Y face
                        cubeUV = float2(normalized.x / absPos.y, normalized.z / absPos.y);
                    }
                    else
                    {
                        // Z face
                        cubeUV = float2(normalized.x / absPos.z, normalized.y / absPos.z);
                    }
                    
                    // Apply grid scale and mesh radius
                    // Invert GridScale so higher values = more spacing (fewer dots)
                    float spacingFactor = 1.0 / max(_GridScale, 0.01);
                    gridUV = cubeUV * spacingFactor * meshRadius * 5.0;
                }
                
                // Get fractional part for dot pattern
                float2 gridFrac = frac(gridUV);
                
                // Center the coordinates around each grid cell
                float2 centered = gridFrac - 0.5;
                
                // Calculate distance from center of each cell
                float dist = length(centered);
                
                // Create dots with smooth edges
                float dotMask = 1.0 - saturate(dist / _DotSize);
                dotMask = pow(dotMask, _DotSharpness);
                
                // Pulse animation
                float pulse = 1.0;
                if (_PulseSpeed > 0.0)
                {
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseIntensity;
                }
                
                // Combine masks
                float finalMask = dotMask * edgeMask * progressFade;
                
                // Calculate final color
                float3 finalColor = _Color.rgb * finalMask * _DotBrightness * pulse;
                
                // Calculate alpha
                float alpha = lerp(_BackgroundAlpha, _Color.a, finalMask);
                
                // Ensure we return something visible for debugging
                // If finalMask is too low, boost it in preview mode
                if (_PreviewMode > 0.5 && finalMask < 0.01)
                {
                    finalColor = _Color.rgb * _DotBrightness * 0.5;
                    alpha = _Color.a * 0.5;
                }
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
}
