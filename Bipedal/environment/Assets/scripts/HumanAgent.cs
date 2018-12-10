using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HumanAgent : Agent {
    public GameObject humanPrefab;
    public GameObject humans;
    public ConfigurableJoint[] configJoints;
    public Rigidbody[] rigids;
    public Vector3[] prev_velocities;
    public Vector3 init_head;
    public float max_z = 4;
    public GameObject human;

	// Use this for initialization
	void Start () {
        human = Instantiate(humanPrefab, humans.transform);
        human.transform.position = Vector3.zero;
        configJoints = human.GetComponentsInChildren<ConfigurableJoint>();
        foreach (var i in configJoints)
        {
            i.rotationDriveMode = RotationDriveMode.Slerp;
            i.slerpDrive = new JointDrive
            {
                maximumForce = 25000,
                positionDamper = 50,
                positionSpring = 10000,
            };
            i.targetRotation = Quaternion.Euler(Vector3.zero);
        }
        Debug.Log(configJoints.Length.ToString());
        rigids = human.GetComponentsInChildren<Rigidbody>();
        init_head = rigids[0].transform.position;
        prev_velocities = new Vector3[rigids.Length];
        for (int i = 0; i < rigids.Length; i++)
        {
            prev_velocities[i] = rigids[i].velocity;
        }
	}

    // Update is called once per frame
    public override void AgentReset()
    {
        Debug.Log("Delete human."+GetStepCount());
        Destroy(human);
        Start();
    }
    public override void CollectObservations()
    {
        var center = rigids[4];
        for (int i = 0; i < rigids.Length; i++)
        {
            if (i == 4) continue;
            AddVectorObs(rigids[i].velocity - center.velocity);
            AddVectorObs(((rigids[i].velocity - prev_velocities[i]) - (center.velocity - prev_velocities[4])) / Time.fixedDeltaTime);
        }
        AddVectorObs(center.velocity);
        AddVectorObs((center.velocity - prev_velocities[4])/Time.fixedDeltaTime);
        foreach (var joint in configJoints)
        {
            AddVectorObs(joint.slerpDrive.maximumForce);
            AddVectorObs(joint.targetRotation.eulerAngles);
        }
        for (int i = 0; i < rigids.Length; i++)
        {
            prev_velocities[i] = rigids[i].velocity;
        }
    }
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        string debug="";
        foreach (var i in vectorAction)
        {
            debug+=i.ToString()+' ';
        }
        Debug.Log("step:"+GetStepCount().ToString()+" vectorAction:"+debug);
        var v = rigids[4].velocity;
        var pos = rigids[4].transform.position;
        AddReward(Mathf.Min(v.z, max_z));
        AddReward((float)-0.005 * (v.z * v.z + v.x * v.x));
        AddReward((float)-0.05 * pos.x * pos.x);
        for (var j=0;j*4+3< vectorAction.Length;j++)
        {
            var i = vectorAction[j * 4+3];
            AddReward((float)-0.000000002 * i * i);
        }
        if (rigids[0].transform.position.y < (init_head.y * 2 / 5))
        {
            Debug.Log("Reward and Done: "+GetReward());
            Done();
        }
        Debug.Log("Reward: "+GetReward());
        AddReward((float)200);
        for (int i = 0; i < configJoints.Length; i++)
        {
            var vv = new float[3];
            for (int j = 0; j < 3; j++)
            {
                vv[j] = (vectorAction[i * 4 + j] + 1.0f) * 0.5f;
            }
            var tmp = configJoints[i];
            vv[0] = Mathf.Lerp(tmp.lowAngularXLimit.limit, tmp.highAngularXLimit.limit, vv[0]);
            vv[1] = Mathf.Lerp(-tmp.angularYLimit.limit, tmp.angularYLimit.limit, vv[1]);
            vv[2] = Mathf.Lerp(-tmp.angularZLimit.limit, tmp.angularZLimit.limit, vv[2]);
            tmp.targetRotation = Quaternion.Euler(new Vector3(vv[0], vv[1], vv[2]));
            
            var force = (vectorAction[i * 4 + 3] + 1.0f) * 0.5f * tmp.slerpDrive.maximumForce;
            tmp.slerpDrive = new JointDrive
                {
                    positionSpring = 10000,
                    positionDamper = 50,
                    maximumForce = force,
                };
                
        }
    }
}
