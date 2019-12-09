using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreefallControl : MovementControl
{  
    private Vector3 inertia = Vector3.zero;

    public override void TransformParams(Vector3 oldVelocity, Quaternion oldRotation)
    {
        inertia = oldVelocity;
    }


    public override void Process()
    {
        _velocity = Vector3.ClampMagnitude( Vector3.Slerp(_velocity, Physics.gravity * 2, 0.3f) + Vector3.Slerp(inertia, Vector3.zero, 0.7f)
                                            , 150f);
    }
}
