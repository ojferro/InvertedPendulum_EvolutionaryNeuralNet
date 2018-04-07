using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {
    public Vector3 myPos;
    public Transform target;
	// Use this for initialization
	void Start () {
        myPos = transform.position;
        target = GameObject.FindGameObjectWithTag("Pendulum").GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = target.position + myPos;
	}
}
