using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyWheelCollider : MonoBehaviour
{
    // SETTINGS
    public float wheelRadius = 1;
    public float wheelWidth = 1;
    public float wheelMass = 1;

    

    // STARTUP VALUES
    
    MeshCollider wheelColl = null;
    private float collisionCheckDistance = 0;
    private List<GameObject> collisionIgnoreList = new List<GameObject>();

    public bool isRight = false;
    public bool isBack = false;

    private Vector3 localRestPoint = Vector3.zero;

    private Quaternion rotationToStrut = Quaternion.identity;

    private Transform vehicleBase = null;


    // PROCESS VALUES

    public Vector3 summNormal = Vector3.zero;
    private Vector3 summDepenetration = Vector3.zero;
    private Dictionary<GameObject, RaycastHit> surfacePoints = new Dictionary<GameObject, RaycastHit>();

    public bool isGrounded = false;

    private Vector3 Strut = Vector3.zero;
    


    void Start() 
    {
        InitValues();
    }


    void OnValidate() 
    {
        InitValues();
    }


    void InitValues()
    {
        collisionIgnoreList.Clear();

        wheelColl = this.gameObject.GetComponent<MeshCollider>();
        if (wheelColl != null) { 
            wheelRadius = wheelColl.sharedMesh.bounds.extents.y * this.transform.lossyScale.y;
            wheelWidth = wheelColl.sharedMesh.bounds.extents.x * this.transform.lossyScale.x;
            collisionCheckDistance = Mathf.Sqrt(wheelRadius*wheelRadius + wheelWidth*wheelWidth);
            collisionIgnoreList.Add(this.gameObject);
        }

    }



    void FixedUpdate()
    {
        FindCollisions();

        ProcessCollisions();

        UpdatePosition();
    }


    void FindCollisions()
    {
        summDepenetration = Vector3.zero;

        surfacePoints.Clear();

        Collider[] sphereColliders = Physics.OverlapSphere(this.transform.position, collisionCheckDistance);

        for (int i = 0; i < sphereColliders.Length; i++)
        {
            Collider otherCollider = sphereColliders[i];

            // IGNORE IGNORED
            if ( collisionIgnoreList.Contains(otherCollider.gameObject) )
                continue;

            // GET DEPENETRATION
            Vector3 direction;
            float distance;
            bool overlapped = Physics.ComputePenetration(   wheelColl, wheelColl.transform.position, wheelColl.transform.rotation,
                                                            otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                                                            out direction, out distance);
            if (!overlapped)
                continue;
            
            Vector3 depenetration = direction * distance;
            summDepenetration -= depenetration;

            // GET CONTACT POINT
            Vector3 closestPoint;
            if ( otherCollider.GetType() == typeof(MeshCollider) ) {
                Collider coll = otherCollider as MeshCollider;
                closestPoint = coll.ClosestPoint(this.transform.position);
            } else 
            {
                Collider coll = otherCollider as Collider;
                closestPoint = coll.ClosestPoint(this.transform.position);
            }

            RaycastHit surfaceHit;
            if ( Physics.Raycast(this.transform.position, (closestPoint - this.transform.position), out surfaceHit, collisionCheckDistance) ) {
                surfacePoints.Add(otherCollider.gameObject, surfaceHit);
                summNormal += surfaceHit.normal;
            }
        }
        summNormal = summNormal.normalized;

    }

    void ProcessCollisions()
    {
        //fixme
        Vector3 wheelImpulse = Vector3.zero;

        foreach (GameObject obj in surfacePoints.Keys)
        {
            Rigidbody body = obj.GetComponent<Rigidbody>();
            if (body != null) {
                if ( Vector3.Dot(surfacePoints[obj].normal, wheelImpulse) < 0 ) {
                    body.AddForceAtPosition(wheelImpulse, surfacePoints[obj].point, ForceMode.Impulse);
                }
            }
        }

    }

    void UpdatePosition()
    {
        Strut = vehicleBase.rotation * Vector3.up;
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        
        Handles.color = Color.white;

        Handles.DrawWireDisc(this.transform.position - this.transform.right * wheelWidth, this.transform.right, wheelRadius);
        Handles.DrawWireDisc(this.transform.position + this.transform.right * wheelWidth, this.transform.right, wheelRadius);

    }
    #endif

}
