Shader "HO/SDFOutlineMulti"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_MaxDistanceA ("Max Distance", float) = 0.5
		_MaxDistanceB ("Max Distance", float) = 0.6

		_GlowSizeA ("Glow Size A", float) = 0.1
		_GlowSizeB ("Glow Size B", float) = 0.1

		_GlowColorA ("Glow Color A", Color) = (1.0, 0.0, 0.0, 0.5)
		_GlowColorB ("Glow Color B", Color) = (1.0, 0.0, 0.0, 0.5)

		_GlowAlpha ("Glow Alpha", float) = 1.0
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

	float _MaxDistanceA;
	float _GlowSizeA;
	fixed4 _GlowColorA;

	float _MaxDistanceB;
	float _GlowSizeB;
	fixed4 _GlowColorB;

	float _GlowAlpha;

	float4 _MainTex_ST;

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.color = v.color;
        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
		float2 muv = i.uv;
		float distance = tex2D(_MainTex, muv).a;

		float aa = 0.0;
		float ab = 0.0;


		aa = smoothstep(_MaxDistanceA - _GlowSizeA * 0.5, _MaxDistanceA, distance);
		aa *= 1.0 - smoothstep(_MaxDistanceA, _MaxDistanceA + _GlowSizeA * 0.5, distance);

		ab = smoothstep(_MaxDistanceB - _GlowSizeB * 0.5, _MaxDistanceB, distance);
		ab *= 1.0 - smoothstep(_MaxDistanceB, _MaxDistanceB + _GlowSizeB * 0.5, distance);

		float4 c = lerp(_GlowColorA, _GlowColorB, (aa + ab) * 0.5);

        return fixed4(c.rgb, max(aa, ab) * max(_GlowColorA.a, _GlowColorB.a) * _GlowAlpha);
    }
    ENDCG

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
		ZTest [unity_GUIZTestMode]
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
