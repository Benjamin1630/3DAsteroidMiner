// ============================================================
// Asteroid Hybrid Shader - Fully Procedural (URP)
// ============================================================
// Features:
// - Swiss cheese holes revealing mineral type colors
// - Procedural craters with depth and raised rims
// - Procedural rock piles with sharp edges
// - Parallax occlusion mapping for depth illusion
// - Cel-shading mixed with realistic lighting
// - All generated from object-space coordinates (no UVs!)
// ============================================================

Shader "Custom/AsteroidHybridShader_URP"
{
    Properties
    {
        [Header(Base Asteroid Surface)]
        _BaseColor ("Base Asteroid Color", Color) = (0.35, 0.35, 0.38, 1)
        _BaseColorVariation ("Base Color Variation", Range(0, 1)) = 0.3
        _SurfaceRoughness ("Surface Roughness", Range(0, 1)) = 0.85
        
        [Header(Asteroid Type Color (Revealed in Holes))]
        _TypeColor ("Type Color", Color) = (1, 0.5, 0, 1)
        _TypeColorIntensity ("Type Color Intensity", Range(0, 3)) = 1.5
        _TypeColorEmission ("Type Color Emission", Range(0, 2)) = 0.3
        
        [Header(Swiss Cheese Holes)]
        _HoleDensity ("Hole Density", Range(0, 20)) = 5.0
        _HoleSize ("Hole Size", Range(0, 1)) = 0.5
        _HoleDepth ("Hole Depth", Range(0, 1)) = 0.4
        _HoleEdgeSharpness ("Hole Edge Sharpness", Range(1, 10)) = 3.0
        _HoleNoiseScale ("Hole Noise Scale", Range(1, 50)) = 15.0
        
        [Header(Procedural Craters)]
        _CraterDensity ("Crater Density", Range(0, 20)) = 6.0
        _CraterSize ("Crater Size", Range(0, 1)) = 0.45
        _CraterDepth ("Crater Depth", Range(0, 1)) = 0.3
        _CraterRimHeight ("Crater Rim Height", Range(0, 1)) = 0.15
        _CraterRimSharpness ("Crater Rim Sharpness", Range(1, 20)) = 8.0
        
        [Header(Procedural Rock Piles)]
        _RockPileScale ("Rock Pile Scale", Range(1, 50)) = 12.0
        _RockPileHeight ("Rock Pile Height", Range(0, 1)) = 0.25
        _RockPileSharpness ("Rock Pile Sharpness", Range(0.5, 5)) = 2.0
        _RockPileDetailScale ("Rock Detail Scale", Range(10, 100)) = 35.0
        _RockPileDetailStrength ("Rock Detail Strength", Range(0, 0.5)) = 0.15
        
        [Header(Surface Detail)]
        _DetailNoiseScale ("Detail Noise Scale", Range(10, 100)) = 45.0
        _DetailNoiseStrength ("Detail Noise Strength", Range(0, 0.5)) = 0.2
        _NormalStrength ("Overall Normal Strength", Range(0, 3)) = 1.5
        
        [Header(Parallax Depth)]
        _ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0.03
        _ParallaxSteps ("Parallax Steps", Range(4, 32)) = 16
        
        [Header(Cel Shading)]
        _CelBands ("Cel Shading Bands", Range(2, 10)) = 3
        _CelSmoothness ("Cel Band Smoothness", Range(0, 0.5)) = 0.15
        
        [Header(Lighting)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0.1
        _RimColor ("Rim Light Color", Color) = (0.3, 0.3, 0.4, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
        
        [Header(Ambient)]
        _AmbientColor ("Ambient Color", Color) = (0.1, 0.1, 0.15, 1)
        _AmbientIntensity ("Ambient Intensity", Range(0, 1)) = 0.3
        
        [Header(Scan Highlight)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1; // Object-space position
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirWS : TEXCOORD5;
                float3 viewDirTS : TEXCOORD6; // Tangent-space view direction for parallax
                float fogFactor : TEXCOORD7;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _BaseColorVariation;
                float _SurfaceRoughness;
                float4 _TypeColor;
                float _TypeColorIntensity;
                float _TypeColorEmission;
                float _NormalStrength;
                
                float _HoleDensity;
                float _HoleSize;
                float _HoleDepth;
                float _HoleEdgeSharpness;
                float _HoleNoiseScale;
                
                float _CraterDensity;
                float _CraterSize;
                float _CraterDepth;
                float _CraterRimHeight;
                float _CraterRimSharpness;
                
                float _RockPileScale;
                float _RockPileHeight;
                float _RockPileSharpness;
                float _RockPileDetailScale;
                float _RockPileDetailStrength;
                
                float _DetailNoiseScale;
                float _DetailNoiseStrength;
                
                float _ParallaxStrength;
                float _ParallaxSteps;
                
                float _CelBands;
                float _CelSmoothness;
                
                float _Smoothness;
                float _Metallic;
                float4 _RimColor;
                float _RimPower;
                float _RimIntensity;
                
                float4 _AmbientColor;
                float _AmbientIntensity;
                
                float4 _EmissionColor;
            CBUFFER_END
            
            // ============================================
            // Noise Functions
            // ============================================
            
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
            
            // Fractional Brownian Motion for detailed noise
            float fbm(float3 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < octaves; i++)
                {
                    value += amplitude * noise3D(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            // Voronoi with cell information
            float2 voronoi(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                
                float minDist = 1.0;
                float secondMinDist = 1.0;
                
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
                            
                            if(dist < minDist)
                            {
                                secondMinDist = minDist;
                                minDist = dist;
                            }
                            else if(dist < secondMinDist)
                            {
                                secondMinDist = dist;
                            }
                        }
                    }
                }
                
                return float2(minDist, secondMinDist);
            }
            
            // ============================================
            // Swiss Cheese Holes
            // ============================================
            
            float calculateHoles(float3 posOS)
            {
                float2 voronoiResult = voronoi(posOS * _HoleDensity);
                float noise = noise3D(posOS * _HoleNoiseScale);
                float holes = voronoiResult.x + noise * 0.2;
                float holeMask = smoothstep(_HoleSize - 0.05, _HoleSize + 0.05, holes);
                holeMask = pow(holeMask, _HoleEdgeSharpness);
                return holeMask;
            }
            
            // ============================================
            // Procedural Craters
            // ============================================
            
            struct CraterData
            {
                float depth;
                float rimHeight;
                float3 normal;
            };
            
            CraterData calculateCraters(float3 posOS)
            {
                CraterData result;
                result.depth = 0.0;
                result.rimHeight = 0.0;
                result.normal = float3(0, 0, 1);
                
                float2 voronoiResult = voronoi(posOS * _CraterDensity);
                float craterDist = voronoiResult.x;
                
                // Add variation with noise
                float craterNoise = noise3D(posOS * _CraterDensity * 2.0);
                craterDist += craterNoise * 0.15;
                
                // Crater bowl (inverted distance)
                float craterBowl = 1.0 - smoothstep(0.0, _CraterSize, craterDist);
                craterBowl = pow(saturate(craterBowl), 2.0); // Sharper falloff
                
                // Crater rim (ring around edge)
                float rimDist = abs(craterDist - _CraterSize * 0.7);
                float craterRim = 1.0 - smoothstep(0.0, _CraterSize * 0.3, rimDist);
                craterRim = pow(saturate(craterRim), _CraterRimSharpness);
                
                result.depth = craterBowl * _CraterDepth;
                result.rimHeight = craterRim * _CraterRimHeight;
                
                // Calculate normal based on crater gradient
                // For a depression: normals should point AWAY from center in the bowl
                // For a rim: normals should point TOWARD center on the raised edge
                
                // Find nearest voronoi cell center
                float3 cellCoord = floor(posOS * _CraterDensity);
                float3 cellOffset = hash(cellCoord) * float3(1, 1, 1);
                float3 craterCenter = (cellCoord + cellOffset) / _CraterDensity;
                
                // Direction from center to current position
                float3 dirFromCenter = normalize(posOS - craterCenter);
                
                // Crater bowl: normals tilt outward (away from center) as you go into the crater
                // Crater rim: normals tilt inward (toward center) on the raised edge
                float normalTilt = 0.0;
                
                if(craterBowl > 0.01)
                {
                    // In the bowl - tilt outward (negative for depression)
                    normalTilt = -craterBowl * 3.0;
                }
                
                if(craterRim > 0.01)
                {
                    // On the rim - tilt inward (positive for raised edge)
                    normalTilt += craterRim * 2.0;
                }
                
                // Build tangent-space normal (X and Y are horizontal tilt, Z is up)
                result.normal = normalize(float3(dirFromCenter.xy * normalTilt, 1.0));
                
                return result;
            }
            
            // ============================================
            // Procedural Rock Piles
            // ============================================
            
            struct RockPileData
            {
                float height;
                float3 normal;
            };
            
            RockPileData calculateRockPiles(float3 posOS)
            {
                RockPileData result;
                
                // Large rock formations
                float rocks = fbm(posOS * _RockPileScale, 3);
                rocks = pow(abs(rocks), _RockPileSharpness); // Make sharper edges
                
                // Add fine detail
                float detail = noise3D(posOS * _RockPileDetailScale);
                rocks += detail * _RockPileDetailStrength;
                
                // Threshold to create distinct piles
                rocks = smoothstep(0.3, 0.7, rocks);
                
                result.height = rocks * _RockPileHeight;
                
                // Calculate normal from height gradient
                float delta = 0.01;
                float heightX = fbm((posOS + float3(delta, 0, 0)) * _RockPileScale, 3);
                float heightZ = fbm((posOS + float3(0, 0, delta)) * _RockPileScale, 3);
                
                heightX = pow(abs(heightX), _RockPileSharpness);
                heightZ = pow(abs(heightZ), _RockPileSharpness);
                
                float3 tangentX = float3(1, 0, (heightX - rocks) / delta);
                float3 tangentZ = float3(0, 1, (heightZ - rocks) / delta);
                result.normal = normalize(cross(tangentZ, tangentX));
                
                return result;
            }
            
            // ============================================
            // Surface Detail Noise
            // ============================================
            
            float calculateSurfaceDetail(float3 posOS)
            {
                return fbm(posOS * _DetailNoiseScale, 4) * _DetailNoiseStrength;
            }
            
            // ============================================
            // Parallax Occlusion Mapping
            // ============================================
            
            float3 parallaxOcclusionMapping(float3 posOS, float3 viewDirTS, float3 normalWS, float3 tangentWS, float3 bitangentWS)
            {
                // Normalize view direction in tangent space
                float3 viewDirTSNorm = normalize(viewDirTS);
                
                // Prevent division by zero and artifacts at grazing angles
                // When looking parallel to surface, reduce parallax effect
                viewDirTSNorm.z = max(abs(viewDirTSNorm.z), 0.2);
                
                // Calculate initial height
                CraterData initialCraters = calculateCraters(posOS);
                RockPileData initialRocks = calculateRockPiles(posOS);
                float initialHeight = initialRocks.height - initialCraters.depth + initialCraters.rimHeight;
                
                // Scale parallax based on view angle (less effect at grazing angles)
                float parallaxScale = _ParallaxStrength * saturate(viewDirTSNorm.z * 2.0);
                
                // Calculate offset direction in tangent space
                float2 parallaxDirection = -viewDirTSNorm.xy / viewDirTSNorm.z;
                
                // Ray marching setup
                float numSteps = _ParallaxSteps;
                float layerHeight = 1.0 / numSteps;
                float currentLayerHeight = 0.0;
                float2 currentOffset = float2(0, 0);
                float currentHeightMapValue = initialHeight;
                
                // Ray march through height field
                [unroll(8)] // Unroll first 8 steps for better performance
                for(int i = 0; i < (int)numSteps; i++)
                {
                    // Break if ray is below surface
                    if(currentLayerHeight >= currentHeightMapValue)
                        break;
                    
                    // Step along ray
                    currentOffset += parallaxDirection * layerHeight * parallaxScale;
                    currentLayerHeight += layerHeight;
                    
                    // Convert tangent-space offset to object-space offset
                    float3 offsetOS = tangentWS * currentOffset.x + bitangentWS * currentOffset.y;
                    float3 samplePos = posOS + offsetOS;
                    
                    // Sample height at new position
                    CraterData craters = calculateCraters(samplePos);
                    RockPileData rocks = calculateRockPiles(samplePos);
                    currentHeightMapValue = rocks.height - craters.depth + craters.rimHeight;
                }
                
                // Apply final offset (convert from tangent space to object space)
                float3 finalOffsetOS = tangentWS * currentOffset.x + bitangentWS * currentOffset.y;
                return posOS + finalOffsetOS;
            }
            
            // ============================================
            // Vertex Shader
            // ============================================
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.positionOS = input.positionOS.xyz;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                // Calculate tangent-space view direction for parallax
                // Build world-to-tangent matrix (inverse of tangent-to-world)
                float3x3 worldToTangent = float3x3(
                    normalInput.tangentWS.x, normalInput.bitangentWS.x, normalInput.normalWS.x,
                    normalInput.tangentWS.y, normalInput.bitangentWS.y, normalInput.normalWS.y,
                    normalInput.tangentWS.z, normalInput.bitangentWS.z, normalInput.normalWS.z
                );
                output.viewDirTS = mul(worldToTangent, normalize(output.viewDirWS));
                
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            // ============================================
            // Fragment Shader
            // ============================================
            
            half4 frag(Varyings input) : SV_Target
            {
                // Normalize interpolated vectors
                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                
                // Apply parallax occlusion mapping for depth effect
                float3 parallaxPos = parallaxOcclusionMapping(
                    input.positionOS, 
                    input.viewDirTS, 
                    normalWS, 
                    tangentWS, 
                    bitangentWS
                );
                
                // Recalculate surface features at parallax-adjusted position
                float holeMask = calculateHoles(parallaxPos);
                CraterData craters = calculateCraters(parallaxPos);
                RockPileData rocks = calculateRockPiles(parallaxPos);
                float surfaceDetail = calculateSurfaceDetail(parallaxPos);
                
                // ============================================
                // Base Color Generation
                // ============================================
                
                // Procedural base color with variation
                float colorVariation = noise3D(parallaxPos * 8.0);
                float3 baseColor = _BaseColor.rgb * lerp(0.8, 1.2, colorVariation * _BaseColorVariation);
                
                // Darken craters
                baseColor *= lerp(1.0, 0.5, craters.depth / _CraterDepth);
                
                // Lighten crater rims
                baseColor += craters.rimHeight * 0.4;
                
                // Vary rock pile colors
                float rockColorVariation = noise3D(parallaxPos * _RockPileScale * 0.5);
                baseColor = lerp(baseColor, baseColor * float3(1.1, 1.05, 1.0), rocks.height * rockColorVariation);
                
                // Type color (revealed in holes)
                half3 typeColor = _TypeColor.rgb * _TypeColorIntensity;
                
                // Blend between base and type color based on holes
                half3 finalColor = lerp(typeColor, baseColor, holeMask);
                
                // ============================================
                // Normal Calculation
                // ============================================
                
                // Build tangent-space normal from procedural features
                // Start with flat normal pointing up
                float3 normalTS = float3(0, 0, 1);
                
                // Add crater normals - use additive blending for depression effect
                float craterInfluence = saturate((craters.depth + craters.rimHeight) * 2.0);
                if(craterInfluence > 0.01)
                {
                    // Blend crater normal based on strength
                    normalTS.xy += (craters.normal.xy - normalTS.xy) * craterInfluence;
                    normalTS = normalize(normalTS);
                }
                
                // Add rock pile normals
                float rockInfluence = saturate(rocks.height * 2.0);
                if(rockInfluence > 0.01)
                {
                    normalTS.xy += (rocks.normal.xy - normalTS.xy) * rockInfluence;
                    normalTS = normalize(normalTS);
                }
                
                // Add fine surface detail
                float detailGradientX = calculateSurfaceDetail(parallaxPos + float3(0.01, 0, 0)) - surfaceDetail;
                float detailGradientZ = calculateSurfaceDetail(parallaxPos + float3(0, 0, 0.01)) - surfaceDetail;
                normalTS.xy += float2(detailGradientX, detailGradientZ) * 20.0;
                
                // Add hole depth normals
                float holeDepthGradient = 1.0 - holeMask;
                normalTS.z -= holeDepthGradient * _HoleDepth;
                
                normalTS = normalize(normalTS);
                
                // Transform to world space using pre-normalized vectors
                float3x3 tangentToWorld = float3x3(
                    tangentWS,
                    bitangentWS,
                    normalWS
                );
                float3 perturbedNormalWS = normalize(mul(normalTS, tangentToWorld));
                perturbedNormalWS = lerp(normalWS, perturbedNormalWS, _NormalStrength);
                
                // ============================================
                // Lighting
                // ============================================
                
                // Get main light
                Light mainLight = GetMainLight();
                half3 lightDir = mainLight.direction;
                half3 lightColor = mainLight.color;
                
                // Calculate basic lighting using perturbed normal
                half NdotL = saturate(dot(perturbedNormalWS, lightDir));
                
                // Apply cel shading
                half celNdotL = floor(NdotL * _CelBands) / _CelBands;
                celNdotL = smoothstep(celNdotL - _CelSmoothness, celNdotL + _CelSmoothness, NdotL);
                
                // Mix cel-shaded with smooth lighting
                half finalLighting = lerp(NdotL, celNdotL, 0.7); // 70% cel, 30% smooth
                
                // ============================================
                // Surface Properties
                // ============================================
                
                // Vary roughness based on surface features
                float roughness = _SurfaceRoughness;
                roughness = lerp(roughness, roughness * 0.7, rocks.height); // Rocks slightly smoother
                roughness = lerp(roughness, 1.0, craters.depth); // Craters rougher
                
                // ============================================
                // Additional Effects
                // ============================================
                
                // Rim lighting
                half3 viewDir = normalize(input.viewDirWS);
                half rim = 1.0 - saturate(dot(viewDir, perturbedNormalWS));
                rim = pow(rim, _RimPower) * _RimIntensity;
                finalColor += _RimColor.rgb * rim;
                
                // Emission in holes (glowing type color)
                half holeEmission = (1.0 - holeMask) * _TypeColorEmission;
                finalColor += _TypeColor.rgb * holeEmission;
                
                // Crater rim highlights (subtle glow)
                finalColor += craters.rimHeight * 0.2;
                
                // Ambient occlusion in craters
                float ao = lerp(1.0, 0.4, craters.depth);
                
                // Ambient lighting
                finalColor += _AmbientColor.rgb * _AmbientIntensity * ao;
                
                // Apply main lighting
                finalColor *= lightColor * max(finalLighting, 0.3); // Minimum 30% brightness
                
                // Add scan highlight emission (from MaterialPropertyBlock or inspector)
                finalColor += _EmissionColor.rgb;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, 1.0);
            }
            
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
