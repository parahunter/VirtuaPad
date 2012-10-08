using UnityEngine;
using System.Collections;

public class DestroyAfterDelay : MonoBehaviour {
	
	public float delay = 2.0f;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		yield return new WaitForSeconds(delay);
		Destroy(gameObject);
	}
	
}
