//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "PackNoise"
{
	Properties
	{
		_hmap("hmap", 2D) = "black" {}
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

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float3 vol : TEXCOORD1;
					float2 uv2 : TEXCOORD2;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1, float2 uv2 : TEXCOORD2)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = uv;
					OUT.vol = float3(vol.x, vol.y, vol.z);
					OUT.uv2 = uv2;
					return OUT;
				}

				//
				// fragment
				//

				half4 frag(vtx_out i) : COLOR
				{
					float4 texel = tex2D(_hmap, i.uv);

					// as unity3d until now does not support to read-back of floating-point textures from GPU to CPU,
					// we will pack the floating-point height into separate byte RGB components, trying
					// to keep some precision for the read-back.

					int hfix = (int)(clamp(texel.a + 1.0, 0.0, 2.0) * 8355840);

					int r = hfix / 65536;
					int g = (hfix - r * 65536) / 256;
					int b = hfix - r * 65536 - g * 256;

					// return the packed heightmap
					// rgb = height
					// b = slope
					return float4((float)r / 255.0, (float)g / 255.0, (float)b / 255.0, texel.b);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
