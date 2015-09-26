#if true

//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;


public enum NoiseLayerType { Cell, Turbulence, Ridged };


public static class CNoiseFactory
{
	public static void ShapeNoise(RenderTexture noiseRT, Texture2D shapeTex, CBoundingVolume volume)
	{
		CGlobal global = CGlobal.GetInstance();

		RenderTexture.active = noiseRT;

		global.ShapeNoiseMaterial.SetTexture("_hmap", noiseRT);
		global.ShapeNoiseMaterial.SetTexture("_shape", shapeTex);

		global.RenderQuadVolume(noiseRT.width, noiseRT.height, global.ShapeNoiseMaterial, volume, false);

		RenderTexture.active = null;
	}

	// uses GPU to accumulate layers of noise
	public static void Execute(CNoiseLayer layer, RenderTexture noiseRT, CBoundingVolume volume, bool flip, float planetRadius)
	{
		CGlobal global = CGlobal.GetInstance();

		int matidx = 0;
		float amp = 1.0f;
		float frequency = layer.m_Frequency;

		switch (layer.m_LayerType)
		{
			case NoiseLayerType.Cell:
				{
					Debug.Log("[ETHEREA1] NoiseLayerType.Cell is not implemented yet! Please select Ridged or Turbulence instead.");
					break;
				}

			case NoiseLayerType.Ridged:
				{
					matidx = 0;
					break;
				}

			case NoiseLayerType.Turbulence:
				{
					matidx = 1;
					break;
				}
		}

		for (int i = 0; i < layer.m_Octaves; i++)
		{
			RenderTexture.active = noiseRT;

			global.GenNoiseMaterial[matidx].SetFloat("_planetRadius", planetRadius);
			global.GenNoiseMaterial[matidx].SetVector("_noiseOffset", layer.m_NoiseOffset);
			global.GenNoiseMaterial[matidx].SetFloat("_offset", layer.m_Offset);
			global.GenNoiseMaterial[matidx].SetFloat("_hpower", layer.m_HPower);
			global.GenNoiseMaterial[matidx].SetTexture("_permTexture", global.PermTex);
			global.GenNoiseMaterial[matidx].SetTexture("_simplexTexture", global.SimplexTex);
			global.GenNoiseMaterial[matidx].SetFloat("_contribution", layer.m_Contribution);

			global.GenNoiseMaterial[matidx].SetTexture("_previousPass", noiseRT);
			global.GenNoiseMaterial[matidx].SetFloat("_amp", amp);
			global.GenNoiseMaterial[matidx].SetFloat("_frequency", frequency);

			// ### render with extended volume coordinates but with normal uv volume coordinates
			//global.RenderQuadVolumeExCoord(noiseRT.width, noiseRT.height, global.GenNoiseMaterial[matidx], volume, flip);
			global.RenderQuadVolume(noiseRT.width, noiseRT.height, global.GenNoiseMaterial[matidx], volume, flip);

			frequency *= layer.m_Lacunarity;
			amp *= layer.m_Persistence;

			RenderTexture.active = null;
		}
	}
}

#endif
