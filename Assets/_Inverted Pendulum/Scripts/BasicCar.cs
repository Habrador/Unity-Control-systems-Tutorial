using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Modified basic car controller from Unity
//https://docs.unity3d.com/Manual/WheelColliderTutorial.html

public class BasicCar : MonoBehaviour
{
    public List<AxleInfo> axleInfos;

    //Car data
    float maxMotorTorque = 5000f;
    float maxSteeringAngle = 40f;

    //To get a more realistic behavior
    public Vector3 centerOfMassChange;

    //The difference between the center of the car and the position where we steer
    public float centerSteerDifference;
    //The position where the car is steering
    private Vector3 steerPosition;

    //All waypoints
    public List<Transform> allWaypoints;
    //The current index of the list with all waypoints
    private int currentWaypointIndex = 0;
    //The waypoint we are going towards and the waypoint we are going from
    private Vector3 currentWaypoint;
    private Vector3 previousWaypoint;

    //Average the steering angles to simulate the time it takes to turn the wheel
    float averageSteeringAngle = 0f;

    PIDController PIDControllerScript;

    //Car modes
    public enum CarModes { Forward, Stop, Reverse };
    public CarModes carMode;

    private float currentMotorTorque;



    void Start()
    {
        //Move the center of mass
        transform.GetComponent<Rigidbody>().centerOfMass = transform.GetComponent<Rigidbody>().centerOfMass + centerOfMassChange;

        carMode = CarModes.Stop;
    }



    //Finds the corresponding visual wheel, correctly applies the transform
    void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }



    void Update()
    {
        //So we can experiment with the position where the car is checking if it should steer left/right
        //doesn't have to be where the wheels are - especially if we are reversing
        steerPosition = transform.position + transform.forward * centerSteerDifference;
    }



    void FixedUpdate()
    {
        //Manual controls for debugging
        //float motor = maxMotorTorque * Input.GetAxis("Vertical");
        //float steeringAngle = maxSteeringAngle * Input.GetAxis("Horizontal");


        //Automatic controls
        //float motorTorque = 0f;

        //if (carMode == CarModes.Forward)
        //{
        //    motorTorque = currentMotorTorque;
        //}
        //else if (carMode == CarModes.Reverse)
        //{
        //    motorTorque = -currentMotorTorque;
        //}

        float motorTorque = currentMotorTorque;

        float steeringAngle = 0f;

        //This is a value between -1 and 1, where -1 is left
        //float steeringFactor = Input.GetAxis("Horizontal");

        ////Now we need to change the motor torque on the wheels depending on if we are steering left or right
        //float leftMotorTorque = motorTorque;
        //float rightMotorTorque = motorTorque;

        ////Steer left = more power to the right wheel
        //if (steeringFactor < 0f)
        //{
        //    leftMotorTorque = motorTorque * (1f - Mathf.Abs(steeringFactor)) * -1f;
        //}
        //else if (steeringFactor > 0f)
        //{
        //    rightMotorTorque = motorTorque * (1f - Mathf.Abs(steeringFactor)) * -1f;
        //}

        //print(rightMotorTorque);


        //Limit speed

        //Get the speed in km/h
        float speed = transform.GetComponent<Rigidbody>().velocity.magnitude * 3.600f;

        //print(speed);

        if (transform.GetComponent<Rigidbody>().velocity.magnitude * 3.600f > 40f)
        {
            motorTorque *= 0.6f;
        }

        //Apply everything to the car 
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steeringAngle;
                axleInfo.rightWheel.steerAngle = steeringAngle;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motorTorque;
                axleInfo.rightWheel.motorTorque = motorTorque;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }


    //Get Set motorTorque
    public float MotorTorque
    {
        get { return this.maxMotorTorque; }

        set { this.currentMotorTorque = value; }
    }
}

