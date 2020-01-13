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
    private Vector3 strutBottomPoint = Vector3.zero;

    private Vector3 localRestPoint = Vector3.zero;



    [SerializeField]
    private Rigidbody carBody = null;
    private Rigidbody wheelBody = null;    




    [SerializeField]
    private float Kp = 1;
    [SerializeField]
    private float Ki = 1;
    [SerializeField]
    private float Kd = 1;

    MyPID forcePID;


    private bool isWheelRight;

    
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
        localRestPoint = wheelBody.position - carBody.position;


        strutTopPoint = Quaternion.Inverse(carBody.rotation) *
                        Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                        Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                        (this.transform.up * StrutToTop) + localRestPoint;
        strutBottomPoint =  Quaternion.Inverse(carBody.rotation) * 
                            Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPoint;


        float angleBetweenAxis = Vector3.Angle(carBody.transform.right, wheelBody.transform.right);
        if (angleBetweenAxis > 90) {
            //wheel is left
            isWheelRight = false;
        } else {
            //wheel is right
            isWheelRight = true;
        }


        forcePID = new MyPID(1000f, 10f, 200f);



        
        inited = true;
    }


    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        forcePID.Kp = Kp;
        forcePID.Ki = Ki;
        forcePID.Kd = Kd;


        ApplyDampingForce();
    }

    void Update()
    {
        ApplyStrutConstraint();

        ApplyAxisConstraint();
    }


    void ApplyStrutConstraint()
    {
        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 strutVector = carBody.rotation * (strutTopPoint - strutBottomPoint);
        Vector3 offsetFromRestPoint = carBody.rotation * localRestPoint - localWheelPos;
        Vector3 horizontalOffsetFromStrut = Vector3.ProjectOnPlane(offsetFromRestPoint, strutVector);


        Vector3 relativeVelocity = wheelBody.velocity - carBody.GetRelativePointVelocity(localWheelPos);
        Vector3 horizontalVelocityToStrut = Vector3.ProjectOnPlane(relativeVelocity, strutVector);
        Vector3 verticalVelocityToStrut = Vector3.Project(relativeVelocity, strutVector);


        // HORIZONTAL
        if ( Vector3.Angle(offsetFromRestPoint, horizontalVelocityToStrut) > 90) {
            wheelBody.velocity -= horizontalVelocityToStrut;
        }
        //wheelBody.AddForce(horizontalOffsetFromStrut, ForceMode.Impulse);
        //carBody.AddForceAtPosition(horizontalOffsetFromStrut*0.5f, wheelBody.position, ForceMode.Impulse);


        // VERTICAL
        Vector3 positionOnStrut = Quaternion.FromToRotation(strutVector, Vector3.up) * Vector3.Project(offsetFromRestPoint, strutVector);

        if (positionOnStrut.y > StrutToTop)
        {
            if (Vector3.Angle(offsetFromRestPoint, verticalVelocityToStrut) > 90) 
                wheelBody.velocity -= verticalVelocityToStrut;

            /*Vector3 verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutTopPoint - localWheelPos, strutVector);
            wheelBody.AddForce(verticalOffsetFromStrut, ForceMode.Impulse);*/
            wheelBody.MovePosition(carBody.position + carBody.rotation * strutBottomPoint);
        } else
        if (positionOnStrut.y < StrutToBottom)
        {
            if (Vector3.Angle(offsetFromRestPoint, verticalVelocityToStrut) > 90) 
                wheelBody.velocity -= verticalVelocityToStrut;

            /*Vector3 verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutBottomPoint - localWheelPos, strutVector);
            wheelBody.AddForce(verticalOffsetFromStrut, ForceMode.Impulse);*/
            wheelBody.MovePosition(carBody.position + carBody.rotation * strutTopPoint);
        }
        
        //print(relativeVelocity - horizontalVelocityToStrut - verticalVelocityToStrut);
        print(wheelBody.velocity);
        print(carBody.GetRelativePointVelocity(localWheelPos));
        
        
        //Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, horizontalOffsetFromStrut, Color.magenta, Time.deltaTime);
        //Debug.DrawRay(wheelBody.position, horizontalOffsetFromStrut, Color.magenta, Time.deltaTime);
    }

    void ApplyAxisConstraint()
    {
        
        Vector3 wheelAxis;
        if (isWheelRight)
            wheelAxis = -carBody.transform.right;
        else
            wheelAxis = carBody.transform.right;

        float angleToAxis = Vector3.Angle(this.transform.right, wheelAxis);
        //print(angleToAxis);



        // ROTATION OFFSET
        // find offset
        Quaternion wheelRotationOffset = Quaternion.FromToRotation(this.transform.right, wheelAxis);
        // remove offset
        wheelBody.rotation = wheelRotationOffset * wheelBody.rotation;




        // RELATIVE VELOCITY 
        // find relativeVelocity 
        // remove relative angular velocity
        Vector3 wheelAngularVelocity = wheelBody.angularVelocity;
        Vector3 carAngularVelocity = carBody.angularVelocity;
        
        //wheelBody.angularVelocity.Set(wheelAngularVelocity.x + carAngularVelocity.x, carAngularVelocity.y, carAngularVelocity.z);
        Vector3 transformedRotation = Vector3.ProjectOnPlane(wheelBody.angularVelocity, wheelAxis);
        
        wheelBody.angularVelocity = wheelRotationOffset * wheelBody.angularVelocity - transformedRotation;





        // fix me
        wheelBody.AddTorque(carBody.transform.right * Input.GetAxisRaw("Vertical") * wheelBody.mass * 100);
        //carBody.AddTorque(carBody.transform.right * Input.GetAxisRaw("Vertical") * carBody.mass * 100);

        Debug.DrawRay(carBody.transform.right, wheelAxis*3, Color.blue, Time.deltaTime);
    }

    void ApplyDampingForce()
    {
        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 relativeVelocity = wheelBody.velocity - carBody.GetRelativePointVelocity(localWheelPos);
        Vector3 dampingAcceleration = relativeVelocity * dampingValue;

        //wheelBody.AddForce(-dampingAcceleration, ForceMode.Impulse);
        //carBody.AddForceAtPosition(dampingAcceleration, carBody.position + carBody.rotation * localRestPoint, ForceMode.Impulse);


        //Debug.DrawRay(wheelBody.position, dampingAcceleration*10, Color.magenta, Time.deltaTime);
    }


    void OnDrawGizmos()
    {
        Handles.color = Color.white;

        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);


        if (inited)
            Handles.DrawLine(   carBody.position + carBody.rotation * strutTopPoint,
                                carBody.position + carBody.rotation * strutBottomPoint );




        if (!inited) {

            localRestPoint = this.transform.position - carBody.position;

            strutTopPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToTop) + localRestPoint;
            strutBottomPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPoint;



            Handles.DrawLine(   carBody.position + strutTopPoint,
                                carBody.position + strutBottomPoint );
        }
    }
}

