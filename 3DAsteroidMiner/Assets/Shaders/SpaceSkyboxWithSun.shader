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
            
            float4 _SunColor;
            float _SunSize;
            float _SunIntensity;
            float _SunGlow;
            float3 _SunDirection;
            sampler2D _StarfieldTex;
            float _StarfieldIntensity;
            
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
                
                // Optional starfield background
                float2 skyUV = float2(
                    atan2(viewDir.x, viewDir.z) / (2.0 * 3.14159265) + 0.5,
                    asin(viewDir.y) / 3.14159265 + 0.5
                );
                float3 starfield = tex2D(_StarfieldTex, skyUV).rgb * _StarfieldIntensity;
                
                // Combine black background + starfield + sun
                float3 finalColor = float3(0, 0, 0) + starfield + sunColor;
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
