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
    private float collisionCheckDistance = 0;
    private List<GameObject> collisionIgnoreList = new List<GameObject>();


    // PROCESS VALUES
    private Vector3 velocityDelta = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;

    private Dictionary<GameObject, Vector3> surfacePoints = new Dictionary<GameObject, Vector3>();


    void Start()
    {
        prevPosition = this.transform.position;

        collisionCheckDistance = Mathf.Sqrt(wheelRadius*wheelRadius + wheelWidth*wheelWidth);
    }

    void OnValidate()
    {
        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null) { 
            wheelRadius = meshFilter.sharedMesh.bounds.extents.y * this.transform.lossyScale.y;
            wheelWidth = meshFilter.sharedMesh.bounds.extents.x * this.transform.lossyScale.x;
        }
    }



    void FixedUpdate()
    {
        UpdateVelocity();
        
        FindCollisions();

        ProcessCollisions();
    }


    void FindCollisions()
    {
        surfacePoints.Clear();

        List<Collider> colliders = new List<Collider>();

        Collider[] sphereColliders = Physics.OverlapSphere(this.transform.position, collisionCheckDistance);
        Collider[] boxColliders =  Physics.OverlapBox(this.transform.position, new Vector3(wheelWidth, wheelRadius, wheelRadius), this.transform.rotation);

        for (int i = 0; i < sphereColliders.Length; i++)
        {
            Collider sphereItem = sphereColliders[i];
            for (int j = 0; j < boxColliders.Length; j++)
            {
                Collider boxItem = boxColliders[j];
                if (sphereItem == boxItem)
                    colliders.Add(boxItem);
            }
        }
        
        foreach (Collider item in colliders)
        {
            Vector3 closestPoint;
            if ( item.GetType() == typeof(MeshCollider) ) {
                Collider coll = item as MeshCollider;
                closestPoint = coll.ClosestPoint(this.transform.position);
            } else 
            {
                Collider coll = item as Collider;
                closestPoint = coll.ClosestPoint(this.transform.position);
            }

            surfacePoints.Add(item.gameObject, closestPoint);
            

        }


    }

    void ProcessCollisions()
    {
        Vector3 impulse = velocity * wheelMass;

        foreach (GameObject gameObject in surfacePoints.Keys)
        {
            Vector3 surfacePoint = surfacePoints[gameObject];
            Vector3 fromTo = this.transform.position - surfacePoint;
            
            Rigidbody body = gameObject.GetComponent<Rigidbody>();
            
            if ( (body != null) && (Vector3.Dot(impulse, fromTo) > 0) )
                body.AddForceAtPosition(-impulse, surfacePoints[gameObject], ForceMode.Impulse);
        }
    }

    void UpdateVelocity()
    {
        velocity = prevPosition - this.transform.position;

        foreach (Vector3 surfacePoint in surfacePoints.Values)
        {
            Vector3 fromTo = this.transform.position - surfacePoint;
            velocityDelta = fromTo * velocity.magnitude;            
        }

        this.transform.position += velocityDelta;


        prevPosition = this.transform.position;
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
