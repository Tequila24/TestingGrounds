using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotationControl
{
    static public float GetSurfaceVelocity(Dictionary<GameObject, RaycastHit> surfaces, WheelCollisionDetection.WheelCheckData wheelData)
    {
        Vector3 summSurfaceVelocity = Vector3.zero;

        foreach (GameObject surface in surfaces.Keys)
        {
            RaycastHit contact = surfaces[surface];
            
            Rigidbody surfaceBody = surface.GetComponent<Rigidbody>();
            if (surfaceBody == null)
                continue;

            Vector3 surfacePointVelocity = surfaceBody.GetPointVelocity(contact.point);

            Vector3 forwardDirection = ( Vector3.Cross( Vector3.ProjectOnPlane(contact.normal, wheelData.wheelCollider.transform.right),
                                                        wheelData.wheelCollider.transform.right) ).normalized;

            Vector3 rotatedVelocity = Quaternion.FromToRotation(forwardDirection, Vector3.up) * Vector3.Project(surfacePointVelocity, forwardDirection);

            summSurfaceVelocity += rotatedVelocity;

            Debug.DrawRay(contact.point, rotatedVelocity, Color.blue, Time.deltaTime, false);
        }

        Debug.DrawRay(wheelData.wheelCollider.transform.position, summSurfaceVelocity, Color.red, Time.deltaTime, false);

        float torque = summSurfaceVelocity.magnitude * Vector3.Dot(summSurfaceVelocity, Vector3.up);

        return torque;
    }
}
