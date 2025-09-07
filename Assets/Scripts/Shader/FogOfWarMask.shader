Shader "Unlit/FogOfWarMask"
{
	Properties
	{
		_Color ("Color", Color) = (0,0,0,0.8)
		_CircleCount ("Circle Count", int) = 0
		_CircleSecondRadius ("Circle Second Radius", float) = 0.1
		_CircleScale ("Circle Scale", float) = 1
		[Toggle] _FadeRadius ("Should Fade Radius", float) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Overlay+1" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			fixed4 _Color;
			int _CircleCount;
			float4 _CirclePositions[50];
			float _CircleRadii[50];
			float _CircleSecondRadius;
			float _CircleScale;
			bool _FadeRadius;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;

				// aspect ratio
				uv.x *= _ScreenParams.x;
				uv.y *= _ScreenParams.y;

				float alpha = _Color.a;

				for (int j = 0; j < _CircleCount; ++j)
				{
					float2 circleUV = _CirclePositions[j].xy;
					circleUV.x *= _ScreenParams.x;
					circleUV.y *= _ScreenParams.y;

					float dist = distance(uv, circleUV) / _CircleScale;
					float scale = _ScreenParams.x / 2;

					if (_FadeRadius)
						alpha = max(0, alpha - lerp(_Color.a, 0.0, smoothstep(_CircleRadii[j] * scale, (_CircleRadii[j] + _CircleSecondRadius) * scale, dist)));
					else
					{
						float cutoff1 = step(_CircleRadii[j] * scale, dist);
						float cutoff2 = step((_CircleRadii[j] + _CircleSecondRadius) * scale, dist);
						float finalAlpha = lerp(1.0, lerp(0.5, 0.0, cutoff2), cutoff1);
						alpha = max(0, alpha - _Color.a * finalAlpha);
					}
					if (alpha <= 0) break;
				}

				return fixed4(_Color.rgb, alpha);
			}
			ENDCG
		}
	}
}