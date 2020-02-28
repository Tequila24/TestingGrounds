using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DepenetrationCalculation;






public class WheelController : MonoBehaviour
{
    // SETTINGS
    public bool isDrive = false;
    public bool isSteerable = false;

    public float MaxSteeringAngle = 20;
    
    public float dampingValue = 0.5f;
    public float springValue = 1;

    public float maxExtension = 1;    
    public float minExtension = -1;

    public float CasterAngle = 0;
    public float CamberAngle = 0;


    // 
    private bool isGrounded = false;
    private bool isFloored = false;
    private bool isRight = false;
    private bool isBack = false;

    private float extension = 0;
    private float extensionVelocity = 0;

    private Vector3 Strut = Vector3.zero;
    private Vector3 localRestPoint = Vector3.zero;
    private Quaternion rotationToStrut = Quaternion.identity;

    private Transform vehicleBase = null;
    private Rigidbody vehicleBody = null;

    private Collider wheelCollider = null;
    private DepenCalc.CollisionCheckInfo collisionCheckInfo;
    private List<GameObject> ignoreList = new List<GameObject>();

    private Vector3 depenetrationVector = Vector3.zero;
    private Vector3 wheelVelocity = Vector3.zero;
    private Vector3 surfaceNormal = Vector3.zero;


    private float steeringAngle;
    public float Steer {
        get {   return steeringAngle;  }
        set {   steeringAngle = Mathf.Clamp(steeringAngle + value, -MaxSteeringAngle, MaxSteeringAngle);    }
    }
    private float Torque = 0;

    private float axialRotationAngle = 0f;
    private float axialRotationVelocity = 40f;




    void Awake()
    {
        Init();
    }


    void Init()
    {
        vehicleBase = this.transform.parent;
        vehicleBody = this.transform.parent.GetComponent<Rigidbody>();
        localRestPoint = this.transform.localPosition;

        wheelCollider = this.gameObject.GetComponent<MeshCollider>();

        isRight = Vector3.Angle(vehicleBase.right, (this.transform.position - vehicleBase.position )) > 90 ? true : false;
        isBack = Vector3.Angle(vehicleBase.forward, (this.transform.position - vehicleBase.position )) > 90 ? true : false;

        Quaternion casterRotation = isBack ?    Quaternion.AngleAxis(CasterAngle, Vector3.right) :
                                                Quaternion.AngleAxis(-CasterAngle, Vector3.right);
        Quaternion camberRotation = isRight ?   Quaternion.AngleAxis(-CamberAngle, Vector3.forward) :
                                                Quaternion.AngleAxis(CamberAngle, Vector3.forward);
                                                
        rotationToStrut = camberRotation * casterRotation;

        Strut = vehicleBase.rotation * rotationToStrut * Vector3.up;
        
        ignoreList.Add(vehicleBase.gameObject);
        ignoreList.Add(this.gameObject);
        
        collisionCheckInfo = new DepenCalc.CollisionCheckInfo(  wheelCollider,
                                                                wheelCollider.transform.position,
                                                                wheelCollider.transform.rotation,
                                                                wheelCollider.bounds.extents.y + wheelCollider.bounds.extents.x,
                                                                ignoreList );
    }


    void FixedUpdate()
    {
        UpdatePosition();
        UpdateRotation();

        UpdateSurface();
    }


    void UpdatePosition()
    {
        Strut = rotationToStrut * Vector3.up;
        
        // Depenetration value
        collisionCheckInfo.colliderPosition = this.transform.position;
        collisionCheckInfo.colliderRotation = this.transform.rotation;
        depenetrationVector = DepenCalc.GetDepenetration(collisionCheckInfo);
        float wheelDepenetrationInThisFrame = (Quaternion.Inverse(rotationToStrut) * Vector3.Project(depenetrationVector, Strut)).y;
        extensionVelocity += wheelDepenetrationInThisFrame;

        // Spring value
        float springAcceleration = (Quaternion.Inverse(rotationToStrut) * (-Strut * (extension * springValue) * Time.deltaTime)).y;
        extensionVelocity += springAcceleration;


        // apply values
        extension += extensionVelocity;
        extension = Mathf.Clamp(extension, minExtension, maxExtension);
        
        this.transform.localPosition = localRestPoint + Strut * extension;


        // damp velocity
        extensionVelocity -= extensionVelocity * dampingValue;
    }


    void UpdateRotation()
    {
        axialRotationAngle += axialRotationVelocity * Time.deltaTime;
        if (axialRotationAngle > 360.0f)
            axialRotationAngle -= 360.0f;
        Quaternion axialRotation = Quaternion.AngleAxis(axialRotationAngle, vehicleBase.right);

        Quaternion steerRotation = Quaternion.AngleAxis( (isBack ? -steeringAngle : steeringAngle), vehicleBase.up);

        Quaternion rotationToBase = isRight ? Quaternion.AngleAxis(180, vehicleBase.up) * vehicleBase.rotation : vehicleBase.rotation;

        // apply rotation
        this.transform.rotation = steerRotation * axialRotation * rotationToBase;
    }


    void UpdateSurface()
    {

    }

    void OnDrawGizmos()
    {
        #if UNITY_EDITOR

        if (vehicleBase == null) {
            Init();
        }

        Handles.color = Color.white;
        Handles.DrawLine(   vehicleBase.position + (vehicleBase.rotation * (localRestPoint + Strut * maxExtension)),
                            vehicleBase.position + (vehicleBase.rotation * (localRestPoint + Strut * minExtension)) );

        Handles.color = Color.white;
        Handles.DrawLine(   vehicleBase.position + vehicleBase.rotation * localRestPoint + this.transform.right*0.2f,
                            vehicleBase.position + vehicleBase.rotation * localRestPoint - this.transform.right*0.2f );

        Handles.color = Color.red;
        Handles.DrawLine(   this.transform.position + this.transform.right*0.2f,
                            this.transform.position - this.transform.right*0.2f );


        
        #endif
    }

}
