using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class CarController : MonoBehaviour
{

    private List<WheelController> wheels = new List<WheelController>();
    private Vector3 sharedNormal = Vector3.zero;

    private Rigidbody vehicleBody = null;



    void Awake()
    {
        vehicleBody = this.gameObject.GetComponent<Rigidbody>();

        GetWheels();
    }

    void GetWheels()
    {
        SortedList<float, WheelController> wheelsInOrder = new SortedList<float, WheelController>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            WheelController wheel = this.transform.GetChild(i).GetComponent<WheelController>();
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

                // Apply suspension forces
                Vector3 springForce = Vector3.Project(-wheel.SpringForce, sharedNormal)/* / vehicleBody.mass*/;
                vehicleBody.AddForceAtPosition( springForce, wheel.RestPoint, ForceMode.Impulse);

                // Apply velocity damping
                Vector3 verticalRestPointVelocity = Vector3.Project(restPointVelocity, sharedNormal) * wheel.dampingValue * wheels.Count;
                vehicleBody.AddForceAtPosition( -verticalRestPointVelocity, wheel.RestPoint, ForceMode.VelocityChange );

                // Apply side friction
                Vector3 sideSlideVelocity = Vector3.Project(restPointVelocity, wheel.SideDirection) * wheels.Count;
                vehicleBody.AddForceAtPosition(-sideSlideVelocity, wheel.RestPoint, ForceMode.VelocityChange);

                // Apply gear friction and losses
                if (wheel.isDrive) {
                    //Vector3 gearLosses = Vector3.Project(restPointVelocity, wheel.ForwardDirection) * 0.5f * wheels.Count;
                    Vector3 gearLosses = Vector3.Project(vehicleBody.velocity * Time.deltaTime, wheel.ForwardDirection) * 0.5f * wheels.Count;
                    vehicleBody.AddForceAtPosition( -gearLosses, wheel.RestPoint, ForceMode.VelocityChange);
                }

                // Remove fat
                print(vehicleBody.velocity.ToString("F4"));
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