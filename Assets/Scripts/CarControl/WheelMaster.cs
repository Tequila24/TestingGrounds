using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshCollider))]

public class WheelMaster : MonoBehaviour
{
    // SETTINGS
    public float wheelMass = 1;

    public bool isDrive = false;
    public bool isSteerable = false;
    
    public float springValue = 1;
    [Range (0.01f, 0.99f)]
    public float dampingValue = 0.5f;

    public float maxExtension = 1;    
    public float minExtension = -1;

    public float CasterAngle = 0;
    public float CamberAngle = 0;
    // =======


    // START VALUES
    WheelCollisionDetection.WheelCheckData wheelData; 

    private Transform vehicleBase = null;
    private Rigidbody vehicleBody = null;

    private Vector3 localRestPoint = Vector3.zero;

    private Quaternion rotationToStrut = Quaternion.identity;

    private bool isRight = false;
    private bool isBack = false;


    // =======


    // OPERATION VALUES
    private Vector3 forwardDirection = Vector3.zero;
    private Vector3 sideDirection = Vector3.zero;

    private float extension = 0;
    private float extensionVelocity = 0;

    private float steeringAngle;

    private float axialVelocity;
    // ================


    
    



    void Awake()
    {
        Init();
    }

    void OnValidate()
    {
    }

    void Init()
    {
        vehicleBase = this.transform.parent;
        vehicleBody = this.transform.parent.GetComponent<Rigidbody>();

        localRestPoint = Quaternion.Inverse(vehicleBase.rotation) * (this.transform.position - vehicleBase.position);

        MeshCollider wheelCollider = this.gameObject.GetComponent<MeshCollider>();
        wheelData = new WheelCollisionDetection.WheelCheckData(wheelCollider);
        wheelData.Ignore(this.gameObject);


        isRight = Vector3.Angle(vehicleBase.right, (this.transform.position - vehicleBase.position )) > 90 ? true : false;
        isBack = Vector3.Angle(vehicleBase.forward, (this.transform.position - vehicleBase.position )) > 90 ? true : false;

        Quaternion casterRotation = isBack ?    Quaternion.AngleAxis(CasterAngle, Vector3.right) :
                                                Quaternion.AngleAxis(-CasterAngle, Vector3.right);
        Quaternion camberRotation = isRight ?   Quaternion.AngleAxis(-CamberAngle, Vector3.forward) :
                                                Quaternion.AngleAxis(CamberAngle, Vector3.forward);
                                                
        rotationToStrut = camberRotation * casterRotation;
    }

    
    void FixedUpdate()
    {
        Vector3 Strut = vehicleBase.rotation * rotationToStrut * Vector3.up;
        
        Dictionary<GameObject, RaycastHit> surfaces;
        WheelCollisionDetection.FindSurfaces(wheelData, out surfaces);

        foreach (GameObject obj in surfaces.Keys)
        {
            Debug.DrawRay(surfaces[obj].point, surfaces[obj].normal, Color.yellow, Time.deltaTime, false);
        }

        float surfacesAxialVelocity = WheelRotationControl.GetSurfaceVelocity(surfaces, wheelData);

        axialVelocity = Mathf.Lerp(axialVelocity, surfacesAxialVelocity, 0.02f);

        
        // damp velocity
        extensionVelocity -= extensionVelocity * dampingValue;



        // Spring value
        float springForce = (extension * springValue);
        float springAcceleration = springForce / vehicleBody.mass;
        extensionVelocity -= springAcceleration;

        print(extensionVelocity);
        

        float summDepenetration = 0;
        foreach (RaycastHit contact in surfaces.Values)
        {
            summDepenetration += (Quaternion.Inverse(rotationToStrut) * Vector3.Project(contact.normal, Strut)).y;
        }

        if ( Mathf.Sign(summDepenetration) != Mathf.Sign(extensionVelocity) & (summDepenetration != 0) )
            extensionVelocity=0;

        extension += (extensionVelocity + summDepenetration);

        extension = Mathf.Clamp(extension, minExtension, maxExtension);


        this.transform.position =   vehicleBase.position + 
                                    vehicleBase.rotation * localRestPoint + 
                                    Strut * extension;
    }


}
