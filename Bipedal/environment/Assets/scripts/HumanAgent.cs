using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HumanAgent : Agent
{
    [Header("Specific to Walker")]
    [Header("Target To Walk Towards")]
    [Space(10)]
    public Transform target;
    Vector3 dirToTarget;
    public Transform head;
    public Transform chest;
    public Transform spine;
    public Transform hip;
    public Transform leftthigh;
    public Transform leftshin;
    public Transform leftfoot;
    public Transform rightthigh;
    public Transform rightshin;
    public Transform rightfoot;
    JointDriveController jdController;
    bool isNewDecisionStep;
    int currentDecisionStep;

    public override void InitializeAgent()
    {
        jdController = GetComponent<JointDriveController>();
        jdController.SetupBodyPart(head); // 10
        jdController.SetupBodyPart(chest); // 14
        jdController.SetupBodyPart(spine); // 14
        jdController.SetupBodyPart(hip); // 10
        jdController.SetupBodyPart(leftthigh); // 14
        jdController.SetupBodyPart(leftshin); // 14
        jdController.SetupBodyPart(leftfoot); // 10
        jdController.SetupBodyPart(rightthigh); // 14
        jdController.SetupBodyPart(rightshin); // 14
        jdController.SetupBodyPart(rightfoot); // 10
        // right upper arm // 14
        // right lower arm // 14
        // right hand // 10
        // left upper arm // 14
        // left lower arm // 14
        // left hand // 10
        // 12
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // 1
        AddVectorObs(rb.velocity); // 3
        AddVectorObs(rb.angularVelocity); // 3
        Vector3 localPosRelToHip = hip.InverseTransformPoint(rb.position);
        AddVectorObs(localPosRelToHip); //3

        if (bp.rb.transform != hip && bp.rb.transform != leftfoot && bp.rb.transform != rightfoot && bp.rb.transform != head)
        {
            AddVectorObs(bp.currentXNormalizedRot); // 1
            AddVectorObs(bp.currentYNormalizedRot); // 1
            AddVectorObs(bp.currentZNormalizedRot); // 1
            AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit); // 1
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations()
    {
        jdController.GetCurrentJointForces();

        AddVectorObs(dirToTarget.normalized); // 3
        AddVectorObs(jdController.bodyPartsDict[hip].rb.position); // 3
        AddVectorObs(hip.forward); // 3
        AddVectorObs(hip.up); // 3

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            CollectObservationBodyPart(bodyPart);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        dirToTarget = target.position - jdController.bodyPartsDict[hip].rb.position;

        // Apply action to all relevant body parts. 
        if (isNewDecisionStep)
        {
            var bpDict = jdController.bodyPartsDict;
            int i = -1;

            bpDict[spine].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[leftthigh].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[rightthigh].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[leftshin].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[rightshin].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[rightfoot].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[leftfoot].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            // head // 2
            // chest // 3
            // right upper arm // 2
            // right lower arm // 1
            // left upper arm // 2
            // left lower arm // 1

            //update joint strength settings
            bpDict[spine].SetJointStrength(vectorAction[++i]);
            bpDict[leftthigh].SetJointStrength(vectorAction[++i]);
            bpDict[leftshin].SetJointStrength(vectorAction[++i]);
            bpDict[leftfoot].SetJointStrength(vectorAction[++i]);
            bpDict[rightthigh].SetJointStrength(vectorAction[++i]);
            bpDict[rightshin].SetJointStrength(vectorAction[++i]);
            bpDict[rightfoot].SetJointStrength(vectorAction[++i]);
            // head
            // chest
            // right upper arm
            // right lower arm
            // left upper arm
            // left lower arm
        }

        IncrementDecisionTimer();

        // Set reward for this step according to mixture of the following elements.
        // a. Velocity alignment with goal direction.
        // b. Rotation alignment with goal direction.
        // c. Encourage head height.
        // d. Discourage head movement.
        AddReward(
            +0.03f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hip].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, hip.forward)
            + 0.02f * (head.position.y - hip.position.y)
            - 0.01f * Vector3.Distance(jdController.bodyPartsDict[head].rb.velocity,
                jdController.bodyPartsDict[hip].rb.velocity)
        );
    }

    /// <summary>
    /// Only change the joint settings based on decision frequency.
    /// </summary>
    public void IncrementDecisionTimer()
    {
        if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
            agentParameters.numberOfActionsBetweenDecisions == 1)
        {
            currentDecisionStep = 1;
            isNewDecisionStep = true;
        }
        else
        {
            currentDecisionStep++;
            isNewDecisionStep = false;
        }
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        if (dirToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dirToTarget);
        }

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        isNewDecisionStep = true;
        currentDecisionStep = 1;
    }
}
