using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour,MovingObject {

	Rigidbody m_Rigidbody;

	void Start()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		//m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
	}

	public string getName(){
		return this.name;
	}

	public void Move(Vector3 move, bool crouch, bool jump)
	{
		transform.position += move;
	}
}
