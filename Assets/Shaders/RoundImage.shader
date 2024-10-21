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
            float4 _MainTex_ST;
            float _Radius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                // 获取当前UV坐标
                float2 uv = i.uv * 2.0 - 1.0; // UV 范围转换到 [-1,1]
                float2 absUV = abs(uv); // 取绝对值，简化处理

                // 计算距离是否在圆角区域外
                float dist = max(absUV.x - 1.0 + _Radius, absUV.y - 1.0 + _Radius);
                if (dist > 0.0)
                {
                    discard; // 如果超出圆角区域，丢弃像素
                }

                return col;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
