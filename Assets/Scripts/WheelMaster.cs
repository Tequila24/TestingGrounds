﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




public class WheelMaster : MonoBehaviour
{
    Rigidbody carBody = null;

    List<WheelControl> wheels = new List<WheelControl>();
    Vector3 sharedNormal = Vector3.up;

    
    struct CollisionCheckInfo {
        public Collider collider;
        public Vector3 colliderPosition;
        public Quaternion colliderRotation;
        public float checkBoxDistance;
        
        public CollisionCheckInfo(Collider newCollider, Vector3 newColliderPosition, Quaternion newColliderRotation, float newCheckBoxDistance)
        {
            this.collider = newCollider;
            this.colliderPosition = newColliderPosition;
            this.colliderRotation = newColliderRotation;
            this.checkBoxDistance = newCheckBoxDistance;
        }
    }


    void Awake()
    {
        carBody = this.gameObject.transform.parent.Find("CarBody").GetComponent<Rigidbody>();

        GetWheels();
        InitWheels();
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
    }

    void UpdateWheelPosition(WheelControl wheel)
    {
        wheel.strut = (carBody.transform.rotation * (wheel.defaultStrut)).normalized;

        Vector3 carRestPointVelocity = ( carBody.GetPointVelocity(carBody.position + carBody.rotation * wheel.restPoint) ) * Time.deltaTime;

        // wheel position
        Vector3 springAcceleration = -wheel.strut * ((wheel.offsetFromRestPoint * wheel.springValue) / wheel.wheelMass) * Time.deltaTime;
        wheel.velocityOnStrut += springAcceleration;

        CollisionCheckInfo newCheck = new CollisionCheckInfo(wheel.meshCollider, wheel.transform.position, wheel.transform.rotation, wheel.checkBoxDistance);
        Vector3 depenetrationInNextFrame = GetAllignedDepenetration(newCheck, wheel.strut);
        wheel.velocityOnStrut += depenetrationInNextFrame;
        wheel.surfaceReaction = depenetrationInNextFrame;

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

        wheel.isGrounded = depenetrationInNextFrame.sqrMagnitude > 0 ? true : false;

    }

    void UpdateWheelRotation(WheelControl wheel)
    {
        Quaternion axisRotation = (wheel.isRight ? Quaternion.AngleAxis(180, carBody.transform.up) * carBody.rotation : carBody.rotation);
        wheel.transform.rotation = axisRotation;
    }

    void ApplyCarPhysics(WheelControl wheel)
    {
        // apply spring
        Vector3 springForce = wheel.strut * ((wheel.offsetFromRestPoint * wheel.springValue));
        carBody.AddForceAtPosition(springForce, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.Impulse);


        // damp speed

        Vector3 carRelativeVerticalSpeed = (wheel.velocityOnStrut) * wheel.dampingValue;
        if (wheel.isFloored) {
            carRelativeVerticalSpeed -= wheel.surfaceReaction;
        }
        carRelativeVerticalSpeed = Vector3.Project(carRelativeVerticalSpeed, wheel.strut);
        carBody.AddForceAtPosition(carRelativeVerticalSpeed, carBody.position + carBody.rotation * wheel.restPoint, ForceMode.VelocityChange);
        
    }

    void UpdateWheelsNormal()
    {
        foreach (WheelControl wheel in wheels)
        {
            
        }
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


    Vector3 GetDepenetration(CollisionCheckInfo newInfo)
    {
        Vector3 surfacePenetration = Vector3.zero;
        Collider[] surfaces = new Collider[16];

        int count = Physics.OverlapSphereNonAlloc(newInfo.colliderPosition, newInfo.checkBoxDistance, surfaces);

        if (count<2)
            return surfacePenetration;

        for (int i=0; i<count; ++i)
        {
            Collider collider = surfaces[i];

            if (collider == newInfo.collider || collider.gameObject == carBody.gameObject)
                continue;

            Vector3 otherPosition = collider.gameObject.transform.position;
            Quaternion otherRotation = collider.gameObject.transform.rotation;
            Vector3 direction;
            float distance;

            bool overlapped = Physics.ComputePenetration(   newInfo.collider, newInfo.colliderPosition, newInfo.colliderRotation,
                                                            collider, otherPosition, otherRotation,
                                                            out direction, out distance);

            if (overlapped)
            {
                surfacePenetration += direction * distance;
            }
        }

        return surfacePenetration;
    }

    Vector3 GetAllignedDepenetration(CollisionCheckInfo newInfo, Vector3 strut)
    {
        Vector3 depenetrationVector = GetDepenetration(newInfo);

        if (depenetrationVector.sqrMagnitude == 0)
            return Vector3.zero;


        float angle = Vector3.Angle(depenetrationVector, strut);
        float newScale = depenetrationVector.magnitude;

        if (angle < 90) {
            depenetrationVector = strut * newScale;
        } else {
            depenetrationVector = -strut * newScale;
        }

        return depenetrationVector;
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
