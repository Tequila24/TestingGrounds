using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// controlling character
// enabling 


public class CharControl : MonoBehaviour
{
    CharSurfaceControl _surfaceControl;
    protected WalkControl _walkControl;



    protected Vector3 _lookDirection;
    protected Vector2 _stepDirection;
    public Vector3 LookDirection { get {return _lookDirection; } set {_lookDirection = value;} }
    public Vector3 StepDirection { get {return _stepDirection; } set {_stepDirection = value;} }


    private void Start() 
    {
        Init();
    }

    protected void Init()
    {
        _surfaceControl = this.gameObject.GetComponent<CharSurfaceControl>();
        if (_surfaceControl == null) {
            _surfaceControl = this.gameObject.AddComponent<CharSurfaceControl>();
        }


        _walkControl = this.gameObject.AddComponent<WalkControl>();


        _lookDirection = this.transform.forward;
        _stepDirection = Vector2.zero;
    }


    public void FixedUpdate()
    {
    }

/*
    private void UpdateCharState()
    {
        previousCharState = currentCharState;
        
        if (_surfaceControl.surfaceObject == null) {
            currentCharState = CharStates.FreeFall;
        }

        if (_surfaceControl.surfaceObject != null) {
            currentCharState = CharStates.Walk;
        }

        if (previousCharState != currentCharState)
        {
            Debug.Log(previousCharState + "->" + currentCharState);
            TransformVelocity();
        }
    }
*/

}
