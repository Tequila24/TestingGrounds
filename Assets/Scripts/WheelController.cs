using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WheelController : MonoBehaviour
{

    [SerializeField]
    [Range (4,32)]
    uint radialAccuracy = 4;
    [SerializeField]
    [Range (2,32)]
    uint widthAccuracy = 2;

    [SerializeField]
    private float wheelRadius = 1;
    [SerializeField]
    private float wheelWidth = 1;
    [SerializeField]
    private float wheelMass = 1;
    [SerializeField]
    [Range (0,1)]
    private float dampingValue = 0.5f;
    [SerializeField]
    private float springValue = 1;



    private Vector3 localRestPosition = Vector3.zero;
    private Vector3 suspensionJointRelativeToCar = Vector3.zero;



    [SerializeField]
    private Rigidbody carBody = null;
    private Rigidbody wheelBody = null;

    private List<Vector3> raycastVectors = new List<Vector3>();

    void Awake()
    {
        //carBody = this.transform.parent.GetComponent<Rigidbody>();
        wheelBody = this.gameObject.GetComponent<Rigidbody>();
        if (wheelBody == null) {
            wheelBody = this.gameObject.AddComponent<Rigidbody>();
        }
        
        wheelBody.mass = wheelMass;
        localRestPosition = wheelBody.transform.position - carBody.transform.position;
 
        //GenerateRaycastVectors();
    }

    void GenerateRaycastVectors()
    {
        wheelWidth = this.gameObject.GetComponent<MeshRenderer>().bounds.extents.x;
        wheelRadius = this.gameObject.GetComponent<MeshRenderer>().bounds.extents.z;

        for (float j = -wheelWidth; j <= wheelWidth; j += (wheelWidth*2)/widthAccuracy)
        {
            

            for (int i = 0; i < radialAccuracy; i++)
            {
                Quaternion rotation = Quaternion.AngleAxis(360/radialAccuracy * i, this.transform.right);
                Vector3 vec = rotation *    (this.transform.up * wheelRadius) + 
                                            (new Vector3(j, 0, 0));

                raycastVectors.Add(vec);
            }
        }

        Debug.Log(raycastVectors.Count);
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        CastRays();

        ApplyForces();
    }


    void CastRays()
    {
        /*List<RaycastHit> rayHits = new List<RaycastHit>();

        foreach (Vector3 vector in raycastVectors)
        {
            RaycastHit hit;
            if ( Physics.Raycast(this.transform.position, vector, out hit, wheelRadius) ) 
            {
                rayHits.Add(hit);
            }
        }

        foreach (RaycastHit hit in rayHits)
        {
            Vector3 pointVelocity = wheelBody.GetPointVelocity(hit.point);
            float cosAngle = Mathf.Cos(Vector3.Angle(pointVelocity, hit.normal) * Mathf.Deg2Rad);
            Debug.Log(cosAngle);
            Vector3 normalForce = wheelMass * pointVelocity * cosAngle;

            wheelBody.AddForceAtPosition(normalForce, this.transform.position);

            Debug.DrawRay(hit.point, normalForce, Color.red, 10);
        }*/
    }


    void ApplyForces()
    {
        CalculateSuspension();
    }


    void CalculateSuspension()
    {
        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 wheelOffset = (localWheelPos - carBody.rotation * localRestPosition);

        // do not let wheels move anywhere except axis
        Vector3 horizontalOffset = Vector3.ProjectOnPlane(wheelOffset, carBody.transform.up);
        wheelBody.transform.position -= horizontalOffset;

        // spring force
        Vector3 verticalOffset = wheelOffset - horizontalOffset;
        Vector3 springForce = verticalOffset * springValue;
        wheelBody.AddForce(-springForce * 0.5f);
        carBody.AddForceAtPosition(springForce * 0.5f, carBody.position + carBody.rotation * localRestPosition);

        // damping force
        Vector3 carPointVelocity = carBody.GetRelativePointVelocity(localRestPosition);
        Vector3 wheelVelocity = wheelBody.velocity;
        Vector3 relativeVelocity = wheelVelocity - carPointVelocity;
        Vector3 dampingAcceleration = relativeVelocity * dampingValue;
        wheelBody.AddForce(-dampingAcceleration * 0.5f, ForceMode.Impulse);
        carBody.AddForceAtPosition(dampingAcceleration * 0.5f, carBody.position + carBody.rotation * localRestPosition, ForceMode.Impulse);



        Debug.DrawRay(wheelBody.position, relativeVelocity, Color.blue, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPosition, -springForce, Color.red, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPosition, horizontalOffset, Color.yellow, Time.deltaTime);
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.green;

        /*foreach (Vector3 vec in raycastVectors)
        {
            Handles.DrawLine(this.transform.position, this.transform.position + vec);
        }*/


        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);
    }
}
