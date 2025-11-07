Shader "Custom/ProceduralSpaceSkybox"
{
    Properties
    {
        [Header(Sun Settings)]
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunSize ("Sun Size", Range(0.001, 0.1)) = 0.0125
        _SunIntensity ("Sun Intensity", Range(0, 10)) = 3.0
        _SunGlow ("Sun Glow", Range(0, 2)) = 0.5
        _SunDirection ("Sun Direction", Vector) = (0, 0.5, 0.5, 0)
        
        [Header(Background Texture Options)]
        [KeywordEnum(None, Texture2D, Cubemap)] _BackgroundMode("Background Mode", Float) = 0
        _StarfieldTex ("Starfield Texture 2D", 2D) = "black" {}
        _StarfieldCube ("Starfield Cubemap", Cube) = "black" {}
        _BackgroundIntensity ("Background Intensity", Range(0, 2)) = 0.5
        
        [Header(Star Properties)]
        _StarDensity ("Star Density", Range(100, 2000)) = 800
        _StarBrightness ("Star Brightness", Range(0, 5)) = 1.5
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0, 2)) = 0.3
        _StarColorVariation ("Star Color Variation", Range(0, 1)) = 0.4
        _AmbientStarlight ("Ambient Starlight Multiplier", Range(0, 3)) = 0.8
        
        [Header(Nebula Background)]
        _NebulaColor1 ("Nebula Color 1", Color) = (0.1, 0.05, 0.2, 1)
        _NebulaColor2 ("Nebula Color 2", Color) = (0.05, 0.1, 0.15, 1)
        _NebulaIntensity ("Nebula Intensity", Range(0, 0.5)) = 0.05
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
            #pragma shader_feature _BACKGROUNDMODE_NONE _BACKGROUNDMODE_TEXTURE2D _BACKGROUNDMODE_CUBEMAP
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
            };
            
            // Properties
            uniform float4 _SunColor;
            uniform float _SunSize;
            uniform float _SunIntensity;
            uniform float _SunGlow;
            uniform float3 _SunDirection;
            
            // Background textures
            uniform sampler2D _StarfieldTex;
            uniform samplerCUBE _StarfieldCube;
            uniform float _BackgroundIntensity;
            
            uniform float _StarDensity;
            uniform float _StarBrightness;
            uniform float _StarTwinkleSpeed;
            uniform float _StarColorVariation;
            uniform float _AmbientStarlight;
            
            uniform float4 _NebulaColor1;
            uniform float4 _NebulaColor2;
            uniform float _NebulaIntensity;
            
            // ============ NOISE FUNCTIONS ============
            
            // High-quality hash function for star distribution
            float hash13(float3 p3)
            {
                p3 = frac(p3 * 0.1031);
                p3 += dot(p3, p3.zyx + 31.32);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float3 hash33(float3 p3)
            {
                p3 = frac(p3 * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yxx) * p3.zyx);
            }
            
            // 3D value noise
            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(lerp(hash13(i + float3(0,0,0)), hash13(i + float3(1,0,0)), f.x),
                         lerp(hash13(i + float3(0,1,0)), hash13(i + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash13(i + float3(0,0,1)), hash13(i + float3(1,0,1)), f.x),
                         lerp(hash13(i + float3(0,1,1)), hash13(i + float3(1,1,1)), f.x), f.y),
                    f.z);
            }
            
            // Voronoi-based star field (creates distinct points instead of noise clouds)
            float3 voronoiStars(float3 dir)
            {
                // Scale direction to create star grid
                float3 p = dir * _StarDensity;
                float3 cellId = floor(p);
                float3 cellPos = frac(p);
                
                float minDist = 1.0;
                float3 starColor = float3(0, 0, 0);
                float3 closestPoint;
                
                // Check neighboring cells for closest star
                for(int z = -1; z <= 1; z++)
                {
                    for(int y = -1; y <= 1; y++)
                    {
                        for(int x = -1; x <= 1; x++)
                        {
                            float3 neighbor = float3(x, y, z);
                            float3 neighborCell = cellId + neighbor;
                            
                            // Get random point in this cell
                            float3 randomOffset = hash33(neighborCell);
                            float3 pointPos = neighbor + randomOffset - cellPos;
                            
                            float dist = length(pointPos);
                            
                            if(dist < minDist)
                            {
                                minDist = dist;
                                closestPoint = randomOffset;
                                
                                // Star properties based on cell ID
                                float starSeed = hash13(neighborCell);
                                float starBrightness = pow(starSeed, 2.0); // Some stars brighter than others
                                
                                // Star color variation (blue-white to yellow-white)
                                float colorTemp = hash13(neighborCell * 7.531);
                                float3 baseColor = lerp(
                                    float3(0.7, 0.8, 1.0),   // Blue-white (hot stars)
                                    float3(1.0, 0.95, 0.8),  // Yellow-white (cooler stars)
                                    colorTemp
                                );
                                baseColor = lerp(float3(1, 1, 1), baseColor, _StarColorVariation);
                                
                                // Twinkle effect
                                float twinkle = 1.0;
                                if(_StarTwinkleSpeed > 0.01)
                                {
                                    float twinklePhase = hash13(neighborCell * 13.7) * 6.28318;
                                    twinkle = 0.8 + 0.2 * sin(_Time.y * _StarTwinkleSpeed + twinklePhase);
                                }
                                
                                // Calculate star intensity (sharp falloff for point-like appearance)
                                float starSize = 0.015 + starBrightness * 0.01; // Vary size slightly
                                float starIntensity = smoothstep(starSize, 0.0, dist);
                                starIntensity = pow(starIntensity, 3.0); // Sharp edges
                                
                                starColor = baseColor * starIntensity * starBrightness * _StarBrightness * twinkle;
                            }
                        }
                    }
                }
                
                return starColor;
            }
            
            // Subtle nebula clouds
            float3 nebula(float3 dir)
            {
                float n1 = noise3D(dir * 2.0);
                float n2 = noise3D(dir * 4.0);
                float n3 = noise3D(dir * 8.0);
                
                float nebulaMask = (n1 * 0.5 + n2 * 0.3 + n3 * 0.2);
                nebulaMask = pow(saturate(nebulaMask), 2.0);
                
                float3 nebulaColor = lerp(_NebulaColor1.rgb, _NebulaColor2.rgb, n2);
                return nebulaColor * nebulaMask * _NebulaIntensity;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize view direction
                float3 viewDir = normalize(i.viewDir);
                float3 sunDir = normalize(_SunDirection);
                
                // ============ SUN ============
                float sunDot = dot(viewDir, sunDir);
                
                // Sun disc with soft edge
                float sunDisc = smoothstep(1.0 - _SunSize * 1.2, 1.0 - _SunSize * 0.8, sunDot);
                
                // Sun glow/corona
                float sunGlow = pow(saturate(sunDot), 20.0) * _SunGlow;
                sunGlow += pow(saturate(sunDot), 5.0) * _SunGlow * 0.5;
                
                float sunMask = saturate(sunDisc + sunGlow);
                float3 sunColor = _SunColor.rgb * _SunIntensity * sunMask;
                
                // ============ STARS ============
                float3 stars = voronoiStars(viewDir);
                
                // Add ambient contribution from stars
                float3 starAmbient = stars * _AmbientStarlight;
                
                // ============ NEBULA ============
                float3 nebulaColor = nebula(viewDir);
                
                // ============ BACKGROUND TEXTURE/CUBEMAP ============
                float3 backgroundTex = float3(0, 0, 0);
                
                #if defined(_BACKGROUNDMODE_TEXTURE2D)
                    // Convert view direction to spherical UV coordinates
                    float2 skyUV = float2(
                        atan2(viewDir.x, viewDir.z) / (2.0 * 3.14159265) + 0.5,
                        asin(viewDir.y) / 3.14159265 + 0.5
                    );
                    backgroundTex = tex2D(_StarfieldTex, skyUV).rgb * _BackgroundIntensity;
                #elif defined(_BACKGROUNDMODE_CUBEMAP)
                    // Sample cubemap directly with view direction
                    backgroundTex = texCUBE(_StarfieldCube, viewDir).rgb * _BackgroundIntensity;
                #endif
                
                // ============ COMBINE ============
                // Deep space is pure black, only stars and sun provide light
                float3 finalColor = backgroundTex + nebulaColor + stars + starAmbient + sunColor;
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    
    FallBack Off
}
