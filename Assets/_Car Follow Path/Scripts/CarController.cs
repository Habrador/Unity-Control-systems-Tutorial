using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Modified basic car controller from Unity
//https://docs.unity3d.com/Manual/WheelColliderTutorial.html

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;

    //Car data
    float maxMotorTorque = 3000f;
    float maxSteeringAngle = 40f;

    //To get a more realistic behavior
    public Vector3 centerOfMassChange;

    //The difference between the center of the car and the position where we steer
    public float centerSteerDifference;
    //The position where the car is steering
    private Vector3 steerPosition;

    //PID parameters
    public float gain_P = 0f;
    public float gain_I = 0f;
    public float gain_D = 0f;
    //Sometimes you have to limit the total sum of all errors used in the I
    //public float error_sumMax = 20f;

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



    void Start()
    {
        //Move the center of mass
        transform.GetComponent<Rigidbody>().centerOfMass = transform.GetComponent<Rigidbody>().centerOfMass + centerOfMassChange;

        //Init the waypoints
        currentWaypoint = allWaypoints[currentWaypointIndex].position;

        previousWaypoint = GetPreviousWaypoint();

        PIDControllerScript = new PIDController();
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

        //Check if we should change waypoint
        if (Math.HasPassedWaypoint(steerPosition, previousWaypoint, currentWaypoint))
        {
            currentWaypointIndex += 1;

            if (currentWaypointIndex == allWaypoints.Count)
            {
                currentWaypointIndex = 0;
            }

            currentWaypoint = allWaypoints[currentWaypointIndex].position;

            previousWaypoint = GetPreviousWaypoint();
        }
    }



    //Get the waypoint before the current waypoint we are driving towards
    Vector3 GetPreviousWaypoint()
    {
        previousWaypoint = Vector3.zero;

        if (currentWaypointIndex - 1 < 0)
        {
            previousWaypoint = allWaypoints[allWaypoints.Count - 1].position;
        }
        else
        {
            previousWaypoint = allWaypoints[currentWaypointIndex - 1].position;
        }

        return previousWaypoint;
    }



    void FixedUpdate()
    {
        float motor = maxMotorTorque;

        //Manual controls for debugging
        //float motor = maxMotorTorque * Input.GetAxis("Vertical");
        //float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        //
        //Calculate the steering angle
        //
        //The simple but less accurate way -> will produce drunk behavior
        //float steeringAngle = maxSteeringAngle * Math.SteerDirection(transform, steerPosition, currentWaypoint);

        //Get the cross track error, which is what we want to minimize with the pid controller
        float CTE = Math.GetCrossTrackError(steerPosition, previousWaypoint, currentWaypoint);

        //Test to minimize the angle between the car and previous wp and the previous wp and upcoming wp
        //float CTE = Vector3.Angle((steerPosition - previousWaypoint).normalized, (currentWaypoint - previousWaypoint).normalized);

        //But we still need a direction to steer
        CTE *= Math.SteerDirection(transform, steerPosition, currentWaypoint);

        //The self driving car Stanley is using an equation to determine the steering angle without a PID controller
        //steeringAngle = psi + arctan((k * CTE) / velocity)
        //psi - the angle between the car and the path
        //k - parameter

        float steeringAngle = PIDControllerScript.GetFactorFromPIDController(gain_P, gain_I, gain_D, CTE);

        //Limit the steering angle
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);


        //The angle between the car and previous wp and the previous wp and upcoming wp
        //float angle = Vector3.Angle((steerPosition - previousWaypoint).normalized, (currentWaypoint - previousWaypoint).normalized);

        ////If we are close to the ideal path, then we should limit the steering angle
        //if (angle < 10f)
        //{
        //    if (steeringAngle < 0f)
        //    {
        //        steeringAngle = -angle;
        //    }
        //    else
        //    {
        //        steeringAngle = angle;
        //    }
        //}
        ////Steer as much as possible
        //else
        //{
        //    steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);
        //}
 
        //Average the steering angles to simulate the time it takes to turn the steering wheel
        float averageAmount = 10f;

        averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / averageAmount);


        //
        //Apply everything to the car 
        //
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = averageSteeringAngle;
                axleInfo.rightWheel.steerAngle = averageSteeringAngle;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}
