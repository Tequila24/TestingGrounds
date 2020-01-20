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

    private Vector3 localWheelPos = Vector3.zero;
    private Vector3 strutVector = Vector3.zero;
    private Vector3 offsetFromRestPoint = Vector3.zero;


    private Rigidbody carBody = null;
    private Transform wheelBody = null;
    private Collider wheelCollider = null;
    private Vector3 halfBoundingBoxSize;

    private bool isWheelRight;
    private Collider[] surfaces;



    void Awake()
    {
        Init();
    }

    void Init()
    {
        carBody = this.transform.parent.GetComponent<Rigidbody>();
        wheelBody = this.gameObject.transform;

        halfBoundingBoxSize = GetComponent<MeshFilter>().mesh.bounds.size * 0.5f;
        //wheelBody = this.gameObject.GetComponent<Rigidbody>();
        /*if (wheelBody == null) {
            wheelBody = this.gameObject.AddComponent<Rigidbody>();
        }*/
        wheelCollider = this.gameObject.GetComponent<MeshCollider>();
        

        localRestPoint = wheelBody.position - carBody.position;


        strutTopPoint = Quaternion.Inverse(carBody.rotation) *
                        Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                        Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                        (this.transform.up * StrutToTop) + localRestPoint;
        strutBottomPoint =  Quaternion.Inverse(carBody.rotation) * 
                            Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPoint;

        surfaces = new Collider[16];

        float angleBetweenAxis = Vector3.Angle(carBody.transform.right, wheelBody.transform.right);
        if (angleBetweenAxis > 90) {
            //wheel is left
            isWheelRight = false;
        } else {
            //wheel is right
            isWheelRight = true;
        }
        


        
        inited = true;
    }


    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateValues();

        ApplyStrutConstraint();

        //ApplyAxisConstraint();

        //ApplyDampingForce();

        //ApplySpringForce();
    }

    void UpdateValues()
    {
        localWheelPos = wheelBody.position - carBody.position;
        strutVector = (carBody.rotation * (strutBottomPoint - strutTopPoint)).normalized;
        offsetFromRestPoint =   carBody.rotation * localRestPoint - localWheelPos;


    }

    void ApplyStrutConstraint()
    {
        Vector3 HOffset = GetHOffsetForPos(offsetFromRestPoint);
        Vector3 VOffset = GetVOffsetForPos(offsetFromRestPoint);
        Vector3 velocityInFrame = GetRelativeVelocity();
        
        /*Vector3 VOffsetInNextStep = GetVOffsetForPos(offsetFromRestPoint - velocityInFrame * Time.deltaTime);
        if (VOffsetInNextStep.sqrMagnitude > 0) 
        {
            print("VOffsetInNext " + VOffsetInNextStep);
            print("VOffset " + VOffset);
            print("Diff: " + (VOffset - VOffsetInNextStep).sqrMagnitude);

            Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, Vector3.forward, Color.green, 10);
            Debug.DrawRay(wheelBody.position, Vector3.forward, Color.red, 10);
            Debug.DrawRay(wheelBody.position + velocityInFrame * Time.deltaTime, -Vector3.forward, Color.blue, 10);

            Debug.Break();
        }*/

        int count = Physics.OverlapSphereNonAlloc(wheelBody.position, wheelRadius, surfaces);

        for (int i=0; i<count; ++i)
        {
            Collider collider = surfaces[i];

            if (collider == wheelCollider)
                continue;

            Vector3 otherPosition = collider.gameObject.transform.position;
            Quaternion otherRotation = collider.gameObject.transform.rotation;
            Vector3 direction;
            float distance;
            
            bool overlapped = Physics.ComputePenetration(   wheelCollider, wheelBody.position, wheelBody.rotation,
                                                            collider, otherPosition, otherRotation,
                                                            out direction, out distance);

            if (overlapped)
            {
                Handles.color = Color.red;
                Vector3 closestPoint = Physics.ClosestPoint(wheelBody.position + direction * wheelRadius, collider, otherPosition, otherRotation);
                Debug.DrawRay(closestPoint, direction, Color.red, 10);
            }
        }
        
    }

    Vector3 GetHOffsetForPos(Vector3 newOffset)
    {
        Vector3 horizontalOffsetFromStrut = Vector3.ProjectOnPlane(newOffset, strutVector);

        return horizontalOffsetFromStrut;
    }

    Vector3 GetVOffsetForPos(Vector3 newOffset)
    {
        Vector3 verticalOffsetFromStrut = Vector3.zero;

        Vector3 positionOnStrut = Quaternion.FromToRotation(strutVector, Vector3.up) * Vector3.Project(newOffset, strutVector);
        if (positionOnStrut.y > StrutToTop)
        {
            verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutTopPoint - localWheelPos, strutVector);
        }
        if (positionOnStrut.y < StrutToBottom)
        {
            verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutBottomPoint - localWheelPos, strutVector);
        }

        return verticalOffsetFromStrut;
    }

    Vector3 GetRelativeVelocity()
    {
        Vector3 relativeVelocity = Vector3.zero;//wheelBody.velocity - carBody.GetRelativePointVelocity(localWheelPos);
        return relativeVelocity;
    }

    void ApplyAxisConstraint()
    {
        /*
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
        */
    }

    void ApplyDampingForce()
    {
        /*
        Vector3 localWheelPos = wheelBody.position - carBody.position;
        Vector3 relativeVelocity = wheelBody.velocity - carBody.GetRelativePointVelocity(localWheelPos);
        Vector3 dampingAcceleration = relativeVelocity * dampingValue;

        wheelBody.AddForce(-dampingAcceleration * Time.deltaTime, ForceMode.VelocityChange);
        carBody.AddForceAtPosition(dampingAcceleration * Time.deltaTime, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);
        */
    }

    void ApplySpringForce()
    {
        /*
        Vector3 localWheelPos = wheelBody.position - carBody.position;
        Vector3 strutVector = (carBody.rotation * (strutBottomPoint - strutTopPoint)).normalized;
        Vector3 localOffset = localWheelPos - localRestPoint;

        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, localOffset, Color.red, Time.deltaTime);

        Vector3 springForce = localOffset * springValue;
        wheelBody.AddForce(-springForce);
        carBody.AddForceAtPosition(springForce, carBody.position + carBody.rotation * localRestPoint);

        */
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.white;

        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);


        if (inited)
            Handles.DrawLine(   carBody.position + carBody.rotation * strutTopPoint,
                                carBody.position + carBody.rotation * strutBottomPoint );




        if (!inited) {

            localRestPoint = this.transform.position;

            strutTopPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToTop) + localRestPoint;
            strutBottomPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPoint;



            Handles.DrawLine(   strutTopPoint,
                                strutBottomPoint );
        }
    }
}

