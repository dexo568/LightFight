//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;

public static class CMath
{
	public static Vector3 ProjectToSphere(Vector3 v, float radius)
	{
		Vector3 vr = v;
		vr.Normalize();
		vr.x *= radius;
		vr.y *= radius;
		vr.z *= radius;
		return vr;
	}


	public static Vector3 TriNormal(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Vector3 vt1;
		vt1.x = v2.x - v1.x;
		vt1.y = v2.y - v1.y;
		vt1.z = v2.z - v1.z;

		Vector3 vt2;
		vt2.x = v3.x - v2.x;
		vt2.y = v3.y - v2.y;
		vt2.z = v3.z - v2.z;

		Vector3 normal = Vector3.Cross(vt1, vt2);
		normal.Normalize();

		return normal;
	}


	public static float QuickDistance(Vector3 v1, Vector3 v2)
	{
		Vector3 diff = v2 - v1;
		return diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
	}
}
