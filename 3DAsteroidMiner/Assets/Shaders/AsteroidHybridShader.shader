Shader "Custom/AsteroidHybridShader"
{
    Properties
    {
        [Header(Base Asteroid Texture)]
        _MainTex ("Asteroid Texture", 2D) = "grey" {}
        _BaseColor ("Base Asteroid Color", Color) = (0.4, 0.4, 0.4, 1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        
        [Header(Asteroid Type Color (Revealed in Holes))]
        _TypeColor ("Type Color", Color) = (1, 0.5, 0, 1)
        _TypeColorIntensity ("Type Color Intensity", Range(0, 3)) = 1.5
        _TypeColorEmission ("Type Color Emission", Range(0, 2)) = 0.3
        
        [Header(Swiss Cheese Holes)]
        _HoleDensity ("Hole Density", Range(0, 20)) = 8.0
        _HoleSize ("Hole Size", Range(0, 1)) = 0.35
        _HoleDepth ("Hole Depth", Range(0, 0.5)) = 0.15
        _HoleEdgeSharpness ("Hole Edge Sharpness", Range(1, 10)) = 3.0
        _HoleNoiseScale ("Hole Noise Scale", Range(1, 50)) = 15.0
        
        [Header(Cel Shading)]
        _CelBands ("Cel Shading Bands", Range(2, 10)) = 4
        _CelSmoothness ("Cel Band Smoothness", Range(0, 0.5)) = 0.05
        
        [Header(Lighting)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0.1
        _RimColor ("Rim Light Color", Color) = (0.3, 0.3, 0.4, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
        
        [Header(Ambient)]
        _AmbientColor ("Ambient Color", Color) = (0.1, 0.1, 0.15, 1)
        _AmbientIntensity ("Ambient Intensity", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf CelShaded fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        
        float4 _BaseColor;
        float4 _TypeColor;
        float _TypeColorIntensity;
        float _TypeColorEmission;
        float _NormalStrength;
        
        float _HoleDensity;
        float _HoleSize;
        float _HoleDepth;
        float _HoleEdgeSharpness;
        float _HoleNoiseScale;
        
        float _CelBands;
        float _CelSmoothness;
        
        float _Smoothness;
        float _Metallic;
        float4 _RimColor;
        float _RimPower;
        float _RimIntensity;
        
        float4 _AmbientColor;
        float _AmbientIntensity;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
        };
        
        // ============================================
        // Noise Functions for Procedural Holes
        // ============================================
        
        // 3D Noise function
        float hash(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }
        
        float noise3D(float3 x)
        {
            float3 i = floor(x);
            float3 f = frac(x);
            f = f * f * (3.0 - 2.0 * f);
            
            return lerp(
                lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                     lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                     lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y),
                f.z);
        }
        
        // Voronoi-like function for creating holes
        float voronoiHoles(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            
            float minDist = 1.0;
            
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    for(int z = -1; z <= 1; z++)
                    {
                        float3 neighbor = float3(x, y, z);
                        float3 cellPoint = hash(i + neighbor) * float3(1, 1, 1);
                        float3 diff = neighbor + cellPoint - f;
                        float dist = length(diff);
                        minDist = min(minDist, dist);
                    }
                }
            }
            
            return minDist;
        }
        
        // Combined hole pattern
        float calculateHoles(float3 worldPos)
        {
            // Voronoi pattern for main holes
            float voronoi = voronoiHoles(worldPos * _HoleDensity);
            
            // Additional noise for variation
            float noise = noise3D(worldPos * _HoleNoiseScale);
            
            // Combine patterns
            float holes = voronoi + noise * 0.2;
            
            // Create sharp-edged holes
            float holeMask = smoothstep(_HoleSize - 0.05, _HoleSize + 0.05, holes);
            
            // Apply edge sharpness
            holeMask = pow(holeMask, _HoleEdgeSharpness);
            
            return holeMask;
        }
        
        // ============================================
        // Custom Cel-Shaded Lighting Model
        // ============================================
        
        half4 LightingCelShaded(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            // Calculate basic lighting
            half NdotL = max(0, dot(s.Normal, lightDir));
            
            // Apply cel shading bands
            half celNdotL = floor(NdotL * _CelBands) / _CelBands;
            
            // Smooth the bands slightly
            celNdotL = smoothstep(celNdotL - _CelSmoothness, celNdotL + _CelSmoothness, NdotL);
            
            // Specular highlight (cel-shaded)
            half3 halfVector = normalize(lightDir + viewDir);
            half NdotH = max(0, dot(s.Normal, halfVector));
            half specular = pow(NdotH, s.Gloss * 128.0);
            
            // Cel-shade the specular
            specular = step(0.5, specular) * s.Gloss;
            
            // Combine diffuse and specular
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (celNdotL * atten + specular);
            c.a = s.Alpha;
            
            return c;
        }
        
        // ============================================
        // Surface Shader
        // ============================================
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            // Calculate hole mask
            float holeMask = calculateHoles(IN.worldPos);
            
            // Base asteroid texture
            fixed4 baseTex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 baseColor = baseTex * _BaseColor;
            
            // Type color (revealed in holes)
            fixed4 typeColor = _TypeColor * _TypeColorIntensity;
            
            // Blend between base and type color based on holes
            fixed4 finalColor = lerp(typeColor, baseColor, holeMask);
            
            // Normal mapping
            float3 normalTex = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
            normalTex.xy *= _NormalStrength;
            
            // Modify normals in holes (make them look like indentations)
            float holeDepthGradient = 1.0 - holeMask;
            normalTex.xy += holeDepthGradient * 0.3;
            
            // Rim lighting
            float3 worldNormal = WorldNormalVector(IN, normalTex);
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), worldNormal));
            rim = pow(rim, _RimPower) * _RimIntensity;
            
            // Apply rim light (stronger on edges)
            finalColor.rgb += _RimColor.rgb * rim;
            
            // Add emission to holes (glowing type color)
            float holeEmission = (1.0 - holeMask) * _TypeColorEmission;
            finalColor.rgb += _TypeColor.rgb * holeEmission;
            
            // Ambient lighting
            finalColor.rgb += _AmbientColor.rgb * _AmbientIntensity;
            
            // Output
            o.Albedo = finalColor.rgb;
            o.Normal = normalTex;
            o.Gloss = _Smoothness;
            o.Specular = _Metallic;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}
