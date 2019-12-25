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
    private float dampingValue = 1;
    [SerializeField]
    private float springValue = 1;
    [SerializeField]
    private Vector3 suspensionAxis = Vector3.zero;

    private Rigidbody carBody = null;
    private Rigidbody wheelBody = null;

    private List<Vector3> raycastVectors = new List<Vector3>();

    void Awake()
    {
        carBody = this.transform.parent.GetComponent<Rigidbody>();
        wheelBody = this.gameObject.AddComponent<Rigidbody>();
        wheelBody.mass = wheelMass;

        GenerateRaycastVectors();
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
        List<RaycastHit> rayHits = new List<RaycastHit>();

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
        }
    }


    void ApplyForces()
    {
        float wheelOffset = 0;

        //Vector3 springForce = wheelOffset * springValue;

        //Vector3 carBodyVelocityAtPoint = carBody.GetPointVelocity(this.transform.position - suspensionAxis) - this.transform.;
        //Vector3 dampingVector = carBodyVelocityAtPoint * dampingValue;

        //Vector3 forceVector = suspensionVector - dampingVector;

        //carBody.AddForceAtPosition( forceVector, this.transform.position - suspensionAxis);
    }


    void OnDrawGizmos()
    {
        Handles.color = Color.green;

        foreach (Vector3 vec in raycastVectors)
        {
            Handles.DrawLine(this.transform.position, this.transform.position + vec);
        }


        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.transform.position + suspensionAxis - this.transform.forward*0.25f,
                        this.transform.position + suspensionAxis + this.transform.forward*0.25f );
    }
}
