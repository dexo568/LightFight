using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlanePilot : MonoBehaviour {
	public int playerNum;
	public Camera player3rdCamera;
	public Camera player1stCamera;
	public Indicator checkpointIndicator1stPerson;
	public Rigidbody playerBullet;
	public Rigidbody playerHomingBullet;
	public GameObject otherPlane;
	private int homingAmmo = 0;
	public float speed = 30.0f;
	private int boost = 100;
	public BoostGauge boostGauge;
	public LapCounter lapCounter;
	private Vector3 lastCheckpoint;
	private Quaternion lastCheckpointRotation;
	private bool isFirstPerson = true;
	private int boxCount = 0;
	private int boostCount = 0;
	// Use this for initialization
	void Start () {
		player1stCamera.enabled=true;
		player3rdCamera.enabled=false;
		Screen.lockCursor = true;
		lastCheckpoint = transform.position;
		lastCheckpointRotation = transform.rotation;
		player3rdCamera.transform.position = transform.position - transform.forward;
		//Debug.Log("PlanePilot script added to " + gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		//Update booster
		//boostGauge.text = "Boost: " + boost + "%";
		boostGauge.updateBoostGauge(boost/100f);
		if(boostCount > 10) {
			boostCount = 0;
			boost++;
			boost = Mathf.Min(100, boost);
		}
		boostCount++;
		//Update Will's mysterious box code
		if (boxCount > 5) {
			boxCount = 0;
		}
		//Collision checks
		Collider[] hitColliders = Physics.OverlapSphere(transform.position+transform.forward*2, 1f);
		for(int i = 0; i < hitColliders.Length; i++){
			Collider curCollider = hitColliders[i];
			GameObject colliderParent = curCollider.gameObject;
			if (colliderParent.name.StartsWith("Checkpoint")){
				if(colliderParent.layer == playerNum+10){ //Checks that the checkpoint is this player's checkpoint -- eg. p1 has checkpoints in layer 11
					boost+=10;
					boost = Mathf.Min(100, boost);
					CheckpointUpdater checkpoint = colliderParent.GetComponent<CheckpointUpdater>();
					checkpoint.advanceCheckpoint();
					checkpointIndicator1stPerson.tracked = checkpoint.nextCheckpoint;
					lastCheckpoint = transform.position;
					lastCheckpointRotation = transform.rotation;
					if(checkpoint.nextCheckpoint.GetComponent<CheckpointUpdater>().isStartingCheckpoint){
						lapCounter.updateLaps();
					}
				}
			}else if (!colliderParent.name.StartsWith(""+playerNum)){
				explode();
				return;
			}
		}
		//"Light Scrape" checks
		bool isScraping = false;
		int otherPlayerNum = playerNum == 1 ? 2 : 1;
		hitColliders = Physics.OverlapSphere(transform.position, 3);
		for(int i = 0; i < hitColliders.Length; i++){
			Collider curCollider = hitColliders[i];
			GameObject colliderParent = curCollider.gameObject;
			if (colliderParent.name.StartsWith(""+otherPlayerNum)){
				boost += 5;
				boost = Mathf.Min (100, boost);
				Debug.Log ("Scraping!");
			}
		}
		//Camera perspective switch
		if(Input.GetButtonDown("CamSwitch"+playerNum)){
			if(isFirstPerson){
				player1stCamera.enabled=false;
				player3rdCamera.enabled=true;
			}else{
				player1stCamera.enabled=true;
				player3rdCamera.enabled=false;
			}
			isFirstPerson=!isFirstPerson;
		}
		//3rd person camera movement
		Vector3 moveCamTo = transform.position - transform.forward*10.0f + Vector3.up*5.0f;
		float bias=0.5f; // How quickly the camera moves positions, higher = slower
		player3rdCamera.transform.position = player3rdCamera.transform.position * bias + moveCamTo*(1.0f-bias);
		player3rdCamera.transform.LookAt(transform.position+transform.forward*30.0f);

		//Plane Control
		transform.Rotate(-5.0f*Input.GetAxis("Vertical"+playerNum),0.0f, -5.0f*Input.GetAxis("Horizontal"+playerNum));
		if(!isScraping){
			if(Input.GetButton("Boost"+playerNum) && boost > 0){
				speed = 50.0f;//20.0f;
				boost--;
			}else{
				speed = 30.0f;//10.0f;
			}
		}
		transform.position+=transform.forward*Time.deltaTime*speed;

		//Ribbon Collider Generation
		// TODO
		if(boxCount==0){
			//Debug.Log ("The global rotation eulers " + this.transform.rotation.eulerAngles.ToString());
			//Debug.Log ("The local rotation eulers " + this.transform.localRotation.eulerAngles.ToString());
			GameObject colliderSection = GameObject.CreatePrimitive(PrimitiveType.Cube);
			colliderSection.name = playerNum+"Ribbon";
			colliderSection.transform.position = this.transform.position;
			colliderSection.transform.localScale = new Vector3(2.0f,.1f,1f);
			colliderSection.transform.rotation.eulerAngles.Set(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y,this.transform.rotation.eulerAngles.z);
			//colliderSection.transform.localRotation = this.transform.localRotation;
			colliderSection.transform.up = transform.TransformDirection(Vector3.up);
			colliderSection.GetComponent<MeshRenderer>().enabled = false;
		}


		if (Input.GetButtonDown("Fire"+playerNum)){
				fire();
		}
		boxCount += 1;
	
	}
	void fire() {
		if(homingAmmo == 0){
			Rigidbody bullet = (Rigidbody) Instantiate(playerBullet, (transform.position+transform.forward*5), transform.rotation);
			bullet.gameObject.name = playerNum+"Bullet";
			bullet.gameObject.transform.GetChild(0).name = playerNum+"Bullet";
			bullet.velocity = transform.forward*(speed+80.0f);
		}else{
			Debug.Log("firing fancy bullet");
			Rigidbody homingBullet = (Rigidbody) Instantiate(playerHomingBullet, (transform.position+transform.forward*5), transform.rotation);
			HomingBullet hbScript = homingBullet.gameObject.GetComponent<HomingBullet>();
			hbScript.trackedTarget = otherPlane;
			homingBullet.gameObject.name = playerNum+"HomingBullet";
			homingAmmo--;
		}
	}

	public void explode(){
		transform.position = lastCheckpoint;
		transform.rotation = lastCheckpointRotation;
		player3rdCamera.transform.position = transform.position - transform.forward;
	}
}
