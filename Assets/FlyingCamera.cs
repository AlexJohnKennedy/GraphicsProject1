using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour {

    public float speed;     //Movement speed

    private float angleX;
    private float angleY;
    private float angleZ;

	// Use this for initialization
	void Start () {
        angleX = 0;
        angleY = 0;     //Start just looking forward.
        angleZ = 0;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        //Make Z axis zeroed relative vectors
        Vector3 forward = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
        //forward.Normalize();

        Vector3 right = new Vector3(transform.right.x, transform.right.y, transform.right.z);
        //right.Normalize();

        if (Input.GetKey(KeyCode.W)) {
            this.transform.localPosition += forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S)) {
            this.transform.localPosition += -forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A)) {
            this.transform.localPosition += -right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D)) {
            this.transform.localPosition += right * speed * Time.deltaTime;
        }

        mouseLook();
    }


    private void mouseLook() {
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        angleX -= mouseDelta.x;
        angleY += mouseDelta.y;
        
        this.transform.eulerAngles = new Vector3(angleX + mouseDelta.x, angleY + mouseDelta.y, angleZ);
    }
}
