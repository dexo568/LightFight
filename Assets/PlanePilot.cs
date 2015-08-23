using UnityEngine;
using System.Collections;

public class PlanePilot : MonoBehaviour {
	public int playerNum;
	public Camera playerCamera;
	public Rigidbody playerBullet;
	public float speed = 10.0f;
	private Vector3 lastCheckpoint;
	private Quaternion lastCheckpointRotation;
	private int cooldown = 15;
	// Use this for initialization
	void Start () {
		lastCheckpoint = transform.position;
		lastCheckpointRotation = transform.rotation;
		playerCamera.transform.position = transform.position - transform.forward;
		Debug.Log("PlanePilot script added to " + gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		if (cooldown > 0) {
			cooldown--;
		}
		if (Physics.CheckSphere(transform.position+(transform.forward*2.0f), .1f)){
			Collider[] hitColliders = Physics.OverlapSphere(transform.position+transform.forward*2.0f, .1f);
			for(int i = 0; i < hitColliders.Length; i++){
				Collider curCollider = hitColliders[i];
				GameObject colliderParent = curCollider.gameObject;
				if (colliderParent.name.StartsWith("Checkpoint")){
					CheckpointUpdater checkpoint = colliderParent.GetComponent<CheckpointUpdater>();
					checkpoint.advanceCheckpoint();
					lastCheckpoint = colliderParent.transform.position;
					lastCheckpointRotation = colliderParent.transform.rotation;
				}else if (!colliderParent.name.StartsWith(""+playerNum)){
					Debug.Log (gameObject.name + " collided with " +colliderParent.name);
					transform.position = lastCheckpoint;
					transform.rotation = lastCheckpointRotation;
					playerCamera.transform.position = transform.position - transform.forward;
					return;
				}
			}
		}
		Vector3 moveCamTo = transform.position - transform.forward *10.0f + Vector3.up*5.0f;
		float bias=0.96f;
		playerCamera.transform.position = playerCamera.transform.position * bias + moveCamTo*(1.0f-bias);
		playerCamera.transform.LookAt(transform.position+transform.forward*30.0f);

		//this was my original movement code
		transform.Rotate(-5.0f*Input.GetAxis("Vertical"+playerNum),0.0f, -5.0f*Input.GetAxis("Horizontal"+playerNum));
		if(Input.GetButton("Boost"+playerNum)){
			speed = 20.0f;
		}else{
			speed = 10.0f;
		}
		transform.position+=transform.forward*Time.deltaTime*speed;

//		//Add force from jet engine , set enginePower to 15000
//		Rigidbody rb = this.GetComponent<Rigidbody>();
//		if (Input.GetKey (KeyCode.Space)) {
//			rb.AddForce (transform.forward);
//		}
//		
//		//Add lift force ,  set liftBooster to 100 
//		Vector3 lift = Vector3.Project (rb.velocity, transform.forward);
//		rb.AddForce (transform.up * lift.magnitude);
//		
//		//Banking controls, turning turning left and right on Z axis
//		rb.AddTorque (Input.GetAxis ("Horizontal2") * transform.forward * -1f);
//		
//		//Pitch controls, turning the nose up and down
//		rb.AddTorque (Input.GetAxis ("Vertical2") * transform.right );
//		
//		//Set drag and angular drag according relative to speed
//		rb.drag = 0.001f * rb.velocity.magnitude;
//		rb.angularDrag = 0.01f * rb.velocity.magnitude;


		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if(terrainHeightWhereWeAre>transform.position.y){
			transform.position = new Vector3(transform.position.x,terrainHeightWhereWeAre,transform.position.z);
		}
		GameObject colliderSection = GameObject.CreatePrimitive(PrimitiveType.Cube);
		colliderSection.name = playerNum+"Ribbon";
		colliderSection.transform.position = this.transform.position;
		colliderSection.transform.localScale = new Vector3(1.9f,.001f,1f);
		colliderSection.transform.localRotation = this.transform.localRotation;
		colliderSection.transform.up = transform.TransformDirection(Vector3.up);
		colliderSection.GetComponent<MeshRenderer>().enabled = false;
		if (Input.GetButton("Fire"+playerNum) && cooldown == 0){
				fire();
				cooldown = 15;
		}
	}
	void fire() {
		Rigidbody bullet = (Rigidbody) Instantiate(playerBullet, (transform.position+transform.forward*5), transform.rotation);
		bullet.gameObject.name = playerNum+"Bullet";
		bullet.gameObject.transform.GetChild(0).name = playerNum+"Bullet";
		bullet.velocity = transform.forward*(speed+40.0f);
	}
}
