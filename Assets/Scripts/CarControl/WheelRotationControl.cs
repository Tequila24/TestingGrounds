using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotationControl
{
    // Start is called before the first frame update
    Vector3 GetTorque(Dictionary<GameObject, RaycastHit> surfaces, WheelCheckData wheelData)
    {
        Vector3 summSurfaceMoments;

        foreach (GameObject surface in surfaces.keys)
        {
            Vector3 surfaceVelocity = Vector3.zero;
            
            Rigidbody surfaceBody = surface.GetComponent<Rigidbody>();
            if (surfaceBody = null)
                continue;

            surfaceBody.
        }
    }
}
