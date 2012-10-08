using UnityEngine;
using System.Collections;

public class ColorAssignmentTest : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		StartCoroutine(co());
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	IEnumerator co()
	{
		byte i = 255;
		while(true)
		{
			Color col = ColorAssigner.AssignColor(i);
			Camera.main.backgroundColor = col;
			print(i + " " + col.ToString());
			i--;
			
			yield return new WaitForSeconds(0.1f);
		}
		
	}
}
