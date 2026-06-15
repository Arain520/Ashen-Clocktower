Shader "AshenClocktower/AshenPhaseFilter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FilterColor ("Filter Color", Color) = (1, 0.55, 0.1, 1)
        _Intensity ("Intensity", Range(0, 1)) = 0
        _Darkness ("Darkness", Range(0, 1)) = 0.35
        _AshScale ("Ash Scale", Float) = 90
        _AshStrength ("Ash Strength", Range(0, 1)) = 0.35
        _AshSpeed ("Ash Speed", Float) = 0.15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _FilterColor;
            float _Intensity;
            float _Darkness;
            float _AshScale;
            float _AshStrength;
            float _AshSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 movingUv = i.uv;
                movingUv.y += _Time.y * _AshSpeed;

                float2 cell = floor(movingUv * _AshScale);
                float ashNoise = Hash21(cell);
                float ashMask = smoothstep(0.965, 1.0, ashNoise) * _AshStrength;

                float3 darkened = lerp(float3(0, 0, 0), _FilterColor.rgb, 1.0 - _Darkness);
                float3 finalColor = lerp(darkened, _FilterColor.rgb, ashMask);
                float finalAlpha = saturate((_Intensity + ashMask * _Intensity) * _FilterColor.a);

                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
