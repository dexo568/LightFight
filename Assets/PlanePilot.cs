using UnityEngine;
using System.Collections;

public class PlanePilot : MonoBehaviour {
	public float speed = 5.0f;
	// Use this for initialization
	void Start () {
		Debug.Log("PlanePilot script added to " + gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		if (Physics.CheckSphere(transform.position+(transform.forward*2.0f),0.0f)){
			return;
		}
		Vector3 moveCamTo = transform.position - transform.forward *10.0f + Vector3.up*5.0f;
		float bias=0.96f;
		Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo*(1.0f-bias);
		Camera.main.transform.LookAt(transform.position+transform.forward*30.0f);
		transform.Rotate(-5.0f*Input.GetAxis("Vertical"),0.0f, -5.0f*Input.GetAxis("Horizontal"));
		transform.position+=transform.forward*Time.deltaTime*speed;
		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if(terrainHeightWhereWeAre>transform.position.y){
			transform.position = new Vector3(transform.position.x,terrainHeightWhereWeAre,transform.position.z);
		}
	}
}
