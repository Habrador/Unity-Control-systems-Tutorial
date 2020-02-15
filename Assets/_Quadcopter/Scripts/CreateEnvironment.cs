using UnityEngine;
using System.Collections;

public class CreateEnvironment : MonoBehaviour 
{
    public GameObject cubeObj;
	public Material[] materials;


	void Start() 
	{
        AddStuffToEnvironment();
	}
	
	
	void AddStuffToEnvironment() 
	{
        float maxScale = 100f;

        for (int i = 0; i < 200; i++)
        {
            Vector3 pos = GenerateRandomPos();

            Vector3 scale = new Vector3(Random.Range(1f, maxScale), Random.Range(1f, maxScale), Random.Range(1f, maxScale));

            GameObject newCube = Instantiate(cubeObj, pos, Quaternion.identity, transform) as GameObject;

            newCube.transform.localScale = scale;

            newCube.GetComponent<MeshRenderer>().material = materials[Random.Range(0, materials.Length)];
        }
	}



    Vector3 GenerateRandomPos()
    {
        float mapSize = 1000f;

        //To avoid creating buildings on the start positon
        float startSize = 20f;

        float randomX = Random.Range(-mapSize, mapSize);

        float randomZ = Random.Range(-mapSize, mapSize);

        while (randomX < startSize && randomX > -startSize)
        {
            randomX = Random.Range(-mapSize, mapSize);
        }

        while (randomZ < startSize && randomZ > -startSize)
        {
            randomZ = Random.Range(-mapSize, mapSize);
        }

        Vector3 finalPos = new Vector3(randomX, 0f, randomZ);

        return finalPos;
    }
}
