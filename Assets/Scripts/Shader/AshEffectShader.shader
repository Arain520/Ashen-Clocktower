Shader "Custom/AshEffectShaderWithGlow"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.5, 0.5, 0.5, 1) // 主要颜色（灰色）
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0, 1)  // 橙色发光（RGBA）
        _GlowStrength ("Glow Strength", Range(0, 5)) = 1  // 发光强度
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            float _Transparency;
            float _GlowStrength;
            float4 _GlowColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 灰色化效果
                half4 col = half4(0.5, 0.5, 0.5, 1) * i.color;  // 灰色化处理
                col.a = _Transparency;  // 控制透明度

                // 添加橙色/黄色发光效果
                half4 glow = _GlowColor * _GlowStrength;
                col += glow;  // 叠加发光效果

                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}