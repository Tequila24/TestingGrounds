using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelController : MonoBehaviour
{

    // killme
    bool inited = false;
    private int counter = 0;


    [SerializeField]
    private float wheelRadius = 1;
    [SerializeField]
    private float wheelMass = 1;
    [SerializeField]
    [Range (0.01f ,0.99f)]
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

    private Vector3 wheelVelocity = Vector3.zero;

    [SerializeField]
    private Rigidbody carBody = null;
    private Transform wheelBody = null;
    private Collider wheelCollider = null;
    private Vector3 halfBoundingBoxSize;

    private bool isWheelRight;


    void Awake()
    {
        Init();
    }

    void Init()
    {
        //carBody = this.transform.parent.GetComponent<Rigidbody>();
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
        

        float angleBetweenAxis = Vector3.Angle(carBody.transform.right, wheelBody.transform.right);
        if (angleBetweenAxis > 90) {
            isWheelRight = false;
        } else {
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

        Vector3 depenetrationInThisFrame = GetAllignedDepenetration(wheelBody.position);
        Vector3 springAccelerationInFrame = Vector3.zero;
        Vector3 depenetrationInNextFrame = Vector3.zero;

        if (depenetrationInThisFrame.sqrMagnitude > 0) {
            wheelVelocity = Vector3.zero;
            wheelBody.position += depenetrationInThisFrame;
        } else {
            
            springAccelerationInFrame = GetSpringAcceleration() * Time.deltaTime;
            depenetrationInNextFrame = GetAllignedDepenetration(wheelBody.position + wheelVelocity + springAccelerationInFrame);


            Vector3 correctedSpringAcceleration;
            correctedSpringAcceleration = springAccelerationInFrame + depenetrationInNextFrame;

            //Debug.DrawRay(wheelBody.position - wheelBody.right*0.1f + wheelBody.forward*0.05f, springAccelerationInFrame, Color.red, Time.deltaTime, false);
            Debug.DrawRay(wheelBody.position - wheelBody.right*0.2f + wheelBody.forward*0.1f, depenetrationInNextFrame, Color.blue, Time.deltaTime, false);
            //Debug.DrawRay(wheelBody.position - wheelBody.right*0.3f + wheelBody.forward*0.15f, correctedSpringAcceleration, Color.green, Time.deltaTime, false);


            wheelVelocity += correctedSpringAcceleration;

            wheelVelocity = GetDampedVelocity(wheelVelocity);

            wheelBody.position += wheelVelocity;
            
        }

        

        //wheelBody.position += wheelVelocity * Time.deltaTime + depenetrationInThisFrame;
    }

    void UpdateValues()
    {
        localWheelPos = wheelBody.position - carBody.position;
        strutVector = (carBody.rotation * (strutBottomPoint - strutTopPoint)).normalized;
        offsetFromRestPoint =   carBody.rotation * localRestPoint - localWheelPos;
    }

    Vector3 GetSurfaceReaction()
    {
        Vector3 wheelReactionInFrame = Vector3.zero;
        /*Vector3 surfaceReaction = GetDepenetrationVector();

        Vector3 surfaceReactionAcceleration = Vector3.Project(surfaceReaction, strutVector);
        Vector3 vOffsetInNextFrame = GetVOffsetForPos(localWheelPos + surfaceReactionAcceleration);

        if (vOffsetInNextFrame.sqrMagnitude > 0) {
            surfaceReactionAcceleration += vOffsetInNextFrame;
        }

        wheelReactionInFrame = surfaceReactionAcceleration;*/

        return wheelReactionInFrame;
    }

    Vector3 GetDepenetrationForPosition(Vector3 newPosition)
    {
        Vector3 surfacePenetration = Vector3.zero;
        Collider[] surfaces = new Collider[16];

        int count = Physics.OverlapSphereNonAlloc(newPosition, wheelRadius, surfaces);

        if (count<2)
            return surfacePenetration;

        for (int i=0; i<count; ++i)
        {
            Collider collider = surfaces[i];

            if (collider == wheelCollider)
                continue;

            Vector3 otherPosition = collider.gameObject.transform.position;
            Quaternion otherRotation = collider.gameObject.transform.rotation;
            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(   wheelCollider, newPosition, wheelBody.rotation,
                                                            collider, otherPosition, otherRotation,
                                                            out direction, out distance);

            if (overlapped)
            {
                surfacePenetration += direction * distance;
            }
        }

        return surfacePenetration;
    }

    Vector3 GetAllignedDepenetration(Vector3 newPosition)
    {
        Vector3 depenetrationVector = GetDepenetrationForPosition(newPosition);
        if (depenetrationVector.sqrMagnitude == 0)
            return Vector3.zero;


        float angle = Vector3.Angle(depenetrationVector, strutVector);
        float newScale = depenetrationVector.magnitude;//depenetrationVector.magnitude / angleCos;

        if (angle < 90) {
            depenetrationVector = strutVector * newScale;
        } else {
            depenetrationVector = -strutVector * newScale;
        }

        return depenetrationVector;
    }

    Vector3 GetHOffsetForPos(Vector3 newOffset)
    {
        Vector3 horizontalOffsetFromStrut = Vector3.ProjectOnPlane(newOffset, strutVector);

        return horizontalOffsetFromStrut;
    }

    Vector3 GetVOffsetForPos(Vector3 newOffset)
    {
        Vector3 verticalOffsetFromStrut = Vector3.zero;

        Vector3 positionOnStrut = Quaternion.FromToRotation(strutVector, -Vector3.up) * Vector3.Project(newOffset, strutVector);
        if (positionOnStrut.y > StrutToTop)
        {
            verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutTopPoint - newOffset, strutVector);
        }
        if (positionOnStrut.y < StrutToBottom)
        {
            verticalOffsetFromStrut = Vector3.Project(carBody.rotation * strutBottomPoint - newOffset, strutVector);
        }

        return verticalOffsetFromStrut;
    }

    Vector3 GetDampedVelocity(Vector3 velocity)
    {
        Vector3 dampedVelocity = velocity - (velocity * dampingValue);

        return dampedVelocity;
    }

    Vector3 GetSpringAcceleration()
    {
        Vector3 springAcceleration = Vector3.zero;
        
        localWheelPos = wheelBody.position - carBody.position;
        strutVector = (carBody.rotation * (strutBottomPoint - strutTopPoint)).normalized;
        Vector3 localOffset = localRestPoint - localWheelPos;
        /*if (localOffset.sqrMagnitude > ((StrutToTop - StrutToBottom)*10) )
        {
            return Vector3.zero;
        }*/

        Vector3 springForce = localOffset * springValue;
        springAcceleration = springForce/wheelMass;

        return springAcceleration;
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

//Debug.DrawRay(carBody.position + localRestPoint - wheelBody.right*0.4f + wheelBody.forward*0.01f * counter, depenetrationVector, Color.yellow, 5, false);