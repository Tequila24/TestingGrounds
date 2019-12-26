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



    //velocities
    private Vector3 slideStrafe = Vector3.zero;
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

            TransformVelocity();
        }
    }


    private void Walking(Vector2 stepDirection, Vector3 lookDirection)
    {
        HeightAdjustment();


        Vector3 stepVelocity =  Quaternion.FromToRotation(Vector3.up, _surface.contactPointNormal) * 
                                Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z)) * 
                                new Vector3(stepDirection.x, 0, stepDirection.y) * 5;

        _rotation = Quaternion.Lerp(    _rotation,
                                        Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z)),
                                        0.5f);

        charBody.velocity = stepVelocity;
        this.transform.rotation = _rotation;
    }

    private void Freefall(Vector2 stepDirection, Vector3 lookDirection)
    {
    }

    private void SlideFall(Vector2 stepDirection, Vector3 lookDirection)
    {
        charBody.velocity -= slideStrafe;

        Quaternion rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.forward, new Vector3(stepDirection.x, 0, 0)), 0.01f);

        charBody.velocity = rotation * charBody.velocity;
    }

    private void HeightAdjustment()
    {
        float heightOffset = _surface.contactPoint.y +  1.0f - this.transform.position.y;
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

        switch (previousCharState)
        {
            case CharStates.Walk:
                break;

            case CharStates.FreeFall:
                break;

            case CharStates.SlideFall:
                //charBody.velocity -= slideStrafe;
                //slideStrafe = Vector3.zero;
                break;

            default:
            break;
        }


        switch (currentCharState)
        {
            case CharStates.Walk:
                charBody.useGravity = false;
                transformedVelocity = Vector3.ProjectOnPlane(charBody.velocity, _surface.contactPointNormal);
                break;

            case CharStates.FreeFall:
                charBody.useGravity = true;
                //charBody.velocity += _velocity;
                break;

            case CharStates.SlideFall:
                charBody.useGravity = true;
                //transformedVelocity = Vector3.ProjectOnPlane(_velocity, _surface.contactPointNormal);
                break;

            default:
            break;
        }

        //_velocity = Vector3.zero;
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