using UnityEngine;
using System.Collections;

//Removes a ball after x seconds
public class RemoveBall : MonoBehaviour 
{

	
	void Start() 
	{
        Destroy(gameObject, 120f);
	}
	
	
	void Update() 
	{
	
	}
}
