using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelController : MonoBehaviour
{

    // killme
    bool inited = false;
    private int counter = 0;

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

    private Vector3 Strut = Vector3.zero;

    private float offsetFromRestPoint = 0;

    [SerializeField]
    private Rigidbody carBody = null;
    private Transform wheelBody = null;
    private Collider wheelCollider = null;
    private float wheelCheckBoxDistance = 0;

    private bool isWheelRight;


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


    void UpdateGlobalValues()
    {
        Strut = (carBody.rotation * (strutTopPoint - strutBottomPoint)).normalized;
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateGlobalValues();

        bool isSuspensionFloored = false;

        Quaternion strutRotation = Quaternion.FromToRotation(Strut, Vector3.up);

        Vector3 localWheelPosition = wheelBody.position - carBody.position;
        Vector3 springAcceleration = Strut * ((offsetFromRestPoint * springValue) / wheelMass) * Time.deltaTime;
        Vector3 depenetrationInNextFrame = (GetAllignedDepenetration(wheelBody.position - springAcceleration));
        
        offsetFromRestPoint += (strutRotation * (-springAcceleration + depenetrationInNextFrame)).y;

        if ( (offsetFromRestPoint > StrutToTop) || (offsetFromRestPoint < StrutToBottom) )
            isSuspensionFloored = true;

        offsetFromRestPoint = Mathf.Clamp(offsetFromRestPoint, StrutToBottom, StrutToTop);

        

        wheelBody.position = carBody.position + carBody.rotation * localRestPoint +
                             Strut * offsetFromRestPoint;
        
        wheelBody.rotation = isWheelRight ? Quaternion.AngleAxis(180, carBody.transform.up) * carBody.rotation : carBody.rotation;


        if (depenetrationInNextFrame.sqrMagnitude > 0) {
            
            Vector3 carRestPointVelocity = carBody.GetPointVelocity(carBody.position + carBody.rotation * localRestPoint) * Time.deltaTime;


            //apply spring force
            Vector3 carSpringAccceleration = carBody.transform.up * ((offsetFromRestPoint * springValue) / carBody.mass);
            carBody.AddForceAtPosition(carSpringAccceleration, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);


            // damp speed
            Vector3 carVerticalSpeed = Vector3.zero;
            if (isSuspensionFloored) {
                carVerticalSpeed = Vector3.Project(carRestPointVelocity, carBody.transform.up);
            } else {
                carVerticalSpeed = GetDampedVelocity( Vector3.Project(carRestPointVelocity, carBody.transform.up) );
            }
            carBody.AddForceAtPosition(-carVerticalSpeed, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);


            // apply traction
            Vector3 carSlipVelocity = Vector3.ProjectOnPlane(carRestPointVelocity, depenetrationInNextFrame);
            carBody.AddForceAtPosition(-carSlipVelocity, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);
        }
    }


    Vector3 GetDepenetrationForPosition(Vector3 newPosition)
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
        Vector3 depenetrationVector = GetDepenetrationForPosition(newPosition);

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

    Vector3 GetHOffsetForPos(Vector3 newLocalPosition)
    {
        Vector3 offsetFromRestPoint = newLocalPosition - carBody.rotation * localRestPoint;

        Vector3 horizontalOffsetFromStrut = Vector3.ProjectOnPlane(offsetFromRestPoint, Strut);

        return horizontalOffsetFromStrut;
    }

    Vector3 GetVOffsetForPos(Vector3 newLocalPosition)
    {
        Vector3 verticalOffsetFromStrut = Vector3.zero;

        Vector3 offsetFromRestPoint = newLocalPosition - carBody.rotation * localRestPoint;

        Vector3 positionOnStrut = Quaternion.FromToRotation(Strut, Vector3.up) * Vector3.Project(offsetFromRestPoint, Strut);

        if (positionOnStrut.y > StrutToTop)
        {
            verticalOffsetFromStrut = Vector3.Project(newLocalPosition - carBody.rotation * strutTopPoint, Strut);
        }
        if (positionOnStrut.y < StrutToBottom)
        {
            verticalOffsetFromStrut = Vector3.Project(newLocalPosition - carBody.rotation * strutBottomPoint, Strut);
        }
        

        return verticalOffsetFromStrut;
    }

    Vector3 GetDampedVelocity(Vector3 velocity)
    {
        Vector3 dampedVelocity = velocity * dampingValue;

        return dampedVelocity;
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.white;

        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);


        if (inited) {
            Handles.DrawLine(   carBody.position + carBody.rotation * strutTopPoint,
                                carBody.position + carBody.rotation * strutBottomPoint );
            
            Handles.DrawLine(   carBody.position + carBody.rotation * localRestPoint + wheelBody.right*0.2f,
                                carBody.position + carBody.rotation * localRestPoint - wheelBody.right*0.2f );
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