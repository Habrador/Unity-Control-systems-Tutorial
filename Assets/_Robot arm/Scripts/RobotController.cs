using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour 
{
    //All joints belong to this robot
    public Transform[] jointTrans;

    //The tip of the robot
    public Transform robotTip;

    //The target the tip of the robot arm is trying to reach
    public Transform targetTrans;

    //A help array with all joint scripts to speed up the gradient calculations
    private RobotJoint[] joints;



    void Start() 
	{
        joints = new RobotJoint[jointTrans.Length];
    
        for (int i = 0; i < jointTrans.Length; i++)
        {
            joints[i] = jointTrans[i].GetComponent<RobotJoint>();
        }
    }
	
	

	void Update() 
	{
        //Move the robot to the wanted position
        //Alternative 1 - check if each joint should move "left/right to reach a waypoint" - start with the joint
        //furthest away
        //Alternative 2 - Gradient descent
        //Alternative 3 - move each joint with transform.LookAt()
        MoveRobotGradientDescent();

        //Debug
        //DebugCalculateTipPosition();
    }



    //Move the robot arm to a desired position with gradient descent
    private void MoveRobotGradientDescent()
    {
        float[] angles = GetLocalAngles();

        //The distance to the wanted pos
        float distance = CalculateDistance(angles);

        //Main loop
        float[] gradients = new float[jointTrans.Length];

        //To avoid ending up in infinite loop if we never reaches the min error because the robot is too far away
        int iterations = 0;

        float learningRate = 0.5f;

        while (distance > 0.001f && iterations < 200)
        {
            iterations += 1;

            //Calculate the gradients with central difference derivation
            float delta = 0.01f;

            for (int i = 0; i < gradients.Length; i++)
            {
                float startAngle = angles[i];

                angles[i] = startAngle + delta;

                float gradientPositive = CalculateDistance(angles);

                angles[i] = startAngle - delta;

                float gradientNegative = CalculateDistance(angles);

                gradients[i] = (gradientPositive - gradientNegative) / (2f * delta);

                angles[i] = startAngle;
            }


            //Change the angle based on the gradients
            for (int i = 0; i < angles.Length; i++)
            {
                angles[i] -= learningRate * gradients[i];
            }


            //Calculate the new distance 
            float newDistance = CalculateDistance(angles);

            //Change learning rate if we improved
            if (newDistance < distance)
            {
                learningRate *= 1.05f;
            }
            else
            {
                learningRate = 1.0f;
            }

            distance = newDistance;
        }

        if (iterations == 200)
        {
            //print(iterations);
        }

        //Add the new angles to the robot
        for (int i = 0; i < angles.Length; i++)
        {
            joints[i].AddRotationAngle(angles[i]);
        }
    }



    //Calculate the distance from the tip to the target - this is the cost function we want to minimize
    //angles[] is local rotation
    private float CalculateDistance(float[] angles)
    {
        float distance = (targetTrans.position - CalculateTipPosition(angles)).magnitude;

        return distance;
    }



    //Returns the position of the end effector in global coordinates given an array of angles
    //where each angle is the angle a joint has
    //Is used so we dont have to move the "physical" robot to test a new set of angles
    private Vector3 CalculateTipPosition(float[] angles)
    {
        Vector3 prevPoint = joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        
        for (int i = 1; i < joints.Length; i++)
        {
            //Rotates around a new axis
            //Multiplying two quaternions creates a new quaternion, which incorporates both rotations. 
            //During each iteration of the for loop, the variable rotation is multiplied by the current quaternion. 
            //This means that it incorporate the rotations for all the joints.
            rotation *= Quaternion.AngleAxis(angles[i - 1], joints[i - 1].rotationAxis);
            Vector3 nextPoint = prevPoint + rotation * joints[i].startOffset;

            prevPoint = nextPoint;
        }


        //The tip which is never rotating
        prevPoint = prevPoint + rotation * robotTip.localPosition;

        return prevPoint;
    }



    //Display the tip of the robot with a ray
    private void DebugCalculateTipPosition()
    {
        float[] angles = GetLocalAngles();

        Vector3 tipPos = CalculateTipPosition(angles);

        Debug.DrawRay(tipPos, Vector3.up);
    }



    //Add all local joint angles to an array
    private float[] GetLocalAngles()
    {
        float[] angles = new float[jointTrans.Length];

        for (int i = 0; i < jointTrans.Length; i++)
        {
            angles[i] = joints[i].GetRotationAngle();
        }

        return angles;
    }
}
