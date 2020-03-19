using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WheelCollisionDetection
{

    public struct WheelCheckData {

        public MeshCollider wheelCollider;
        private List<GameObject> ignoreList;
        public float wheelRadius;
        public float wheelWidth;
        public float  collisionCheckDistance;
        
        public WheelCheckData(MeshCollider newCollider)
        {
            this.wheelCollider = newCollider;
            this.ignoreList = new List<GameObject>();
            this.wheelRadius = newCollider.sharedMesh.bounds.extents.y * newCollider.transform.lossyScale.y;;
            this.wheelWidth = newCollider.sharedMesh.bounds.extents.x * newCollider.transform.lossyScale.x;
            this.collisionCheckDistance = Mathf.Sqrt(this.wheelRadius * this.wheelRadius + this.wheelWidth * this.wheelWidth);
        }

        public void Ignore(GameObject newObject)
        {
            ignoreList.Add(newObject);
        }

        public bool IsIgnored(GameObject newObject)
        {
            return ignoreList.Contains(newObject);
        }

    }


    


    static public void FindSurfaces(WheelCheckData checkParams, out Dictionary<GameObject, RaycastHit> surfaces)
    {
        var colliders = FindColliders(checkParams);

        surfaces = FindContactPoints(checkParams, colliders);
    }


    static private List<Collider> FindColliders(WheelCheckData newParams)
    {
        List<Collider> colliders = new List<Collider>();

        Collider[] sphereColliders = Physics.OverlapSphere( newParams.wheelCollider.transform.position, 
                                                            newParams.collisionCheckDistance);
        Collider[] boxColliders =  Physics.OverlapBox(  newParams.wheelCollider.transform.position, 
                                                        new Vector3(newParams.wheelWidth, newParams.wheelRadius, newParams.wheelRadius), 
                                                        newParams.wheelCollider.transform.rotation);

        for (int i = 0; i < sphereColliders.Length; i++)
        {
            Collider sphereItem = sphereColliders[i];

            if ( newParams.IsIgnored(sphereItem.gameObject) )
                continue;

            for (int j = 0; j < boxColliders.Length; j++)
            {
                Collider boxItem = boxColliders[j];
                if (sphereItem == boxItem)
                    colliders.Add(boxItem);
            }
        }

        return colliders;
    }

    
    static private Dictionary<GameObject, RaycastHit> FindContactPoints(WheelCheckData newParams, List<Collider> colliders)
    {
        Dictionary<GameObject, RaycastHit> surfacePoints = new Dictionary<GameObject, RaycastHit>();

        foreach (Collider collider in colliders)
        {
            Vector3 closestPointOnWheel = newParams.wheelCollider.ClosestPoint(collider.transform.position);
            Vector3 direction = closestPointOnWheel - newParams.wheelCollider.transform.position;

            RaycastHit surfaceHit;
            if ( Physics.Raycast(newParams.wheelCollider.transform.position, direction, out surfaceHit, newParams.collisionCheckDistance) ) {
                float distance = (surfaceHit.point - closestPointOnWheel).magnitude;
                surfaceHit.normal *= distance;
                surfacePoints.Add(collider.gameObject, surfaceHit);
            }
        }

        return surfacePoints;
    }

}