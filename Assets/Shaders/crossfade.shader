Shader "HOPA/CrossFade"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _SecondaryTex("Sprite Texture Dst", 2D) = "white" {}
        [PerRendererData] _CrossFade("Fade", Float) = 0.5
        [PerRendererData] _SrcBounds("Source UV Bounds", Vector) = (0,0,1,1)
        [PerRendererData] _DstBounds("Dest UV Bounds", Vector) = (0,0,1,1)
        _Color("Tint", Color) = (1,1,1,1)
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    CGINCLUDE
        #include "UnityCG.cginc"
        #include "UnityUI.cginc"
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
            fixed4 color : COLOR;
        };
        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
            fixed4 color : COLOR;
        };
        sampler2D _MainTex;     
        sampler2D _SecondaryTex;
        float4 _MainTex_ST;
        float4 _SecondaryTex_ST;
        fixed4 _Color;
        float _CrossFade;
        float4 _SrcBounds;
        float4 _DstBounds;
        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);   // probably not necessary unless using bias
            o.uv2 = TRANSFORM_TEX(v.uv2, _SecondaryTex);
            o.color = v.color;
            return o;
        }
        fixed4 frag(v2f i) : SV_Target
        {
            const float minAlpha = 5.0 / 255.0; // help filter out dirty pixels, can raise lower (don't go below 1/255)
            fixed4 col = tex2D(_MainTex, i.uv);
            fixed4 colb = tex2D(_SecondaryTex, i.uv2);
            // alpha = 0 if outside of uv bounds
            col.a *= step(i.uv.x, _SrcBounds.z) * step(_SrcBounds.x, i.uv.x) * step(i.uv.y, _SrcBounds.w) * step(_SrcBounds.y, i.uv.y);
            colb.a *= step(i.uv2.x, _DstBounds.z) * step(_DstBounds.x, i.uv2.x) * step(i.uv2.y, _DstBounds.w) * step(_DstBounds.y, i.uv2.y);
            float alphaSrcFlag = step(minAlpha, col.a);
            float alphaDstFlag = step(minAlpha, colb.a);
            fixed3 outcol = fixed3(0,0,0);
            // branching is usually pretty evil, but these dno't involve sampling so it's trivial for the shader engine to precalculate all 3 branches
            if (col.a >= minAlpha && colb.a >= minAlpha)
            {
                outcol = lerp(col.rgb, colb.rgb, _CrossFade);
            }
            else if (col.a >= minAlpha)
            {
                outcol = col.rgb;
            }
            else if (colb.a >= minAlpha)
            {
                outcol = colb.rgb;
            }
            return fixed4(outcol, lerp(col.a, colb.a, _CrossFade));
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
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]
        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            ENDCG
        }
    }
}