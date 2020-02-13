using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourWheelMaster : MonoBehaviour
{
    [SerializeField]
    Mesh wheelMesh = null;
    [SerializeField]
    Mesh wheelColliderMesh = null;
    [SerializeField]
    Material wheelMaterial = null;

    [SerializeField]
    Vector3 frontAxisOffset = Vector3.zero;
    [SerializeField]
    float frontAxis = 0;
    [SerializeField]
    float CasterAngle = 0;
    [SerializeField]
    Vector3 rearAxisOffset = Vector3.zero;

    [SerializeField]
    float springValue = 0;
    [SerializeField]
    [Range (0.1f ,0.9f)]
    float dampingValue = 0;
    [SerializeField]
    float CamberAngle = 0;
    [SerializeField]
    float CasterAngle = 0;
    


    // related objects references
    GameObject frontRight = null;
    GameObject frontLeft = null;
    GameObject rearRight = null;
    GameObject rearLeft = null;

    Rigidbody carBody = null;




    bool awake = false;


    void Awake()
    {
        awake = true;
        carBody = this.gameObject.transform.parent.Find("CarBody").GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        
    }


    void GetWheelsGameObjects()
    {

        frontRight = GetInitWheelObject("FrontRight");
        frontLeft = GetInitWheelObject("FrontLeft");
        rearRight = GetInitWheelObject("RearRight");
        rearLeft = GetInitWheelObject("RearLeft");
        
    }

    GameObject GetInitWheelObject(string wheelName)
    {
        Transform wheel = null;
        GameObject wheelObject;

        wheel = this.transform.Find(wheelName);

        if (wheel == null) {

            wheelObject = new GameObject(wheelName);
            wheelObject.transform.SetParent(this.transform);

            wheelObject.AddComponent<MeshFilter>().mesh = wheelMesh;

            wheelObject.AddComponent<MeshRenderer>().material = wheelMaterial;

            MeshCollider collider = wheelObject.AddComponent<MeshCollider>();
            collider.sharedMesh = wheelColliderMesh;
            collider.convex = true;
            collider.isTrigger = true;
            

        } else {
            wheelObject = wheel.gameObject;
        }
        return wheelObject;
    }

    void OnDrawGizmos()
    {
        if (awake)
            return;



        GetWheelsGameObjects();

        carBody = this.gameObject.transform.parent.Find("CarBody").GetComponent<Rigidbody>();
        Quaternion toLeftRotation = Quaternion.AngleAxis(180, carBody.transform.up);
        


        //front
        frontRight.transform.position = this.transform.position + frontAxisOffset;
        frontRight.transform.rotation = carBody.transform.rotation;

        frontLeft.transform.position = this.transform.position + Vector3.Scale(frontAxisOffset, new Vector3(-1, 1, 1));
        frontLeft.transform.rotation = toLeftRotation * carBody.transform.rotation;

        //rear
        rearRight.transform.position = this.transform.position + rearAxisOffset;
        rearRight.transform.rotation = carBody.transform.rotation;

        rearLeft.transform.position = this.transform.position + Vector3.Scale(rearAxisOffset, new Vector3(-1, 1, 1));
        rearLeft.transform.rotation = toLeftRotation * carBody.transform.rotation;

        
    }
}
