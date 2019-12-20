using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkControl : MonoBehaviour
{
    private CharSurfaceControl _surface = null;

    private Rigidbody charBody = null;

    protected Vector3 _velocity = Vector3.zero;
    protected Quaternion _rotation = Quaternion.identity;

    public Vector3 Velocity { get {return _velocity;} /*set {_velocity = value;}*/ }
    public Quaternion Rotation { get {return _rotation;} /*set {_rotation = value;}*/ }

    private enum CharStates : byte {    None = 0, FreeFall, SlideFall, Walk    };
    private CharStates previousCharState = CharStates.None;
    private CharStates currentCharState = CharStates.None;
    private bool isStateChanged = false;



    //velocities
    private Vector3 heightAdjust = Vector3.zero;
    private Vector3 inertiaVector = Vector3.zero;

    //private Vector3 fallVelocity = Vector3.zero;
    //private Vector3 slideVelocity = Vector3.zero;
    //==========

    private void Start() 
    {
        _surface = this.gameObject.GetComponent<CharSurfaceControl>();
        if (_surface == null) {
            _surface = this.gameObject.AddComponent<CharSurfaceControl>();
        }

        charBody = this.gameObject.GetComponent<Rigidbody>();
    }

    public void TryJump()
    {
        
    }


    public void Process(Vector2 stepDirection, Vector3 lookDirection)
    {
        if (_surface == null)
            return;

        UpdateCharState();

        switch (currentCharState)
        {
            case CharStates.FreeFall:
                Freefall(stepDirection, lookDirection);
                break;

            case CharStates.SlideFall:
                SlideFall(stepDirection, lookDirection);
                break;
            
            case CharStates.Walk:
                Walking(stepDirection, lookDirection);
                break;

            case CharStates.None:
            default:
            break;
        }
    }


    private void UpdateCharState()
    {
        previousCharState = currentCharState;
        
        if (_surface.surfaceObject == null) {
            currentCharState = CharStates.FreeFall;
        }

        if ( (_surface.surfaceObject != null) && (_surface.angleToGravity >= 45) )
        {
            currentCharState = CharStates.SlideFall;
        }

        if ( (_surface.surfaceObject != null) && (_surface.angleToGravity < 45) )
        {
            currentCharState = CharStates.Walk;
        }

        if (previousCharState != currentCharState) 
        {
            Debug.Log(previousCharState + "->" + currentCharState);
            isStateChanged = true;

            TransformVelocity();
        }
    }


    private void Walking(Vector2 stepDirection, Vector3 lookDirection)
    {
        HeightAdjustment();


        Vector3 stepVelocity =  Quaternion.FromToRotation(Vector3.up, _surface.contactPointNormal) * 
                                Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z)) * 
                                new Vector3(stepDirection.x, 0, stepDirection.y) * 5;

        inertiaVector = Vector3.Lerp(inertiaVector, Vector3.zero, 0.1f);

        _velocity = _surface.contactPointVelocity + stepVelocity + inertiaVector;
        _rotation = Quaternion.Lerp(    _rotation,
                                        Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z)),
                                        0.5f);

        charBody.velocity = _velocity;
        this.transform.rotation = _rotation;
    }

    private void Freefall(Vector2 stepDirection, Vector3 lookDirection)
    {
        Vector3 fallVelocity = Physics.gravity * 2;

        inertiaVector = Vector3.Lerp(inertiaVector, Vector3.zero, 0.05f);

        _velocity = Vector3.Lerp(_velocity, fallVelocity, 0.5f) + inertiaVector;
        _rotation = Quaternion.Lerp(    _rotation,
                                        Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z)),
                                        0.1f    );

        charBody.velocity = _velocity;
    }

    private void SlideFall(Vector2 stepDirection, Vector3 lookDirection)
    {
        Vector3 slideVelocity = Vector3.ProjectOnPlane(Physics.gravity * 1f, _surface.contactPointNormal);
        Vector3 strafe = Quaternion.FromToRotation(Vector3.forward, slideVelocity) * new Vector3(stepDirection.x, 0, 0) * 5;
        Debug.DrawRay(this.transform.position, strafe * 4, Color.blue, 100);
        Debug.DrawRay(this.transform.position + Vector3.up, inertiaVector * 4, Color.yellow, 100);
        
        inertiaVector = Vector3.Lerp(inertiaVector, Vector3.zero, 0.04f);

        _velocity = Vector3.Lerp(_velocity, slideVelocity + strafe, 0.5f) + inertiaVector;
        
        charBody.velocity = _velocity;
    }

    private void HeightAdjustment()
    {
        float heightOffset = _surface.contactPoint.y +  1.0f - this.transform.position.y;
        Debug.Log(heightOffset);
        if (heightOffset > 0.1f) {
            heightOffset+= 0.1f;
            Vector3 adjustVector = new Vector3(0, heightOffset, 0);
            float angleToSurface = Vector3.Angle(Physics.gravity, -_surface.contactPointNormal) * Mathf.Deg2Rad;
            Quaternion rotationToSurface = Quaternion.FromToRotation(Physics.gravity, -_surface.contactPointNormal);
            /*heightAdjust = Vector3.Lerp(   heightAdjust,
                                            ( rotationToSurface * adjustVector * Mathf.Cos(angleToSurface) ), 
                                            0.8f    );*/
            heightAdjust = (rotationToSurface * adjustVector * Mathf.Cos(angleToSurface)) * Time.deltaTime;

            this.transform.position += heightAdjust;
        }
    }

    private void TransformVelocity() 
    {
        Vector3 transformedVelocity = Vector3.zero;


        switch (currentCharState)
        {
            case CharStates.Walk:
                transformedVelocity = Vector3.ProjectOnPlane(_velocity, _surface.contactPointNormal);
                break;

            case CharStates.FreeFall:
                transformedVelocity = _velocity;
                break;

            case CharStates.SlideFall:
                transformedVelocity = Vector3.ProjectOnPlane(_velocity, _surface.contactPointNormal);
                break;

            default:
            break;
        }

        _velocity = Vector3.zero;
        inertiaVector = transformedVelocity;
    }




}


/*
Debug.DrawRay(charBody.velocity, _velocity, Color.red, 10f);
Debug.DrawRay(charBody.velocity, transformedVelocity, Color.blue, 10f);
*/

/*\
*/

/*
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(oldVelocity, _surface.contactPointNormal);
            Debug.DrawRay(charBody.velocity, horizontalVelocity * 10, Color.green, Time.deltaTime);   
            transformedVelocity = horizontalVelocity;
*/