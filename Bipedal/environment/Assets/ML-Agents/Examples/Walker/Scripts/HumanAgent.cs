﻿using System.Collections;
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
        jdController.SetupBodyPart(head);
        jdController.SetupBodyPart(chest);
        jdController.SetupBodyPart(spine);
        jdController.SetupBodyPart(hip);
        jdController.SetupBodyPart(leftthigh);
        jdController.SetupBodyPart(leftshin);
        jdController.SetupBodyPart(leftfoot);
        jdController.SetupBodyPart(rightthigh);
        jdController.SetupBodyPart(rightshin);
        jdController.SetupBodyPart(rightfoot);
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
        AddVectorObs(rb.velocity);
        AddVectorObs(rb.angularVelocity);
        Vector3 localPosRelToHip = hip.InverseTransformPoint(rb.position);
        AddVectorObs(localPosRelToHip);

        if (bp.rb.transform != hip && bp.rb.transform != leftfoot && bp.rb.transform != rightfoot && bp.rb.transform != head)
        {
            AddVectorObs(bp.currentXNormalizedRot);
            AddVectorObs(bp.currentYNormalizedRot);
            AddVectorObs(bp.currentZNormalizedRot);
            AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations()
    {
        jdController.GetCurrentJointForces();

        AddVectorObs(dirToTarget.normalized);
        AddVectorObs(jdController.bodyPartsDict[hip].rb.position);
        AddVectorObs(hip.forward);
        AddVectorObs(hip.up);

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

            //update joint strength settings
            bpDict[spine].SetJointStrength(vectorAction[++i]);
            bpDict[leftthigh].SetJointStrength(vectorAction[++i]);
            bpDict[leftshin].SetJointStrength(vectorAction[++i]);
            bpDict[leftfoot].SetJointStrength(vectorAction[++i]);
            bpDict[rightthigh].SetJointStrength(vectorAction[++i]);
            bpDict[rightshin].SetJointStrength(vectorAction[++i]);
            bpDict[rightfoot].SetJointStrength(vectorAction[++i]);
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
