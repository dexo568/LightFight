//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;

public class DropObjAI : MonoBehaviour
{
	public CPlanet m_Planet;
	Rigidbody m_ActorRigidBody;
	int m_Collisions = 0;

	const float m_MaxJetPower = 50;
	float m_JetPower = m_MaxJetPower;

	public float MaxJetPower { get { return m_MaxJetPower; } }
	public float JetPower { get { return m_JetPower; } }


	void Start()
	{
		m_ActorRigidBody = gameObject.GetComponent<Rigidbody>();
	}


	void FixedUpdate()
	{
		// add gravity in planet's direction
		Vector3 gravDir = m_Planet.transform.position - m_ActorRigidBody.transform.position;
		gravDir.Normalize();
		gravDir *= m_Planet.m_GravityPower;
		m_ActorRigidBody.AddForce(gravDir, ForceMode.Acceleration);

		float accel;
		float rotAccel;

		if (m_Collisions > 0)
		{
			// ground
			accel = 50;
			rotAccel = 150;
		}
		else
		{
			// air
			accel = 5;
			rotAccel = 70;
		}

		if (Input.GetKey(KeyCode.UpArrow))
		{
			m_ActorRigidBody.AddForce(transform.forward * accel, ForceMode.Acceleration);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			m_ActorRigidBody.AddForce(-transform.forward * accel, ForceMode.Acceleration);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			m_ActorRigidBody.AddTorque(-transform.up * rotAccel, ForceMode.Acceleration);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			m_ActorRigidBody.AddTorque(transform.up * rotAccel, ForceMode.Acceleration);
		}

		if (Input.GetKey(KeyCode.RightControl))
		{
			m_ActorRigidBody.AddForce(-transform.up * m_JetPower, ForceMode.Acceleration);
		}
		else if (Input.GetKey(KeyCode.RightShift))
		{
			m_ActorRigidBody.AddForce(transform.up * m_JetPower, ForceMode.Acceleration);
		}
		else
		{
			if (m_JetPower < m_MaxJetPower) m_JetPower += 0.1f;
		}

		//if (m_Collisions == 0)
		//{
		//    // stabilize player to up
		//    Vector3 euler = m_ActorRigidBody.transform.rotation.eulerAngles;
		//    euler = new Vector3(euler.x * -gravDir.x, euler.y * -gravDir.y, euler.z * -gravDir.z);
		//    Quaternion qn = Quaternion.Euler(euler);
		//    m_ActorRigidBody.transform.rotation = Quaternion.Lerp(m_ActorRigidBody.transform.rotation, qn, 0.1f);
		//}
	}


	void OnCollisionEnter(Collision collision)
	{
		m_Collisions++;
	}


	void OnCollisionExit(Collision collision)
	{
		m_Collisions--;
	}


	//void OnGUI()
	//{
	//    GUI.Label(new Rect(0, 0, 32, 32), m_Collisions.ToString());
	//}
}
