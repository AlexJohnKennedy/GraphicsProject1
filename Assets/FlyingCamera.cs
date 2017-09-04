using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour {

    public float speed;     //Movement speed

    private float angleX;
    private float angleY;
    private float angleZ;

    private MeshCollider collider;

    //private Vector3 prevPos;
    //private bool collide;

    private Rigidbody rb;   //We will be using this to apply force in order to make the camera move instead of just hardcoding transforms, so rigid body collisions work better!

    // Use this for initialization
    void Start () {
        angleX = 0;
        angleY = 0;     //Start just looking forward.
        angleZ = 0;

        //collider = this.gameObject.AddComponent<MeshCollider>();
        //collider.convex = true;
        //collider.sharedMesh = (this.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;

        //collide = false;
        rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void LateUpdate () {

        //Make Z axis zeroed relative vectors
        Vector3 forward = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
        //forward.Normalize();

        Vector3 right = new Vector3(transform.right.x, transform.right.y, transform.right.z);
        //right.Normalize();

        if (Input.GetKey(KeyCode.W)) {
            //this.transform.localPosition += forward * speed * Time.deltaTime;
            rb.AddForce(forward * speed);
        }
        if (Input.GetKey(KeyCode.S)) {
            //this.transform.localPosition += -forward * speed * Time.deltaTime;
            rb.AddForce(-forward * speed);
        }
        if (Input.GetKey(KeyCode.A)) {
            //this.transform.localPosition += -right * speed * Time.deltaTime;
            rb.AddForce(-right * speed);
        }
        if (Input.GetKey(KeyCode.D)) {
            //this.transform.localPosition += right * speed * Time.deltaTime;
            rb.AddForce(right * speed);
        }

        if (Input.GetKey(KeyCode.Q)) {
            //this.transform.Rotate(transform.forward,  20f* Time.deltaTime);
            angleZ += 8f * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E)) {
            //this.transform.Rotate(transform.forward, -20f * Time.deltaTime);
            angleZ -= 8f * Time.deltaTime;
        }
        
        mouseLook();

    }

    private void mouseLook() {
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        angleX -= mouseDelta.x;
        angleY += mouseDelta.y;

        this.transform.eulerAngles = new Vector3(angleX + mouseDelta.x, angleY + mouseDelta.y, angleZ);
        //this.transform.Rotate(transform.up, angleY * Time.deltaTime);
        //this.transform.Rotate(transform.right, angleX * Time.deltaTime);
    }

    public void OnCollisionEnter(Collision collision) {
        //This should hopefully trigger whenever two mesh colliders become overlapping!
        //Primitive way to do collision detection is to just detect when they happen, and then just RESET position to
        //whatever it was BEFORE it occurred in the first place.. (we will basically just 'freeze' when attempting to enter a wall..)
        Debug.Log("COLLISION DETECTED ++++++++++++++++++++++++++++++++++++++++++");

        //ShitEEEEE! there was a collision let's set our position back to the previous one..
        //collide = true;
        // this.transform.localPosition = prevPos;
    }
}
