using UnityEngine;
using System.Collections;

public class FireBall : MonoBehaviour 
{
    //The ball we will fire
    public GameObject ballObj;
    //The force we will use to throw balls
    public float ballForce;
	
	void Start() 
	{
	
	}
	
	
	void Update() 
	{
	    //Fire when click with left mouse
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                //From where should we fire the ball
                Vector3 firePos = Camera.main.transform.position;
                
                //Create a new ball and parent it to this transform
                GameObject newBall = Instantiate(ballObj, firePos, Quaternion.identity, transform) as GameObject;

                //In which direction should we fire the ball
                Vector3 fireDir = (hit.point - firePos).normalized;

                //Fire the ball against the position we hit
                newBall.GetComponent<Rigidbody>().AddForce(ballForce * fireDir, ForceMode.Impulse);
            }
        }
	}
}
