using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    private static CameraManager _instance = null;
    public static CameraManager Instance  { get {return _instance;} }

    private GameObject mainCamera = null;

    void Awake() {

		if (_instance != null && _instance != this) {
			Destroy(gameObject);
		} else {
			_instance = this;
		}

        //Create or Find camera
        mainCamera = GameObject.Find("MainCamera");
        if (mainCamera == null) {
            mainCamera = new GameObject();
            mainCamera.name = "MainCamera";
            mainCamera.AddComponent<Camera>();
        }

	}


    
    
    void FixedUpdate()
    {           
    }

    public void UpdateCameraPosition(Vector3 newCameraPosition)
    {
        mainCamera.transform.position = newCameraPosition;
    }

    public void UpdateCameraRotation(Quaternion newRotation)
    {
        mainCamera.transform.rotation = newRotation;
    }
}