/*

    void ApplyStrutConstraint()
    {
        Vector3 wheelVerticalOffset = Vector3.zero;
        Vector3 wheelHorizontalOffset = Vector3.zero;


        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 wheelOffset = (localWheelPos - carBody.rotation * localRestPoint);
        wheelHorizontalOffset = Vector3.ProjectOnPlane(wheelOffset, carBody.rotation * (strutTopPoint - strutBottomPoint) );
        wheelBody.position -= wheelHorizontalOffset;

        Vector3 relativeWheelVelocity = wheelBody.velocity - carBody.GetPointVelocity(carBody.position + carBody.rotation * localRestPoint);
        Vector3 horizontalPartVelocity = Vector3.ProjectOnPlane(relativeWheelVelocity, carBody.rotation * (strutTopPoint - strutBottomPoint) );
        wheelBody.velocity -= horizontalPartVelocity;

        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, horizontalPartVelocity, Color.red, Time.deltaTime);
        



        Vector3 wheelPositionOnStrut =  Quaternion.FromToRotation( carBody.rotation * (strutTopPoint - strutBottomPoint), Vector3.up) * 
                                        wheelOffset - wheelHorizontalOffset;

        if (wheelPositionOnStrut.y <= StrutToBottom) 
        {
            wheelVerticalOffset = localWheelPos - carBody.rotation * strutBottomPoint;
            wheelBody.position -= wheelVerticalOffset;

            Debug.DrawRay(carBody.position + carBody.rotation * strutBottomPoint, wheelVerticalOffset, Color.magenta, Time.deltaTime);
        }
        if (wheelPositionOnStrut.y >= StrutToTop) 
        {
            wheelVerticalOffset = localWheelPos - carBody.rotation * strutTopPoint;
            Debug.DrawRay(carBody.position + carBody.rotation * strutTopPoint, wheelVerticalOffset, Color.magenta, Time.deltaTime);
        }
        wheelBody.position -= wheelVerticalOffset;
        Vector3 verticalPartVelocity = relativeWheelVelocity - horizontalPartVelocity;
        wheelBody.velocity -= verticalPartVelocity;

        
        
        
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, wheelHorizontalOffset, Color.blue, Time.deltaTime);
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
        Vector3 wheelOffset = (localWheelPos - carBody.rotation * localRestPoint);

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
        carBody.AddForceAtPosition(springForce * 0.5f, carBody.position + carBody.rotation * localRestPoint);


        // damping force
        Vector3 carPointVelocity = carBody.GetRelativePointVelocity(localRestPoint);
        Vector3 wheelVelocity = wheelBody.velocity;
        Vector3 relativeVelocity = wheelVelocity - carPointVelocity;
        Vector3 dampingAcceleration = relativeVelocity * dampingValue;
        wheelBody.AddForce(-dampingAcceleration * 0.5f * Time.deltaTime, ForceMode.VelocityChange);
        carBody.AddForceAtPosition(dampingAcceleration * 0.5f * Time.deltaTime, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);



        Debug.DrawRay(wheelBody.position, relativeVelocity, Color.blue, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, -springForce, Color.red, Time.deltaTime);
        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, horizontalOffset, Color.yellow, Time.deltaTime);
    }
*/    