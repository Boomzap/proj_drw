Shader "HO/SDFColorFromRenderer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaxDistance("Max Distance", float) = 10.0
		_GlowSize("Glow Size", float) = 0.1
		_GlowColor("Glow Color", Color) = (1.0, 0.0, 0.0, 0.5)
		_GlowAlpha("Glow Alpha", float) = 1.0
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "UnityUI.cginc"


			struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			fixed4 color : COLOR;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			fixed4 color : COLOR;
		};

		sampler2D _MainTex;		// fill texture
		float _MaxDistance;
		float _GlowSize;
		float _GlowAlpha;
		float4 _MainTex_ST;
		fixed4 _GlowColor;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			float2 muv = i.uv;
			float2 uvd = float2(0.05, -0.05);
			float2 duv = muv + uvd;

			float distance = tex2D(_MainTex, muv).a;
			float distance2 = tex2D(_MainTex, duv).a;

			// 'smooth clamp' distance to 0.5->0.48, then 

			float alpha = smoothstep(0.55, 0.5, 1 - distance) * (1 - smoothstep(0.45, 0.65, distance));


			/*alpha = smoothstep(_MaxDistance - _GlowSize * 0.5, _MaxDistance, distance);
			alpha *= 1.0 - smoothstep(_MaxDistance, _MaxDistance + _GlowSize * 0.5, distance);*/
			//float alpha2 = smoothstep(0.40, 0.60, distance2);

			//alpha *= alpha2;

			return fixed4(i.color.rgb, alpha * _GlowColor.a * _GlowAlpha * i.color.a);
		}
			ENDCG

			SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

				Cull Off
				Lighting Off
				ZWrite Off
				ZTest[unity_GUIZTestMode]
				Blend SrcAlpha OneMinusSrcAlpha

				Pass
			{
				CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
				ENDCG
			}
		}
}
