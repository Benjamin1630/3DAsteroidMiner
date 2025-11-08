Shader "Custom/SpaceSkyboxWithSun"
{
    Properties
    {
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunSize ("Sun Size", Range(0.01, 0.5)) = 0.05
        _SunIntensity ("Sun Intensity", Range(0, 5)) = 2.0
        _SunGlow ("Sun Glow", Range(0, 1)) = 0.3
        _SunDirection ("Sun Direction", Vector) = (0, 0.5, 0.5, 0)
        _StarfieldTex ("Starfield Texture (Optional)", 2D) = "black" {}
        _StarfieldIntensity ("Starfield Intensity", Range(0, 2)) = 0.5
        
        // Procedural Stars
        _StarDensity ("Star Density", Range(0.1, 10)) = 2.0
        _StarBrightness ("Star Brightness", Range(0, 2)) = 0.8
        _StarSize ("Star Size", Range(0.0001, 0.01)) = 0.003
        _StarColorVariation ("Star Color Variation", Range(0, 1)) = 0.3
        _AmbientStarlight ("Ambient Starlight", Range(0, 1.5)) = 0.25
        
        [Header(Fog Integration)]
        _FogBlendStrength ("Fog Blend Strength", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            
            float4 _SunColor;
            float _SunSize;
            float _SunIntensity;
            float _SunGlow;
            float3 _SunDirection;
            sampler2D _StarfieldTex;
            float _StarfieldIntensity;
            
            // Procedural star properties
            float _StarDensity;
            float _StarBrightness;
            float _StarSize;
            float _StarColorVariation;
            float _AmbientStarlight;
            
            float _FogBlendStrength;
            
            // Hash function for pseudo-random generation
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            
            // 3D noise function
            float noise3D(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(lerp(hash(p + float3(0,0,0)), hash(p + float3(1,0,0)), f.x),
                         lerp(hash(p + float3(0,1,0)), hash(p + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(p + float3(0,0,1)), hash(p + float3(1,0,1)), f.x),
                         lerp(hash(p + float3(0,1,1)), hash(p + float3(1,1,1)), f.x), f.y),
                    f.z);
            }
            
            // Generate procedural stars
            float3 proceduralStars(float3 dir)
            {
                // Create multiple layers of stars at different scales
                float3 starColor = float3(0, 0, 0);
                
                // Layer 1: Dense small stars
                float3 p1 = dir * 100.0 * _StarDensity;
                float n1 = noise3D(p1);
                float star1 = pow(max(0.0, n1 - 0.85), 3.0) / _StarSize;
                
                // Layer 2: Medium stars
                float3 p2 = dir * 50.0 * _StarDensity;
                float n2 = noise3D(p2);
                float star2 = pow(max(0.0, n2 - 0.88), 4.0) / (_StarSize * 1.5);
                
                // Layer 3: Bright stars
                float3 p3 = dir * 25.0 * _StarDensity;
                float n3 = noise3D(p3);
                float star3 = pow(max(0.0, n3 - 0.92), 5.0) / (_StarSize * 2.0);
                
                // Combine layers
                float starIntensity = saturate(star1 + star2 * 1.5 + star3 * 2.5) * _StarBrightness;
                
                // Add color variation (some stars are blue-white, others yellow-white)
                float colorSeed = hash(floor(p1));
                float3 baseStarColor = lerp(
                    float3(1, 1, 1),  // White
                    lerp(
                        float3(0.8, 0.9, 1.0),  // Blue-white
                        float3(1.0, 0.95, 0.8),  // Yellow-white
                        colorSeed
                    ),
                    _StarColorVariation
                );
                
                starColor = baseStarColor * starIntensity;
                
                return starColor;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = v.texcoord;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize view direction
                float3 viewDir = normalize(i.viewDir);
                float3 sunDir = normalize(_SunDirection);
                
                // Calculate angle between view direction and sun direction
                float sunDot = dot(viewDir, sunDir);
                
                // Sun disc (sharp edge)
                float sunDisc = smoothstep(1.0 - _SunSize, 1.0 - _SunSize + 0.001, sunDot);
                
                // Sun glow (soft gradient around sun)
                float sunGlow = smoothstep(1.0 - _SunSize * 4.0, 1.0 - _SunSize, sunDot) * _SunGlow;
                
                // Combine sun disc and glow
                float sunMask = saturate(sunDisc + sunGlow);
                
                // Sun color with intensity
                float3 sunColor = _SunColor.rgb * _SunIntensity * sunMask;
                
                // Optional starfield texture
                float2 skyUV = float2(
                    atan2(viewDir.x, viewDir.z) / (2.0 * 3.14159265) + 0.5,
                    asin(viewDir.y) / 3.14159265 + 0.5
                );
                float3 starfield = tex2D(_StarfieldTex, skyUV).rgb * _StarfieldIntensity;
                
                // Generate procedural stars
                float3 proceduralStarfield = proceduralStars(viewDir);
                
                // Ambient starlight contribution (affects scene lighting)
                // This creates a soft fill light based on the star presence
                float starPresence = saturate(length(proceduralStarfield) * 3.0);
                float3 ambientContribution = proceduralStarfield * _AmbientStarlight;
                
                // Combine all elements: texture starfield + procedural stars + ambient from stars + sun
                // Note: No base ambient color - space stays black except where stars exist
                float3 finalColor = starfield + proceduralStarfield + ambientContribution + sunColor;
                
                // ============ FOG INTEGRATION ============
                // Blend fog based on viewing angle (more fog at horizon, less when looking up/down)
                float viewDirY = normalize(viewDir).y;
                float horizonFogFactor = saturate(1.0 - abs(viewDirY) * 2.0); // More fog at horizon (y=0)
                
                // Apply Unity's fog color with custom blending
                #ifdef UNITY_PASS_FORWARDBASE
                    float fogAmount = horizonFogFactor * _FogBlendStrength;
                    finalColor = lerp(finalColor, unity_FogColor.rgb, fogAmount);
                #endif
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
