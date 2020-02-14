using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WheelControl : MonoBehaviour
{
    public bool isDrive = false;
    public bool isSteerable = false;
    public bool isGrounded = false;
    public float steerAngle = 0;

    public Vector3 restPoint = Vector3.zero;
    public Vector3 strut = Vector3.zero;
    public float offsetFromRestPoint = 0;
    public Vector3 wheelVelocityOnStrut = Vector3.zero;


    [SerializeField]
    public float wheelMass = 1;
    [SerializeField]
    [Range (0.01f ,0.99f)]
    public float dampingValue = 0.5f;
    [SerializeField]
    public float springValue = 1;
    [SerializeField]
    public float StrutToTop = 0;    
    [SerializeField]
    public float StrutToBottom = 0;
    [SerializeField]
    public float CasterAngle = 0;
    [SerializeField]
    public float CamberAngle = 0;


    private bool isWheelRight = false;

}
