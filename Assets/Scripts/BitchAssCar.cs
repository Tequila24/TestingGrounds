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
        Vector3 newMoveDirection = new Vector3( Input.GetAxisRaw("Horizontal"),
                                                0,
                                                Input.GetAxisRaw("Vertical")  );

        thisBody.AddForce(this.transform.rotation * newMoveDirection * 1000);
    }
}
