using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestingController : MonoBehaviour {

	float speed=10.0f;
	float rotationSpeed=90.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("up")){
			print ("up");
			transform.position += transform.forward*speed*Time.deltaTime;
		}
		if(Input.GetKey("down")){
			print ("down");
			transform.position -= transform.forward*speed*Time.deltaTime;
		}
		if(Input.GetKey("right")){
			transform.Rotate (Vector3.up*Time.deltaTime*rotationSpeed,Space.World);
		}
		if(Input.GetKey("left")){
			transform.Rotate (-Vector3.up*Time.deltaTime*rotationSpeed,Space.World);
		}
	}
}
