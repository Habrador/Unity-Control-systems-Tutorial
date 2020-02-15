using UnityEngine;
using System.Collections;

//Will just rotate the propeller
public class RotatePropeller : MonoBehaviour 
{
    public float rotationSpeed;

    public bool shouldRotate = true;

    public bool shouldRotateOtherDirection = false;

	void Start() 
	{
	
	}
	
	
	void Update() 
	{
        //Rotate the propeller around local axis
        if (shouldRotate)
        {
            if (shouldRotateOtherDirection && rotationSpeed > 0f)
            {
                rotationSpeed *= -1;
            }

            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
	}
}
