using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public Camera mainCamera = null;

    public bool isGrounded = false;

    private float forward = 0;
    private float side = 0; 
    private float stepSpeed = 10;

    private Vector2 lookAngles;
    private Vector3 cameraPosition;

    private RaycastHit surfaceHit = new RaycastHit();

    private Collider thisBody = null;

    void Start()
    {
        thisBody = this.gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        CheckSurface();

        forward = Input.GetAxisRaw("Vertical");
        side = Input.GetAxisRaw("Horizontal");

        lookAngles += new Vector2( Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") );

        Quaternion pitch = Quaternion.AngleAxis(-lookAngles.y, Vector3.right);
        Quaternion yaw = Quaternion.AngleAxis(lookAngles.x, Vector3.up);
        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, surfaceHit.normal);

        Vector3 lookDirection = yaw * pitch * Vector3.forward;

        if (isGrounded) {
            Vector3 heightAdjust = (surfaceHit.point + this.transform.up * thisBody.bounds.extents.y * 0.05f) - this.transform.position;
            this.transform.position += heightAdjust + surfaceRotation * ( yaw * ((Vector3.forward * forward  + Vector3.right * side) * stepSpeed)) * Time.deltaTime;
        }

        mainCamera.transform.position = this.transform.position + this.transform.up;
        mainCamera.transform.rotation = yaw * pitch;

    }



    void CheckSurface()
    {
        Debug.DrawRay(this.transform.position -this.transform.up * thisBody.bounds.extents.y, Physics.gravity * Time.deltaTime, Color.black, Time.deltaTime, true);

		if (Physics.Raycast(this.transform.position -this.transform.up * thisBody.bounds.extents.y, Physics.gravity, out surfaceHit, Mathf.Abs(Physics.gravity.y * Time.deltaTime))) {

            if (isGrounded == false) {
                isGrounded = true;
            }

		} else {

            if (isGrounded == true) {
                isGrounded = false;
                surfaceHit = new RaycastHit();
            }

		}
    }
}
