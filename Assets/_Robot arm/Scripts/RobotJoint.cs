using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotJoint : MonoBehaviour 
{
    //The joint can onlyy rotate around 1 axis, so indicate this with 1
    public Vector3 rotationAxis;
    //The distance between this joint and the previous joint
    public Vector3 startOffset;
    //The local angle
    public Vector3 localRotationAngle;


    void Awake()
    {
        startOffset = transform.localPosition;

        localRotationAngle = transform.localEulerAngles;
    }



    //Get the angle of the active rotation axis
    public float GetRotationAngle()
    {
        if (rotationAxis.x != 0f)
        {
            return transform.localEulerAngles.x;
        }
        else if (rotationAxis.y != 0f)
        {
            return transform.localEulerAngles.y;
        }
        else 
        {
            return transform.localEulerAngles.z;
        }
    }



    //Add a new angle to the robot
    public void AddRotationAngle(float angle)
    {
        if (rotationAxis.x != 0f)
        {
            transform.localEulerAngles = new Vector3(angle, 0f, 0f);
        }
        else if (rotationAxis.y != 0f)
        {
            transform.localEulerAngles = new Vector3(0f, angle, 0f);
        }
        else
        {
            transform.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    } 
}
