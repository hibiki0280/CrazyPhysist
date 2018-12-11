using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

	public GameObject subject;
    private float x;
	private float z;

	// Update is called once per frame
	void Update () {
        x = subject.transform.position.x;
        z = subject.transform.position.z;
        this.transform.position = new Vector3(30 +x,3,10 + z);
		this.transform.rotation = Quaternion.Euler(0, 240,0);
	}
}
