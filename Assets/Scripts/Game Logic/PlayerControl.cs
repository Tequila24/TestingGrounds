using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// reading inputs
// passing inputs to selected character controller


public class PlayerControl : CharControl
{

    CameraManager _cameraManager;
    InputManager _inputManager;
    
    private Vector2 lookDirectionAngles = Vector2.zero;
    
    // Start is called before the first frame update
    void Awake()
    {
        _cameraManager = GameObject.Find("MasterObject").GetComponent<CameraManager>();
        _inputManager = GameObject.Find("MasterObject").GetComponent<InputManager>();
    }

    private void Start() 
    {
        Init();
    }


    // Update is called once per frame
    new void FixedUpdate()
    {
        UpdateInputs();

        _walkControl.Process(_stepDirection, _lookDirection);
    }

    void Update()
    {
        UpdateCamera();
    }


    void UpdateInputs()
    {
        //update character
        //  get controls

        _stepDirection = new Vector2(   _inputManager.IsKeyDown(KeyCode.D) - _inputManager.IsKeyDown(KeyCode.A),
                                        _inputManager.IsKeyDown(KeyCode.W) - _inputManager.IsKeyDown(KeyCode.S) );

        lookDirectionAngles += _inputManager.GetMouseDelta();

        _lookDirection = Quaternion.Euler(  lookDirectionAngles.y * -1,
                                            lookDirectionAngles.x,
                                            0 ) * Vector3.forward;

        Debug.DrawRay(this.transform.position, _lookDirection * 10, Color.red, Time.deltaTime);

        //  move char
        if ( _inputManager.IsKeyDown(KeyCode.Space) != 0 ) 
            _walkControl.TryJump();
    }


    void UpdateCamera()
    {
        // update camera
        _cameraManager.UpdateCameraPosition(this.transform.position + this.transform.up);
        _cameraManager.UpdateCameraRotation(Quaternion.Euler(lookDirectionAngles.y * -1, lookDirectionAngles.x, 0));
    }
}
