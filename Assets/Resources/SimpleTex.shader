//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "SimpleTex"
{
	Properties
	{
		_tex("tex", 2D) = "black" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

				//#pragma target 3.0
				//#pragma exclude_renderers opengl
				//#pragma exclude_renderers d3d9
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform sampler2D _tex;

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0)
				{
					vtx_out OUT;
					OUT.position = mul(UNITY_MATRIX_MVP, position);
					OUT.uv = uv;
					return OUT;
				}

				//
				// fragment
				//

				half4 frag(vtx_out i) : COLOR
				{
					return tex2D(_tex, i.uv);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
