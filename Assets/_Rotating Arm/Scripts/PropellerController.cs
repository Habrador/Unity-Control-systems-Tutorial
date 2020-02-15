using UnityEngine;
using System.Collections;

public class PropellerController : MonoBehaviour 
{
    //The propeller obj
    public Transform propellerTrans;
    //Propeller force
    public float maxPropellerForce;
    //PID controller parameters
    public float Kp;
    public float Ki;
    public float Kd;
    //Sometimes you should limit the sum in I
    public float maxSum;

    //PID private variables
    private float errorSum = 0f;
    private float lastError = 0f;
    //To get the error before the lastError
    private float lastError2 = 0f;

    //The hinge joint so we can measure the angle
    //Cant use the arms rotation because of strange angles
    HingeJoint armHinge;

    //The rigid body attached to the arm
    private Rigidbody armRB;

    //The angle we want the arm to be at
    private float wantedAngle = 0f;

    //The propeller rotation script so we can stop the propeller
    private RotatePropeller rotatePropellerScript;


	void Start() 
	{
        //The arm is the parent to the propeller
        Transform armTrans = propellerTrans.parent;

        //The rb is attached to the arm
        armRB = armTrans.GetComponent<Rigidbody>();

        //The hinge 
        armHinge = armTrans.GetComponent<HingeJoint>();

        rotatePropellerScript = propellerTrans.GetComponent<RotatePropeller>();
    }
	
	

	void FixedUpdate() 
	{
        AddForceToPropeller();
    }



    //Add a force to the propeller
    void AddForceToPropeller()
    {
        //To control we need an error. We could use height, whoch is easy to get with an Altimeter,
        //but we are going to use the angle
        
        //What's the error to the wanted angle, this is what we want to minimize
        float error = wantedAngle - armHinge.angle;

        //print(error);

        //float propellerForce = GetForceNaive(error);
        //float propellerForce = GetForceProportional(error);
        float propellerForce = GetForcePID(error);

        //Give the propeller an up force like a real propeller
        armRB.AddForceAtPosition(propellerTrans.up * propellerForce, propellerTrans.position);

        //Add a torque - doesnt matter where we add the torque, so no need to add it at a position
        //armRB.AddTorque();

        //Test that we are applying the force in the correct direction
        //Debug.DrawRay(propellerTrans.position, propellerTrans.up * 2f, Color.red);
    }



    //Method 1 - Naive where we start the propeller if the arm is below
    private float GetForceNaive(float error)
    {
        float propellerForce = 0f;

        //Negative if above, so no propeller force needed
        //Positive if below, so propeller force needed
        //Also not needed to add force when angle is > 90 because gravity will move 
        //the arm down in that case, or the entire arm will move round-round if not using this parameter
        if (error > 0f && error < 90f)
        {
            propellerForce = maxPropellerForce;

            //Start the propeller
            rotatePropellerScript.shouldRotate = true;
        }
        else
        {
            //Stop the propeller
            rotatePropellerScript.shouldRotate = false;
        }

        return propellerForce;
    }



    //Method 2 - Proportional controller
    private float GetForceProportional(float error)
    {
        float propellerForce = 0f;

        //Negative if above, so no propeller force needed
        //Positive if below, so propeller force needed
        //Also not needed to add force when angle is > 90 because gravity will move 
        //the arm down in that case, or the entire arm will move round-round if not using this parameter
        if (error > 0f && error < 90f)
        {
            //The motor can be adjusted so we don't have to always give it a max force
            //Also limit it so we dont give the propeller more than it can take
            propellerForce = Mathf.Min(Mathf.Max(Kp * Mathf.Abs(error) * maxPropellerForce, 0f), maxPropellerForce);

            //Start the propeller
            rotatePropellerScript.shouldRotate = true;
        }
        else
        {
            //Stop the propeller
            rotatePropellerScript.shouldRotate = false;
        }

        return propellerForce;
    }



    //Method 3 - PID controller
    private float GetForcePID(float error)
    {
        float propellerForce = 0f;

        float pidController = 0f;

        float t = Time.fixedDeltaTime;

        //P
        pidController += Kp * error;

        //I
        errorSum += t * error;

        //Sometimes you should clamp the sum to limit it
        errorSum = Mathf.Clamp(errorSum, -maxSum, maxSum);

        //Or take the sum of the last updates
        //float averageAmount = 20f;

        //errorSum = errorSum + (((t * error) - errorSum) / maxSum);

        pidController += Ki * errorSum;

        //D
        float deltaError = (error - lastError2) / t;

        //Will make it more robust to noise
        lastError2 = lastError;

        lastError = error;

        pidController += Kd * deltaError;

        //print(pidController);

        //The motor can be adjusted so we don't have to always give it a max force
        //The force cant be negative
        //Also limit it so we dont give the propeller more than it can take
        propellerForce = Mathf.Min(Mathf.Max(pidController, 0f), maxPropellerForce);

        //Start the propeller
        rotatePropellerScript.shouldRotate = true;

        if (propellerForce <= 0f)
        {
            //Stop the propeller
            rotatePropellerScript.shouldRotate = false;
        }


        ////Negative if above, so no propeller force needed
        ////Positive if below, so propeller force needed
        ////Also not needed to add force when angle is > 90 because gravity will move 
        ////the arm down in that case, or the entire arm will move round-round if not using this parameter
        //if (error > 0f && error < 90f)
        //{
        //    //The motor can be adjusted so we don't have to always give it a max force
        //    //The force cant be negative
        //    //Also limit it so we dont give the propeller more than it can take
        //    propellerForce = Mathf.Min(Mathf.Max(pidController * maxPropellerForce, 0f), maxPropellerForce);

        //    //Start the propeller
        //    rotatePropellerScript.shouldRotate = true;
        //}
        //else
        //{
        //    //Stop the propeller
        //    rotatePropellerScript.shouldRotate = false;
        //}

        return propellerForce;
    }
}
