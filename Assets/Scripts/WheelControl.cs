using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelControl : MonoBehaviour
{
    public bool isDrive = false;
    public bool isSteerable = false;
    public bool isGrounded = false;
    public bool isFloored = false;

    public Vector3 restPoint = Vector3.zero;
    public Vector3 strut = Vector3.zero;
    public Vector3 defaultStrut = Vector3.zero;
    public float offsetFromRestPoint = 0;
    public Vector3 velocityOnStrut = Vector3.zero;
    public MeshCollider meshCollider = null;
    public Vector3 depenetrationInNextFrame = Vector3.zero;
    public Vector3 surfaceNormal = Vector3.zero;
    public Vector3 restPointVelocity = Vector3.zero;
    public float axialRotationAngle = 0;
    public float steeringAngle = 0;

    public float wheelMass = 1;
    [Range (0.01f ,0.99f)]
    public float dampingValue = 0.5f;
    public float springValue = 1;
    public float StrutToTop = 1;    
    public float StrutToBottom = -1;
    public float CasterAngle = 0;
    public float CamberAngle = 0;


    public bool isRight = false;
    public float checkBoxDistance = 1;
    public float wheelRadius = 1;

}
