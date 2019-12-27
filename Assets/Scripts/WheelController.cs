using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelController : MonoBehaviour
{

    // killme
    bool inited = false;


    [SerializeField]
    private float wheelRadius = 1;
    [SerializeField]
    private float wheelMass = 1;
    [SerializeField]
    [Range (0,1)]
    private float dampingValue = 0.5f;
    [SerializeField]
    private float springValue = 1;


    [SerializeField]
    [Range (0,10)]
    private float StrutToTop = 0;    
    [SerializeField]
    [Range (-10,0)]
    private float StrutToBottom = 0;
    [SerializeField]
    private float CasterAngle = 0;
    [SerializeField]
    private float CamberAngle = 0;



    private Vector3 strutTopPoint = Vector3.zero;
    private Vector3 strutLowPoint = Vector3.zero;

    private Vector3 localRestPosition = Vector3.zero;



    [SerializeField]
    private Rigidbody carBody = null;
    private Rigidbody wheelBody = null;    

    
    void Awake()
    {
        Init();
    }

    void Init()
    {
        //carBody = this.transform.parent.GetComponent<Rigidbody>();
        wheelBody = this.gameObject.GetComponent<Rigidbody>();
        if (wheelBody == null) {
            wheelBody = this.gameObject.AddComponent<Rigidbody>();
        }
        

        wheelBody.mass = wheelMass;
        localRestPosition = wheelBody.position - carBody.position;


        strutTopPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                        Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                        (this.transform.up * StrutToTop) + localRestPosition;
        strutLowPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                        Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                        (this.transform.up * StrutToBottom) + localRestPosition;


        
        inited = true;
    }


    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyConstraints();
    }


    void ApplyConstraints()
    {
        Vector3 wheelVerticalOffset = Vector3.zero;
        Vector3 wheelHorizontalOffset = Vector3.zero;


        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 wheelOffset = (localWheelPos - carBody.rotation * localRestPosition);
        wheelHorizontalOffset = Vector3.ProjectOnPlane(wheelOffset, carBody.rotation * (strutTopPoint - strutLowPoint) );
        wheelBody.position -= wheelHorizontalOffset;

        Vector3 relativeWheelVelocity = wheelBody.velocity - carBody.GetRelativePointVelocity(carBody.position + carBody.rotation * localRestPosition);
        Vector3 horizontalPartVelocity = Vector3.ProjectOnPlane(relativeWheelVelocity, carBody.rotation * (strutTopPoint - strutLowPoint) );
        Vector3 horizontalPartGravity = Vector3.ProjectOnPlane(Physics.gravity, carBody.rotation * (strutTopPoint - strutLowPoint) );
        wheelBody.AddForce(-horizontalPartGravity);
        wheelBody.velocity -= horizontalPartVelocity * Time.deltaTime;
        



        Vector3 wheelPositionOnStrut =  Quaternion.FromToRotation( carBody.rotation * (strutTopPoint - strutLowPoint), Vector3.up) * 
                                        wheelOffset - wheelHorizontalOffset;

        if (wheelPositionOnStrut.y <= StrutToBottom) 
        {
            Vector3 wheelLocalPosition = wheelBody.position - carBody.position;
            wheelVerticalOffset = wheelLocalPosition - carBody.rotation * strutLowPoint;
            wheelBody.position -= wheelVerticalOffset;

            Vector3 verticalPartVelocity = relativeWheelVelocity - horizontalPartVelocity;
            wheelBody.velocity -= verticalPartVelocity * Time.deltaTime;

            Vector3 verticalPartGravity = Vector3.Project(Physics.gravity, carBody.rotation * (strutTopPoint - strutLowPoint));
            wheelBody.AddForce(-verticalPartGravity);


            Debug.DrawRay(carBody.position + carBody.rotation * strutLowPoint, wheelVerticalOffset, Color.magenta, Time.deltaTime);
        }

        if (wheelPositionOnStrut.y >= StrutToTop) 
        {
            Vector3 wheelLocalPosition = wheelBody.position - carBody.position;
            wheelVerticalOffset = wheelLocalPosition - carBody.rotation * strutTopPoint;
            wheelBody.position -= wheelVerticalOffset;

            Vector3 verticalPartVelocity = relativeWheelVelocity - horizontalPartVelocity;
            wheelBody.velocity -= verticalPartVelocity * Time.deltaTime;

            Vector3 verticalPartGravity = Vector3.Project(Physics.gravity, carBody.rotation * (strutTopPoint - strutLowPoint));
            wheelBody.AddForce(-verticalPartGravity);


            Debug.DrawRay(carBody.position + carBody.rotation * strutTopPoint, wheelVerticalOffset, Color.magenta, Time.deltaTime);
        }

        
        
        
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPosition, wheelHorizontalOffset, Color.blue, Time.deltaTime);
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.green;

        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);


        if (inited)
            Handles.DrawLine(   carBody.position + carBody.rotation * strutTopPoint,
                                carBody.position + carBody.rotation * strutLowPoint );




        if (!inited) {

            localRestPosition = this.transform.position - carBody.position;

            strutTopPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToTop) + localRestPosition;
            strutLowPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPosition;



            Handles.DrawLine(   carBody.position + strutTopPoint,
                                carBody.position + strutLowPoint );
        }
    }
}

/*

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
        }
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
        wheelBody.AddForce(-horizontalOffset * wheelMass, ForceMode.VelocityChange);


        // rotation correction
        Quaternion deltaRotation = Quaternion.FromToRotation(this.transform.right, carBody.transform.right);
        //this.transform.rotation = Quaternion.Lerp(this.transform.rotation, deltaRotation;
        this.transform.rotation = deltaRotation * this.transform.rotation;




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
        wheelBody.AddForce(-dampingAcceleration * 0.5f * Time.deltaTime, ForceMode.VelocityChange);
        carBody.AddForceAtPosition(dampingAcceleration * 0.5f * Time.deltaTime, carBody.position + carBody.rotation * localRestPosition, ForceMode.VelocityChange);



        Debug.DrawRay(wheelBody.position, relativeVelocity, Color.blue, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPosition, -springForce, Color.red, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPosition, horizontalOffset, Color.yellow, Time.deltaTime);
    }
*/    