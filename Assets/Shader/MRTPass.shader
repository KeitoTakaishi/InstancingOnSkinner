Shader "Unlit/MRTShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		//ZTest Always ZWrite Off
		Tags { "MRT" = "Source" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal: NORMAL;

			};

			struct v2f {
				float4 position : SV_POSITION;
				float3 texcoord: TEXCOORD0;
				float3 normal: NORMAL;
			};

			v2f vert(appdata v) {
				v2f o;
				o.position = float4(v.texcoord.x * 2.0 - 1.0, 0.0, 0.0, 1);
				o.texcoord = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			struct FragmentOutput
			{
				float4 rt0 : COLOR0;
				float4 rt1 : COLOR1;
			};

			sampler2D RT0;
			sampler2D RT1;

			FragmentOutput frag(v2f i)
			{
				/*
				normal.w = 1.0 is for Debug View
				*/
				FragmentOutput o;
				o.rt0 = float4(i.texcoord, 1.0);
				o.rt1 = float4(i.normal, 1.0);
				return o;
			}
			ENDCG
	}
	}
}