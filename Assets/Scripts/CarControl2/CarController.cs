using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class CarController : MonoBehaviour
{

    private List<WheelController> wheels = new List<WheelController>();
    private Vector3 sharedNormal = Vector3.zero;




    void Awake()
    {
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
    }


    void UpdateWheelsNormal()
    {
        sharedNormal = this.transform.up;


        Vector3 middlePoint = Vector3.zero;
        foreach (WheelController wheel in wheels)
        {
            middlePoint += wheel.transform.position;
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

            Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, localFirst, Color.yellow, Time.deltaTime, false);
            Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, localSecond, Color.yellow, Time.deltaTime, false);

            Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.yellow, Time.deltaTime, false);
        }
        localFirst = wheels[wheels.Count-1].transform.position - middlePoint;
        localSecond = wheels[0].transform.position - middlePoint;
        normal = Vector3.Cross(localFirst, localSecond).normalized;
        sharedNormal += normal;
        Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.yellow, Time.deltaTime, false);

        sharedNormal = sharedNormal.normalized;

        //Debug.DrawRay(middlePoint, sharedNormal*2, Color.blue, Time.deltaTime, false);
    }

}
