//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;


public enum en_PatchQuality { Minimum, Low, Standard, High, Maximum };
public enum en_NormalQuality { Minimum, Low, Standard, High, Maximum };
public enum en_NeighborDirection { NB_Top = 0, NB_Right = 1, NB_Bottom = 2, NB_Left = 3 };


public class CPatchConfig
{

	ushort m_PatchSize;
	ushort m_MaxHeightMapSize;
	ushort m_LowResHeightMapSize;

	ushort m_AtmosSlices;
	ushort m_AtmosStacks;

	ushort[] m_LevelHeightMapRes;

	ushort m_GridSize;

	// low resolution heightmap -- used to scan and displace vertices on CPU side
	Texture2D m_LowResHeightmapTex = null;
	RenderTexture m_LowResHeightmapRT = null;

	// --

	public ushort PatchSize { get { return m_PatchSize; } }
	public ushort GridSize { get { return m_GridSize; } }
	public ushort MaxHeightMapSize { get { return m_MaxHeightMapSize; } }
	public ushort LowResHeightMapSize { get { return m_LowResHeightMapSize; } }
	public ushort AtmosSlices { get { return m_AtmosSlices; } }
	public ushort AtmosStacks { get { return m_AtmosStacks; } }
	public ushort LevelHeightMapRes(ushort level) { return m_LevelHeightMapRes[(level >= MaxSplitLevel ? MaxSplitLevel : level)]; }
	public ushort MaxSplitLevel { get { return (ushort)(m_LevelHeightMapRes.Length - 1); } }
	public Texture2D LowResHeightmapTex { get { return m_LowResHeightmapTex; } }
	public RenderTexture LowResHeightmapRT { get { return m_LowResHeightmapRT; } }


	public CPatchConfig(en_PatchQuality patchQuality, en_NormalQuality normalQuality, bool filteredHeights)
	{
		switch (patchQuality)
		{
			case en_PatchQuality.Maximum:
			{
				m_PatchSize = 33;
				m_AtmosSlices = 128;
				m_AtmosStacks = 96;
				break;
			}

            case en_PatchQuality.High:
			{
				m_PatchSize = 21;
				m_AtmosSlices = 128;
				m_AtmosStacks = 96;
				break;
			}

			case en_PatchQuality.Standard:
			{
				m_PatchSize = 17;
				m_AtmosSlices = 128;
				m_AtmosStacks = 96;
				break;
			}

			case en_PatchQuality.Low:
			{
				m_PatchSize = 11;
				m_AtmosSlices = 96;
				m_AtmosStacks = 72;
				break;
			}

			case en_PatchQuality.Minimum:
			{
				m_PatchSize = 7;
				m_AtmosSlices = 96;
				m_AtmosStacks = 72;
				break;
			}
		}

		switch (normalQuality)
		{
			case en_NormalQuality.Maximum:
			{
				m_MaxHeightMapSize = 512;
				// heightmap resolution at each level, without lod
				m_LevelHeightMapRes = new ushort[]
				{
					512, 512, 256, 256, 256, 128
				};
				break;
			}

			case en_NormalQuality.High:
			{
				m_MaxHeightMapSize = 256;
				// heightmap resolution at each level, without lod
				m_LevelHeightMapRes = new ushort[]
				{
					256, 256, 256, 128
				};
				break;
			}

			case en_NormalQuality.Standard:
			{
				m_MaxHeightMapSize = 256;
				// heightmap resolution at each level, without lod
				m_LevelHeightMapRes = new ushort[]
				{
					256, 256, 128, 128, 128, 64
				};
				break;
			}

			case en_NormalQuality.Low:
			{
				m_MaxHeightMapSize = 128;
				// heightmap resolution at each level, without lod
				m_LevelHeightMapRes = new ushort[]
				{
					128, 128, 128, 64
				};
				break;
			}

			case en_NormalQuality.Minimum:
			{
				m_MaxHeightMapSize = 64;
				// heightmap resolution at each level, without lod
				m_LevelHeightMapRes = new ushort[]
				{
					64
				};
				break;
			}
		}

		m_LowResHeightMapSize = m_PatchSize;
		m_GridSize = (ushort)(m_PatchSize * m_PatchSize);

		// if the low-res heightmap texture is already created, destroy it
		// because the resolution may have been changed
		if (m_LowResHeightmapTex != null)
		{
			#if UNITY_EDITOR
				Texture2D.DestroyImmediate(m_LowResHeightmapTex);
				RenderTexture.DestroyImmediate(m_LowResHeightmapRT);
			#else
				Texture2D.Destroy(m_LowResHeightmapTex);
				RenderTexture.Destroy(m_LowResHeightmapRT);
			#endif
		}

		// recreate the low-res heightmap texture (the one downloaded from the gpu)
		m_LowResHeightmapTex = new Texture2D(m_LowResHeightMapSize, m_LowResHeightMapSize, TextureFormat.ARGB32, false);
		m_LowResHeightmapTex.anisoLevel = 0;
		m_LowResHeightmapTex.filterMode = (filteredHeights ? FilterMode.Bilinear : FilterMode.Point);
		m_LowResHeightmapTex.wrapMode = CGlobal.WRAP_MODE;
		m_LowResHeightmapTex.Apply();

		m_LowResHeightmapRT = new RenderTexture(m_LowResHeightMapSize, m_LowResHeightMapSize, 0, RenderTextureFormat.ARGB32);
		m_LowResHeightmapRT.anisoLevel = 0;
		m_LowResHeightmapRT.isPowerOfTwo = false;
		m_LowResHeightmapRT.wrapMode = CGlobal.WRAP_MODE;
		m_LowResHeightmapRT.filterMode = (filteredHeights ? FilterMode.Bilinear : FilterMode.Point);
		m_LowResHeightmapRT.useMipMap = false;
		m_LowResHeightmapRT.Create();
	}


	~CPatchConfig()
	{
		//Destroy();
	}


	public void Destroy()
	{
#if UNITY_EDITOR
		if (m_LowResHeightmapTex != null)
		{
			Texture2D.DestroyImmediate(m_LowResHeightmapTex);
			RenderTexture.DestroyImmediate(m_LowResHeightmapRT);
		}
#else
		if (m_LowResHeightmapTex != null)
		{
			Texture2D.Destroy(m_LowResHeightmapTex);
			RenderTexture.Destroy(m_LowResHeightmapRT);
		}
#endif

		m_LowResHeightmapTex = null;
		m_LowResHeightmapRT = null;
	}
}