/*

    void ApplyStrutConstraint()
    {
        Vector3 localWheelPos = wheelBody.transform.position - carBody.transform.position;
        Vector3 strutVector = carBody.rotation * (strutTopPoint - strutBottomPoint);
        Vector3 offsetFromRestPoint = carBody.rotation * localRestPoint - localWheelPos;
        Vector3 horizontalOffsetFromStrut = GetHOffset();


        Vector3 relativeVelocity = wheelBody.velocity - carBody.GetRelativePointVelocity(localWheelPos);
        Vector3 horizontalVelocityToStrut = Vector3.ProjectOnPlane(relativeVelocity, strutVector);
        Vector3 verticalVelocityToStrut = Vector3.Project(relativeVelocity, strutVector);


        // HORIZONTAL
        if ( Vector3.Angle(offsetFromRestPoint, horizontalVelocityToStrut) > 90) {
            wheelBody.velocity -= horizontalVelocityToStrut * 0.5f;
            carBody.AddForceAtPosition(horizontalVelocityToStrut * 0.5f * Time.deltaTime, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);
        }

        Vector3 offsetForce = horizontalOffsetFromStrut * 5 * 0.5f;
        wheelBody.AddForce(offsetForce, ForceMode.VelocityChange);
        carBody.AddForceAtPosition(-offsetForce, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);




        // VERTICAL
        Vector3 positionOnStrut = Quaternion.FromToRotation(strutVector, Vector3.up) * Vector3.Project(offsetFromRestPoint, strutVector);

        if (positionOnStrut.y > StrutToTop)
        {
            if (Vector3.Angle(offsetFromRestPoint, verticalVelocityToStrut) > 90) {
                wheelBody.velocity -= verticalVelocityToStrut;
                carBody.AddForceAtPosition(verticalVelocityToStrut * 0.5f * Time.deltaTime, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);
            }

            Vector3 verticalOffsetFromStrut = GetVOffset();
            wheelBody.AddForce(verticalOffsetFromStrut, ForceMode.VelocityChange);
        } else
        if (positionOnStrut.y < StrutToBottom)
        {
            if (Vector3.Angle(offsetFromRestPoint, verticalVelocityToStrut) > 90) {
                wheelBody.velocity -= verticalVelocityToStrut;
                carBody.AddForceAtPosition(verticalVelocityToStrut * 0.5f * Time.deltaTime, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);
            }

            Vector3 verticalOffsetFromStrut = GetVOffset();
            wheelBody.AddForce(verticalOffsetFromStrut, ForceMode.VelocityChange);
        }


        Debug.DrawRay( wheelBody.position, GetHOffset() + GetVOffset(), Color.magenta, Time.deltaTime);
    }


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