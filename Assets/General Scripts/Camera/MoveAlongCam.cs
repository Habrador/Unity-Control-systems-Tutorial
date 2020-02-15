using UnityEngine;
using System.Collections;

public class MoveAlongCam : MonoBehaviour 
{
    public Transform carTrans;

    public float sideDistance;
    public float upDistance;

	void Start() 
	{
	
	}
	
	
	void Update() 
	{
        //Get the cameras new position
        Vector3 newPos = carTrans.position;

        //Move to the side
        newPos += carTrans.right * sideDistance;

        //Move up
        newPos += transform.up * upDistance;

        //Move the camera to this new position
        transform.position = newPos;

        //Look at the car
        transform.LookAt(carTrans);
	}
}
