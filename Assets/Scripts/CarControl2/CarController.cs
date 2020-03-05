using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class CarController : MonoBehaviour
{
    public float EnginePower = 200;

    private List<WheelController> wheels = new List<WheelController>();
    private Vector3 sharedNormal = Vector3.zero;

    private Rigidbody vehicleBody = null;



    void Awake()
    {
        Debug.Log("Awaken");
        vehicleBody = this.gameObject.GetComponent<Rigidbody>();

        GetWheels();
    }

    void GetWheels()
    {
        SortedList<float, WheelController> wheelsInOrder = new SortedList<float, WheelController>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            WheelController wheel = this.transform.GetChild(i).GetComponent<WheelController>();
            if (wheel == null)
                continue;
            Vector3 wheelLocalPos = wheel.transform.position - this.transform.position;
            float angle = Vector3.SignedAngle(this.transform.forward, wheelLocalPos, this.transform.up);
            if (angle<0)
                angle+=360;
            wheelsInOrder.Add(angle, wheel);
        }


        ICollection<float> angles = wheelsInOrder.Keys;
        foreach (float angle in angles)
        {
            wheels.Add(wheelsInOrder[angle]);
        }

    }

    void FixedUpdate()
    {
        UpdateWheelsNormal();

        foreach (WheelController wheel in wheels)
        {
            if (wheel.IsGrounded) {

                Vector3 restPointVelocity = vehicleBody.GetPointVelocity(wheel.RestPoint) * Time.deltaTime;


                // Apply gear friction and losses
                if (wheel.isDrive) {
                    //Vector3 gearLosses = Vector3.Project(vehicleBody.velocity, wheel.ForwardDirection) * 0.02f * 4;
                    //vehicleBody.velocity -= gearLosses;
                    Vector3 gearLosses = Vector3.Project(restPointVelocity, wheel.ForwardDirection) * 0.1f * wheels.Count *2;
                    vehicleBody.AddForceAtPosition( -gearLosses, wheel.RestPoint, ForceMode.VelocityChange);
                }

                // Apply suspension forces
                Vector3 springForce = Vector3.Project(-wheel.SpringForce, sharedNormal)/* / vehicleBody.mass*/;
                vehicleBody.AddForceAtPosition( springForce, wheel.RestPoint, ForceMode.Impulse);

                // Apply side friction
                Vector3 sideSlideVelocity = Vector3.Project(restPointVelocity, wheel.SideDirection) * wheels.Count;
                vehicleBody.AddForceAtPosition(-sideSlideVelocity, wheel.RestPoint, ForceMode.VelocityChange);


                // GRAVITY ISSUES (?)
                // Apply 
                Vector3 sideGrav = Vector3.ProjectOnPlane(Physics.gravity * Time.deltaTime, sharedNormal) * 0.25f;
                vehicleBody.AddForce(-sideGrav, ForceMode.VelocityChange);
                //print(wheel.gameObject.name + " " + wheel.transform.position.y);
                //print(this.gameObject.name + " " + vehicleBody.velocity.ToString("F4"));


                // Apply velocity damping
                Vector3 verticalRestPointVelocity = Vector3.Project(restPointVelocity, sharedNormal) * wheel.dampingValue * wheels.Count;
                vehicleBody.AddForceAtPosition( -verticalRestPointVelocity, wheel.RestPoint, ForceMode.VelocityChange );




                float throttle = Input.GetAxis("Vertical");
                float steer = Input.GetAxis("Horizontal");
                float brake = Input.GetKey("space") ? 1 : 0;

                if (wheel.isDrive) 
                {
                    Vector3 driveForce = wheel.ForwardDirection * throttle * EnginePower * Time.deltaTime;
                    vehicleBody.AddForceAtPosition( driveForce, wheel.transform.position, ForceMode.Impulse);
                }

                if (wheel.isSteerable) 
                {
                    wheel.Steer = Mathf.Lerp(wheel.Steer, wheel.MaxSteeringAngle * steer, 0.1f);
                }



                
                
            }

        }
    }


    void UpdateWheelsNormal()
    {
        sharedNormal = this.transform.up;


        Vector3 middlePoint = Vector3.zero;
        foreach (WheelController wheel in wheels)
        {
            middlePoint += (wheel.RestPoint);
        }
        middlePoint /= wheels.Count;



        Vector3 localFirst;
        Vector3 localSecond;
        Vector3 normal;
        for(int i = 0; i < (wheels.Count-1); i++)
        {
            localFirst = wheels[i].SurfacePoint - middlePoint;
            localSecond = wheels[i+1].SurfacePoint - middlePoint;
            normal = Vector3.Cross(localFirst, localSecond).normalized;
            sharedNormal += normal;

            Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.yellow, Time.deltaTime, false);
        }
        localFirst = wheels[wheels.Count-1].SurfacePoint - middlePoint;
        localSecond = wheels[0].SurfacePoint - middlePoint;
        normal = Vector3.Cross(localFirst, localSecond).normalized;
        sharedNormal += normal;
        Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.yellow, Time.deltaTime, false);

        sharedNormal = (sharedNormal / wheels.Count).normalized;

        Debug.DrawRay(middlePoint, Vector3.up*2, Color.blue, Time.deltaTime, false);
        Debug.DrawRay(middlePoint, sharedNormal*2, Color.yellow, Time.deltaTime, false);
    }

}