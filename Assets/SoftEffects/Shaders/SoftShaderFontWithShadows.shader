Shader "SoftEffects/SoftShaderFontWithShadows"
{
	Properties
	{
		_MainTex("Font Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
		[Toggle]_UseFace("Use Face", Int) = 0

		_ShadowTex("Shadow Texture", 2D) = "black" {}
		_ShadowColor("ShadowTint", Color) = (1,1,1,1)
		_OffsetX("Shadow offset X", Float) = 0
		_OffsetY("Shadow offset Y", Float) = 0
	}

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
		BlendOp Add // my test
		Blend SrcAlpha OneMinusSrcAlpha// src.rgb * src.a + dst.rgb * (1 - src.a)
		ColorMask[_ColorMask]

		Pass
		{
			Name  "Shadow Pass"
		CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteFrag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex   : POSITION;
				fixed4  color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4  color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float _OffsetX;
			float _OffsetY;
			fixed4 _Color;


			v2f SpriteVert(appdata_t IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.vertex = UnityObjectToClipPos(IN.vertex + float4(_OffsetX,_OffsetY, 0,0));
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color *_Color;
				return OUT;
			}

			sampler2D _ShadowTex;
			sampler2D _AlphaTex;
			fixed4 _ShadowColor;

		float softlight_f(float b, float t)
		{
			return (t < 0.5f) ? 2.0f*b*t + b*b*(1.0f - 2.0f*t) : 2.0f*b*(1.0f - t) + sqrt(b)*(2.0f*t - 1.0f);
		}

		float overlay_f(float bot, float top)
		{
			return (bot < 0.5f) ? 2.0f * bot * top : 1.0f - 2.0f*(1.0f - bot)*(1.0f - top);
		}

		fixed4 SampleSpriteTexture(float2 uv)
		{
			fixed4 color = tex2D(_ShadowTex,uv);

#if ETC1_EXTERNAL_ALPHA
		fixed4 alpha = tex2D(_AlphaTex, uv);
		color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif
		return color;
		}

		fixed4 SpriteFrag(v2f IN) : SV_Target
		{
			fixed4 c = fixed4(0,0,0,0);
			c = SampleSpriteTexture(IN.texcoord) * IN.color;

			float a = c.a;
			c = _ShadowColor;
			c.a *= a;
			return c;
		}
		ENDCG
	} // end shadow pass

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color *_Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;//half4 ct = color;color = color.a;color = half4(1,1,1, color.a);color.rgb = ct.rgb;
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
