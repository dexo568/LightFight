#if false

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
		// actually, do nothing
	}

	// uses CPU to accumulate layers of noise
	public static void ExecuteCPU(CNoiseLayer layer, Color[] heights, int w, int h, CBoundingVolume volume, bool flip, float planetRadius)
	{
		CGlobal global = CGlobal.GetInstance();
		// actually, do nothing.
	}
}

#endif
