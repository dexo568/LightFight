//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenNoiseTurbulence"
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
	}

	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

				#pragma target 3.0
				#pragma exclude_renderers opengl
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

				#define ONE 0.00390625
				#define ONEHALF 0.001953125
				#define F3 0.333333333333
				#define G3 0.166666666667


				float3 fade(float3 t)
				{
					return t * t * t * (t * (t * 6 - 15) + 10);
				}

				// ------ test version

				int testperm(int d)
				{
					d = d % 255;
					float2 t = float2(d%15,d/15)/15.0;
					return tex2D(_permTexture,t).r * 255;
				}

				float grad(int hash, float x, float y, float z)
				{
					int h	= hash % 15;										// & 15;
					float u = h<8 ? x : y;
					float v = h<4 ? y : (h==12||h==14 ? x : z);
					return ((h%1) == 0 ? u : -u) + (((h/2)%2) == 0 ? v : -v); 	// h&1, h&2 
				}

				float testnoise(float3 p)
				{	
					int X = (int)floor(p.x) % 255;
					int Y = (int)floor(p.y) % 255;
					int Z = (int)floor(p.z) % 255;

					p.x -= floor(p.x);
					p.y -= floor(p.y);
					p.z -= floor(p.z);

					float3 uvw = fade(float3(p.x, p.y, p.z));

					int A	= testperm(X  	)+Y;
					int AA	= testperm(A	)+Z;
					int AB	= testperm(A+1	)+Z; 
					int B	= testperm(X+1	)+Y;
					int BA	= testperm(B	)+Z;
					int BB	= testperm(B+1	)+Z;

					return
						lerp(

							lerp(
								lerp( grad(testperm(AA  ), p.x  , p.y  , p.z   ), grad(testperm(BA  ), p.x-1, p.y  , p.z   ), uvw.x),
								lerp( grad(testperm(AB  ), p.x  , p.y-1, p.z   ), grad(testperm(BB  ), p.x-1, p.y-1, p.z   ), uvw.x),
							uvw.y),

							lerp(
								lerp( grad(testperm(AA+1), p.x  , p.y  , p.z-1 ), grad(testperm(BA+1), p.x-1, p.y  , p.z-1 ), uvw.x),
								lerp( grad(testperm(AB+1), p.x  , p.y-1, p.z-1 ), grad(testperm(BB+1), p.x-1, p.y-1, p.z-1 ), uvw.x),
							uvw.y),

						uvw.z);
				}

				// ------ old noise version

				float snoise(float3 P)
				{
					float s = (P.x + P.y + P.z) * F3;
					float3 Pi = floor(P + s);
					float t = (Pi.x + Pi.y + Pi.z) * G3;
					float3 P0 = Pi - t;
					Pi = Pi * ONE + ONEHALF;

					float3 Pf0 = P - P0;

					float c1 = (Pf0.x > Pf0.y) ? 0.5078125 : 0.0078125; // 1/2 + 1/128
					float c2 = (Pf0.x > Pf0.z) ? 0.25 : 0.0;
					float c3 = (Pf0.y > Pf0.z) ? 0.125 : 0.0;
					float sindex = c1 + c2 + c3;
					float3 offsets = tex2D(_simplexTexture, float2(sindex, 0.0)).rgb;
					float3 o1 = step(0.375, offsets);
					float3 o2 = step(0.125, offsets);

					float perm0 = tex2D(_permTexture, Pi.xy).a;
					float3  grad0 = tex2D(_permTexture, float2(perm0, Pi.z)).rgb * 4.0 - 1.0;
					float t0 = 0.6 - dot(Pf0, Pf0);
					float n0;
					if (t0 < 0.0) n0 = 0.0;
					else
					{
						t0 *= t0;
						n0 = t0 * t0 * dot(grad0, Pf0);
					}

					// Noise contribution from second corner
					float3 Pf1 = Pf0 - o1 + G3;
					float perm1 = tex2D(_permTexture, Pi.xy + o1.xy*ONE).a;
					float3  grad1 = tex2D(_permTexture, float2(perm1, Pi.z + o1.z*ONE)).rgb * 4.0 - 1.0;
					float t1 = 0.6 - dot(Pf1, Pf1);
					float n1;
					if (t1 < 0.0) n1 = 0.0;
					else
					{
						t1 *= t1;
						n1 = t1 * t1 * dot(grad1, Pf1);
					}

					float3 Pf2 = Pf0 - o2 + 2.0 * G3;
					float perm2 = tex2D(_permTexture, Pi.xy + o2.xy*ONE).a;
					float3  grad2 = tex2D(_permTexture, float2(perm2, Pi.z + o2.z*ONE)).rgb * 4.0 - 1.0;
					float t2 = 0.6 - dot(Pf2, Pf2);
					float n2;
					if (t2 < 0.0) n2 = 0.0;
					else
					{
						t2 *= t2;
						n2 = t2 * t2 * dot(grad2, Pf2);
					}

					float3 Pf3 = Pf0 - float3(1.0-3.0*G3);
					float perm3 = tex2D(_permTexture, Pi.xy + float2(ONE, ONE)).a;
					float3  grad3 = tex2D(_permTexture, float2(perm3, Pi.z + ONE)).rgb * 4.0 - 1.0;
					float t3 = 0.6 - dot(Pf3, Pf3);
					float n3;
					if(t3 < 0.0) n3 = 0.0;
					else
					{
						t3 *= t3;
						n3 = t3 * t3 * dot(grad3, Pf3);
					}

					return 32.0 * (n0 + n1 + n2 + n3);
				}

				// ------ new noise version

				float4 perm2d(float2 p)
				{
					return tex2D(_permTexture, p);
				}

				float gradperm(float x, float3 p)
				{
					return dot(tex2D(_simplexTexture, float2(x, 0)).rgb, p);
				}

				float optnoise(float3 p)
				{
					float3 P = fmod(floor(p), 256.0);	// FIND UNIT CUBE THAT CONTAINS POINT
  					p -= floor(p);                      // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
					float3 f = fade(p);                 // COMPUTE FADE CURVES FOR EACH OF X,Y,Z.

					P = P / 256.0;
					const float one = 1.0 / 256.0;

					float4 AA = perm2d(P.xy) + P.z;

  					return lerp( lerp( lerp( gradperm(AA.x, p ),  
											 gradperm(AA.z, p + float3(-1, 0, 0) ), f.x),
									   lerp( gradperm(AA.y, p + float3(0, -1, 0) ),
											 gradperm(AA.w, p + float3(-1, -1, 0) ), f.x), f.y),
                             
								 lerp( lerp( gradperm(AA.x+one, p + float3(0, 0, -1) ),
											 gradperm(AA.z+one, p + float3(-1, 0, -1) ), f.x),
									   lerp( gradperm(AA.y+one, p + float3(0, -1, -1) ),
											 gradperm(AA.w+one, p + float3(-1, -1, -1) ), f.x), f.y), f.z);
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
					// generate turbulence noise
					half h = snoise(normalize(i.vol) * _planetRadius * _frequency + _noiseOffset);		// spherical noise sampling
//					half h = snoise(i.vol * _frequency + _noiseOffset);									// flat noise sampling
					half a = (h + _offset);

					// do the accumulation with the previous fixed-point height
					a = (a*_contribution) * _amp;

					// return the heightmap
					// r = normalmap dx (not filled yet -- will be filled by the normal generator)
					// g = normalmap dy (not filled yet -- will be filled by the normal generator)
					// b = slope (not filled yet -- will be filled by the normal generator)
					// a = height
					return half4(0, 0, 0, a);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
