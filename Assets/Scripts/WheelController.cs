using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WheelController : MonoBehaviour
{

    [SerializeField]
    private float wheelRadius = 0;
    [SerializeField]
    private float dampingValue = 0;
    [SerializeField]
    private float springForce = 0;
    [SerializeField]
    private Vector3 suspensionAxis = Vector3.zero;

    private bool isGrounded = false;
    private Rigidbody carBody = null;

    void Awake()
    {
        carBody = this.transform.parent.GetComponent<Rigidbody>();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundCheck();

        if (isGrounded == true) {
            ApplyForces();
        }
    }


    void GroundCheck()
    {
        RaycastHit rayHit;
        if ( Physics.Raycast(this.transform.position, -this.transform.up, out rayHit, wheelRadius) )
        {
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }


    void ApplyForces()
    {
        Debug.Log("im forcing");
        Vector3 forceVector = suspensionAxis;

        carBody.AddForceAtPosition( forceVector, this.transform.position - suspensionAxis);
    }



    void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(this.transform.position, this.transform.right, wheelRadius);
        Handles.DrawWireDisc(this.transform.position + (this.transform.up * (wheelRadius/10 - wheelRadius)), this.transform.right, wheelRadius/10);

        Gizmos.DrawLine(this.transform.position + suspensionAxis - this.transform.forward*0.25f,
                        this.transform.position + suspensionAxis + this.transform.forward*0.25f );
    }
}
