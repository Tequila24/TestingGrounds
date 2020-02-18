using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class DepenCalc
{
    public List<GameObject> ignoreList = new List<GameObject>();

    public struct CollisionCheckInfo {
        public Collider collider;
        public Vector3 colliderPosition;
        public Quaternion colliderRotation;
        public float checkBoxDistance;
        
        public CollisionCheckInfo(Collider newCollider, Vector3 newColliderPosition, Quaternion newColliderRotation, float newCheckBoxDistance)
        {
            this.collider = newCollider;
            this.colliderPosition = newColliderPosition;
            this.colliderRotation = newColliderRotation;
            this.checkBoxDistance = newCheckBoxDistance;
        }
    }

    
    public Vector3 GetDepenetration(CollisionCheckInfo newInfo)
    {
        Vector3 surfacePenetration = Vector3.zero;
        Collider[] surfaces = new Collider[16];

        int count = Physics.OverlapSphereNonAlloc(newInfo.colliderPosition, newInfo.checkBoxDistance, surfaces);

        if (count<2)
            return surfacePenetration;

        for (int i=0; i<count; ++i)
        {
            Collider collider = surfaces[i];

            if ( ignoreList.Contains(collider.gameObject) )
                continue;

            Vector3 otherPosition = collider.gameObject.transform.position;
            Quaternion otherRotation = collider.gameObject.transform.rotation;
            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(   newInfo.collider, newInfo.colliderPosition, newInfo.colliderRotation,
                                                            collider, otherPosition, otherRotation,
                                                            out direction, out distance);

            if (overlapped)
            {
                surfacePenetration += direction * distance;
            }
        }

        return surfacePenetration;
    }

    public Vector3 GetAllignedDepenetration(CollisionCheckInfo newInfo, Vector3 strut)
    {
        Vector3 depenetrationVector = GetDepenetration(newInfo);

        if (depenetrationVector.sqrMagnitude == 0)
            return Vector3.zero;


        float angle = Vector3.Angle(depenetrationVector, strut);
        float newScale = depenetrationVector.magnitude;

        if (angle < 90) {
            depenetrationVector = strut * newScale;
        } else {
            depenetrationVector = -strut * newScale;
        }

        return depenetrationVector;
    }
}