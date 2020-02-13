using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class WheelMaster : MonoBehaviour
{
    Rigidbody carBody = null;

    List<WheelControl> wheels = new List<WheelControl>();
    Vector3 sharedNormal = Vector3.up;



    void Awake()
    {
        carBody = this.gameObject.transform.parent.Find("CarBody").GetComponent<Rigidbody>();

        GetWheels();
    }

    
    void GetWheels()
    {
        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            WheelControl control = this.gameObject.transform.GetChild(i).GetComponent<WheelControl>();
            wheels.Add(control);
        }
    }



    void FixedUpdate()
    {
        foreach (WheelControl wheel in wheels)
        {
            UpdateWheelPosition();

            UpdateWheelRotation();

            ApplyCarPhysics();
        }
    }

    void UpdateWheelPosition()
    {

    }

    void UpdateWheelRotation()
    {

    }

    void ApplyCarPhysics()
    {
        UpdateWheelsNormal();
    }

    void UpdateWheelsNormal()
    {
        Vector3 wheelLocalPosition;
        Vector3 nextWheelLocalPosition;
        Vector3 cross;

        int i = 0;
        for (; i < (wheels.Count-1); i++)
        {
            wheelLocalPosition = wheels[i].transform.position - this.transform.position;
            nextWheelLocalPosition = wheels[i+1].transform.position - this.transform.position;
            cross = Vector3.Cross(wheelLocalPosition, nextWheelLocalPosition);
            sharedNormal += cross;
            //Debug.DrawRay(this.transform.position, cross, Color.yellow, Time.deltaTime*25f, false);
        }
        wheelLocalPosition = wheels[wheels.Count-1].transform.position - this.transform.position;
        nextWheelLocalPosition = wheels[0].transform.position - this.transform.position;
        cross = Vector3.Cross(wheelLocalPosition, nextWheelLocalPosition);
        sharedNormal += cross;
        Debug.DrawRay(this.transform.position, wheelLocalPosition, Color.blue, Time.deltaTime*25f, false);
        Debug.DrawRay(this.transform.position, nextWheelLocalPosition, Color.blue, Time.deltaTime*25f, false);
        Debug.DrawRay(this.transform.position, cross, Color.red, Time.deltaTime*25f, false);


        sharedNormal /= wheels.Count;

        Debug.DrawRay(this.transform.position, sharedNormal, Color.red, Time.deltaTime, false);
    }



    void OnDrawGizmos()
    {

    }
}
