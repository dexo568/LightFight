//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;



[System.Serializable]
public class CNoiseLayer
{
    public NoiseLayerType m_LayerType = NoiseLayerType.Turbulence;

	public Vector3 m_NoiseOffset = new Vector3(0, 0, 0);
	public int m_Octaves = 1;
	public float m_Persistence = 0.60f;
	public float m_Frequency = 0.001f;
	public float m_Offset = 0.0f;
	public float m_Lacunarity = 1.0f;
	public int m_HPower = 2;
	public float m_Contribution = 1.0f;						// level contribution to final height (1 = 100%)

	public CNoiseLayer(NoiseLayerType layerType, Vector3 noiseOffset, int octaves, float persistence, float frequency, float offset, float lacunarity, int hpower, float contribution)
	{
		m_LayerType = layerType;
		m_NoiseOffset = noiseOffset;
		m_Octaves = octaves;
		m_Persistence = persistence;
		m_Frequency = frequency;
		m_Offset = offset;
		m_Lacunarity = lacunarity;
		m_HPower = hpower;
		m_Contribution = contribution;
	}
}


[ExecuteInEditMode]
public class CPlanet : MonoBehaviour
{
	public bool m_Sphere = true;
	public bool m_Wysiwyg = false;

	//
	// planet parameters
	//

	public Transform m_SunTransform;
	public Camera m_Camera;

	public en_PatchQuality m_PatchQuality = en_PatchQuality.Standard;
	public en_NormalQuality m_NormalQuality = en_NormalQuality.Standard;

	public float m_Radius = 64;
	public float m_Temperature = 2.0f;
	public float m_WaterLevel = 0.1f;
	public float m_HeightScale = 1.0f;
	public float m_NormalMultiplier = 16.0f;

	public int m_MaxSplitLevel = 10;
	public float m_SizeSplit = 5;
	public float m_SizeRejoin = 8;

	public bool m_EnablePhysics = false;
	public float m_GravityPower = 8.0f;
	public int m_MinPhysicsSplitLevel = 0;
	public int m_MinShadowsSplitLevel = 10;

	public bool m_FilteredHeights = true;

	// fractal layers
	public CNoiseLayer[] layers = { new CNoiseLayer(NoiseLayerType.Turbulence, Vector3.zero, 16, 0.52f, 0.1f, 0, 2, 2, 1) };

	//
	// textures
	//

	public Texture2D m_WaterTex;				// water texture
	public Texture2D m_WaterNormal;				// water normalmap texture
	public float m_WaterTexTile = 1;			// water texture tiling
	public float m_WaterSpeed = 0.1f;			// water waving speed

	public Texture2D m_SnowTex;					// snow texture
	public float m_SnowTexTile = 16;				// snow texture tiling

	public Texture2D m_BaseTex;					// base texture
	public float m_BaseTexTile = 32;				// base texture tiling

	public Texture2D m_Diffuse1Tex;				// diffuse texture 1
	public float m_Diffuse1TexTile = 32;			// diffuse texture 1 tiling
	public CLutLayer m_Diffuse1LutLayer = new CLutLayer(0.0f, 0.0f, 0.0f, 0.0f);

	public Texture2D m_Diffuse2Tex;				// diffuse texture 2
	public float m_Diffuse2TexTile = 32;			// diffuse texture 2 tiling
	public CLutLayer m_Diffuse2LutLayer = new CLutLayer(0.0f, 0.0f, 0.0f, 0.0f);

	public Texture2D m_Diffuse3Tex;				// diffuse texture 3
	public float m_Diffuse3TexTile = 32;			// diffuse texture 3 tiling
	public CLutLayer m_Diffuse3LutLayer = new CLutLayer(0.0f, 0.0f, 0.0f, 0.0f);

	public Texture2D m_Diffuse4Tex;				// diffuse texture 4
	public float m_Diffuse4TexTile = 32;			// diffuse texture 4 tiling
	public CLutLayer m_Diffuse4LutLayer = new CLutLayer(0.0f, 0.0f, 0.0f, 0.0f);

	public Texture2D m_ShapeTop = null;
	public Texture2D m_ShapeBottom = null;
	public Texture2D m_ShapeLeft = null;
	public Texture2D m_ShapeRight = null;
	public Texture2D m_ShapeFront = null;
	public Texture2D m_ShapeBack = null;

