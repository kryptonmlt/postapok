using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour,MovingObject {
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
	[SerializeField] float m_MoveSpeedMultiplier = 1f;

	Rigidbody m_Rigidbody;
	const float k_Half = 0.5f;
	float m_TurnAmount;
	float m_ForwardAmount;

	void Start()
	{
		m_Rigidbody = GetComponent<Rigidbody>();

		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
	}

	public string getName(){
		return this.name;
	}


	public void Move(Vector3 move, bool crouch, bool jump)
	{

		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction.
		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane(move, new Vector3(0f,1f,0f));
		m_TurnAmount = Mathf.Atan2(move.x, move.z);
		m_ForwardAmount = move.z;

		ApplyExtraTurnRotation();
		// send input and other state parameters to the animator
		m_Rigidbody.transform.Translate(move * m_MoveSpeedMultiplier * Time.deltaTime);
		m_Rigidbody.transform.RotateAround (m_Rigidbody.centerOfMass,new Vector3(0f,1f,0f),m_TurnAmount*Time.deltaTime);
	}

	void ApplyExtraTurnRotation()
	{
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
		transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
	}
}
