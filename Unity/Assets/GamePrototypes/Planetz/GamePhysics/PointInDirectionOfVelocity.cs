using UnityEngine;
using System.Collections;

public class PointInDirectionOfVelocity : MonoBehaviour {
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		transform.up = rigidbody.velocity.normalized;
	}
}
