using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitchAssCar : MonoBehaviour
{

    private Rigidbody carBody;

    private List<WheelController> allWheels = new List<WheelController>();
    private List<WheelController> driveWheels = new List<WheelController>();
    private List<WheelController> steerableWheels = new List<WheelController>();


    private bool grounded = false;

    public float throttle = 0;
    public float steer = 0;
    public bool brake = false;
    


    // Start is called before the first frame update
    void Awake()
    {
        carBody = this.gameObject.transform.Find("CarBody").GetComponent<Rigidbody>();

        Transform wheelsContainer = this.gameObject.transform.Find("Wheels");
        
        for(int i = wheelsContainer.childCount; i > 0; --i) {

            WheelController wheelControl = wheelsContainer.GetChild(i-1).gameObject.GetComponent<WheelController>();

            allWheels.Add(wheelControl);

            if (wheelControl.isDrive)
                driveWheels.Add(wheelControl);
            
            if (wheelControl.isSteerable)
                steerableWheels.Add(wheelControl);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateGrounded();

        UpdateInput();

        ApplyTraction();

        if (grounded) {
            if (Mathf.Abs(throttle) > 0.1f) {
                foreach (WheelController control in driveWheels)
                {
                    carBody.AddForceAtPosition(carBody.transform.forward * throttle * 0.1f, control.transform.position, ForceMode.VelocityChange);
                }
            } else {
                foreach (WheelController control in allWheels)
                {
                    carBody.AddForceAtPosition(-carBody.velocity * Time.deltaTime * 0.5f, control.transform.position, ForceMode.VelocityChange);
                }
            }

            if (Mathf.Abs(steer) > 0.01f) {
                foreach (WheelController control in steerableWheels)
                {
                    carBody.AddForceAtPosition(carBody.transform.right * steer * 0.1f, control.transform.position, ForceMode.VelocityChange);
                }
            } else {
                
            }

        }
    }

    void UpdateGrounded()
    {
        grounded = false;

        foreach (WheelController control in allWheels)
        {
            if (control.isGrounded) {
                grounded = true;
                break;
            }
        }
    }

    void UpdateInput()
    {

            throttle = Mathf.Lerp(throttle, Input.GetAxisRaw("Vertical"), 0.051f);
            steer = Mathf.Lerp(steer, Input.GetAxisRaw("Horizontal"), 0.051f);

            if (Input.GetKey("space") )
                brake = true;
            else
                brake = false;

    }
    
    void ApplyTraction()
    {
        bool grounded = false;

        foreach (WheelController control in allWheels)
        {
            if (control.isGrounded) {
                grounded = true;
                break;
            }
        }

        if (grounded) {
            carBody.velocity = Vector3.ProjectOnPlane(carBody.velocity, carBody.transform.right);
            carBody.angularVelocity -= carBody.angularVelocity * 0.5f;
        }
    }

}
