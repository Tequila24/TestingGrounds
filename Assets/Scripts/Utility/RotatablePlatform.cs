using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatablePlatform : MonoBehaviour {

	private Rigidbody thisBody;
	private float time = 0.0f;
	
	public float VelocityMultiplier = 0.0f;
	public float Radius = 10.0f;
	public float X_Multiplier = 0.0f;
	public float Y_Multiplier = 0.0f;

	private Vector3 initialPosition = Vector3.zero;

	void Start () {
		thisBody = gameObject.GetComponent<Rigidbody>();
		if (thisBody == null)
			thisBody = gameObject.AddComponent<Rigidbody>();

		thisBody.mass = 1000;
		initialPosition = this.transform.position;
	}
	

	void FixedUpdate () {
		time += 0.01f;

		Quaternion rotationAroudInitialPosition = Quaternion.AngleAxis(time * VelocityMultiplier, Vector3.up);
		Vector3 desiredPosition = initialPosition + rotationAroudInitialPosition * Vector3.forward * Radius;

		thisBody.velocity = (desiredPosition - this.transform.position);

		thisBody.angularVelocity = new Vector3(Mathf.Sin(time) * X_Multiplier, Mathf.Sin(time) * Y_Multiplier, Mathf.Sin(time) * X_Multiplier);
	}


}
