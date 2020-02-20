using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




public class WheelMaster : MonoBehaviour
{
    Rigidbody carBody = null;

    List<WheelControl> wheels = new List<WheelControl>();
    Vector3 sharedNormal = Vector3.up;

    DepenCalc depenCalc = new DepenCalc();


    void Awake()
    {
        carBody = this.gameObject.transform.parent.Find("CarBody").GetComponent<Rigidbody>();

        GetWheels();
        InitWheels();

        depenCalc.ignoreList.Add(carBody.gameObject);
        foreach (WheelControl wheel in wheels)
        {
            depenCalc.ignoreList.Add(wheel.gameObject);
        }

    }

    
    void GetWheels()
    {
        SortedList<float, WheelControl> wheelsInOrder = new SortedList<float, WheelControl>();

        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            WheelControl wheel = this.gameObject.transform.GetChild(i).GetComponent<WheelControl>();
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

    void InitWheels()
    {
        if (wheels.Count == 0)
            return;
        foreach (WheelControl wheel in wheels)
        {
            wheel.restPoint = Quaternion.Inverse(carBody.rotation) * (wheel.transform.position - carBody.transform.position);
            
            wheel.meshCollider = wheel.gameObject.GetComponent<MeshCollider>();

            wheel.checkBoxDistance = wheel.meshCollider.bounds.extents.magnitude;
            
            wheel.isRight = (Vector3.Angle(carBody.transform.right, wheel.transform.right) > 90) ? true : false;

            Quaternion casterRotation = Quaternion.AngleAxis(-wheel.CasterAngle, Vector3.right);
            Quaternion camberRotation = wheel.isRight? Quaternion.AngleAxis(-wheel.CamberAngle, Vector3.forward) : Quaternion.AngleAxis(wheel.CamberAngle, Vector3.forward);

            wheel.defaultStrut = camberRotation * casterRotation * Vector3.up;
        }
    }

    void FixedUpdate()
    {
        UpdateWheelsNormal();


        foreach (WheelControl wheel in wheels)
        {
            UpdateWheelPosition(wheel);

            UpdateWheelRotation(wheel);

            ApplyCarPhysics(wheel);
        }

    }

    void UpdateWheelPosition(WheelControl wheel)
    {
        wheel.strut = (carBody.transform.rotation * (wheel.defaultStrut)).normalized;

        wheel.restPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * wheel.restPoint) ) * Time.deltaTime;

        Vector3 springAcceleration = (-wheel.strut * (wheel.offsetFromRestPoint * wheel.springValue) + Physics.gravity) / wheel.wheelMass * Time.deltaTime;
        wheel.velocityOnStrut += springAcceleration;

        DepenCalc.CollisionCheckInfo newCheck = new DepenCalc.CollisionCheckInfo(wheel.meshCollider, wheel.transform.position, wheel.transform.rotation, wheel.checkBoxDistance);
        wheel.depenetrationInNextFrame = depenCalc.GetDepenetration(newCheck);
        wheel.velocityOnStrut += wheel.depenetrationInNextFrame;
        
        
        RaycastHit hit;
        if (Physics.Raycast(wheel.transform.position, -wheel.depenetrationInNextFrame, out hit, wheel.checkBoxDistance))
        {
            wheel.surfaceNormal = hit.normal;
            Debug.DrawRay(hit.point, wheel.surfaceNormal, Color.yellow, Time.deltaTime, false);
        }

        wheel.offsetFromRestPoint += ( Quaternion.FromToRotation(wheel.strut, Vector3.up) * (wheel.velocityOnStrut) ).y;


        if ( (wheel.offsetFromRestPoint > wheel.StrutToTop) || (wheel.offsetFromRestPoint < wheel.StrutToBottom) )
            wheel.isFloored = true;
        else 
            wheel.isFloored = false;


        wheel.offsetFromRestPoint = Mathf.Clamp(wheel.offsetFromRestPoint, wheel.StrutToBottom, wheel.StrutToTop);
        

