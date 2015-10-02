using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UIPlaneControl : MonoBehaviour {
	private Vector3 restLocation;
	// Use this for initialization
	void Start () {
		restLocation = transform.position;
		transform.position = transform.position - transform.forward * 100;
	}
	
	// Update is called once per frame
	void Update () {
		if((transform.position - restLocation).magnitude > 5){
			transform.position += transform.forward;
		}
	}
	public void continueMoving(){
		restLocation = new Vector3(999f,999f,999f);
	}
}
