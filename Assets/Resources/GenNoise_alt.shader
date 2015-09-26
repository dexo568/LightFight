//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenNoise_alt"
{
	Properties
	{
		_planetRadius("planetRadius", Float) = 16.0
		_noiseOffset("noiseOffset", Vector) = (0.0, 0.0, 0.0)
		_frequency("frequency", Range(0.0, 1.0)) = 0.0
		_offset("offset", Range(0.0, 1.0)) = 1.0
		_hpower("hpower", Range(1.0, 8.0)) = 2.0
		_amp("amp", Float) = 0.5
		_contribution("contribution", Float) = 1.0

		_permTexture("permTexture", 2D) = "white" {}
		_simplexTexture("simplexTexture", 2D) = "white" {}
		_previousPass("previous", 2D) = "black" {}
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

				uniform float _planetRadius;
				uniform float3 _noiseOffset;
				uniform float _frequency;
				uniform float _offset;
				uniform float _hpower;
				uniform float _amp;
				uniform float _contribution;
				uniform sampler2D _permTexture;
				uniform sampler2D _simplexTexture;
				uniform sampler2D _previousPass;

				//
				// Description : Array and textureless GLSL 2D/3D/4D simplex 
				//               noise functions.
				//      Author : Ian McEwan, Ashima Arts.
				//  Maintainer : ijm
				//     Lastmod : 20110822 (ijm)
				//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
				//               Distributed under the MIT License. See LICENSE file.
				//               https://github.com/ashima/webgl-noise
				// 

				float3 mod289(float3 x)
				{
					return x - floor(x * (1.0 / 289.0)) * 289.0;
				}

				float4 mod289(float4 x)
				{
					return x - floor(x * (1.0 / 289.0)) * 289.0;
				}

				float4 permute(float4 x)
				{
					return mod289(((x*34.0)+1.0)*x);
				}

				float4 taylorInvSqrt(float4 r)
				{
					return 1.79284291400159 - 0.85373472095314 * r;
				}

				float snoise(float3 v)
				{
					const float2  C = float2(1.0/6.0, 1.0/3.0) ;
					const float4  D = float4(0.0, 0.5, 1.0, 2.0);

					// First corner
					float3 i  = floor(v + dot(v, C.yyy) );
					float3 x0 =   v - i + dot(i, C.xxx) ;

					// Other corners
					float3 g = step(x0.yzx, x0.xyz);
					float3 l = 1.0 - g;
					float3 i1 = min( g.xyz, l.zxy );
					float3 i2 = max( g.xyz, l.zxy );

					//   x0 = x0 - 0.0 + 0.0 * C.xxx;
					//   x1 = x0 - i1  + 1.0 * C.xxx;
					//   x2 = x0 - i2  + 2.0 * C.xxx;
					//   x3 = x0 - 1.0 + 3.0 * C.xxx;
					float3 x1 = x0 - i1 + C.xxx;
					float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
					float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

					// Permutations
					i = mod289(i); 
					float4 p = permute( permute( permute( 
					i.z + float4(0.0, i1.z, i2.z, 1.0 ))
					+ i.y + float4(0.0, i1.y, i2.y, 1.0 )) 
					+ i.x + float4(0.0, i1.x, i2.x, 1.0 ));

					// Gradients: 7x7 points over a square, mapped onto an octahedron.
					// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
					float n_ = 0.142857142857; // 1.0/7.0
					float3  ns = n_ * D.wyz - D.xzx;

					float4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

					float4 x_ = floor(j * ns.z);
					float4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

					float4 x = x_ *ns.x + ns.yyyy;
					float4 y = y_ *ns.x + ns.yyyy;
					float4 h = 1.0 - abs(x) - abs(y);

					float4 b0 = float4( x.xy, y.xy );
					float4 b1 = float4( x.zw, y.zw );

					//float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
					//float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
					float4 s0 = floor(b0)*2.0 + 1.0;
					float4 s1 = floor(b1)*2.0 + 1.0;
					float4 sh = -step(h, float4(0.0));

					float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
					float4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

					float3 p0 = float3(a0.xy,h.x);
					float3 p1 = float3(a0.zw,h.y);
					float3 p2 = float3(a1.xy,h.z);
					float3 p3 = float3(a1.zw,h.w);

					//Normalise gradients
					float4 norm = taylorInvSqrt(float4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
					p0 *= norm.x;
					p1 *= norm.y;
					p2 *= norm.z;
					p3 *= norm.w;

					// Mix final noise value
					float4 m = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
					m = m * m;
					return 42.0 * dot( m*m, float4( dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3) ) );
				}

				float ridge(float h, float _hpower, float _offset)
				{
					h = abs(h);
					h = _offset - h;
					float p = _hpower;
					h = pow(h, _hpower);
					return h;
				}

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

				half4 frag(vtx_out i) : COLOR
				{
					#if 1

						half4 prev = tex2D(_previousPass, i.uv);

						#if 1
							// generate ridged noise
							half h = snoise(normalize(i.vol) * _planetRadius * _frequency + _noiseOffset);
							half a = abs(ridge(h, _hpower, _offset));
						#else
							// generate turbulence noise
							half h = testnoise(normalize(i.vol) * _planetRadius * _frequency + _noiseOffset);
							half a = abs(h + _offset);
						#endif

						// do the accumulation with the previous fixed-point height
						a = (a*_contribution) * _amp + prev.a;

						// return the heightmap
						// r = normalmap dx (not filled yet -- will be filled by the normal generator)
						// g = normalmap dy (not filled yet -- will be filled by the normal generator)
						// b = slope (not filled yet -- will be filled by the normal generator)
						// a = height
						return half4(0, 0, 0, a);

					#else

						// debugging
						half a = testnoise(normalize(i.vol) * _planetRadius * _frequency + _noiseOffset);
						return float4(a, a, a, a);

					#endif
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
