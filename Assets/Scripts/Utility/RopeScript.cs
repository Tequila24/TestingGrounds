using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeScript : MonoBehaviour
{

	[SerializeField]
	private Vector3 firstPoint = Vector3.zero;
	//private GameObject firstObject = null;
	[SerializeField]
	private Rigidbody firstBody = null;
	[SerializeField]
	private Vector3 secondPoint = Vector3.zero;
	//private GameObject secondObject = null;
	[SerializeField]
	private Rigidbody secondBody = null;

	private int numberOfRigids = 2;
	[SerializeField]
	[Range(1.0f, 10.0f)]
	private float ropeLength = 10.0f;
	[SerializeField]
	[Range(0.01f, 200.0f)]
	private float Young_Modulus = 200.0f;
	[SerializeField]
	[Range(0.0001f, 1.0f)]
	private float Rope_area = 0.001f;

	[SerializeField]
	private float RopeStretching = 0.0f;

	private LineRenderer lineRender = null;





	void Start()
	{
		lineRender = this.gameObject.AddComponent<LineRenderer>();
		lineRender.positionCount = 2;
		lineRender.startWidth = lineRender.endWidth = 0.15f;
		lineRender.material = new Material(Shader.Find("Sprites/Default"));
		lineRender.startColor = lineRender.endColor = Color.black;
	}

	void SetPoint(Vector3 newPoint, GameObject newObject)
	{
		if (firstPoint.magnitude == Mathf.Infinity) {
			firstPoint = newPoint;
			firstBody = newObject.GetComponent<Rigidbody>();
			if (firstBody != null)
				numberOfRigids++;
			
			return;
		}
		if (secondPoint.magnitude == Mathf.Infinity) {
			secondPoint = newPoint;
			secondBody = newObject.GetComponent<Rigidbody>();
			if (secondBody != null)
				numberOfRigids++;

			ropeLength = (secondBody.transform.position - firstBody.transform.position).magnitude;
			return;
		}
	}

	void FixedUpdate() 
	{
		// yay physics
		if (!ArePointsSet())
			return;

		Vector3 firstContactPointWorldPosition = firstBody.transform.position + firstBody.transform.rotation * firstPoint;
		Vector3 secondContactPointWorldPosition = secondBody.transform.position + secondBody.transform.rotation * secondPoint;
		Vector3 fromFirstToSecond = (secondContactPointWorldPosition - firstContactPointWorldPosition).normalized;

		float distanceBetween = (firstContactPointWorldPosition - secondContactPointWorldPosition).magnitude;
		float distanceInNextFrame = ((firstContactPointWorldPosition + firstBody.velocity * Time.deltaTime) - (secondContactPointWorldPosition + secondBody.velocity * Time.deltaTime)).magnitude;

		RopeStretching = Mathf.Round((distanceBetween / ropeLength) * 100);

		if (distanceInNextFrame > ropeLength) { 
			
			// ======================== INTEGRATING HOOKE'S LAW ======================== 

			int steps = 10;

			//float deltaT = Time.deltaTime / steps;

			// 
			float deltaT = Time.deltaTime / Mathf.Pow(2, steps);

			Vector3 firstPositionDelta = Vector3.zero;
			Vector3 secondPositionDelta = Vector3.zero;

			// yay intergration
			for (int step = 0; step < steps; step++) {

				Vector3 newFirstPointPosition = firstBody.transform.position + firstPositionDelta + firstBody.transform.rotation * firstPoint;
				Vector3 newSecondPointPosition = secondBody.transform.position + secondPositionDelta + secondBody.transform.rotation * secondPoint;
				Vector3 newFromFirstToSecond = (newSecondPointPosition - newFirstPointPosition).normalized;
				
				float newDistanceBetween = (newFirstPointPosition - newSecondPointPosition).magnitude;
				float deltaLength = (newDistanceBetween - ropeLength);
				float k_coeff = ((Young_Modulus * Mathf.Pow(10, 9)) * Rope_area) / (ropeLength);
				float pullForceMagnitude = 0;
				if (deltaLength > 0) {
					
					//first body
					pullForceMagnitude = k_coeff * deltaLength * Mathf.Clamp01(firstBody.mass / Mathf.Pow(10,5));		// mass dependency hack
					Vector3 firstPullForce = newFromFirstToSecond * pullForceMagnitude * 0.5f;
					Vector3 firstAcceleration = firstPullForce / firstBody.mass;
					firstAcceleration += Physics.gravity * Time.deltaTime;
					firstBody.AddForceAtPosition(firstAcceleration *  Mathf.Pow(deltaT, 1), newFirstPointPosition, ForceMode.Acceleration);

					// second body
					pullForceMagnitude = k_coeff * deltaLength * Mathf.Clamp01(secondBody.mass / Mathf.Pow(10,5));		// mass dependency hack
					Vector3 secondPullForce = -newFromFirstToSecond * pullForceMagnitude * 0.5f;
					Vector3 secondAcceleration = secondPullForce / secondBody.mass;
					secondAcceleration += Physics.gravity * Time.deltaTime;
					secondBody.AddForceAtPosition(secondAcceleration *  Mathf.Pow(deltaT, 1), newSecondPointPosition, ForceMode.Acceleration);
				}
				firstPositionDelta += firstBody.velocity * deltaT;
				secondPositionDelta += secondBody.velocity * deltaT;

				deltaT = deltaT*2;	
			}
			// ======================== ======================== ======================== 
			

			// ==================== VELOCITY DAMPING ====================
			/*{
				Vector3 dampingVelocity;
				float Angle;
				dampingVelocity = Vector3.Project(firstBody.GetPointVelocity(firstContactPointWorldPosition), fromFirstToSecond);
				Angle = Vector3.Angle(dampingVelocity, fromFirstToSecond);
				if (Angle > 90) {
					Debug.DrawLine(firstContactPointWorldPosition, firstContactPointWorldPosition + dampingVelocity, Color.red);
					firstBody.AddForceAtPosition(-dampingVelocity * 0.9f * Time.deltaTime, firstContactPointWorldPosition, ForceMode.VelocityChange);
				}
				dampingVelocity = Vector3.Project(firstBody.GetPointVelocity(secondContactPointWorldPosition), -fromFirstToSecond);
				Angle = Vector3.Angle(dampingVelocity, -fromFirstToSecond);
				if (Angle > 90) {
					Debug.DrawLine(secondContactPointWorldPosition, secondContactPointWorldPosition + dampingVelocity, Color.red);
					secondBody.AddForceAtPosition(-dampingVelocity * 0.9f * Time.deltaTime, secondContactPointWorldPosition, ForceMode.VelocityChange);
				}
			}*/
			// ==================== VELOCITY DAMPING ====================
		}
	}

	void Update()
	{
		lineRender.SetPosition(0, firstBody.transform.position + firstBody.transform.rotation * firstPoint);
		lineRender.SetPosition(1, secondBody.transform.position + secondBody.transform.rotation * secondPoint);
	}

	private bool ArePointsSet() {
		if (	(firstPoint.magnitude == Mathf.Infinity) ||
				(secondPoint.magnitude == Mathf.Infinity)	) 
		{
			Debug.Log("points not set!");
			return false;
		} else {
			return true;
		}
	}


}
