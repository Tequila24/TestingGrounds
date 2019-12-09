using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControl : MonoBehaviour
{

    protected Vector3 _velocity = Vector3.zero;
    protected Quaternion _rotation = Quaternion.identity;

    public Vector3 Velocity { get {return _velocity;} /*set {_velocity = value;}*/ }
    public Quaternion Rotation { get {return _rotation;} /*set {_rotation = value;}*/ }

    public virtual void TransformParams(Vector3 oldVelocity, Quaternion oldRotation)
    {

    }


    public virtual void Process()
    {
        _velocity = Vector3.Slerp(_velocity, Vector3.zero, 0.1f);
        _rotation = Quaternion.Slerp(_rotation, Quaternion.identity, 0.1f);
    }
}
