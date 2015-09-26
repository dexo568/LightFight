#if false

//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct TNeighbor
{
	public CQuadtree node;
	public en_NeighborDirection directionThere;
	public bool isFixed;
}


public class CQuadtree
{
	CPlanet m_Planet;

	Vector3[] vertices;
	Vector2[] uvs;
	Vector3[] vols;
	Vector2[] uvvols;
//	Vector3[] normals;
	Vector4[] tangents;

	ushort m_SplitLevel;
	bool m_HasChildren;
	bool m_NeedsTerrain;
	bool m_NeedsReedge;			// needs to determine edge levels (correct vbo)

	float m_Size;
	float m_LastCamDist;
	float m_Min, m_Max;			// max generated heightmap heights

	Vector3 m_Up, m_Front, m_Right;			// not sphere projected
	Vector3 m_SFront, m_SUp;		// sphere projected

	CBoundingVolume m_Volume;	// not sphere projected
	CBoundingVolume m_SVolume;	// sphere projected
	Plane m_Plane;

	Vector3 m_Center;			// center of the node (not projected to sphere)
	Vector3 m_SCenter;		// center of the bounding (projected to sphere)

	Vector3 m_Normal;			// not projected node normal

	int m_HeightMapRes;
	Texture2D m_NormalMapTex;				// Unity Indie will use a simpler Texture2D instead of a RenderTexture

	Texture2D m_ShapeTex;			// optional terrain heightmap shaping texture

	GameObject m_GameObj;			// object for the patch
	Mesh m_Mesh;
	MeshCollider m_MeshCollider;

	// children nodes
	CQuadtree[] m_Children = new CQuadtree[4];

	// parent node
	CQuadtree m_Parent;
	byte m_NeedsRejoinCount;		// how many children needs to rejoin to upper level - if all four need, then really rejoin...

	// neighbors
	TNeighbor[] m_Neighbors = new TNeighbor[4];
	byte m_Edges;					// bitmask of edges that are at half-resolution
	byte m_GapFixMask;			// bitmask of edges that need gap fix


	public CQuadtree(Vector3 up, Vector3 front, CPlanet planet, Texture2D shapeTex)
	{
		m_Planet = planet;
		m_ShapeTex = shapeTex;

		m_Up = m_SUp = up;
		m_Front = m_SFront = front;
		m_Right = -Vector3.Cross(m_Up, m_Front);

		m_Parent = null;
		m_SplitLevel = 0;
		m_Size = m_Planet.m_Radius * 2;

		m_Neighbors[0].node = m_Neighbors[1].node = m_Neighbors[2].node = m_Neighbors[3].node = null;
		m_Neighbors[0].isFixed = m_Neighbors[1].isFixed = m_Neighbors[2].isFixed = m_Neighbors[3].isFixed = false;
		m_Children[0] = m_Children[1] = m_Children[2] = m_Children[3] = null;

		m_NeedsRejoinCount = 0;
		m_HasChildren = false;
		m_NeedsReedge = true;
		m_NeedsTerrain = true;
		m_GapFixMask = 15;

		m_NormalMapTex = null;

		GenVolume();

		m_Plane = new Plane(m_Volume.vertices[0], m_Volume.vertices[2], m_Volume.vertices[1]);
	}


	// volume is already passed as 1/2 of the parent's volume
	public CQuadtree(CQuadtree parent, CBoundingVolume volume)
	{
		m_Parent = parent;
		m_Planet = parent.m_Planet;
		m_ShapeTex = parent.m_ShapeTex;

		m_SplitLevel = (ushort)(m_Parent.m_SplitLevel + 1);
		if (m_SplitLevel > m_Planet.HighestSplitLevel) m_Planet.HighestSplitLevel = m_SplitLevel;
		m_Size = m_Parent.m_Size / 2;

		//
		// make the new spherical projected volume
		//

		m_Volume = volume;

		Vector3 v1 = m_Volume.vertices[0];
		Vector3 v2 = m_Volume.vertices[1];
		Vector3 v3 = m_Volume.vertices[2];
		Vector3 v4 = m_Volume.vertices[3];

		Vector2 uv1 = m_Volume.uvs[0];
		Vector2 uv2 = m_Volume.uvs[1];
		Vector2 uv3 = m_Volume.uvs[2];
		Vector2 uv4 = m_Volume.uvs[3];

		m_SVolume = new CBoundingVolume();

		m_SVolume.vertices.Add(CMath.ProjectToSphere(v1, m_Planet.m_Radius));
		m_SVolume.vertices.Add(CMath.ProjectToSphere(v2, m_Planet.m_Radius));
		m_SVolume.vertices.Add(CMath.ProjectToSphere(v3, m_Planet.m_Radius));
		m_SVolume.vertices.Add(CMath.ProjectToSphere(v4, m_Planet.m_Radius));

		m_SVolume.uvs.Add(uv1);
		m_SVolume.uvs.Add(uv2);
		m_SVolume.uvs.Add(uv3);
		m_SVolume.uvs.Add(uv4);

		//
		// extrapolate the flat volume vertices and uvs for normalmapping
		//

		float vertSpace = m_Size / (m_Planet.PatchConfig.PatchSize - 1);		// vertex spacing
		Vector3 left = -m_Right;

		v1 += left * vertSpace + m_Up * vertSpace;
		v2 += m_Right * vertSpace + m_Up * vertSpace;
		v3 += m_Right * vertSpace - m_Up * vertSpace;
		v4 += left * vertSpace - m_Up * vertSpace;

		m_Volume.exvertices.Add(v1);
		m_Volume.exvertices.Add(v2);
		m_Volume.exvertices.Add(v3);
		m_Volume.exvertices.Add(v4);

		// --

		m_Neighbors[0].node = m_Neighbors[1].node = m_Neighbors[2].node = m_Neighbors[3].node = null;
		m_Neighbors[0].isFixed = m_Neighbors[1].isFixed = m_Neighbors[2].isFixed = m_Neighbors[3].isFixed = false;
		m_Children[0] = m_Children[1] = m_Children[2] = m_Children[3] = null;

		m_NeedsRejoinCount = 0;
		m_HasChildren = false;
		m_NeedsReedge = true;
		m_NeedsTerrain = true;
		m_GapFixMask = 15;

		m_NormalMapTex = null;

		m_Normal = m_Parent.m_Normal;

		m_Up = m_Parent.m_Up;

		m_Center = (volume.vertices[0] + volume.vertices[1] + volume.vertices[2] + volume.vertices[3]) / 4;
		m_SCenter = m_Center;
		m_SCenter = CMath.ProjectToSphere(m_SCenter, m_Planet.m_Radius);

		m_SUp = m_SCenter;
		m_SUp.Normalize();

		m_Front = m_Parent.m_Front;
		m_SFront = Vector3.Lerp(m_SVolume.vertices[0], m_SVolume.vertices[1], 0.5f) - m_SCenter;
		m_SFront.Normalize();

		m_Plane = m_Parent.m_Plane;

		m_Right = m_Parent.m_Right;
	}


