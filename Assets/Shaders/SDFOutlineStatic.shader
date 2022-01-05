Shader "HO/SDFOutlineStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MaxDistance ("Max Distance", float) = 10.0
		_GlowSize ("Glow Size", float) = 0.1
		_GlowColor ("Glow Color", Color) = (1.0, 0.0, 0.0, 0.5)
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
	float _MaxDistance;
	float _GlowSize;
	float _GlowAlpha;
	float4 _MainTex_ST;
	fixed4 _GlowColor;

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
		float alpha = 0;
		alpha = smoothstep(_MaxDistance - _GlowSize * 0.5, _MaxDistance, distance);
		alpha *= 1.0 - smoothstep(_MaxDistance, _MaxDistance + _GlowSize * 0.5, distance);
        return fixed4(_GlowColor.rgb, alpha * _GlowColor.a * _GlowAlpha);
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
