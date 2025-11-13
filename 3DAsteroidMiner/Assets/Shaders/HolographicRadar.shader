Shader "Custom/HolographicRadar"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.2, 0.8, 1, 1)
        _EmissionColor ("Emission Color", Color) = (0.2, 0.8, 1, 2)
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3
        _FresnelIntensity ("Fresnel Intensity", Range(0, 5)) = 2
        _ScanlineSpeed ("Scanline Speed", Range(0, 10)) = 2
        _ScanlineFrequency ("Scanline Frequency", Range(1, 50)) = 20
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _Opacity ("Opacity", Range(0, 1)) = 0.7
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 5
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.5)) = 0.1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        // Two-sided rendering for hologram
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
            };
            
            fixed4 _Color;
            fixed4 _EmissionColor;
            float _FresnelPower;
            float _FresnelIntensity;
            float _ScanlineSpeed;
            float _ScanlineFrequency;
            float _ScanlineIntensity;
            float _Opacity;
            float _FlickerSpeed;
            float _FlickerIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel effect (holographic rim lighting)
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, i.viewDir)), _FresnelPower);
                fresnel *= _FresnelIntensity;
                
                // Scanlines (horizontal moving lines)
                float scanline = sin((i.worldPos.y + _Time.y * _ScanlineSpeed) * _ScanlineFrequency);
                scanline = scanline * 0.5 + 0.5; // Remap to 0-1
                scanline *= _ScanlineIntensity;
                
                // Flicker effect (random subtle brightness variation)
                float flicker = sin(_Time.y * _FlickerSpeed + i.worldPos.x * 10.0) * _FlickerIntensity;
                flicker += 1.0;
                
                // Combine effects
                fixed4 col = _Color;
                col.rgb += _EmissionColor.rgb;
                col.rgb += fresnel;
                col.rgb += scanline;
                col.rgb *= flicker;
                
                // Apply opacity
                col.a = _Opacity + fresnel * 0.3;
                
                // Grid pattern (optional)
                float2 gridUV = frac(i.uv * 10.0);
                float grid = step(0.9, gridUV.x) + step(0.9, gridUV.y);
                col.rgb += grid * 0.1;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
