using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSurfaceControl : MonoBehaviour
{
    private float rayLength = 1.5f;
	private Vector3 gravityDirection;
    
	public GameObject surfaceObject;
	public Vector3 contactPoint;
	public Vector3 contactPointNormal;
	public Vector3 contactPointVelocity;
	public Vector3 angularVelocity;
	public float angleToGravity;


    private void Start() 
	{
		gravityDirection = Physics.gravity.normalized * -1;
    }

	void FixedUpdate()
	{
		CheckSurface();
	}

    private void CastRay() 
	{
        Debug.DrawRay(this.transform.position, -Vector3.up * rayLength, Color.black, Time.deltaTime);

		RaycastHit surfaceRay;
		if (Physics.Raycast(this.transform.position, -Vector3.up, out surfaceRay, rayLength)) {
			// ray hit something!
			GetSurfaceInfo(surfaceRay);
		} else {
			// ray did not hit anything 
			ResetSurface();
		}
    }

	private void GetSurfaceInfo(RaycastHit surfaceRayHit) {

		surfaceObject = surfaceRayHit.transform.gameObject;
		contactPoint = surfaceRayHit.point;
		contactPointNormal = surfaceRayHit.normal;

		// if surface have rigid body - get its velocities
		Rigidbody surfaceBody = surfaceObject.GetComponent<Rigidbody>();
		if (surfaceBody != null) {
			contactPointVelocity = surfaceBody.GetPointVelocity(surfaceRayHit.point);
			if (surfaceBody.useGravity) {
				contactPointVelocity += Physics.gravity * Time.deltaTime;
			}
			angularVelocity = surfaceBody.angularVelocity;
		} else {
			contactPointVelocity = Vector3.zero;
			angularVelocity = Vector3.zero;
		}

		// calculate angle between gravity direction and surface normal
		float angle = Vector3.Angle(gravityDirection, contactPointNormal);
		angleToGravity = angle;
	}

	private void ResetSurface()
	{
		surfaceObject = null;
		contactPoint = Vector3.zero;
		contactPointNormal = Vector3.zero;
		contactPointVelocity = Vector3.zero;
		angularVelocity = Vector3.zero;
		angleToGravity = 0;
	}


    public void CheckSurface()
	{
		CastRay();
	}
    
}
