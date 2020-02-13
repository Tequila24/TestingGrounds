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
    

    [SerializeField]
    private float wheelMass = 1;
    [SerializeField]
    [Range (0.01f ,0.99f)]
    private float dampingValue = 0.5f;
    [SerializeField]
    private float springValue = 1;
    [SerializeField]
    private float StrutToTop = 0;    
    [SerializeField]
    private float StrutToBottom = 0;
    [SerializeField]
    private float CasterAngle = 0;
    [SerializeField]
    private float CamberAngle = 0;


    private bool isWheelRight = false;
}
