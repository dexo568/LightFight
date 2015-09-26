//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CCameraOrbit : MonoBehaviour
{
	public CPlanet m_Planet;
	public float m_RotSpeed = 0.5f;
	public float m_RotDecay = 0.85f;

	public float m_Accel = 0.01f;
	public float m_AfterBurn = 5.0f;
	public float m_FrontSpeedDecay = 1.0f;
	public float m_SideSpeedDecay = 0.85f;

	public float m_MaxSpeed = 5.0f;

	// ---

	float frontSpeed = 0;
	float sideSpeed = 0;
	float rotXSpd = 0;
	float rotYSpd = 0;
	float rotZSpd = 0;


	Vector3 initialPos;
	Quaternion initialRot;
	Material dropObjMat;


	// list of all planets in the scene
//	CPlanet[] m_AllPlanets;


	void Start()
	{
		initialPos = gameObject.transform.position;
		initialRot = gameObject.transform.rotation;

		dropObjMat = (Material)Resources.Load("zSample/Materials/temp");

//		m_AllPlanets = GameObject.FindObjectsOfType(typeof(CPlanet)) as CPlanet[];
	}


	void LateUpdate()
	{
		//
		// rotations
		//

		float dx = Input.GetAxis("Mouse X");
		float dy = Input.GetAxis("Mouse Y");

		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
		{
			rotYSpd += dx * m_RotSpeed;
			rotXSpd += dy * m_RotSpeed;
		}
		else
		{
			rotZSpd += -dx * m_RotSpeed;
			rotXSpd += dy * m_RotSpeed;
		}

		Quaternion q = Quaternion.Euler(new Vector3(rotXSpd, rotYSpd, rotZSpd));
		gameObject.transform.localRotation *= q;

		rotXSpd *= m_RotDecay;
		rotYSpd *= m_RotDecay;
		rotZSpd *= m_RotDecay;

		//
		// acceleration forward/backward
		//

		float frontAccel = m_Accel + (m_Accel*0.1f) * Mathf.Abs(frontSpeed);
		float sideAccel = m_Accel + (m_Accel * 0.1f) * Mathf.Abs(sideSpeed);

		// turbo acceleration

		if (Input.GetKey(KeyCode.LeftShift))
		{
			frontAccel *= m_AfterBurn;
			sideAccel *= m_AfterBurn;
		}

		// move forward/backward

		if (Input.GetKey(KeyCode.W))
		{
			frontSpeed += frontAccel;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			frontSpeed -= frontAccel;
		}
		else
		{
			frontSpeed *= m_FrontSpeedDecay;
		}

		if (frontSpeed > m_MaxSpeed) frontSpeed = m_MaxSpeed;
		if (frontSpeed < -m_MaxSpeed) frontSpeed = -m_MaxSpeed;

		// slide left/right

		if (Input.GetKey(KeyCode.E))
		{
			sideSpeed += sideAccel;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			sideSpeed -= sideAccel;
		}
		else
		{
			sideSpeed *= m_SideSpeedDecay;
		}

		// update positions
		Vector3 force = gameObject.transform.forward * frontSpeed + gameObject.transform.right * sideSpeed;
		gameObject.transform.position += force;

		// return to origin, stop immediately, etc

		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			this.transform.position = initialPos;
			this.transform.rotation = initialRot;
			frontSpeed = 0;
		}

		if (Input.GetKey(KeyCode.Space))
		{
			frontSpeed *= 0.95f;
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			// drop a primitive at the place
			GameObject obj;

			float r = Random.value;
			if (r < 0.25f)
			{
				obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			}
			else if (r < 0.5f)
			{
				obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			}
			else if (r < 0.75f)
			{
				obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			}
			else
			{
				obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			}

			obj.transform.position = this.transform.position + 2 * this.transform.forward;
			obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			obj.GetComponent<Renderer>().castShadows = true;
			obj.GetComponent<Renderer>().receiveShadows = true;
			obj.GetComponent<Renderer>().sharedMaterial = dropObjMat;

			Rigidbody body = (Rigidbody)obj.AddComponent<Rigidbody>();
			body.isKinematic = false;
			body.useGravity = false;
			body.mass = 1.0f;
			body.drag = 0.1f;
			body.angularDrag = 1.0f;
			body.collisionDetectionMode = CollisionDetectionMode.Discrete;

			DropObjAI ai = (DropObjAI)obj.AddComponent<DropObjAI>();
			ai.m_Planet = m_Planet;
		}

	}


	void OnGUI()
	{
		GUILayout.Label("\nSpeed: " + frontSpeed);
		//foreach (CPlanet planet in m_AllPlanets)
		//{
		//    GUILayout.Label("planet " + planet.HighestSplitLevel);
		//}
	}
}
