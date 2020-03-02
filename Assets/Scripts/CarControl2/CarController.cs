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
                Vector3 forceCalculated = Vector3.Project(-wheel.SpringForce / vehicleBody.mass, sharedNormal);

                vehicleBody.AddForceAtPosition( forceCalculated, wheel.RestPoint, ForceMode.VelocityChange);

                Vector3 restPointVelocity = Vector3.Project(vehicleBody.GetPointVelocity(wheel.RestPoint), sharedNormal) * wheel.dampingValue * Time.deltaTime * wheels.Count;
                vehicleBody.AddForceAtPosition( -restPointVelocity, wheel.RestPoint, ForceMode.VelocityChange );
            }

        }
    }

    void ApplyCarBodyPhysics()
    {
        
    }


    void UpdateWheelsNormal()
    {
        sharedNormal = this.transform.up;


        Vector3 middlePoint = Vector3.zero;
        foreach (WheelController wheel in wheels)
        {
            middlePoint += (this.transform.position + wheel.RestPoint);
        }
        middlePoint /= wheels.Count;



        Vector3 localFirst;
        Vector3 localSecond;
        Vector3 normal;
        for(int i = 0; i < (wheels.Count-1); i++)
        {
            localFirst = wheels[i].transform.position - middlePoint;
            localSecond = wheels[i+1].transform.position - middlePoint;
            normal = Vector3.Cross(localFirst, localSecond).normalized;
            sharedNormal += normal;

            //Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.blue, Time.deltaTime, false);
        }
        localFirst = wheels[wheels.Count-1].transform.position - middlePoint;
        localSecond = wheels[0].transform.position - middlePoint;
        normal = Vector3.Cross(localFirst, localSecond).normalized;
        sharedNormal += normal;
        //Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.blue, Time.deltaTime, false);

        sharedNormal = sharedNormal.normalized;

        //Debug.DrawRay(middlePoint, sharedNormal*2, Color.blue, Time.deltaTime, false);
    }

}
    