	public void DestroyTree()
	{
		if (m_HasChildren)
		{
			for (int i = 0; i < 4; i++)
			{
				m_Children[i].DestroyTree();
				m_Children[i] = null;
			}
			m_HasChildren = false;
		}

		DestroyNode();
	}


	private void DestroyNode()
	{
		if (m_GameObj != null)
		{
			m_GameObj.transform.parent = null;

			#if UNITY_EDITOR
				Material.DestroyImmediate(m_GameObj.renderer.sharedMaterial);
				Mesh.DestroyImmediate(m_Mesh);
				GameObject.DestroyImmediate(m_GameObj);
				RenderTexture.DestroyImmediate(m_NormalMapTex);
			#else
				Material.Destroy(m_GameObj.renderer.sharedMaterial);
				Mesh.Destroy(m_Mesh);
				GameObject.Destroy(m_GameObj);
				RenderTexture.Destroy(m_NormalMapTex);
			#endif

			m_NormalMapTex = null;
			m_Mesh = null;
			m_GameObj = null;
		}
	}


	private void GenVolume()
	{
		// only root uses this
		// define its own bounding volume, depending on the direction vector
		// the bounding volume is based on the planet radius
		// and is always relative to the origin at (0,0,0)
		// the planet that will be instancing this quadtree is a CEntity, though, having position and rotation.

		Vector3 left = -m_Right;

		// volume vertices are clockwise
		Vector3 v1 = (left * m_Planet.m_Radius) + (m_Front * m_Planet.m_Radius) + (m_Up * m_Planet.m_Radius);		// left far
		Vector3 v2 = (left * -m_Planet.m_Radius) + (m_Front * m_Planet.m_Radius) + (m_Up * m_Planet.m_Radius);   	// right far
		Vector3 v3 = (left * -m_Planet.m_Radius) + (m_Front * -m_Planet.m_Radius) + (m_Up * m_Planet.m_Radius);   // right near
		Vector3 v4 = (left * m_Planet.m_Radius) + (m_Front * -m_Planet.m_Radius) + (m_Up * m_Planet.m_Radius);   	// left near

		Vector3 uv1 = new Vector3(0,0,0);
		Vector3 uv2 = new Vector3(1,0,0);
		Vector3 uv3 = new Vector3(1,1,0);
		Vector3 uv4 = new Vector3(0,1,0);

		m_Volume = new CBoundingVolume();
		m_Volume.vertices.Add(v1); m_Volume.uvs.Add(uv1);
		m_Volume.vertices.Add(v2); m_Volume.uvs.Add(uv2);
		m_Volume.vertices.Add(v3); m_Volume.uvs.Add(uv3);
		m_Volume.vertices.Add(v4); m_Volume.uvs.Add(uv4);

		//
		// extrapolate the volume vertices and uvs for normalmapping
		//

		float vertSpace = m_Size / (m_Planet.PatchConfig.PatchSize - 1);		// vertex spacing

		v1 += left * vertSpace + m_Up * vertSpace;
		v2 += m_Right * vertSpace + m_Up * vertSpace;
		v3 += m_Right * vertSpace - m_Up * vertSpace;
		v4 += left * vertSpace - m_Up * vertSpace;

		m_Volume.exvertices.Add(v1);
		m_Volume.exvertices.Add(v2);
		m_Volume.exvertices.Add(v3);
		m_Volume.exvertices.Add(v4);

		//
		// make the spherical deformed volume
		//

		Vector3 v5 = v1;
		Vector3 v6 = v2;
		Vector3 v7 = v3;
		Vector3 v8 = v4;

		v5 = CMath.ProjectToSphere(v5, m_Planet.m_Radius);
		v6 = CMath.ProjectToSphere(v6, m_Planet.m_Radius);
		v7 = CMath.ProjectToSphere(v7, m_Planet.m_Radius);
		v8 = CMath.ProjectToSphere(v8, m_Planet.m_Radius);

		m_SVolume = new CBoundingVolume();
		m_SVolume.vertices.Add(v5);
		m_SVolume.vertices.Add(v6);
		m_SVolume.vertices.Add(v7);
		m_SVolume.vertices.Add(v8);

		m_Normal = m_Up;

		m_Center = (v1 + v2 + v3 + v4) / 4;
		m_SCenter = m_Center;
		m_SCenter = CMath.ProjectToSphere(m_SCenter, m_Planet.m_Radius);
	}


