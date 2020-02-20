using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideTest : MonoBehaviour
{

    MeshCollider thisCollider = null;
    float checkBoxDistance = 0;

    // Start is called before the first frame update
    void Awake()
    {
        thisCollider = this.gameObject.GetComponent<MeshCollider>();
        checkBoxDistance = thisCollider.bounds.extents.magnitude;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 surfacePenetration = Vector3.zero;
        Collider[] surfaces = new Collider[16];

        int count = Physics.OverlapSphereNonAlloc(thisCollider.transform.position, checkBoxDistance, surfaces);

        for (int i=0; i<count; ++i)
        {
            Collider otherCollider = surfaces[i];

            if (otherCollider == thisCollider)
                continue;

            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(   thisCollider, thisCollider.transform.position, thisCollider.transform.rotation,
                                                            otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                                                            out direction, out distance);

            if (overlapped)
            {
                surfacePenetration += direction * distance;
            }
            
            Debug.DrawRay(this.transform.position, surfacePenetration, Color.red, Time.deltaTime, false);
        }

    }



}
