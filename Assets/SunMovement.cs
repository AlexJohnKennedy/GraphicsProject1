using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Sun rotates around zaxis
        transform.RotateAround(Vector3.zero, Vector3.forward, 25f * Time.deltaTime);

        //Have the sun always face 0,0,0
        transform.LookAt(Vector3.zero);
	}
}
