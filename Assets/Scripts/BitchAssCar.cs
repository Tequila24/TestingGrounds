using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitchAssCar : MonoBehaviour
{

    Rigidbody thisBody;


    // Start is called before the first frame update
    void Start()
    {
        thisBody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        thisBody.AddForce(this.transform.forward * Input.GetAxisRaw("Vertical") * thisBody.mass);
        thisBody.AddTorque(this.transform.up * Input.GetAxisRaw("Horizontal") * thisBody.mass);
    }
}
