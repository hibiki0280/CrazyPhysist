using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
    public int index = -1;
    private float dist = 0.1f;
    private int m = 90;
    public int st = 10;
    private int f = 0;
    private int max_f = 1;
    public Vector3 v = Vector3.zero;
    ConfigurableJoint[] joints;
	// Use this for initialization
	void Start () {
        joints = GetComponentsInChildren<ConfigurableJoint>();
        foreach (var i in joints)
        {
            i.targetRotation = Quaternion.Euler(Vector3.zero);
        }
	}
	
	// Update is called once per frame
	void Update () {
        /*
        f++;
        if (f < max_f)
        {
            return;
        }
        else
        {
            f = 0;
        }
        if (index >= 0)
        {
            if (v.x > joints[index].highAngularXLimit.limit)
            {
                v.x = joints[index].lowAngularXLimit.limit;
                st++;
            }
            if (v.y > joints[index].angularYLimit.limit)
            {
                v.y = -joints[index].angularYLimit.limit;
                st++;
            }
            if (v.z > joints[index].angularZLimit.limit)
            {
                v.z = -joints[index].angularZLimit.limit;
                st++;
            }
            joints[index].targetRotation = Quaternion.Euler(v);
        }
        switch (st)
        {
            case 0:
            case 3:
                v.x += dist;
                break;
            case 1:
            case 4:
                v.y += dist;
                break;
            case 2:
            case 5:
                v.z += dist;
                break;
            default:
                st = 0;
                v = Vector3.zero;
                if (index >= 0)
                {
                    v.x = joints[index].lowAngularXLimit.limit;
                    v.y = -joints[index].angularYLimit.limit;
                    v.z = -joints[index].angularZLimit.limit;
                }
                    
                index += 1;
                if (index == joints.Length)
                {
                    index = 0;
                }
                break;
        }
        */
    }
}