	// atmosphere
	public bool hasAtmosphere = false;
	public float m_AtmosScale = 1.0f;
	public Color m_AtmosColor1 = new Color(0.8f, 0.8f, 1.1f) * 0.4f;
	public Color m_AtmosColor2 = new Color(0.3f, 0.3f, 1.3f) * 0.3f;
	public float m_sunIntensity = 2.0f;
	public float m_MinAtmosAlpha = 0.8f;
	public float m_HorizonHeight = 1.0f;
	public float m_HorizonIntensity = 32.0f;
	public float m_HorizonPower = 5;
	public float m_FogIntensity = 0.0f;
	public float m_FogMaxAlpha = 0.0f;
	public float m_FogHeight = 0.5f;
	public float m_FogNear = 5.0f;
	public float m_FogFar = 50.0f;

	float m_AtmosRadius;

	Vector3 m_InvSunPos;
	Vector3 m_SunDir;

	// ---

	List<CQuadtree> m_Quadtrees = new List<CQuadtree>();
	float m_MinCamDist = 0;
	CQuadtree m_CloserNode = null;

	// the highest split-level of the associated quadtrees
	ushort m_HighestSplitLevel;

	// flags if the quadtree has already splitted a node in current frame
	// (allows to block multiple splits at same frame)
	bool m_Splitted;
	bool m_Rejoined;

	CPatchManager m_PatchManager;
	CPatchConfig m_PatchConfig;
	CLutGenerator m_LutGen;
	CLutLayer[] m_LutLayers = new CLutLayer[4];

	CLodSphere[] m_LodSpheres = { null, null };

	//
	// properties
	//

	public ushort HighestSplitLevel
	{
		get { return m_HighestSplitLevel; }
		set { m_HighestSplitLevel = value; }
	}

	public bool Splitted
	{
		get { return m_Splitted; }
		set { m_Splitted = value; }
	}

	public bool Rejoined
	{
		get { return m_Rejoined; }
		set { m_Rejoined = value; }
	}

	public float MinCamDist
	{
		get { return m_MinCamDist; }
		set { m_MinCamDist = value; }
	}

	public CQuadtree CloserNode
	{
		get { return m_CloserNode; }
		set { m_CloserNode = value; }
	}

	public CPatchManager PatchManager { get { return m_PatchManager; } }
	public CPatchConfig PatchConfig { get { return m_PatchConfig; } }
	public CLutGenerator LutGen { get { return m_LutGen; } }

	public float AtmosRadius { get { return m_AtmosRadius; } }

	public Vector3 InvSunPos { get { return m_InvSunPos; } }
	public Vector3 SunDir { get { return m_SunDir; } }

	//
	// methods
	//


	void Awake()
	{
		DestroyPlanet();
		Rebuild();
	}


	void DestroyPlanet()
	{
		// destroy patches
		for (int i = 0; i < m_Quadtrees.Count; i++)
		{
			m_Quadtrees[i].DestroyTree();
		}
		m_Quadtrees.Clear();

		// destroy any child gameobject
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			#if UNITY_EDITOR
				GameObject.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
			#else
				GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
			#endif
		}

		if (m_LodSpheres[0] != null) m_LodSpheres[0].Destroy();
		if (m_LodSpheres[1] != null) m_LodSpheres[1].Destroy();

