using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

	private GameObject hip;
	private float h;

	// Update is called once per frame
	void Update () {
		hip = GameObject.Find("hip");
		if( hip == null){
			h = 0;
		}else{
			h = hip.transform.position.z;
		}
		this.transform.position = new Vector3(10,3,6 + h);
		this.transform.rotation = Quaternion.Euler(0, -120,0);
	}
}
