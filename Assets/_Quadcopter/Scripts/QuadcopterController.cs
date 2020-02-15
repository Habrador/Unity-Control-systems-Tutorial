using UnityEngine;
using System.Collections;

public class QuadcopterController : MonoBehaviour 
{
    //The propellers
    public GameObject propellerFR;
    public GameObject propellerFL;
    public GameObject propellerBL;
    public GameObject propellerBR;

    //Quadcopter parameters
    [Header("Internal")]
    public float maxPropellerForce;
    public float maxTorque;
    public float throttle;
    //public float steerForce;
    //PID
    public Vector3 PID_pitch_gains;
    public Vector3 PID_roll_gains;
    public Vector3 PID_yaw_gains;

    //External parameters
    [Header("External")]
    public float windForce;
    //0 -> 360
    public float forceDir;


    Rigidbody quadcopterRB;


    //The PID controllers
    private PIDController PID_pitch;
    private PIDController PID_roll;
    private PIDController PID_yaw;

    //Movement factors
    float moveForwardBack;
    float moveLeftRight;
    float yawDir;


    void Start() 
	{
        quadcopterRB = gameObject.GetComponent<Rigidbody>();

        PID_pitch = new PIDController();
        PID_roll = new PIDController();
        PID_yaw = new PIDController();
    }
	
	
	void Update() 
	{
        //Vector3 vel = quadcopterRB.angularVelocity;

        //print(quadcopterRB.angularVelocity);

        //float roll = Mathf.Atan2(vel.y, vel.z);

        //print(roll);

    }



    void FixedUpdate()
    {
        AddControls();

        AddMotorForce();

        AddExternalForces();
    }



    void AddControls()
    {
        //Change throttle to move up or down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            throttle += 5f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            throttle -= 5f;
        }

        throttle = Mathf.Clamp(throttle, 0f, 200f);

