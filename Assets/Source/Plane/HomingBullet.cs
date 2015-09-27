using UnityEngine;
using System.Collections;

public class HomingBullet : MonoBehaviour {
	public GameObject trackedTarget;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(trackedTarget.transform.position+trackedTarget.transform.forward*1.0f);
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.velocity = transform.forward*80.0f;
	}
	void OnCollisionEnter(Collision c){
		PlanePilot pp = c.gameObject.GetComponent<PlanePilot>();
		pp.explode();
		Destroy(gameObject);
	}
}
