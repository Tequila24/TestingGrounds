using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelControl : MonoBehaviour
{
    public bool isDrive = false;
    public bool isSteerable = false;
    [HideInInspector]
    public bool isGrounded = false;
    [HideInInspector]
    public bool isFloored = false;

    [HideInInspector]
    public Vector3 restPoint = Vector3.zero;
    [HideInInspector]
    public Vector3 strut = Vector3.zero;
    [HideInInspector]
    public Vector3 defaultStrut = Vector3.zero;
    [HideInInspector]
    public float offsetFromRestPoint = 0;
    [HideInInspector]
    public Vector3 velocityOnStrut = Vector3.zero;
    [HideInInspector]
    public MeshCollider meshCollider = null;
    [HideInInspector]
    public Vector3 depenetrationInNextFrame = Vector3.zero;
    [HideInInspector]
    public Vector3 surfaceNormal = Vector3.zero;
    [HideInInspector]
    public Vector3 restPointVelocity = Vector3.zero;
    [HideInInspector]
    public Vector3 lateralDirection = Vector3.zero;
    [HideInInspector]
    public Vector3 forwardDirection = Vector3.zero;
    [HideInInspector]
    public float axialRotationAngle = 0;
    [HideInInspector]
    public float steeringAngle = 0;


    public float wheelMass = 1;
    [Range (0.01f ,0.99f)]
    public float dampingValue = 0.5f;
    [Range (0.01f ,0.99f)]
    public float RubberTractionValue = 0.75f;
    public float springValue = 1;
    public float StrutToTop = 1;    
    public float StrutToBottom = -1;
    public float CasterAngle = 0;
    public float CamberAngle = 0;
    public float MaxSteerAngle = 20;
    

    [HideInInspector]
    public bool isRight = false;
    [HideInInspector]
    public float checkBoxDistance = 1;
    [HideInInspector]
    public float wheelRadius = 1;

}
