using UnityEngine;
using System.Collections;

public class OnTriggerExplosion : MonoBehaviour 
{
	public float explosionForce = 1000;
	
	
	void OnTriggerEnter(Collider other)
	{
		if(other.rigidbody != null)
		{
			Vector3 forceVector = (other.transform.position - transform.position).normalized * explosionForce;
			other.rigidbody.AddForce(forceVector);	
		}
	}
	
}