		// force garbage collection
		System.GC.Collect();
	}


	void OnApplicationQuit()
	{
		//DestroyPlanet();
	}


	~CPlanet()
	{
		//DestroyPlanet();
	}


	public void Rebuild()
	{
		CGlobal global = CGlobal.GetInstance();

		m_LutGen = new CLutGenerator();
		m_LutLayers[0] = new CLutLayer(m_Diffuse1LutLayer.minh, m_Diffuse1LutLayer.maxh, m_Diffuse1LutLayer.slope, m_Diffuse1LutLayer.aperture);
		m_LutLayers[1] = new CLutLayer(m_Diffuse2LutLayer.minh, m_Diffuse2LutLayer.maxh, m_Diffuse2LutLayer.slope, m_Diffuse2LutLayer.aperture);
		m_LutLayers[2] = new CLutLayer(m_Diffuse3LutLayer.minh, m_Diffuse3LutLayer.maxh, m_Diffuse3LutLayer.slope, m_Diffuse3LutLayer.aperture);
		m_LutLayers[3] = new CLutLayer(m_Diffuse4LutLayer.minh, m_Diffuse4LutLayer.maxh, m_Diffuse4LutLayer.slope, m_Diffuse4LutLayer.aperture);
		m_LutGen.UpdateLutTex(m_LutLayers);

		m_PatchConfig = new CPatchConfig(m_PatchQuality, m_NormalQuality, m_FilteredHeights);
		m_PatchManager = new CPatchManager(m_PatchConfig, this);
		global.Setup();

		// ---

		// destroy the previous planet tree
		DestroyPlanet();

		//
		// rebuild patch tree
		//

		m_Quadtrees.Add(new CQuadtree(new Vector3(0, 1, 0), new Vector3(0, 0, -1), this, m_ShapeTop));		// 0: top
		m_Quadtrees.Add(new CQuadtree(new Vector3(0, -1, 0), new Vector3(0, 0, 1), this, m_ShapeBottom));		// 1: bottom

		m_Quadtrees.Add(new CQuadtree(new Vector3(-1, 0, 0), new Vector3(0, 1, 0), this, m_ShapeLeft));		// 2: left
		m_Quadtrees.Add(new CQuadtree(new Vector3(1, 0, 0), new Vector3(0, 1, 0), this, m_ShapeRight));		// 3: right

		m_Quadtrees.Add(new CQuadtree(new Vector3(0, 0, 1), new Vector3(0, 1, 0), this, m_ShapeFront));		// 4: front
		m_Quadtrees.Add(new CQuadtree(new Vector3(0, 0, -1), new Vector3(0, 1, 0), this, m_ShapeBack));		// 5: back

		// link neighbors of TOP quadtree
		m_Quadtrees[0].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[5], en_NeighborDirection.NB_Top); // back
		m_Quadtrees[0].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[4], en_NeighborDirection.NB_Top); // front
		m_Quadtrees[0].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[2], en_NeighborDirection.NB_Top); // left
		m_Quadtrees[0].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[3], en_NeighborDirection.NB_Top); // right

		// link neighbors of BOTTOM quadtree
		m_Quadtrees[1].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[4], en_NeighborDirection.NB_Bottom); // front
		m_Quadtrees[1].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[5], en_NeighborDirection.NB_Bottom); // back
		m_Quadtrees[1].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[2], en_NeighborDirection.NB_Bottom); // left
		m_Quadtrees[1].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[3], en_NeighborDirection.NB_Bottom); // right

		// link neighbors of LEFT quadtree
		m_Quadtrees[2].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[0], en_NeighborDirection.NB_Left); // top
		m_Quadtrees[2].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[1], en_NeighborDirection.NB_Left); // bottom
		m_Quadtrees[2].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[5], en_NeighborDirection.NB_Right); // back
		m_Quadtrees[2].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[4], en_NeighborDirection.NB_Left); // front

		// link neighbors of RIGHT quadtree
		m_Quadtrees[3].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[0], en_NeighborDirection.NB_Right); // top
		m_Quadtrees[3].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[1], en_NeighborDirection.NB_Right); // bottom
		m_Quadtrees[3].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[4], en_NeighborDirection.NB_Right); // front
		m_Quadtrees[3].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[5], en_NeighborDirection.NB_Left); // back

		// link neighbors of FRONT quadtree
		m_Quadtrees[4].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[0], en_NeighborDirection.NB_Bottom); // top
		m_Quadtrees[4].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[1], en_NeighborDirection.NB_Top); // bottom
		m_Quadtrees[4].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[2], en_NeighborDirection.NB_Right); // left
		m_Quadtrees[4].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[3], en_NeighborDirection.NB_Left); // right

		// link neighbors of BACK quadtree
		m_Quadtrees[5].SetNeighbor(en_NeighborDirection.NB_Top, m_Quadtrees[0], en_NeighborDirection.NB_Top); // top
		m_Quadtrees[5].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Quadtrees[1], en_NeighborDirection.NB_Bottom); // bottom
		m_Quadtrees[5].SetNeighbor(en_NeighborDirection.NB_Left, m_Quadtrees[3], en_NeighborDirection.NB_Right); // right
		m_Quadtrees[5].SetNeighbor(en_NeighborDirection.NB_Right, m_Quadtrees[2], en_NeighborDirection.NB_Left); // left

		// atmosphere
		m_AtmosRadius = m_Radius + m_HeightScale * m_AtmosScale;
		m_LodSpheres = new CLodSphere[]
		{
			new CLodSphere(true, this),		// outer sphere
			new CLodSphere(false, this)		// inner sphere
		};

		if (m_Camera == null)
		{
			Debug.Log("[ETHEREA1] Planet " + this.name + " had no associated Camera. Main Camera is being selected as the Camera for this Planet.");
			m_Camera = Camera.main;
		}

		if (m_SunTransform == null)
		{
			Debug.Log("[ETHEREA1] Planet " + this.name + " has no Sun. Please select a Light as the Sun for this Planet.");
		}
	}


	public void LateUpdate()
	{
		Vector3 camPos;
		if (!Application.isPlaying)
		{
			#if UNITY_EDITOR
				if (UnityEditor.SceneView.lastActiveSceneView != null)
				{
					Camera sceneCam = UnityEditor.SceneView.lastActiveSceneView.camera;
					camPos = transform.InverseTransformPoint(sceneCam.transform.position);
				}
				else
				{
					camPos = new Vector3(65535, 65535, 65535);
				}
			#else
				camPos = new Vector3(65535, 65535, 65535);
			#endif
		}
		else
		{
			camPos = transform.InverseTransformPoint(m_Camera.transform.position);
		}

		m_Splitted = m_Rejoined = false;
		m_HighestSplitLevel = 0;
		m_MinCamDist = 9999999.0f;
		m_CloserNode = null;

		// update quadtrees splits|rejoins
		for (byte i = 0; i < m_Quadtrees.Count; i++)
		{
			m_Quadtrees[i].DoAnUpdate(camPos);
		}

		// refresh quadtrees terrain meshes
		for (byte i = 0; i < m_Quadtrees.Count; i++)
		{
			m_Quadtrees[i].RefreshTerrain(camPos);
		}

		// fix gaps between nodes
		for (byte i = 0; i < m_Quadtrees.Count; i++)
		{
			m_Quadtrees[i].RefreshGaps();
		}

		// determine which atmosphere sphere lod is visible
		// note that camPos here is already inverse transformed, so its distance to the planet's center is just its magnitude.
		float dist2cam = camPos.magnitude;
		// lod is 0:lowest, 1:mid, 2:highest detail
		int lod = 2 - Mathf.Min(2, (int)(dist2cam / (m_AtmosRadius*2.0f)));

#if UNITY_EDITOR
		if (m_SunTransform == null)
		{
			return;
		}
		
		if (m_LodSpheres[0] == null || m_LodSpheres[1] == null)
		{
			return;
		}
#endif

		m_InvSunPos = transform.InverseTransformPoint(m_SunTransform.transform.position);
		m_SunDir = m_InvSunPos;
		m_SunDir.Normalize();

		// activate the correct lod and update dynamic shader parms
		m_LodSpheres[0].Select(hasAtmosphere && dist2cam > m_AtmosRadius, lod, m_SunDir, camPos);
		m_LodSpheres[1].Select(hasAtmosphere && dist2cam <= m_AtmosRadius * 2, lod, m_SunDir, camPos);
	}


	//void OnGUI()
	//{
	//    Vector3 p = m_Camera.transform.position;
	//    GUILayout.Label("\n\n");
	//    GUILayout.Label("cam pos: " + p.x + "," + p.y + "," + p.z);
	//    GUILayout.Label("cam dist: " + m_MinCamDist);
	//    GUILayout.Label("split level: " + m_HighestSplitLevel + " / " + m_MaxSplitLevel);
	//    GUILayout.Label("");
	//    GUILayout.Label(gameObject.transform.childCount + " children");
	//    GUILayout.Label(m_Quadtrees.Count + " quadtrees");
	//    GUILayout.Label("");
	//    GUILayout.Label("All " + Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length);
	//    GUILayout.Label("Textures " + Resources.FindObjectsOfTypeAll(typeof(Texture)).Length);
	//    GUILayout.Label("AudioClips " + Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length);
	//    GUILayout.Label("Meshes " + Resources.FindObjectsOfTypeAll(typeof(Mesh)).Length);
	//    GUILayout.Label("Materials " + Resources.FindObjectsOfTypeAll(typeof(Material)).Length);
	//    GUILayout.Label("GameObjects " + Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length);
	//    GUILayout.Label("Components " + Resources.FindObjectsOfTypeAll(typeof(Component)).Length);
	//}
}