        wheel.transform.position =  carBody.position + carBody.rotation * wheel.restPoint +
                                    wheel.restPointVelocity + 
                                    wheel.strut * wheel.offsetFromRestPoint;


        wheel.velocityOnStrut -= wheel.velocityOnStrut * wheel.dampingValue;

        wheel.isGrounded = wheel.depenetrationInNextFrame.sqrMagnitude > 0 ? true : false;

    }

    void UpdateWheelRotation(WheelControl wheel)
    {
        //Quaternion velocityRotation = 
        Quaternion carRotation = (wheel.isRight ? Quaternion.AngleAxis(180, carBody.transform.up) * carBody.rotation : carBody.rotation);
        wheel.transform.rotation = carRotation;
    }

    void ApplyCarPhysics(WheelControl wheel)
    {
        ApplySuspension(wheel);

        ApplyFriction(wheel);

        ApplyDrive(wheel);
    }

    void ApplySuspension(WheelControl wheel)
    {
        if (wheel.isGrounded) {

            // apply spring
            Vector3 springForce = Vector3.Project(wheel.strut * ((wheel.offsetFromRestPoint * wheel.springValue)), sharedNormal);
            carBody.AddForceAtPosition(springForce, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.Impulse);

            // damp speed
            Vector3 carRelativeVerticalSpeed = Vector3.Project(wheel.restPointVelocity, sharedNormal) * wheel.dampingValue;
            if (wheel.isFloored) {
                carRelativeVerticalSpeed -= wheel.depenetrationInNextFrame;
            }
            carBody.AddForceAtPosition(-carRelativeVerticalSpeed, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
            
            
            
        }
    }

    void ApplyFriction(WheelControl wheel)
    {
        if (wheel.isGrounded) {
            Vector3 sideSlideVelocity = Vector3.Project(wheel.restPointVelocity, carBody.transform.right);
            Debug.DrawRay(wheel.transform.position, sideSlideVelocity, Color.red, Time.deltaTime, false);
            carBody.AddForceAtPosition(-sideSlideVelocity, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
            carBody.velocity -= Vector3.Project(Physics.gravity * Time.deltaTime * 0.25f, carBody.transform.right);
        }
    }

    void ApplyDrive(WheelControl wheel)
    {
        
    }

    void UpdateWheelsNormal()
    {
        sharedNormal = Vector3.zero;


        Vector3 middlePoint = Vector3.zero;
        foreach (WheelControl wheel in wheels)
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

            Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.blue, Time.deltaTime, false);
        }
        localFirst = wheels[wheels.Count-1].transform.position - middlePoint;
        localSecond = wheels[0].transform.position - middlePoint;
        normal = Vector3.Cross(localFirst, localSecond).normalized;
        sharedNormal += normal;
        Debug.DrawRay(middlePoint + (localFirst + localSecond)/2, normal, Color.blue, Time.deltaTime, false);

        sharedNormal = sharedNormal.normalized;

        Debug.DrawRay(middlePoint, sharedNormal*2, Color.blue, Time.deltaTime, false);
    }

    void OnDrawGizmos()
    {
        Handles.color = Color.white;
        foreach (WheelControl wheel in wheels)
        {
            Handles.DrawLine(   carBody.position + carBody.rotation * wheel.restPoint + wheel.strut * wheel.StrutToTop,
                                carBody.position + carBody.rotation * wheel.restPoint + wheel.strut * wheel.StrutToBottom );
        
            Handles.DrawLine(   carBody.position + carBody.rotation * (wheel.restPoint + wheel.transform.right*0.2f),
                                carBody.position + carBody.rotation * (wheel.restPoint - wheel.transform.right*0.2f) );

            Handles.DrawLine(   wheel.transform.position + carBody.rotation * (wheel.transform.right*0.2f),
                                wheel.transform.position + carBody.rotation * (-wheel.transform.right*0.2f) );
        }
    }
}
