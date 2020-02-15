using UnityEngine;
using System.Collections;

public class InvertedPendulum : MonoBehaviour 
{
    //The car the pendulum is attached to
    public GameObject carObj;
    //Have to move the center of mass because it's not at the center of the hinge
    public float centerOfMassChangeUp;
    //The force
    public float force;

    //PID parameters
    public float gain_P = 0f;
    public float gain_I = 0f;
    public float gain_D = 0f;
    //Sometimes you have to limit the total sum of all errors used in the I
    //public float error_sumMax = 20f;
    private PIDController pidController;

    //Scripts and modules
    private HingeJoint hingeJoint;
    //To control if the car should drive forward or reverse
    private BasicCar carScript;
    //The PID controller
    //private PIDController pidScript;
    //The rigid body
    private Rigidbody hingeRB;


	void Start() 
	{
        hingeJoint = gameObject.GetComponent<HingeJoint>();

        carScript = carObj.GetComponent<BasicCar>();

        //pidScript = gameObject.GetComponent<PIDController>();

        hingeRB = gameObject.GetComponent<Rigidbody>();

        hingeRB.centerOfMass = hingeRB.centerOfMass + centerOfMassChangeUp * transform.up;

        pidController = new PIDController();
    }
	

	
	void FixedUpdate() 
	{
        //Move the center of mass
        //Could do this once in the beginning but more fun to experiment with different values
        //transform.GetComponent<Rigidbody>().centerOfMass = hingeJoint.transform.position + centerOfMassChangeUp * transform.GetChild(0).transform.up;

        //MoveCarNaive();

        MoveCarPID();


        //Add a force to the pendulum
        //We can also lift up the pendulum with the force so we dont have to reset if it falls
        //This can be seen as driving forward/reverse with a segway
        //should be 6 degrees according to report
        //hingeJoint.useMotor = false;
        if (Input.GetKey(KeyCode.W) && hingeJoint.angle < 6f)
        {
            //Could maybe control this as well with a PID controller
            hingeRB.AddForceAtPosition(transform.forward * force, transform.position + transform.up * centerOfMassChangeUp * 2f);
            
            //hingeJoint.useMotor = true;

            //JointMotor motor = new JointMotor();

            //motor.force = force;
            //motor.targetVelocity = 5f;

            //hingeJoint.motor = motor;
        }
        else if (Input.GetKey(KeyCode.S) && hingeJoint.angle > -6f)
        {
            hingeRB.AddForceAtPosition(-transform.forward * force, transform.position + transform.up * centerOfMassChangeUp * 2f);

            //JointMotor motor = new JointMotor();

            //motor.force = force;
            //motor.targetVelocity = -5f;

            //hingeJoint.motor = motor;

            //hingeJoint.useMotor = true;
        }


        Debug.DrawRay(transform.position + transform.up * centerOfMassChangeUp * 2f, transform.forward * 2f, Color.red);
        Debug.DrawRay(transform.position + transform.up * centerOfMassChangeUp * 2f, -transform.forward * 2f, Color.blue);
    }



    //Move the car by just using the angle of the stick 
    void MoveCarNaive()
    {
        //print(hingeJoint.angle);

        float angle = hingeJoint.angle;

        //If the pendulum is swinging forward the angle is positive and the car should drive forward
        if (angle > 0f)
        {
            carScript.MotorTorque = carScript.MotorTorque;
        }
        else if (angle < 0f)
        {
            carScript.MotorTorque = -carScript.MotorTorque;
        }
        else
        {
            carScript.MotorTorque = 0f;
        }
    }



    //Move the car with a PID controller
    void MoveCarPID()
    {
        float error = 0f - hingeJoint.angle;

        error *= -1f;

        float outputFromPID = pidController.GetFactorFromPIDController(gain_P, gain_I, gain_D, error);

        float maxMotorTorque = carScript.MotorTorque;

        float motorTorque = Mathf.Clamp(outputFromPID * maxMotorTorque, -maxMotorTorque, maxMotorTorque);

        //Give the motor torque to the car
        carScript.MotorTorque = motorTorque;
    }
}
