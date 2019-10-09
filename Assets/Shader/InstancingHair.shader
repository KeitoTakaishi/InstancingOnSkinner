Shader "Custom/InstancingHair"
{
	Properties
	{

		[HDR]_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Amp("Amplitude", Range(0.0, 5.0)) = 1.0
		_PeriodY("PeriodY", Range(0.0, 100.0)) = 1.0
		_Scattered("Scattered", Range(0.0, 500.0)) = 1.0

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "utils.hlsl"
			#include "noise4d.cginc"
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup

			//-----------------------------------------------------
		float3 snoise3D(float4 x)
		{
			float s = snoise(x);
			float s1 = snoise(float4(x.y - 19.1, x.z + 33.4, x.x + 47.2, x.w));
			float s2 = snoise(float4(x.z + 74.2, x.x - 124.5, x.y + 99.4, x.w));
			float3 c = float3(s, s1, s2);
			return c;

}

		float3 curlNoise(float4 p) {

			const float e = 0.0009765625;
			float4 dx = float4(e, 0.0, 0.0, 0.0);
			float4 dy = float4(0.0, e, 0.0, 0.0);
			float4 dz = float4(0.0, 0.0, e, 0.0);

			float3 p_x0 = snoise3D(p - dx);
			float3 p_x1 = snoise3D(p + dx);
			float3 p_y0 = snoise3D(p - dy);
			float3 p_y1 = snoise3D(p + dy);
			float3 p_z0 = snoise3D(p - dz);
			float3 p_z1 = snoise3D(p + dz);

			float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
			float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
			float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

			const float divisor = 1.0 / (2.0 * e);
			return normalize(float3(x, y, z) * divisor);
		}
		//-----------------------------------------------------



			sampler2D _MainTex;

			struct Input
			{
				//float2 uv_MainTex;
				float3 worldPos;
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent :TANGENT0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				uint instanceID : SV_InstanceID;
			};

			struct instanceData
			{
				float3 position;

			};

			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			// Boidデータの構造体バッファ
			StructuredBuffer<float3> positionBuffer;
			StructuredBuffer<float3> axisBuffer;
			StructuredBuffer<float> rotateBuffer;

			//StructuredBuffer<float3> normalBuffer;
			//StructuredBuffer<float4>tangentBuffer;
			//StructuredBuffer<float3>rotationBuffer;
			#endif



			void setup() {
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				//float3 p = positionBuffer[unity_InstanceID] * 10.0;
				#endif
			}

			float _Amp;
			float _PeriodY;
			float _Scattered;
			int InstanceNum;
			sampler2D positionRenderTexture;
			void vert(inout appdata v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input, o);
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				/*
				//RenderTextureを送信するパターン
				float id = fmod(unity_InstanceID, InstanceNum);
				float uvx = (float)(id) / (float)(InstanceNum);
				float4 _uv = float4(uvx, 0.0, 0.0, 0.0);
				float3 position = tex2Dlod(positionRenderTexture, float4(_uv)).xyz;

				//行列演算出ないと座標変換できない
				id *= 10.0;
				float scaleOff = 0.1* frac(id / 100.0);
				float4  vertex = mul(ScaleMatrix(float3(scaleOff, scaleOff, scaleOff)), v.vertex);
				vertex = mul(RotYMatrix(_Time.y * 10.0 + id), vertex);
				vertex = mul(RotXMatrix(_Time.y * 10.0 + id), vertex);
				vertex = mul(RotZMatrix(_Time.y * 10.0 + id),vertex);
				v.vertex = mul(TranslateMatrix(position * 1.0), vertex);
				o.worldPos = position;
				*/

				int id = unity_InstanceID;
				float3 position = positionBuffer[id];
				
				float l = v.vertex.z;
				float scale = 0.2;
				float4  vertex = mul(ScaleMatrix(float3(scale, scale * 2.0, scale)), v.vertex);
				vertex = mul(Rodrigues(axisBuffer[id], rotateBuffer[id]), vertex);	//rotatte
				v.vertex = mul(TranslateMatrix(position), vertex);//translate
				float t = _Time.y * 2.0;
				float s = sin(t + l * _PeriodY + unity_InstanceID * _Scattered);
				float c = cos(t + l * _PeriodY + unity_InstanceID + _Scattered);
				v.vertex.x += sin(t + l * _PeriodY + unity_InstanceID * _Scattered) * _Amp * l * snoise(float4(position, t));
				v.vertex.y += cos(t + l * _PeriodY + unity_InstanceID + _Scattered) * _Amp * l * snoise(float4(position, t));
				o.worldPos = position;

				/*
				float scaleOff = 0.1;
				float4  vertex = mul(ScaleMatrix(float3(scaleOff, scaleOff, scaleOff)), v.vertex);
				v.vertex = mul(TranslateMatrix(position * 1.0), vertex);
				o.worldPos = position;
				*/

				#endif
			}

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			int width, height;
			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				float len = length(float3(0.2, 0.5, 0.1) - IN.worldPos);
				fixed4 c = fixed4(1.0, 1.0, 1.0, 1.0) * _Color * frac(_Time.y + len) *  2.0 * frac(sin(_Time.y));
				//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
