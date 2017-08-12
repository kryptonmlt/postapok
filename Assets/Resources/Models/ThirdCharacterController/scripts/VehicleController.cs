using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour,MovingObject {

	void Start()
	{
	}

	public string getName(){
		return this.name;
	}

	public void Move(Vector3 move, bool crouch, bool jump)
	{
		transform.position += move;
	}
}
