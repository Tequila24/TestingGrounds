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

        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            WheelControl control = this.gameObject.transform.GetChild(i).GetComponent<WheelControl>();
            wheels.Add(control);
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
            
            Vector3 strutTopPoint = Quaternion.Inverse(carBody.rotation) * 
                                    Quaternion.AngleAxis(wheel.CamberAngle, wheel.transform.forward) * 
                                    Quaternion.AngleAxis(-wheel.CasterAngle, this.transform.right) * 
                                    (wheel.restPoint + (this.transform.up * wheel.StrutToTop));


            Vector3 strutBottomPoint =  Quaternion.Inverse(carBody.rotation) * 
                                        Quaternion.AngleAxis(wheel.CamberAngle, wheel.transform.forward) * 
                                        Quaternion.AngleAxis(-wheel.CasterAngle, this.transform.right) * 
                                        (wheel.restPoint + (this.transform.up * wheel.StrutToBottom));


            wheel.defaultStrut = (carBody.rotation * (strutTopPoint - strutBottomPoint)).normalized;

            wheel.isRight = (Vector3.Angle(carBody.transform.right, wheel.transform.right) > 90) ? true : false;
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


        float throttle = Input.GetAxis("Vertical") * 0.1f;
        float steer = Input.GetAxis("Horizontal") * 3f;

        foreach (WheelControl wheel in wheels)
        {
            if (!wheel.isGrounded)
                continue;

            if (wheel.isDrive)
            {
                carBody.AddForceAtPosition(carBody.transform.forward * throttle, wheel.transform.position, ForceMode.VelocityChange);
            }

            if (wheel.isSteerable)
            {
                carBody.AddForceAtPosition(carBody.transform.right * (steer * Mathf.Clamp(carBody.velocity.magnitude, 0, 1) * Time.deltaTime), wheel.transform.position, ForceMode.VelocityChange);
            }
        }
    }

    void UpdateWheelPosition(WheelControl wheel)
    {
        wheel.strut = (carBody.transform.rotation * (wheel.defaultStrut)).normalized;

        Vector3 carRestPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * wheel.restPoint) ) * Time.deltaTime;

        // wheel position
        Vector3 springAcceleration = -wheel.strut * ((wheel.offsetFromRestPoint * wheel.springValue) / wheel.wheelMass) * Time.deltaTime;
        wheel.velocityOnStrut += springAcceleration;

        DepenCalc.CollisionCheckInfo newCheck = new DepenCalc.CollisionCheckInfo(wheel.meshCollider, wheel.transform.position, wheel.transform.rotation, wheel.checkBoxDistance);
        //wheel.depenetrationInNextFrame = depenCalc.GetAllignedDepenetration(newCheck, wheel.strut);
        wheel.depenetrationInNextFrame = depenCalc.GetDepenetration(newCheck);
        wheel.velocityOnStrut += wheel.depenetrationInNextFrame;
        
        
        RaycastHit hit;
        if (Physics.Raycast(wheel.transform.position, -wheel.depenetrationInNextFrame, out hit, wheel.checkBoxDistance))
        {
            wheel.surfaceNormal = Vector3.ProjectOnPlane(hit.normal, wheel.transform.right).normalized;
            Debug.DrawRay(hit.point, wheel.surfaceNormal, Color.yellow, Time.deltaTime, false);
        }

        wheel.offsetFromRestPoint += ( Quaternion.FromToRotation(wheel.strut, Vector3.up) * (wheel.velocityOnStrut) ).y;


        if ( (wheel.offsetFromRestPoint > wheel.StrutToTop) || (wheel.offsetFromRestPoint < wheel.StrutToBottom) )
            wheel.isFloored = true;
        else 
            wheel.isFloored = false;


        wheel.offsetFromRestPoint = Mathf.Clamp(wheel.offsetFromRestPoint, wheel.StrutToBottom, wheel.StrutToTop);
        

        wheel.transform.position =  carBody.position + carBody.rotation * wheel.restPoint +
                                    carRestPointVelocity + 
                                    wheel.strut * wheel.offsetFromRestPoint;


        wheel.velocityOnStrut -= wheel.velocityOnStrut * wheel.dampingValue;

        wheel.isGrounded = wheel.depenetrationInNextFrame.sqrMagnitude > 0 ? true : false;

    }

    void UpdateWheelRotation(WheelControl wheel)
    {
        Quaternion axisRotation = (wheel.isRight ? Quaternion.AngleAxis(180, carBody.transform.up) * carBody.rotation : carBody.rotation);
        wheel.transform.rotation = axisRotation;
    }

    void ApplyCarPhysics(WheelControl wheel)
    {
        // apply spring
        Vector3 springForce = Vector3.Project(wheel.strut * ((wheel.offsetFromRestPoint * wheel.springValue)), wheel.surfaceNormal);
        carBody.AddForceAtPosition(springForce, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.Impulse);

        

        if (wheel.isGrounded) {

            Vector3 carRestPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * wheel.restPoint)) * Time.deltaTime;

            // damp speed
            Vector3 carRelativeVerticalSpeed = Vector3.Project(carRestPointVelocity, wheel.strut) * wheel.dampingValue;
            if (wheel.isFloored) {
                carRelativeVerticalSpeed -= wheel.depenetrationInNextFrame;
            }
            carRelativeVerticalSpeed = Vector3.Project(carRelativeVerticalSpeed, wheel.surfaceNormal);
            carBody.AddForceAtPosition(-carRelativeVerticalSpeed, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
            
            // stop side sliding            
            //Vector3 slideVelocity = Vector3.Project(carRestPointVelocity + Physics.gravity * 0.25f * Time.deltaTime, wheel.transform.right);
            //carBody.AddForceAtPosition(-slideVelocity, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
        }
    }

    void Update()
    {
        foreach (WheelControl wheel in wheels)
        {
            // stop side sliding
            Vector3 carRestPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * wheel.restPoint)) * Time.deltaTime;
            Vector3 slideVelocity = Vector3.Project(carRestPointVelocity + Physics.gravity * 0.25f * Time.deltaTime, wheel.transform.right);
            carBody.AddForceAtPosition(-slideVelocity, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
        }
    }

    void UpdateWheelsNormal()
    {
        /*Dictionary<float, WheelControl> wheelsInOrder = new Dictionary<float, WheelControl>();

        foreach (WheelControl wheel in wheels)
        {
            float angle = Vector3.SignedAngle(this.transform.forward,)
        }

        sharedNormal = (sharedNormal).normalized;

        Debug.DrawRay(this.transform.position, sharedNormal, Color.yellow, Time.deltaTime, false);*/


        /*Vector3 wheelLocalPosition;
        Vector3 nextWheelLocalPosition;
        Vector3 cross;

        int i = 0;
        for (; i < wheels.Count; i+=3)
        {
            Vector3 firstPoint = wheels[i].transform.position;
            Vector3 secondPoint = wheels[i+1].transform.position;

            Vector3 planeVector1 = secondPoint - this.transform.position;
            Vector3 planeVector2 = firstPoint - this.transform.position;
            Vector3 normalVector = Vector3.Cross(planeVector1, planeVector2);

            sharedNormal += normalVector;

            Debug.DrawRay( (firstPoint + secondPoint + this.transform.position) * 0.33f, normalVector, Color.yellow, Time.deltaTime, false);
        }
        print(i);
        //sharedNormal /= (wheels.Count/2);

        Debug.DrawRay(this.transform.position, sharedNormal, Color.red, Time.deltaTime, false);*/
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
