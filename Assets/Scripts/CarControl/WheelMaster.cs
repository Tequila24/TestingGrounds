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
    private Transform vehicleBase = null;
    private Rigidbody vehicleBody = null;

    WheelCollisionDetection.WheelCheckData wheelData; 

    private Quaternion rotationToStrut = Quaternion.identity;

    private bool isRight = false;
    private bool isBack = false;


    // =======


    // OPERATION VALUES
    private Vector3 forwardDirection = Vector3.zero;
    private Vector3 sideDirection = Vector3.zero;

    private float steeringAngle;
    // ================


    
    



    void Awake()
    {
        Init();
    }

    void OnValidate()
    {
        Init();
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

    
    void Update()
    {
        Dictionary<GameObject, RaycastHit> surfaces;
        WheelCollisionDetection.FindSurfaces(wheelData, out surfaces);

        foreach (GameObject obj in surfaces.Keys)
        {
            Debug.DrawRay(surfaces[obj].point, surfaces[obj].normal, Color.yellow, Time.deltaTime, false);
        }

        UpdateWheelPosition();
    }


    void UpdateWheelPosition()
    {

    }

}
