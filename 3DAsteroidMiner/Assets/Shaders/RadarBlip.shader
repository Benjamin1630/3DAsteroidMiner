Shader "Custom/RadarBlip"
{
    Properties
    {
        _Color ("Color", Color) = (0.2, 1, 0.2, 1)
        _EmissionColor ("Emission", Color) = (0.2, 1, 0.2, 2)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 3
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Cull Off
        ZWrite Off
        Blend One One // Additive blending for glow
        
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
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            
            fixed4 _Color;
            fixed4 _EmissionColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _FresnelPower;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel effect (edge glow)
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, i.viewDir)), _FresnelPower);
                
                // Pulse effect
                float pulse = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;
                
                // Combine color and emission
                fixed4 col = _Color;
                col.rgb += _EmissionColor.rgb * _GlowIntensity;
                col.rgb *= (1.0 + pulse * 0.5); // Brightness pulse
                col.rgb += fresnel * _Color.rgb * 2.0; // Fresnel rim
                
                // Apply alpha
                col.a = _Color.a * (1.0 + pulse * 0.3);
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
