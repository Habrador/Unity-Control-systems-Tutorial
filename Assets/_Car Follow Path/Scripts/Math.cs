using UnityEngine;
using System.Collections;

public static class Math
{
    //Have we passed a waypoint?
    //From http://www.habrador.com/tutorials/linear-algebra/2-passed-waypoint/
    public static bool HasPassedWaypoint(Vector3 carPos, Vector3 goingFromPos, Vector3 goingToPos)
    {
        bool hasPassedWaypoint = false;

        //The vector between the character and the waypoint we are going from
        Vector3 a = carPos - goingFromPos;

        //The vector between the waypoints
        Vector3 b = goingToPos - goingFromPos;

        //Vector projection from https://en.wikipedia.org/wiki/Vector_projection
        //To know if we have passed the upcoming waypoint we need to find out how much of b is a1
        //a1 = (a.b / |b|^2) * b
        //a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
        float progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

        //If progress is above 1 we know we have passed the waypoint
        if (progress > 1.0f)
        {
            hasPassedWaypoint = true;
        }

        return hasPassedWaypoint;
    }



    //Should we turn left or right to reach the next waypoint?
    //From: http://www.habrador.com/tutorials/linear-algebra/3-turn-left-or-right/
    public static float SteerDirection(Transform carTrans, Vector3 steerPosition, Vector3 waypointPos)
    {
        //The right direction of the direction you are facing
        Vector3 youDir = carTrans.right;

        //The direction from you to the waypoint
        Vector3 waypointDir = waypointPos - steerPosition;

        //The dot product between the vectors
        float dotProduct = Vector3.Dot(youDir, waypointDir);

        //Now we can decide if we should turn left or right
        float steerDirection = 0f;
        if (dotProduct > 0f)
        {
            steerDirection = 1f;
        }
        else
        {
            steerDirection = -1f;
        }

        return steerDirection;
    }



    //Get the distance between where the car is and where it should be
    public static float GetCrossTrackError(Vector3 carPos, Vector3 goingFromPos, Vector3 goingToPos)
    {
        //The first part is the same as when we check if we have passed a waypoint
        
        //The vector between the character and the waypoint we are going from
        Vector3 a = carPos - goingFromPos;

        //The vector between the waypoints
        Vector3 b = goingToPos - goingFromPos;

        //Vector projection from https://en.wikipedia.org/wiki/Vector_projection
        //To know if we have passed the upcoming waypoint we need to find out how much of b is a1
        //a1 = (a.b / |b|^2) * b
        //a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
        float progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

        //The coordinate of the position where the car should be
        Vector3 errorPos = goingFromPos + progress * b;

        //The error between the position where the car should be and where it is
        float error = (errorPos - carPos).magnitude;

        return error;
    }
}
