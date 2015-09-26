//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenNormal"
{
	Properties
	{
		_uvStep("uvStep", Float) = 1.0
		_step("step", Float) = 1.0
		_hmap("hmap", 2D) = "black" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

				#pragma target 3.0
				//#pragma exclude_renderers opengl
				//#pragma exclude_renderers d3d9
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform float _uvStep;
				uniform float _step;
				uniform sampler2D _hmap;

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float3 vol : TEXCOORD1;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = uv;
					OUT.vol = float3(vol.x, vol.y, vol.z);
					return OUT;
				}

				//
				// fragment
				//

				float2 sobel(float2 uv)
				{
					float u = uv.s;
					float v = uv.t;

					float tl = tex2D(_hmap, float2(u - _uvStep, v - _uvStep)).a;
					float l = tex2D(_hmap, float2(u - _uvStep, v)).a;
					float bl = tex2D(_hmap, float2(u - _uvStep, v + _uvStep)).a;
					float b = tex2D(_hmap, float2(u, v + _uvStep)).a;
					float br = tex2D(_hmap, float2(u + _uvStep, v + _uvStep)).a;
					float r = tex2D(_hmap, float2(u + _uvStep, v)).a;
					float tr = tex2D(_hmap, float2(u + _uvStep, v - _uvStep)).a;
					float t = tex2D(_hmap, float2(u, v - _uvStep)).a;

					float dX = tr + 2.0 * r + br - tl - 2.0 * l - bl;
					float dY = bl + 2.0 * b + br - tl - 2.0 * t - tr;

					return float2(dX, dY);
				}

				half4 frag(vtx_out i) : COLOR
				{
					float4 prev = tex2D(_hmap, i.uv);

					float2 d = sobel(i.uv);
					float3 n = normalize(float3(-d.x, -d.y, _step));
					float slope = max(abs(n.x), abs(n.y));

					// return the packed heightmap with normals
					// r = normalmap dx
					// g = normalmap dy
					// b = slope
					// a = height
					return half4(n.x, n.y, slope, prev.a);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
