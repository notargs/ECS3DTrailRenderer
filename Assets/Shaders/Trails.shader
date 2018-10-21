Shader "Unlit/Trails"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            // ComputeBufferを受け取る
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<int3> _Segments;
            
            // 頂点を加工し、Trailの位置に移動させる
            void TransformVertex(inout float3 _vertex, uint _instanceID, inout float4 _color : COLOR0) {
                // Segment情報を取得
                int index = _vertex.x;
                int3 segment = _Segments[_instanceID];
                
                // 今計算中のTrail、次のTrail、前のTrailの位置を求める
                float3 p1 = _Positions[segment.x + index];
                float3 p0 = index == 0 ? p1 + p1 - _Positions[segment.x + index + 1] :  _Positions[segment.x + index - 1];
                float3 p2 = index == segment.y - 1 ? p1 + p1 - _Positions[segment.x + index - 1] :  _Positions[segment.x + index + 1];
                
                // Trailの位置関係から、接空間を求める
                float3 tangent = normalize(p2 - p0);
                float3 binormal = cross(tangent, float3(0, 1, 0));
                float3 normal = cross(tangent, binormal);
                
                // 頂点位置を計算する
                _vertex = p1 + 0.05 * (_vertex.y * binormal + _vertex.z * normal);
                
                // 色を適当に計算する
                _color.rgb = hsv2rgb(float3(frac(segment.z * 0.1), 1, 1));
                _color.a = index < segment.y ? 1 : 0; // segmentのサイズ以上の頂点を描画中なら、alphaを0（非表示）にする
            }
            
            // 頂点シェーダー
            // instanceIDに何番目のMeshを描画中か？という情報が入ってくる
            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                TransformVertex(v.vertex.xyz, instanceID, o.color); // 頂点を加工する
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag (v2f i) : SV_Target
            {
                // alphaが1未満なら非表示にする
                fixed4 col = i.color;
                clip(col.a - 0.999);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