        //Steering
        moveForwardBack = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveForwardBack = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveForwardBack = -1f;
        }


        moveLeftRight = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            moveLeftRight = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveLeftRight = 1f;
        }

        //Yaw
        yawDir = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            yawDir = -1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            yawDir = 1f;
        }
    }



    void AddMotorForce()
    {
        //Calculate the errors so we can use a PID controller to stabilize
        //Assume no error is if 0 degrees

        //Pitch
        //Returns positive if pitching forward
        float pitchError = GetPitchError();

        //print(pitchError);

        //Roll
        //Returns positive if rolling left
        float rollError = GetRollError() * -1f;

        //Adapt the PID variables to the throttle
        Vector3 PID_pitch_gains_adapted = throttle > 100f ? PID_pitch_gains * 2f : PID_pitch_gains;

        //Get the output from the PID controllers
        float PID_pitch_output = PID_pitch.GetFactorFromPIDController(PID_pitch_gains_adapted, pitchError);
        float PID_roll_output = PID_roll.GetFactorFromPIDController(PID_roll_gains, rollError);

        //PID_pitch_output = 0f;

        //Calculate the propeller forces
        //FR
        //float throttleFR = pitchError < -0f ? 0f : throttle;
        float propellerForceFR = throttle + (PID_pitch_output + PID_roll_output);

        //Add steering
        propellerForceFR -= moveForwardBack * throttle;
        propellerForceFR -= moveLeftRight * throttle;


        //FL
        //float throttleFL = pitchError < -0f ? 0f : throttle;
        float propellerForceFL = throttle + (PID_pitch_output - PID_roll_output);

        propellerForceFL -= moveForwardBack * throttle;
        propellerForceFL += moveLeftRight * throttle;


        //BR
        //float throttleBR = pitchError > 0f ? 0f : throttle;
        float propellerForceBR = throttle + (-PID_pitch_output + PID_roll_output);

        propellerForceBR += moveForwardBack * throttle;
        propellerForceBR -= moveLeftRight * throttle;


        //BL
        //float throttleBL = pitchError > 0f ? 0f : throttle; 
        float propellerForceBL = throttle + (-PID_pitch_output - PID_roll_output);

        propellerForceBL += moveForwardBack * throttle;
        propellerForceBL += moveLeftRight * throttle;

        //print(propellerForceFR + " " + propellerForceBR);

        //Clamp
        propellerForceFR = Mathf.Clamp(propellerForceFR, 0f, maxPropellerForce);
        propellerForceFL = Mathf.Clamp(propellerForceFL, 0f, maxPropellerForce);
        propellerForceBR = Mathf.Clamp(propellerForceBR, 0f, maxPropellerForce);
        propellerForceBL = Mathf.Clamp(propellerForceBL, 0f, maxPropellerForce);

        AddForceToPropeller(propellerFR, propellerForceFR);
        AddForceToPropeller(propellerFL, propellerForceFL);
        AddForceToPropeller(propellerBR, propellerForceBR);
        AddForceToPropeller(propellerBL, propellerForceBL);

        //Yaw
        //Minimize the yaw error (which is already signed):
        float yawError = quadcopterRB.angularVelocity.y;

        float PID_yaw_output = PID_yaw.GetFactorFromPIDController(PID_yaw_gains, yawError);

        //First we need to add a force (if any)
        quadcopterRB.AddTorque(transform.up * yawDir * maxTorque * throttle);

        //Then we need to minimize the error
        quadcopterRB.AddTorque(transform.up * throttle * PID_yaw_output * -1f);
    }



    void AddForceToPropeller(GameObject propellerObj, float propellerForce)
    {
        Vector3 propellerUp = propellerObj.transform.up;

        Vector3 propellerPos = propellerObj.transform.position;

        quadcopterRB.AddForceAtPosition(propellerUp * propellerForce, propellerPos);

        //Debug
        //Debug.DrawRay(propellerPos, propellerUp * 1f, Color.red);
    }



    //Pitch is rotation around x-axis
    //Returns positive if pitching forward
    private float GetPitchError()
    {
        float xAngle = transform.eulerAngles.x;

        //Make sure the angle is between 0 and 360
        xAngle = WrapAngle(xAngle);

        //print(xAngle);

        //This angle going from 0 -> 360 when pitching forward
        //So if angle is > 180 then it should move from 0 to 180 if pitching back
        if (xAngle > 180f && xAngle < 360f)
        {
            xAngle = 360f - xAngle;

            //-1 so we know if we are pitching back or forward
            xAngle *= -1f;
        }

        return xAngle;
    }



    //Roll is rotation around z-axis
    //Returns positive if rolling left
    private float GetRollError()
    {
        float zAngle = transform.eulerAngles.z;

        //Make sure the angle is between 0 and 360
        zAngle = WrapAngle(zAngle);

        //This angle going from 0-> 360 when rolling left
        //So if angle is > 180 then it should move from 0 to 180 if rolling right
        if (zAngle > 180f && zAngle < 360f)
        {
            zAngle = 360f - zAngle;

            //-1 so we know if we are rolling left or right
            zAngle *= -1f;
        }

        return zAngle;
    }



    float WrapAngle(float inputAngle) // replace int with whatever your type is
    {
        // this will always return an angle between 0 and 360:
        // the inner % 360 restricts everything to +/- 360
        // +360 moves negative values to the positive range, and positive ones to > 360
        // the final % 360 caps everything to 0...360
        return ((inputAngle % 360f) + 360f) % 360f;
    }



    //Add external forces to the quadcopter, such as wind
    private void AddExternalForces()
    {
        //Important to not use the quadcopters forward
        Vector3 windDir = -Vector3.forward;

        //Rotate it 
        windDir = Quaternion.Euler(0, forceDir, 0) * windDir;

        quadcopterRB.AddForce(windDir * windForce);

        //Debug
        //Is showing in which direction the wind is coming from
        //center of quadcopter is where it ends and is blowing in the direction of the line
        Debug.DrawRay(transform.position, -windDir * 3f, Color.red);
    }
}