	// generate heightmap
	private void GenTerrain()
	{
		CGlobal global = CGlobal.GetInstance();

		// heightmap res at current level
		m_HeightMapRes = m_Planet.PatchConfig.LowResHeightMapSize; // m_Planet.PatchConfig.LevelHeightMapRes(m_Planet.HighestSplitLevel);

		// use CPU for fractal generation
		Color[] heights = new Color[m_Planet.PatchConfig.LowResHeightMapSize * m_Planet.PatchConfig.LowResHeightMapSize];

		//
		// update geometry
		//

		m_GameObj = new GameObject();
		m_GameObj.name = "lvl " + m_SplitLevel + " : " + m_Up;
		m_GameObj.AddComponent<MeshFilter>();
		m_GameObj.AddComponent<MeshRenderer>();

		m_Mesh = new Mesh();

		vertices = new Vector3[m_Planet.PatchConfig.GridSize];
		uvs = new Vector2[m_Planet.PatchConfig.GridSize];
		vols = new Vector3[m_Planet.PatchConfig.GridSize];
		uvvols = new Vector2[m_Planet.PatchConfig.GridSize];
//		normals = new Vector3[m_Planet.PatchConfig.GridSize];
		tangents = new Vector4[m_Planet.PatchConfig.GridSize];

		Vector3 origin = m_Volume.vertices[0];
		float vertStep = m_Size / (m_Planet.PatchConfig.PatchSize - 1);		// vertex spacing

		float startHMap = 1.0f / m_HeightMapRes;
		float endHMap = 1.0f - startHMap;
		float uvStep = (endHMap - startHMap) / (m_Planet.PatchConfig.PatchSize - 1);			// uv coordinate stepping
		float uCoord = startHMap, vCoord = startHMap;

		float uVolCoord, vVolCoord = m_Volume.uvs[0].y;
		float volCoordStep = (m_Volume.uvs[1].x - m_Volume.uvs[0].x) / (m_Planet.PatchConfig.PatchSize-1);

		float maxHeight = -999999999.0f;

		// loop through the lowres heightmap
		int idx = 0;
		for (ushort y=0; y<m_Planet.PatchConfig.PatchSize; y++)
		{
			Vector3 offset = origin;
			uCoord = startHMap;
			uVolCoord = m_Volume.uvs[0].x;
			for (ushort x=0; x<m_Planet.PatchConfig.PatchSize; x++)
			{
				// get sampled height from CPU noise generator
				float height = 0;
				foreach (CNoiseLayer layer in m_Planet.layers)
				{
					height += global.SNoiseCPU.GetNoise3D(offset.x, offset.y, offset.z, layer.m_Persistence, layer.m_Octaves, layer.m_Frequency) * layer.m_Contribution;
				}

				heights[idx].a = height;

				if (height <= m_Planet.m_WaterLevel) height = m_Planet.m_WaterLevel;

				height = height * m_Planet.m_HeightScale;
				if (height > maxHeight) maxHeight = height;

				// heightmap texture coordinates
				uvs[idx] = new Vector2(uCoord, vCoord);
				uCoord += uvStep;

				// volume texture coordinates
				uvvols[idx] = new Vector2(uVolCoord, vVolCoord);
				uVolCoord += volCoordStep;

				// calculate vertex position
				Vector3 vtx = offset;

				// use normalized vertex position as vertex normal
				vtx.Normalize();
//				normals[idx] = vtx;

				// calculate vertex right (tangent) vector, based on patch front and right
				tangents[idx] = -Vector3.Cross(vtx, m_SFront);
				tangents[idx].w = 1; // (m_Up.y == 0 ? 1 : -1);

				if (m_Planet.m_Sphere)
				{
					// scale to sphere
					vtx = vtx * (m_Planet.m_Radius + height);
				}
				else
				{
					// scale to cube
					vtx = offset + (m_Up * height);
				}

				// store
				vertices[idx] = vtx;
				vols[idx] = offset;

				idx++;
				offset += m_Right * vertStep;
			}
			origin -= m_Front * vertStep;
			vCoord += uvStep;
			vVolCoord += volCoordStep;
		}

		m_NormalMapTex = new Texture2D(m_Planet.PatchConfig.LowResHeightMapSize, m_Planet.PatchConfig.LowResHeightMapSize, CGlobal.TEXTURE_FORMAT, false);
		m_NormalMapTex.anisoLevel = 0;
		m_NormalMapTex.filterMode = (m_Planet.m_FilteredHeights ? FilterMode.Bilinear : FilterMode.Point);
		m_NormalMapTex.wrapMode = TextureWrapMode.Clamp;
		m_NormalMapTex.SetPixels(heights);
		m_NormalMapTex.Apply(false);

		// update projected center
		m_SCenter = CMath.ProjectToSphere(m_SCenter, m_Planet.m_Radius + maxHeight);

		// save original parent transformations
		Vector3 parentPos = m_Planet.gameObject.transform.position;
		Quaternion parentQua = m_Planet.gameObject.transform.rotation;

		// reset parent transformations before assigning mesh data (so our vertices will be centered on the parent transform)
		m_Planet.gameObject.transform.position = Vector3.zero;
		m_Planet.gameObject.transform.rotation = Quaternion.identity;

		// put this node as a child of parent
		m_GameObj.transform.parent = m_Planet.gameObject.transform;

		// assign data to this node's mesh
		m_Mesh.vertices = vertices;
		m_Mesh.uv = uvs;				// vertex uv coordinates
		m_Mesh.uv1 = uvvols;			// passing flat patch volume uv coordinates as second texcoords
		m_Mesh.normals = vols;			// passing flat volume coordinates as vertex normals
		m_Mesh.tangents = tangents;
		m_Mesh.triangles = m_Planet.PatchManager.Patches[m_Edges];
		m_GameObj.renderer.sharedMaterial = (Material)Material.Instantiate(global.PatchMaterial);

		//m_Mesh.Optimize();
		m_Mesh.RecalculateBounds();
		//m_Mesh.RecalculateNormals();
		m_GameObj.GetComponent<MeshFilter>().mesh = m_Mesh;

		// set fixed shader parameters
		// indie uses CPU generated Texture2D heightmap
		m_GameObj.renderer.sharedMaterial.SetTexture("_hmap", m_NormalMapTex);
		m_GameObj.renderer.sharedMaterial.SetTexture("_rndmap", global.PermTex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_planetRadius", m_Planet.m_Radius);
		m_GameObj.renderer.sharedMaterial.SetFloat("_atmosRadius", m_Planet.AtmosRadius);
		m_GameObj.renderer.sharedMaterial.SetFloat("_temperature", m_Planet.m_Temperature);
		m_GameObj.renderer.sharedMaterial.SetTexture("_blendLut", m_Planet.LutGen.m_LutTex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_waterLevel", m_Planet.m_WaterLevel);
		m_GameObj.renderer.sharedMaterial.SetTexture("_waterTex", m_Planet.m_WaterTex);
		m_GameObj.renderer.sharedMaterial.SetTexture("_waterNormal", m_Planet.m_WaterNormal);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tilingWater", m_Planet.m_WaterTexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_snowTex", m_Planet.m_SnowTex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tilingSnow", m_Planet.m_SnowTexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_diffuseBase", m_Planet.m_BaseTex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tilingBase", m_Planet.m_BaseTexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_diffuse1", m_Planet.m_Diffuse1Tex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tiling1", m_Planet.m_Diffuse1TexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_diffuse2", m_Planet.m_Diffuse2Tex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tiling2", m_Planet.m_Diffuse2TexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_diffuse3", m_Planet.m_Diffuse3Tex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tiling3", m_Planet.m_Diffuse3TexTile);
		m_GameObj.renderer.sharedMaterial.SetTexture("_diffuse4", m_Planet.m_Diffuse4Tex);
		m_GameObj.renderer.sharedMaterial.SetFloat("_tiling4", m_Planet.m_Diffuse4TexTile);

		// more fixed shader parameters - related to the atmosphere
		m_GameObj.renderer.sharedMaterial.SetFloat("_sunIntensity", m_Planet.m_sunIntensity);
		m_GameObj.renderer.sharedMaterial.SetFloat("_horizonHeight", m_Planet.m_HorizonHeight);
		m_GameObj.renderer.sharedMaterial.SetFloat("_horizonIntensity", m_Planet.m_HorizonIntensity);
		m_GameObj.renderer.sharedMaterial.SetFloat("_horizonPower", m_Planet.m_HorizonPower);
		m_GameObj.renderer.sharedMaterial.SetColor("_color1", m_Planet.m_AtmosColor1);
		m_GameObj.renderer.sharedMaterial.SetColor("_color2", m_Planet.m_AtmosColor2);
		m_GameObj.renderer.sharedMaterial.SetFloat("_fogIntensity", m_Planet.m_FogIntensity);
		m_GameObj.renderer.sharedMaterial.SetFloat("_fogHeight", m_Planet.m_FogHeight);
		m_GameObj.renderer.sharedMaterial.SetFloat("_fogNear", m_Planet.m_FogNear);
		m_GameObj.renderer.sharedMaterial.SetFloat("_fogFar", m_Planet.m_FogFar);
		m_GameObj.renderer.sharedMaterial.SetFloat("_fogMaxAlpha", m_Planet.m_FogMaxAlpha);

		// restore parent transformations
		m_Planet.gameObject.transform.position = parentPos;
		m_Planet.gameObject.transform.rotation = parentQua;

		// inactivate this gameobject
		m_GameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
		// --

		if (m_Planet.m_EnablePhysics)
		{
			if (m_SplitLevel >= m_Planet.m_MinPhysicsSplitLevel)
			{
				// add mesh collider to the patch
				m_MeshCollider = (MeshCollider)m_GameObj.AddComponent<MeshCollider>();
				m_MeshCollider.enabled = true;
			}
		}

		if (m_SplitLevel >= m_Planet.m_MinShadowsSplitLevel)
		{
			// cast and receive shadows
			m_GameObj.renderer.castShadows = true;
			m_GameObj.renderer.receiveShadows = true;
		}
		else
		{
			// do not cast or receive shadows
			m_GameObj.renderer.castShadows = false;
			m_GameObj.renderer.receiveShadows = false;
		}

		m_NeedsTerrain = false;

		// discard parent's resources
		if (m_Parent != null)
		{
			m_Parent.DestroyNode();
		}
	}


	public byte NEXT_EDGE(byte e) { return (byte)(e == 3 ? 0 : e + 1); }
	public byte PREV_EDGE(byte e) { return (byte)(e == 0 ? 3 : e - 1); }


	// fix gaps between neighbor nodes
	private void GapFix(byte directionsMask)
	{
		short posHere = 0;
		short posThere = 0;

		short incHere = 1;
		short incThere = 1;

		// TBLR (top, right, bottom, left)
		// 0000 == all edges at full-res
		// 0001 == top edge at half-res
		// 0010 == right edge at half-res
		// 0100 == bottom edge at half-res
		// 1000 == left edge at half-res

		short idxTopLeft = 0;
		short idxTopRight = (short)(m_Planet.PatchConfig.PatchSize-1);
		short idxBottomLeft = (short)((m_Planet.PatchConfig.PatchSize-1) * m_Planet.PatchConfig.PatchSize);
		short idxBottomRight = (short)(idxBottomLeft + idxTopRight);

		for (byte direction = 0; direction < 4; direction++)
		{
			byte bit = (byte)(1 << direction);
			if ((bit & directionsMask) > 0)
			{
				short add = 0;

				if (m_Neighbors[direction].node.m_HasChildren)
				{
				}
				else
				{
					switch ((en_NeighborDirection)direction)
					{
						case en_NeighborDirection.NB_Top:
							{
								posHere = 0;
								incHere = 1;

								CQuadtree np = m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Parent;
								add = (short)(np != null && m_Parent != null && np.Equals(m_Parent) && m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Neighbors[(int)en_NeighborDirection.NB_Top].node.Equals(m_Neighbors[direction].node) ? 0 : (m_Planet.PatchConfig.PatchSize>>1));

								switch (m_Neighbors[direction].directionThere)
								{
									case en_NeighborDirection.NB_Top:
										{
											posThere = idxTopRight;
											incThere = -1;
											break;
										}

									case en_NeighborDirection.NB_Bottom:
										{
											posThere = idxBottomLeft;
											incThere = 1;
											break;
										}

									case en_NeighborDirection.NB_Left:
										{
											posThere = idxTopLeft;
											incThere = (short)(m_Planet.PatchConfig.PatchSize);
											break;
										}

									case en_NeighborDirection.NB_Right:
										{
											posThere = idxBottomRight;
											incThere = (short)(-m_Planet.PatchConfig.PatchSize);
											break;
										}
								}
								break;
							}

						case en_NeighborDirection.NB_Right:
							{
								posHere = idxTopRight;
								incHere = (short)(m_Planet.PatchConfig.PatchSize);

								CQuadtree np = m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Parent;
								add = (short)(np != null && m_Parent != null && np.Equals(m_Parent) && m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Neighbors[(int)en_NeighborDirection.NB_Right].node.Equals(m_Neighbors[direction].node) ? 0 : (m_Planet.PatchConfig.PatchSize >> 1));

								switch (m_Neighbors[direction].directionThere)
								{
									case en_NeighborDirection.NB_Top:
										{
											posThere = idxTopRight;
											incThere = -1;
											break;
										}

									case en_NeighborDirection.NB_Bottom:
										{
											posThere = idxBottomLeft;
											incThere = 1;
											break;
										}

									case en_NeighborDirection.NB_Left:
										{
											posThere = idxTopLeft;
											incThere = (short)(m_Planet.PatchConfig.PatchSize);
											break;
										}

									case en_NeighborDirection.NB_Right:
										{
											posThere = idxBottomRight;
											incThere = (short)(-m_Planet.PatchConfig.PatchSize);
											break;
										}
								}
								break;
							}

						case en_NeighborDirection.NB_Bottom:
							{
								posHere = idxBottomRight;
								incHere = -1;

								CQuadtree np = m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Parent;
								add = (short)(np != null && m_Parent != null && np.Equals(m_Parent) && m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.Equals(m_Neighbors[direction].node) ? 0 : (m_Planet.PatchConfig.PatchSize>>1));

								switch (m_Neighbors[direction].directionThere)
								{
									case en_NeighborDirection.NB_Top:
										{
											posThere = idxTopRight;
											incThere = -1;
											break;
										}

									case en_NeighborDirection.NB_Bottom:
										{
											posThere = idxBottomLeft;
											incThere = 1;
											break;
										}

									case en_NeighborDirection.NB_Left:
										{
											posThere = idxTopLeft;
											incThere = (short)(m_Planet.PatchConfig.PatchSize);
											break;
										}

									case en_NeighborDirection.NB_Right:
										{
											posThere = idxBottomRight;
											incThere = (short)(-m_Planet.PatchConfig.PatchSize);
											break;
										}
								}
								break;
							}

						case en_NeighborDirection.NB_Left:
							{
								posHere = idxBottomLeft;
								incHere = (short)(-m_Planet.PatchConfig.PatchSize);

								CQuadtree np = m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Parent;
								add = (short)(np != null && m_Parent != null && np.Equals(m_Parent) && m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Neighbors[(int)en_NeighborDirection.NB_Left].node.Equals(m_Neighbors[direction].node) ? 0 : (m_Planet.PatchConfig.PatchSize>>1));

								switch (m_Neighbors[direction].directionThere)
								{
									case en_NeighborDirection.NB_Top:
										{
											posThere = idxTopRight;
											incThere = -1;
											break;
										}

									case en_NeighborDirection.NB_Bottom:
										{
											posThere = idxBottomLeft;
											incThere = 1;
											break;
										}

									case en_NeighborDirection.NB_Left:
										{
											posThere = idxTopLeft;
											incThere = (short)(m_Planet.PatchConfig.PatchSize);
											break;
										}

									case en_NeighborDirection.NB_Right:
										{
											posThere = idxBottomRight;
											incThere = (short)(-m_Planet.PatchConfig.PatchSize);
											break;
										}
								}
								break;
							}
					}

					ushort loopLen = m_Planet.PatchConfig.PatchSize;

					// check for half-resolution neighbor
					if ((m_Edges & bit) > 0)
					{
						// half resolution
						incHere <<= 1;
						loopLen >>= 1;
						loopLen++;
						posThere += (short)(add*incThere);
					}

					//
					// transfer edge positions
					//
#if true
					// fix the first edge vertex only
					// if it's not already fixed by the edge at left (counter-clockwise) of current edge
					if (!m_Neighbors[PREV_EDGE(direction)].isFixed)
					{
						vertices[posHere] = m_Neighbors[direction].node.vertices[posThere];
					}
					else
					{
						// instead, fix the vertex of the other node
						m_Neighbors[direction].node.vertices[posThere] = vertices[posHere];
					}

					posHere += incHere;
					posThere += incThere;

					ushort x = 0;
					while (x < loopLen-2)
					{
						m_Neighbors[direction].node.vertices[posThere] = vertices[posHere];

						x++;
						posHere += incHere;
						posThere += incThere;
					}

					// fix the last edge vertex only
					// if it's not already fixed by the edge at right (clockwise) of current edge
					if (!m_Neighbors[NEXT_EDGE(direction)].isFixed)
					{
						vertices[posHere] = m_Neighbors[direction].node.vertices[posThere];
					}
					else
					{
						// instead, fix the vertex of the other node
						m_Neighbors[direction].node.vertices[posThere] = vertices[posHere];
					}

					// reupload vertices to the mesh in this node and update its physics mesh
					m_Mesh.vertices = vertices;
					m_Mesh.RecalculateBounds();
					if (m_MeshCollider)
					{
						m_MeshCollider.sharedMesh = null;
						m_MeshCollider.sharedMesh = m_Mesh;
						m_MeshCollider.sharedMesh.RecalculateBounds();
					}

					// reupload vertices to the neighbor mesh in the other node and update its physics mesh
					m_Neighbors[direction].node.m_Mesh.vertices = m_Neighbors[direction].node.vertices;
					m_Neighbors[direction].node.m_Mesh.RecalculateBounds();
					if (m_Neighbors[direction].node.m_MeshCollider)
					{
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh = null;
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh = m_Neighbors[direction].node.m_Mesh;
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh.RecalculateBounds();
					}

					// ok, this edge is fixed.
					m_Neighbors[direction].isFixed = true;

					// the other node's edge is fixed as well.
					byte dirThere = (byte)m_Neighbors[direction].directionThere;
					m_Neighbors[direction].node.m_Neighbors[dirThere].isFixed = true;
#else
					ushort x = 0;
					while (x < loopLen)
					{
						m_Neighbors[direction].node.vertices[posThere] = vertices[posHere];

						x++;
						posHere += incHere;
						posThere += incThere;
					}

					// reupload vertices to the neighbor mesh in the other node and update its physics mesh
					m_Neighbors[direction].node.m_Mesh.vertices = m_Neighbors[direction].node.vertices;
					m_Neighbors[direction].node.m_Mesh.RecalculateBounds();
					if (m_Neighbors[direction].node.m_MeshCollider)
					{
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh = null;
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh = m_Neighbors[direction].node.m_Mesh;
						m_Neighbors[direction].node.m_MeshCollider.sharedMesh.RecalculateBounds();
					}

					// ok, this edge is fixed.
					//m_Neighbors[direction].isFixed = true;

					// the other node's edge is fixed as well.
					byte dirThere = (byte)m_Neighbors[direction].directionThere;
					m_Neighbors[direction].node.m_Neighbors[dirThere].isFixed = true;
#endif
				}
			}
		}
	}


	// will determine correct edges around the node, and fix gaps when needed
	public void RefreshTerrain(Vector3 entityPos)
	{
		if (!m_HasChildren)
		{
			if (m_NeedsTerrain)
			{
				GenTerrain();
			}

			// update shader variable parameters
			UpdateShaderParms(entityPos);
		}
		else
		{
			for (byte i=0; i<4; i++)
			{
				m_Children[i].RefreshTerrain(entityPos);
			}
		}
	}


	// will determine correct edges around the node, and fix gaps when needed
	public void RefreshGaps()
	{
		if (!m_HasChildren)
		{
			if (m_NeedsReedge)
			{
				Reedge();
			}
			if (m_GapFixMask > 0)
			{
				GapFix(m_GapFixMask);
				m_GapFixMask = 0;
			}
		}
		else
		{
			for (byte i=0; i<4; i++)
			{
				m_Children[i].RefreshGaps();
			}
		}
	}


	// walk through all nodes anotating those who needs to rebuild (split/join)
	// note: entityPos is inverse transformed to the planet's rotation
	public void DoAnUpdate(Vector3 entityPos)
	{
		if (!m_HasChildren)
		{
			// =========================================================================
			//
			// this fragment determines which is the node closer to the camera
			// but for posterior collision detection use -- NOT for split()/rejoin().
			//

			// project entityPos to the flat plane
			Vector3 pos = entityPos;
			float t;
			float d1 = m_Plane.GetDistanceToPoint(pos);
			float d2 = m_Plane.distance;
			float dd = d2 - d1;
			if (dd != 0)
				t = -d1 / dd;
			else
				t = 0;

			pos = pos * (1.0f - t);

			// check distance to the center of the patch
			float patchDist = CMath.QuickDistance(m_Center, pos);

			if (/*m_SplitLevel > 1 &&*/ patchDist < m_Planet.MinCamDist)
			{
				m_Planet.MinCamDist = patchDist;
				m_Planet.CloserNode = this;
			}
			// =========================================================================

			if (m_SplitLevel > m_Planet.HighestSplitLevel)
			{
				m_Planet.HighestSplitLevel = m_SplitLevel;
			}

#if UNITY_EDITOR
			if (Application.isPlaying || m_Planet.m_Wysiwyg)
			{
#endif
				if (!m_Planet.Splitted /*&& !m_Planet.Rejoined*/)
				{
					// check distance of this node's center to the camera
					// coordinates are local, as if the planet were in origin
					m_LastCamDist = CMath.QuickDistance(m_SCenter, entityPos);

					if (m_LastCamDist < m_Size * m_Size * m_Planet.m_SizeSplit)
					{
						if (m_SplitLevel < m_Planet.m_MaxSplitLevel)
						{
							Split();
						}
					}
					else
					{
						if (m_Parent != null)
						{
							float lastParentDist = CMath.QuickDistance(m_Parent.m_SCenter, entityPos);
							if (lastParentDist > m_Parent.m_Size * m_Parent.m_Size * m_Planet.m_SizeRejoin)
							{
								if
									(
										(m_SplitLevel > m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_SplitLevel || !m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_HasChildren && m_SplitLevel == m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_SplitLevel)
										&&
										(m_SplitLevel > m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_SplitLevel || !m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_HasChildren && m_SplitLevel == m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_SplitLevel)
										&&
										(m_SplitLevel > m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_SplitLevel || !m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_HasChildren && m_SplitLevel == m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_SplitLevel)
										&&
										(m_SplitLevel > m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_SplitLevel || !m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_HasChildren && m_SplitLevel == m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_SplitLevel)
									)
								{
									m_Parent.m_NeedsRejoinCount++;
								}
							}
						}
					}
				}
#if UNITY_EDITOR
			}
#endif
		}
		else
		{
			m_NeedsRejoinCount = 0;

			for (byte i=0; i<4; i++)
			{
				m_Children[i].DoAnUpdate(entityPos);
			}

			if (m_NeedsRejoinCount == 4)
			{
				// only rejoins if splitlevel is not minus than any neighbor
				Rejoin();
			}
		}
	}


	// split node into 4 children
	private void Split()
	{
		// force too coarse neighbors to split as well
		for (byte i=0; i<4; i++)
		{
			if (m_SplitLevel > m_Neighbors[i].node.m_SplitLevel && !m_Neighbors[i].node.m_HasChildren)
			{
				m_Neighbors[i].node.Split();
				return;
			}
		}

		//
		// split into the children
		//

		// first child - top left
		CBoundingVolume vol1 = new CBoundingVolume();
		Vector3 v1a = m_Volume.vertices[0];
		Vector3 v1b = Vector3.Lerp(m_Volume.vertices[0], m_Volume.vertices[1], 0.5f);
		Vector3 v1c = m_Center;
		Vector3 v1d = Vector3.Lerp(m_Volume.vertices[3], m_Volume.vertices[0], 0.5f);
		vol1.vertices.Add(v1a);
		vol1.vertices.Add(v1b);
		vol1.vertices.Add(v1c);
		vol1.vertices.Add(v1d);
		Vector3 uv1a = m_Volume.uvs[0];
		Vector3 uv1b = Vector3.Lerp(m_Volume.uvs[0], m_Volume.uvs[1], 0.5f);
		Vector3 uv1d = Vector3.Lerp(m_Volume.uvs[3], m_Volume.uvs[0], 0.5f);
		Vector3 uv1c = new Vector3(uv1b.x, uv1d.y, 0);
		vol1.uvs.Add(uv1a);
		vol1.uvs.Add(uv1b);
		vol1.uvs.Add(uv1c);
		vol1.uvs.Add(uv1d);
		CQuadtree q1 = new CQuadtree(this, vol1);

		// second child - top right
		CBoundingVolume vol2 = new CBoundingVolume();
		Vector3 v2a = Vector3.Lerp(m_Volume.vertices[0], m_Volume.vertices[1], 0.5f);
		Vector3 v2b = m_Volume.vertices[1];
		Vector3 v2c = Vector3.Lerp(m_Volume.vertices[1], m_Volume.vertices[2], 0.5f);
		Vector3 v2d = m_Center;
		vol2.vertices.Add(v2a);
		vol2.vertices.Add(v2b);
		vol2.vertices.Add(v2c);
		vol2.vertices.Add(v2d);
		Vector3 uv2a = Vector3.Lerp(m_Volume.uvs[0], m_Volume.uvs[1], 0.5f);
		Vector3 uv2b = m_Volume.uvs[1];
		Vector3 uv2c = Vector3.Lerp(m_Volume.uvs[1], m_Volume.uvs[2], 0.5f);
		Vector3 uv2d = new Vector3(uv2a.x, uv2c.y, 0);
		vol2.uvs.Add(uv2a);
		vol2.uvs.Add(uv2b);
		vol2.uvs.Add(uv2c);
		vol2.uvs.Add(uv2d);
		CQuadtree q2 = new CQuadtree(this, vol2);

		// third child - bottom right
		CBoundingVolume vol3 = new CBoundingVolume();
		Vector3 v3a = m_Center;
		Vector3 v3b = Vector3.Lerp(m_Volume.vertices[1], m_Volume.vertices[2], 0.5f);
		Vector3 v3c = m_Volume.vertices[2];
		Vector3 v3d = Vector3.Lerp(m_Volume.vertices[3], m_Volume.vertices[2], 0.5f);
		vol3.vertices.Add(v3a);
		vol3.vertices.Add(v3b);
		vol3.vertices.Add(v3c);
		vol3.vertices.Add(v3d);
		Vector3 uv3b = Vector3.Lerp(m_Volume.uvs[1], m_Volume.uvs[2], 0.5f);
		Vector3 uv3c = m_Volume.uvs[2];
		Vector3 uv3d = Vector3.Lerp(m_Volume.uvs[3], m_Volume.uvs[2], 0.5f);
		Vector3 uv3a = new Vector3(uv3d.x, uv3b.y, 0);
		vol3.uvs.Add(uv3a);
		vol3.uvs.Add(uv3b);
		vol3.uvs.Add(uv3c);
		vol3.uvs.Add(uv3d);
		CQuadtree q3 = new CQuadtree(this, vol3);

		// fourth child - bottom left
		CBoundingVolume vol4 = new CBoundingVolume();
		Vector3 v4a = Vector3.Lerp(m_Volume.vertices[3], m_Volume.vertices[0], 0.5f);
		Vector3 v4b = m_Center;
		Vector3 v4c = Vector3.Lerp(m_Volume.vertices[3], m_Volume.vertices[2], 0.5f);
		Vector3 v4d = m_Volume.vertices[3];
		vol4.vertices.Add(v4a);
		vol4.vertices.Add(v4b);
		vol4.vertices.Add(v4c);
		vol4.vertices.Add(v4d);
		Vector3 uv4a = Vector3.Lerp(m_Volume.uvs[3], m_Volume.uvs[0], 0.5f);
		Vector3 uv4c = Vector3.Lerp(m_Volume.uvs[3], m_Volume.uvs[2], 0.5f);
		Vector3 uv4d = m_Volume.uvs[3];
		Vector3 uv4b = new Vector3(uv4c.x, uv4a.y, 0);
		vol4.uvs.Add(uv4a);
		vol4.uvs.Add(uv4b);
		vol4.uvs.Add(uv4c);
		vol4.uvs.Add(uv4d);
		CQuadtree q4 = new CQuadtree(this, vol4);

		// -----------------------------------------------------------------------

		// set internal neighbors
		q1.SetNeighbor(en_NeighborDirection.NB_Bottom, q4, en_NeighborDirection.NB_Top);
		q1.SetNeighbor(en_NeighborDirection.NB_Right, q2, en_NeighborDirection.NB_Left);

		// set internal neighbors
		q2.SetNeighbor(en_NeighborDirection.NB_Bottom, q3, en_NeighborDirection.NB_Top);
		q2.SetNeighbor(en_NeighborDirection.NB_Left, q1, en_NeighborDirection.NB_Right);

		// set internal neighbors
		q3.SetNeighbor(en_NeighborDirection.NB_Top, q2, en_NeighborDirection.NB_Bottom);
		q3.SetNeighbor(en_NeighborDirection.NB_Left, q4, en_NeighborDirection.NB_Right);

		// set internal neighbors
		q4.SetNeighbor(en_NeighborDirection.NB_Top, q1, en_NeighborDirection.NB_Bottom);
		q4.SetNeighbor(en_NeighborDirection.NB_Right, q3, en_NeighborDirection.NB_Left);

		// store as children of the current node
		m_Children[0] = q1;
		m_Children[1] = q2;
		m_Children[2] = q3;
		m_Children[3] = q4;

		m_Planet.Splitted = true;
		m_HasChildren = true;
		Relink();
	}


	// reset external neighbors of the recently split node
	private void Relink()
	{
		m_Children[0].SetNeighbor(en_NeighborDirection.NB_Top, m_Neighbors[(int)en_NeighborDirection.NB_Top].node, m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere);
		m_Children[0].SetNeighbor(en_NeighborDirection.NB_Left, m_Neighbors[(int)en_NeighborDirection.NB_Left].node, m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere);

		m_Children[1].SetNeighbor(en_NeighborDirection.NB_Top, m_Neighbors[(int)en_NeighborDirection.NB_Top].node, m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere);
		m_Children[1].SetNeighbor(en_NeighborDirection.NB_Right, m_Neighbors[(int)en_NeighborDirection.NB_Right].node, m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere);

		m_Children[2].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node, m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere);
		m_Children[2].SetNeighbor(en_NeighborDirection.NB_Right, m_Neighbors[(int)en_NeighborDirection.NB_Right].node, m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere);

		m_Children[3].SetNeighbor(en_NeighborDirection.NB_Bottom, m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node, m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere);
		m_Children[3].SetNeighbor(en_NeighborDirection.NB_Left, m_Neighbors[(int)en_NeighborDirection.NB_Left].node, m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere);
	}


	// update neighbors edges bits (select correct vbo in CVBOManager)
	private void Reedge()
	{
		// the flagged directions are of less resolution at that side
		// using binary to represent edge combination
		// TBLR (top, bottom, left, right)
		// 0000 == all edges at full-res
		// 0001 == top edge at half-res
		// 0010 == right edge at half-res
		// 0100 == bottom edge at half-res
		// 1000 == left edge at half-res

		byte oldEdges = m_Edges;
		m_Edges = 0;
		for (byte i=0; i<4; i++)
		{
			if (m_Neighbors[i].node.m_SplitLevel < m_SplitLevel) m_Edges |= (byte)(1 << i);
		}

		if (m_Edges != oldEdges)
		{
			// reassign the index buffer
			m_Mesh.triangles = m_Planet.PatchManager.Patches[m_Edges];
		}

		m_NeedsReedge = false;
	}


	// set a neighbor to this node
	public void SetNeighbor(en_NeighborDirection direction, CQuadtree pToNode, en_NeighborDirection directionFromThere)
	{
		if (pToNode.m_HasChildren)
		{
			// the other node has children, which means this node was in coarse resolution,
			// so find correct child to link to...
			// need to find which two of the 4 children
			// of the other node that links to the parent of this node or to this node itself
			// then, decide which of the two children is closer to this node
			// and update the correct (nearest) child to link to this node

			CQuadtree pCorrectNode = null;
			float dist = 0;
			byte neighDirection = 0;

			// for each child of that node...
			for (byte i=0; i<4; i++)
			{
				CQuadtree pChild = pToNode.m_Children[i];

				// for each direction of that child of that node...
				for (byte j=0; j<4; j++)
				{
					// check if that child links from that direction to our parent
					if (pChild.m_Neighbors[j].node.Equals(m_Parent))
					{
						if (pCorrectNode == null)
						{
							// as there is no best correct child yet,
							// temporarily selects that child as the correct
							pCorrectNode = pChild;
							neighDirection = j;
							dist = CMath.QuickDistance(pChild.m_Center, m_Center);
							break;
						}
						else
						{
							// check if this child is closer than
							// the currently selected as the closer child
							if (CMath.QuickDistance(pChild.m_Center, m_Center) < dist)
							{
								pCorrectNode = pChild;
								neighDirection = j;

								// as we can have only two childs
								// pointing to our own parent, and the other child has been scanned already,
								// we can safely bail out of the outer loop and stop searching
								i = 4;
								break;
							}
						}
					}
					else if (pChild.m_Neighbors[j].node == this)
					{
						// that child relinked to this node first
						// which means both nodes are at same level
						// so just get it and bail out
						pCorrectNode = pChild;
						neighDirection = j;

						// link back to that node
						m_Neighbors[(int)direction].node = pCorrectNode;
						m_Neighbors[(int)direction].directionThere = (en_NeighborDirection)neighDirection;

						// update edges of this node
						m_NeedsReedge = true;

						// bail out
						return;
					}
				}
			}

			if (pCorrectNode != null)
			{
				// link to that node
				m_Neighbors[(int)direction].node = pCorrectNode;
				m_Neighbors[(int)direction].directionThere = (en_NeighborDirection)neighDirection;

				// link that node back to this node
				pCorrectNode.m_Neighbors[neighDirection].node = this;
				pCorrectNode.m_Neighbors[neighDirection].directionThere = direction;

				// update edges and gaps
				m_NeedsReedge = true;
				pCorrectNode.m_NeedsReedge = true;

				// the other node was discarding resolution
				// because this node was at coarse level
				// now that both are at same level,
				// lets force the other node to use full mesh at the edge that links to this node
				pCorrectNode.m_GapFixMask |= (byte)(1 << neighDirection);
				pCorrectNode.m_Neighbors[neighDirection].isFixed = false;
			}
		}
		else
		{
			// the other node has no children...
			// so, the other node is at a coarse level
			// OR at same level (a brother node);
			// link directly to that node
			m_Neighbors[(int)direction].node = pToNode;
			m_Neighbors[(int)direction].directionThere = directionFromThere;
			m_Neighbors[(int)direction].node.m_NeedsReedge = true;

			// only this node needs to update edges and fix gaps
			m_NeedsReedge = true;
			m_GapFixMask |= (byte)(1 << (int)direction);
			m_Neighbors[(int)direction].isFixed = false;

			// the other node stays linked to the node it is already linked to.
		}
	}


	// get a neighbor to this node
	public TNeighbor GetNeighbor(en_NeighborDirection direction)
	{
		return m_Neighbors[(int)direction];
	}


	// removes all children from a node
	private void Rejoin()
	{
		m_HasChildren = false;
		m_NeedsReedge = true;

		// relinks all children neighbors to point to this level
		// then delete children

		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Neighbors[(int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere].node = this;
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Neighbors[(int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere].isFixed = false;
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_GapFixMask |= (byte)(1 << (int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere);
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_NeedsReedge = true;

		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Neighbors[(int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere].node = this;
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Neighbors[(int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere].isFixed = false;
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_GapFixMask |= (byte)(1 << (int)m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere);
		m_Children[0].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_NeedsReedge = true;

		// ---

		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Neighbors[(int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere].node = this;
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_Neighbors[(int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere].isFixed = false;
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_GapFixMask |= (byte)(1 << (int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].directionThere);
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Top].node.m_NeedsReedge = true;

		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Neighbors[(int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere].node = this;
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Neighbors[(int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere].isFixed = false;
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_GapFixMask |= (byte)(1 << (int)m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere);
		m_Children[1].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_NeedsReedge = true;

		// ---

		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Neighbors[(int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere].node = this;
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Neighbors[(int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere].isFixed = false;
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_GapFixMask |= (byte)(1 << (int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere);
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_NeedsReedge = true;

		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Neighbors[(int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere].node = this;
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_Neighbors[(int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere].isFixed = false;
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_GapFixMask |= (byte)(1 << (int)m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].directionThere);
		m_Children[2].m_Neighbors[(int)en_NeighborDirection.NB_Right].node.m_NeedsReedge = true;

		// ---

		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Neighbors[(int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere].node = this;
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_Neighbors[(int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere].isFixed = false;
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_GapFixMask |= (byte)(1 << (int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].directionThere);
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Bottom].node.m_NeedsReedge = true;

		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Neighbors[(int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere].node = this;
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_Neighbors[(int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere].isFixed = false;
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_GapFixMask |= (byte)(1 << (int)m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].directionThere);
		m_Children[3].m_Neighbors[(int)en_NeighborDirection.NB_Left].node.m_NeedsReedge = true;

		// ---

		for(byte i=0; i<4; i++)
		{
			if (m_Planet.CloserNode == m_Children[i]) m_Planet.CloserNode = this;
			m_Children[i].DestroyTree();
			m_Children[i] = null;
		}

		if (m_Parent != null) m_Parent.Relink();
		m_Planet.Rejoined = true;
		m_NeedsTerrain = true;
	}


	// entityPos is inverse transform of the view pos
	void UpdateShaderParms(Vector3 entityPos)
	{
#if UNITY_EDITOR
			if (m_Planet.m_SunTransform == null) return;
#endif

		m_GameObj.renderer.sharedMaterial.SetFloat("_frame", Time.time * m_Planet.m_WaterSpeed);
		m_GameObj.renderer.sharedMaterial.SetVector("_sunDir", m_Planet.SunDir);
		m_GameObj.renderer.sharedMaterial.SetVector("_camPos", entityPos);
	}

}

#endif
