using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour {

	public float flySpeed = 0.5f;
	public GameObject defaultCam;
	public float accelerationAmount = 3f;
	public float accelerationRatio = 1f;
	public float slowDownRatio = 0.5f;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
		{
			flySpeed *= accelerationRatio;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
		{
			flySpeed /= accelerationRatio;
		}
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
		{
			flySpeed *= slowDownRatio;
		}
		if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
		{
			flySpeed /= slowDownRatio;
		}
		if (Input.GetAxis("Vertical") != 0)
		{
			transform.Translate(-defaultCam.transform.forward * flySpeed * Input.GetAxis("Vertical"));
		}
		if (Input.GetAxis("Horizontal") != 0)
		{
			transform.Translate(-defaultCam.transform.right * flySpeed * Input.GetAxis("Horizontal"));
		}
		if (Input.GetKey(KeyCode.E))
		{
			transform.Translate(defaultCam.transform.up * flySpeed*0.5f);
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			transform.Translate(-defaultCam.transform.up * flySpeed*0.5f);
		}
	}
}
