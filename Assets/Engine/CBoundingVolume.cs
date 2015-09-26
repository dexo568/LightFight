//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CBoundingVolume
{
	public List<Vector3> vertices = new List<Vector3>(4);
	public List<Vector2> uvs = new List<Vector2>(4);

	// extrapolated vertices and uvs for normalmapping
	public List<Vector3> exvertices = new List<Vector3>(4);
}
