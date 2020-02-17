using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelControllerOld : MonoBehaviour
{

    // killme
    private bool inited = false;

    public bool isDrive = false;
    public bool isSteerable = false;
    public bool isGrounded = false;
    public float steerAngle = 0;


    private float wheelRadius = 1;
    [SerializeField]
    private float wheelMass = 1;
    [SerializeField]
    [Range (0.01f ,0.99f)]
    private float dampingValue = 0.5f;
    [SerializeField]
    private float springValue = 1;
    [SerializeField]
    [Range (0.01f ,10.99f)]
    private float tractionValue = 3;


    [SerializeField]
    private float StrutToTop = 0;    
    [SerializeField]
    private float StrutToBottom = 0;
    [SerializeField]
    private float CasterAngle = 0;
    [SerializeField]
    private float CamberAngle = 0;


    [SerializeField]
    private Rigidbody carBody = null;
    private Transform wheelBody = null;
    private Collider wheelCollider = null;
    private float wheelCheckBoxDistance = 0;
    private bool isWheelRight = false;


    private Vector3 strutTopPoint = Vector3.zero;
    private Vector3 strutBottomPoint = Vector3.zero;

    private Vector3 localRestPoint = Vector3.zero;
    private Vector3 Strut = Vector3.zero;
    private Vector3 carRestPointVelocity = Vector3.zero;
    private Vector3 depenetrationInNextFrame = Vector3.zero;
    private Vector3 wheelVelocityOnStrut = Vector3.zero;
    private float offsetFromRestPoint = 0;

    bool isSuspensionFloored = false;




    void Awake()
    {
        Init();
    }

    void Init()
    {
        wheelBody = this.gameObject.transform;
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


        // is wheel left
        if (Vector3.Angle(carBody.transform.right, wheelBody.transform.right) > 90) {
            isWheelRight = true;
        } else {
            isWheelRight = false;
        }
        
        
        wheelCheckBoxDistance = wheelCollider.bounds.extents.magnitude;

        
        inited = true;
    }





    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateGlobalValues();

        UpdateWheelPosition();
    
        UpdateWheelRotation();

        ApplyCarPhysics();
        
    }


    void UpdateGlobalValues()
    {
        Strut = (carBody.rotation * (strutTopPoint - strutBottomPoint)).normalized;
        carRestPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * localRestPoint) /* + gravity? */) * Time.deltaTime;
    }


    void UpdateWheelPosition()
    {

        Vector3 springAcceleration = -Strut * ((offsetFromRestPoint * springValue) / wheelMass) * Time.deltaTime;
        wheelVelocityOnStrut += springAcceleration;

        depenetrationInNextFrame = GetAllignedDepenetration(wheelBody.position + wheelVelocityOnStrut + carRestPointVelocity);
        wheelVelocityOnStrut += depenetrationInNextFrame;

        offsetFromRestPoint += ( Quaternion.FromToRotation(Strut, Vector3.up) * (wheelVelocityOnStrut) ).y;



        if ( (offsetFromRestPoint > StrutToTop) || (offsetFromRestPoint < StrutToBottom) )
            isSuspensionFloored = true;
        else 
            isSuspensionFloored = false;


        offsetFromRestPoint = Mathf.Clamp(offsetFromRestPoint, StrutToBottom, StrutToTop);
        

        wheelBody.position =    carBody.position + carBody.rotation * localRestPoint +
                                carRestPointVelocity + 
                                Strut * offsetFromRestPoint;


        wheelVelocityOnStrut -= wheelVelocityOnStrut * dampingValue;

        isGrounded = depenetrationInNextFrame.sqrMagnitude > 0 ? true : false;
    }


    void UpdateWheelRotation()
    {
        
        //Quaternion deltaWheelRotation = Quaternion.AngleAxis(angularVelocity, carBody.transform.right);
        Quaternion steerRotation = Quaternion.AngleAxis(steerAngle, carBody.transform.up);
        Quaternion axisRotation = (isWheelRight ? Quaternion.AngleAxis(180, carBody.transform.up) * carBody.rotation : carBody.rotation);

        wheelBody.rotation = axisRotation * steerRotation;
        //wheelBody.rotation *= deltaWheelRotation;
    }


    void ApplyCarPhysics()
    {

        // apply spring
        Vector3 springForce = carBody.transform.up * ((offsetFromRestPoint * springValue));
        carBody.AddForceAtPosition(springForce, carBody.position + carBody.rotation * localRestPoint, ForceMode.Impulse);


        // damp speed

        Vector3 carRelativeVerticalSpeed = (wheelVelocityOnStrut) * dampingValue;
        if (isSuspensionFloored) {
            carRelativeVerticalSpeed -= depenetrationInNextFrame;
        }
        carRelativeVerticalSpeed = Vector3.Project(carRelativeVerticalSpeed, Strut);
        

        Debug.DrawRay(carBody.position + carBody.rotation * localRestPoint, carRelativeVerticalSpeed, Color.yellow, Time.deltaTime, false);
    }


    Vector3 GetDepenetration(Vector3 newPosition)
    {
        Vector3 surfacePenetration = Vector3.zero;
        Collider[] surfaces = new Collider[16];

        int count = Physics.OverlapSphereNonAlloc(newPosition, wheelCheckBoxDistance, surfaces);

        if (count<2)
            return surfacePenetration;

        for (int i=0; i<count; ++i)
        {
            Collider collider = surfaces[i];

            if (collider == wheelCollider || collider.gameObject == carBody.gameObject)
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
        Vector3 depenetrationVector = GetDepenetration(newPosition);

        if (depenetrationVector.sqrMagnitude == 0)
            return Vector3.zero;


        float angle = Vector3.Angle(depenetrationVector, Strut);
        float newScale = depenetrationVector.magnitude;

        if (angle < 90) {
            depenetrationVector = Strut * newScale;
        } else {
            depenetrationVector = -Strut * newScale;
        }

        return depenetrationVector;
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.white;

        if (inited) {
            Handles.DrawLine(   carBody.position + carBody.rotation * strutTopPoint,
                                carBody.position + carBody.rotation * strutBottomPoint );
            
            Handles.DrawLine(   carBody.position + carBody.rotation * (localRestPoint + wheelBody.right*0.2f),
                                carBody.position + carBody.rotation * (localRestPoint - wheelBody.right*0.2f) );

            Handles.DrawLine(   wheelBody.position + carBody.rotation * (wheelBody.right*0.2f),
                                wheelBody.position + carBody.rotation * (-wheelBody.right*0.2f) );
        }




        if (!inited) {

            localRestPoint = this.transform.position;

            strutTopPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToTop) + localRestPoint;
            strutBottomPoint = Quaternion.AngleAxis(-CasterAngle, Vector3.right) *
                            Quaternion.AngleAxis(CamberAngle, this.transform.forward) *
                            (this.transform.up * StrutToBottom) + localRestPoint;



            Handles.DrawLine(strutTopPoint, strutBottomPoint);
        }
    }
}    

//Debug.DrawRay(carBody.position + localRestPoint - wheelBody.right*0.4f + wheelBody.forward*0.01f * counter, depenetrationVector, Color.yellow, 5, false);