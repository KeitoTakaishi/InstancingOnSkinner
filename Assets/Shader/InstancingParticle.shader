Shader "Custom/InstancingParticle"
{
	Properties
	{

		[HDR]_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "utils.hlsl"
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup

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
			struct Params {
				float3 life;
				float2 size;
			};

			StructuredBuffer<float3> positionBuffer;
			StructuredBuffer<float3> normalBuffer;
			StructuredBuffer<float4>tangentBuffer;
			StructuredBuffer<float3>rotationBuffer;
			StructuredBuffer<Params>paramsBuffer;
			
			#endif



			void setup() {
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				//float3 p = positionBuffer[unity_InstanceID] * 10.0;
				#endif
			}

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
				Params parameter = paramsBuffer[id];
				float3 position = positionBuffer[id];
				float scaleOff = parameter.size.x;
				float4  vertex = mul(ScaleMatrix(float3(scaleOff, scaleOff, scaleOff)), v.vertex);
				vertex = mul(RotYMatrix(_Time.y * 10.0 + id), vertex);
				vertex = mul(RotXMatrix(_Time.y * 10.0 + id), vertex);
				vertex = mul(RotZMatrix(_Time.y * 10.0 + id), vertex);
				v.vertex = mul(TranslateMatrix(position * 1.0), vertex);
				o.worldPos = position;

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

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float curLife = paramsBuffer[ unity_InstanceID].life.x;
				float life = paramsBuffer[unity_InstanceID].life.y;
				float power = 10.0*(life - curLife);
				fixed4 c = fixed4(1.0, 1.0, 1.0, 1.0) * _Color * power;	
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
		#else			
				fixed4 c = fixed4(1.0, 1.0, 1.0, 1.0) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
		#endif
				
				//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				
			}
			ENDCG
		}
			FallBack "Diffuse"
}
