using UnityEngine;
using System.Collections;

public class CheckpointUpdater : MonoBehaviour {
	public GameObject nextCheckpoint;
	public bool isStartingCheckpoint = false;
	public GameObject triggerObject;
	// Use this for initialization
	void Start () {
		if(!isStartingCheckpoint) {
			this.GetComponent<MeshRenderer>().enabled = false;
			this.GetComponent<Collider>().enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void advanceCheckpoint() {
		this.GetComponent<MeshRenderer>().enabled = false;
		this.GetComponent<Collider>().enabled = false;
		nextCheckpoint.GetComponent<MeshRenderer>().enabled = true;
		nextCheckpoint.GetComponent<Collider>().enabled = true;
		if(triggerObject != null){
			triggerObject.GetComponent<Trigger>().trigger();
		}
	}
}
