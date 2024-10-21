Shader "UI/RoundImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // 图像纹理
        _Radius ("Corner Radius", Float) = 0.1 // 圆角半径
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Radius; // 圆角半径

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                // 保持 UV 在 [0, 1] 范围内
                float2 uv = i.uv;

                // 圆角半径
                float radius = _Radius;

                // 左上角圆 (0, 1)
                float2 cornerLT = float2(radius, 1.0 - radius);
                if (uv.x <= radius && uv.y >= 1.0 - radius)
                {
                    float distLT = length(uv - cornerLT);
                    // 保留圆内部分，丢弃三角形部分
                    if (distLT > radius)
                    {
                        discard; // 超出圆角区域，丢弃三角形部分
                    }
                }

                // 右上角圆 (1, 1)
                float2 cornerRT = float2(1.0 - radius, 1.0 - radius);
                if (uv.x >= 1.0 - radius && uv.y >= 1.0 - radius)
                {
                    float distRT = length(uv - cornerRT);
                    // 保留圆内部分，丢弃三角形部分
                    if (distRT > radius)
                    {
                        discard; // 超出圆角区域，丢弃三角形部分
                    }
                }

                // 左下角圆 (0, 0)
                float2 cornerLB = float2(radius, radius);
                if (uv.x <= radius && uv.y <= radius)
                {
                    float distLB = length(uv - cornerLB);
                    // 保留圆内部分，丢弃三角形部分
                    if (distLB > radius)
                    {
                        discard; // 超出圆角区域，丢弃三角形部分
                    }
                }

                // 右下角圆 (1, 0)
                float2 cornerRB = float2(1.0 - radius, radius);
                if (uv.x >= 1.0 - radius && uv.y <= radius)
                {
                    float distRB = length(uv - cornerRB);
                    // 保留圆内部分，丢弃三角形部分
                    if (distRB > radius)
                    {
                        discard; // 超出圆角区域，丢弃三角形部分
                    }
                }

                return col; // 保留在圆角区域内的像素
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
