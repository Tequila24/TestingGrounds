﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DepenetrationCalculation;






public class WheelController : MonoBehaviour
{
    // SETTINGS
    public bool isDrive = false;
    public bool isSteerable = false;

    [Range (0, 45f)]
    public float MaxSteeringAngle = 20;
    
    [Range (0.01f, 0.99f)]
    public float dampingValue = 0.5f;
    public float springValue = 1;

    public float maxExtension = 1;    
    public float minExtension = -1;

    public float CasterAngle = 0;
    public float CamberAngle = 0;


    // 
    private bool isGrounded = false;
    public bool IsGrounded {
        get { return isGrounded; }
        set { }
    }
    private bool isFloored = false;
    public bool IsFloored {
        get { return isFloored; }
        set { }
    }
    private bool isRight = false;
    private bool isBack = false;

    private float extension = 0;
    public float extensionVelocity = 0;

    private Vector3 Strut = Vector3.zero;
    private Vector3 localRestPoint = Vector3.zero;
    public Vector3 RestPoint {
        get {return vehicleBody.position + localRestPoint; }
        set {}
    }
    private Quaternion rotationToStrut = Quaternion.identity;

    private Transform vehicleBase = null;
    private Rigidbody vehicleBody = null;

    private Collider wheelCollider = null;
    private DepenCalc.CollisionCheckInfo collisionCheckInfo;
    private List<GameObject> ignoreList = new List<GameObject>();

    private Vector3 springForce = Vector3.zero;
    public Vector3 SpringForce {
        get { return springForce; }
        set { }
    }

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
    private float axialRotationVelocity = 0f;




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

        // damp velocity
        extensionVelocity -= extensionVelocity * dampingValue;
        
        // Depenetration value
        collisionCheckInfo.colliderPosition = this.transform.position;
        collisionCheckInfo.colliderRotation = this.transform.rotation;
        collisionCheckInfo.ignoreList = ignoreList;
        depenetrationVector = DepenCalc.GetDepenetration(collisionCheckInfo);
        float wheelDepenetrationInThisFrame = (Quaternion.Inverse(rotationToStrut) * Vector3.Project(depenetrationVector, Strut)).y;
        extensionVelocity += wheelDepenetrationInThisFrame;

        isGrounded = depenetrationVector.sqrMagnitude > 0 ? true : false;

        // Spring value
        springForce = (-Strut * (extension * springValue) ) ;
        float springAcceleration = (Quaternion.Inverse(rotationToStrut) * springForce).y / vehicleBody.mass;

        extensionVelocity += springAcceleration;


        // apply values
        extension += extensionVelocity;

        if ( (extension > maxExtension) || (extension < minExtension) )
            isFloored = true;
        else 
            isFloored = false;

        extension = Mathf.Clamp(extension, minExtension, maxExtension);
        
        this.transform.localPosition = localRestPoint + Strut * extension;


        
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
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, -depenetrationVector, out hit, collisionCheckInfo.checkDistance))
        {
            surfaceNormal = Vector3.Lerp(surfaceNormal, hit.normal, 0.1f);
            Debug.DrawRay(hit.point, surfaceNormal * 0.5f, Color.yellow, Time.deltaTime, false);
        }
    }

    void OnDrawGizmos()
    {
        #if UNITY_EDITOR

        if (!Application.isPlaying) {
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
