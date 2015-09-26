//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "ShapeNoise"
{
	Properties
	{
		_hmap("hmap", 2D) = "black" {}
		_shape("shape", 2D) = "white" {}
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

				uniform sampler2D _hmap;
				uniform sampler2D _shape;

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float3 vol : TEXCOORD1;
					float2 uvvol : TEXCOORD2;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1, float2 uvvol : TEXCOORD2)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = uv;
					OUT.vol = float3(vol.x, vol.y, vol.z);
					OUT.uvvol = uvvol;
					return OUT;
				}

				//
				// fragment
				//

				half4 frag(vtx_out i) : COLOR
				{
					float4 texel = tex2D(_hmap, i.uv);
					float4 shape = tex2D(_shape, i.uvvol);

					return texel * shape.a;
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
