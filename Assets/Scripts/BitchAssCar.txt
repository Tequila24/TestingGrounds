using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitchAssCar : MonoBehaviour
{

    private Rigidbody carBody;

    private List<WheelControllerOld> allWheels = new List<WheelControllerOld>();
    private List<WheelControllerOld> driveWheels = new List<WheelControllerOld>();
    private List<WheelControllerOld> steerableWheels = new List<WheelControllerOld>();


    private bool grounded = false;

    public float throttle = 0;
    public float steer = 0;
    public float brakeFactor = 0;
    


    // Start is called before the first frame update
    void Awake()
    {
        carBody = this.gameObject.transform.Find("CarBody").GetComponent<Rigidbody>();

        Transform wheelsContainer = this.gameObject.transform.Find("Wheels");
        
        for(int i = wheelsContainer.childCount; i > 0; --i) {

            WheelControllerOld wheelControl = wheelsContainer.GetChild(i-1).gameObject.GetComponent<WheelControllerOld>();

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
            /*if (Mathf.Abs(throttle) > 0.1f) {
                foreach (WheelControllerOld control in driveWheels)
                {
                    carBody.AddForceAtPosition(carBody.transform.forward * throttle * 0.1f, control.transform.position, ForceMode.VelocityChange);
                }
            } else {
                foreach (WheelControllerOld control in allWheels)
                {
                    carBody.AddForceAtPosition(-carBody.velocity * Time.deltaTime * 0.5f, control.transform.position, ForceMode.VelocityChange);
                }
            }*/

            if (steer != 0) {
                
                foreach (WheelControllerOld control in steerableWheels)
                {
                    control.steerAngle = Mathf.Clamp(control.steerAngle + steer, -45, 45);;
                    //carBody.AddForceAtPosition(carBody.transform.right * steer * 0.1f, control.transform.position, ForceMode.VelocityChange);
                }
            } else {
                foreach (WheelControllerOld control in steerableWheels)
                {
                    control.steerAngle = Mathf.Lerp(control.steerAngle, 0, 0.1f);
                }
            }

        }
    }

    void UpdateGrounded()
    {
        grounded = false;

        foreach (WheelControllerOld control in allWheels)
        {
            if (control.isGrounded) {
                grounded = true;
                break;
            }
        }
    }

    void UpdateInput()
    {

            throttle = Input.GetAxisRaw("Vertical");
            steer = Input.GetAxisRaw("Horizontal");

            if (Input.GetKey("space") )
                brakeFactor = 1;
            else
                brakeFactor = 0;

    }
    
    void ApplyTraction()
    {
        /*if (grounded) {
            carBody.velocity = Vector3.ProjectOnPlane(carBody.velocity, carBody.transform.right);
            carBody.angularVelocity -= carBody.angularVelocity * 0.5f;
        }*/

        /*foreach (WheelControllerOld control in allWheels)
        {
            if (control.isGrounded) {

                Vector3 carSlipVelocity = Vector3.ProjectOnPlane(carRestPointVelocity, carBody.transform.up);
                carBody.AddForceAtPosition(-carSlipVelocity, carBody.position + carBody.rotation * localRestPoint, ForceMode.VelocityChange);

                break;
            }
        }*/

    }

}
